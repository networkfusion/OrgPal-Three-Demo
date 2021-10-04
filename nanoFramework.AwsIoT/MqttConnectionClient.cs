// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using nanoFramework.AwsIoT.Shadows;
using nanoFramework.Json;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace nanoFramework.AwsIoT
{
    // https://github.com/aws/aws-sdk-net/blob/master/sdk/src/Services/IotData/Generated/_netstandard/AmazonIotDataClient.cs
    // https://github.com/aws/aws-iot-device-sdk-embedded-C/tree/master/src
    // https://docs.aws.amazon.com/iot/latest/developerguide/device-shadow-mqtt.html


    /// <summary>
    /// AWS IoT Core MQTT Connection Client for .NET nanoFramework
    /// </summary>
    public class MqttConnectionClient : IDisposable
    {

        private readonly string _iotCoreUri; // FQDN
        const int _mqttsPort = 8883; //Default MQTTS port.
        private readonly X509Certificate2 _clientCert; //ClientRsaSha256Crt and ClientRsaKey
        private readonly X509Certificate _awsRootCACert;

        private M2Mqtt.MqttClient _mqttc;
        private readonly ConnectionState _mqttBrokerStatus = new ConnectionState();
        private readonly ArrayList _methodCallback = new ArrayList();
        private readonly ArrayList _waitForConfirmation = new ArrayList();
        private readonly object _lock = new object();
        private Timer _timerTokenRenew;

        private readonly string _shadowTopic;
        private string _telemetryTopic;
        private string _lwtTopic;
        private string _deviceMessageTopic;

        private Shadow _shadow;
        private bool _shadowReceived;

        /// <summary>
        /// The name of the "Thing"
        /// </summary>
        /// <remarks>
        /// Otherwise known as the "clienttoken" or "Device ID"
        /// </remarks>
        public string ThingName { get; private set; } //Retriveable to make it easier to set custom topics!

        /// <summary>
        /// Device shadow updated event.
        /// </summary>
        public event ShadowUpdated ShadowUpdated;

        /// <summary>
        /// Status change event.
        /// </summary>
        public event StatusUpdated StatusUpdated;

        /// <summary>
        /// Cloud to device message received event.
        /// </summary>
        public event CloudToDeviceMessage CloudToDeviceMessage;


        /// <summary>
        /// Creates an <see cref="MqttConnectionClient"/> class.
        /// </summary>
        /// <param name="iotCoreUri">The AWS IoT Core fully quilified domain name (example: <instance>.<region>.amazonaws.com)</param>
        /// <param name="uniqueId">A unique identity for your device (Device ID / Thing Name).</param>
        /// <param name="clientCert">The certificate used to connect the device to the MQTT broker (containing both the private certificate and private key).</param>
        /// <param name="qosLevel">The default quality of service level for the delivery of MQTT messages, (defaults to the lowest quality)</param>
        /// /// <param name="awsRootCert">The Root (AWS) certificate for the connection to AWS IoT Core</param>
        public MqttConnectionClient(string iotCoreUri, string uniqueId, X509Certificate2 clientCert, MqttQoSLevel qosLevel = MqttQoSLevel.AtMostOnce, X509Certificate awsRootCert = null)
        {
            _clientCert = clientCert;
            _iotCoreUri = iotCoreUri;
            ThingName = uniqueId;
            _shadowTopic = $"$aws/things/{ThingName}/shadow";
            _telemetryTopic = $"device/{ThingName}/messages/events";
            _deviceMessageTopic = $"device/{ThingName}/messages/devicebound";
            _lwtTopic = $"device/{ThingName}/lwt";
            _mqttBrokerStatus.Status = Status.Disconnected;
            _mqttBrokerStatus.Message = string.Empty;
            QosLevel = qosLevel;
            _awsRootCACert = awsRootCert; //TODO: Should override the default one in resources?!
        }

        /// <summary>
        /// The latest Shadow received.
        /// </summary>
        public Shadow LastShadow => _shadow;

        /// <summary>
        /// The latest connection status.
        /// </summary>
        public ConnectionState ConnectionStatus => new ConnectionState(_mqttBrokerStatus);

        /// <summary>
        /// The default quality of service level.
        /// </summary>
        public MqttQoSLevel QosLevel { get; set; }

        /// <summary>
        /// True if the device connected sucessfully.
        /// </summary>
        public bool IsConnected => (_mqttc != null) && _mqttc.IsConnected;



        /// <summary>
        /// Overrides the default topic prefix of "device"
        /// </summary>
        /// <param name="topicPrefix">The topic prefix for telemetry, device messages and LWT.</param>
        /// <returns>True for a successful connection</returns>
        public bool Open(string topicPrefix)
        {
            _telemetryTopic = $"{topicPrefix.TrimEnd('/')}/{ThingName}/messages/events";
            _deviceMessageTopic = $"{topicPrefix.TrimEnd('/')}/{ThingName}/messages/devicebound";
            _lwtTopic = $"{topicPrefix.TrimEnd('/')}/{ThingName}/lwt";
            return Open();
        }

        /// <summary>
        /// Overrides the default connection topics
        /// </summary>
        /// <param name="telemetryTopic">The telemetry topic</param>
        /// <param name="deviceMessageTopic">The device message topic</param>
        /// <param name="lwtTopic">The last will and testiment topic</param>
        /// <returns>True for a successful connection</returns>
        public bool Open(string telemetryTopic, string deviceMessageTopic, string lwtTopic)
        {
            _telemetryTopic = telemetryTopic;
            _deviceMessageTopic = deviceMessageTopic;
            _lwtTopic = lwtTopic;
            return Open();
        }

        /// <summary>
        /// Open the connection with AWS IoT Core. This will connect AWS IoT Core (via MQTT) to the device.
        /// </summary>
        /// <returns>True for a successful connection</returns>
        public bool Open()
        {
            // Creates an MQTT Client with default TLS port 8883 using TLS 1.2 protocol
            _mqttc = new M2Mqtt.MqttClient(
                _iotCoreUri,
                _mqttsPort,
                true,
                _awsRootCACert,
                _clientCert,
                MqttSslProtocols.TLSv1_2);

            // Handler for received messages on the subscribed topics
            _mqttc.MqttMsgPublishReceived += ClientMqttMsgReceived;
            // Handler for publisher
            _mqttc.MqttMsgPublished += ClientMqttMsgPublished;
            // Event when connection has been dropped
            _mqttc.ConnectionClosed += ClientConnectionClosed;

            // Now connect the device
            _mqttc.Connect(
                ThingName,
                "", //TODO: should this be null?
                "", //TODO: should this be null?
                false, //TODO: what does "willretain" actually mean!
                MqttQoSLevel.ExactlyOnce, //TODO: guessing that this is the "default" QOS level?!
                false, _lwtTopic,
                "MQTT connection was unexpectedly disconnected!",
                true, //TODO: this "should" handle persistant connections, and should be configurable?!
                60
                );

            if (_mqttc.IsConnected)
            {
                _mqttc.Subscribe(
                    new[] {
                        $"{_deviceMessageTopic}/#",
                        $"{ _shadowTopic }/#",
                    },
                    new[] {
                        MqttQoSLevel.AtLeastOnce,
                        MqttQoSLevel.AtLeastOnce,
                    }
                );

                _mqttBrokerStatus.Status = Status.Connected;
                _mqttBrokerStatus.Message = string.Empty;
                StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                // We will renew after 10 minutes before midnight just in case:
                _timerTokenRenew = new Timer(TimerCallbackReconnect, null, new TimeSpan(23, 50, 0), TimeSpan.MaxValue);
            }

            return _mqttc.IsConnected;
        }

        /// <summary>
        /// Reconnect to AWS Iot Core MQTT.
        /// </summary>
        public void Reconnect()
        {
            Close();
            Open();
        }

        private void TimerCallbackReconnect(object state)
        {
            _timerTokenRenew.Dispose();
            Reconnect();
        }

        /// <summary>
        /// Close the connection with AWS IoT Core MQTT and disconnect the device.
        /// </summary>
        public void Close()
        {
            if (_mqttc.IsConnected)
            {
                _mqttc.Unsubscribe(new[] {
                    $"{_deviceMessageTopic}/#",
                    $"{ _shadowTopic }/#",
                    });
                _mqttc.Disconnect();
                // Make sure all get disconnected, cleared (TODO: 1 second arbitary value specified)
                Thread.Sleep(1000);
            }

            _timerTokenRenew.Dispose();
        }

        /// <summary>
        /// Gets the device shadow.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <param name="namedShadow">A named shadow</param>
        /// <returns>The shadow.</returns>
        /// <remarks>It is strongly recommended to use a cancellation token that can be canceled and manage this on the 
        /// caller code level. A reasonable time of few seconds is recommended with a retry mechanism.</remarks>
        public Shadow GetShadow(CancellationToken cancellationToken = default, string namedShadow = "")
        {
            _shadowReceived = false;

            var topic = $"{_shadowTopic}/get";
            if (namedShadow != string.Empty)
            {
                topic = $"{_shadowTopic}/name/{namedShadow}/get";
            }

            _mqttc.Publish(topic, Encoding.UTF8.GetBytes(""), MqttQoSLevel.AtLeastOnce, false);

            while (!_shadowReceived && !cancellationToken.IsCancellationRequested) // TODO: not firing (or receiving message?!)
            {
                cancellationToken.WaitHandle.WaitOne(200, true);
            }

            return _shadowReceived ? _shadow : null;
        }

        ///// <summary>
        ///// Publishes a Shadow
        ///// </summary>
        ///// <param name="cancellationToken"></param>
        ///// <param name="namedShadow"></param>
        ///// <returns></returns>
        //public bool PublishShadow(CancellationToken cancellationToken = default, string namedShadow = "")
        //{
        //    //we could possibily use this method (or reserve it for the future) but need a good reason!
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Deletes a Shadow
        ///// </summary>
        ///// <param name="cancellationToken"></param>
        ///// <param name="namedShadow"></param>
        ///// <returns></returns>
        //public bool DeleteShadow(CancellationToken cancellationToken = default, string namedShadow = "")
        //{
        //    //we could possibily use this method (or reserve it for the future) but need a good reason!
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Update the device shadow reported state.
        /// </summary>
        /// <param name="reported">The reported properties.</param>
        /// <param name="cancellationToken">A cancellation token. If you use the default one, the confirmation of delivery will not be awaited.</param>
        /// <returns>True for successful message delivery.</returns>
        public bool UpdateReportedState(Shadow shadowToSend, CancellationToken cancellationToken = default, string namedShadow = "") //was ShadowCollection
        {
            var topic = $"{_shadowTopic}/update";
            if (namedShadow != string.Empty)
            {
                topic = $"{_shadowTopic}/name/{namedShadow}/update";
            }

            string shadow = shadowToSend.ToJson(true);
            Debug.WriteLine($"updatereportedstate shadow content: {shadow}");
            var rid = _mqttc.Publish(topic, Encoding.UTF8.GetBytes(shadow), MqttQoSLevel.AtLeastOnce, false);
            _mqttBrokerStatus.Status = Status.ShadowUpdated;
            _mqttBrokerStatus.Message = string.Empty;
            StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));

            if (cancellationToken.CanBeCanceled)
            {
                ConfirmationStatus confirmSuccess = new(rid);
                _waitForConfirmation.Add(confirmSuccess);
                while (!confirmSuccess.Received && !cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.WaitHandle.WaitOne(200, true);
                }

                _waitForConfirmation.Remove(confirmSuccess);
                return confirmSuccess.Received; //Received == false
            }

                return false; //Received == false
        }

        /// <summary>
        /// Add a callback method.
        /// </summary>
        /// <param name="methodCallback">The callback method to add.</param>
        public void AddMethodCallback(MethodCallback methodCallback)
        {
            _methodCallback.Add(methodCallback);
        }

        /// <summary>
        /// Remove a callback method.
        /// </summary>
        /// <param name="methodCallback">The callback method to remove.</param>
        public void RemoveMethodCallback(MethodCallback methodCallback)
        {
            _methodCallback.Remove(methodCallback);
        }

        /// <summary>
        /// Send a message to Aws IoT Core.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">A cancellation token. If you use the default one, the confirmation of delivery will not be awaited.</param>
        /// <returns>True for successful message delivery.</returns>
        public bool SendMessage(string message, CancellationToken cancellationToken = default)
        {

            var rid = _mqttc.Publish(_telemetryTopic, Encoding.UTF8.GetBytes(message), QosLevel, false);

            if (cancellationToken.CanBeCanceled)
            {
                ConfirmationStatus conf = new(rid);
                _waitForConfirmation.Add(conf);
                while (!conf.Received && !cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.WaitHandle.WaitOne(200, true);
                }

                _waitForConfirmation.Remove(conf);
                return conf.Received;
            }

            return false;
        }

        private void ClientMqttMsgReceived(object sender, MqttMsgPublishEventArgs e) //TODO: can we also add subscriptions publically?!! 
        {
            Debug.WriteLine($"Received message on MqttClient subscribed topic: {e.Topic}");

            if (e.Message.Length > 0 && e.Message != null)
                {
                try
                { //TODO: need to revisit this https://docs.aws.amazon.com/iot/latest/developerguide/device-shadow-mqtt.html#update-documents-pub-sub-topic to understand the full implementation!
                    string jsonMessageBody = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
                    Debug.WriteLine($"Decoded message was: {jsonMessageBody}");

                    if (e.Topic.StartsWith($"{_shadowTopic}/update"))
                    {
                        //TODO: we might have to be more specific with subscribed topics here... I think receiving some of these could cost money, even if they are irrelevent!
                        Debug.WriteLine($"Reached {_shadowTopic}/update");
                        if (e.Topic.IndexOf("/rejected") > 0)
                        {
                            Debug.WriteLine($"Reached {_shadowTopic}/rejected");
                            _mqttBrokerStatus.Status = Status.ShadowUpdateError;
                            _mqttBrokerStatus.Message = jsonMessageBody;
                            StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                        }
                        else if (e.Topic.IndexOf("/delta") > 0) //TODO: if this worked correctly, it should be a partial update!
                        {
                            Debug.WriteLine($"Reached {_shadowTopic}/delta");
                            ShadowUpdated?.Invoke(this, new ShadowUpdateEventArgs(new Shadow(jsonMessageBody))); //ShadowUpdated?.Invoke(this, new ShadowUpdateEventArgs(new ShadowCollection(jsonMessageBody)));
                            _mqttBrokerStatus.Status = Status.ShadowUpdateReceived;
                            _mqttBrokerStatus.Message = jsonMessageBody;
                            StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                        }
                        else if (e.Topic.IndexOf("/accepted") > 0) //TODO: this should not be required, since a delta should take precidence (but what if there is no delta?!)...
                        {
                            Debug.WriteLine($"Reached {_shadowTopic}/accepted");
                            _mqttBrokerStatus.Status = Status.ShadowUpdated;
                            _mqttBrokerStatus.Message = jsonMessageBody;
                            StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                        }
                        //else if (e.Topic.IndexOf("/document") > 0) //TODO: probably not required to handle as the full change?!
                        //{
                        //    Debug.WriteLine($"Reached {_shadowTopic}/document");
                        //    _mqttBrokerStatus.Status = Status.ShadowUpdated;
                        //    _mqttBrokerStatus.Message = jsonMessageBody;
                        //    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                        //}
                        else
                        {
                            if (string.IsNullOrEmpty(jsonMessageBody)) //!!! AWS is so wasteful... this cost money !!!
                            {
                                Debug.WriteLine($"Received an empty message on: {e.Topic}");
                            }
                            else
                            {
                                Debug.WriteLine($"Received a message on: {e.Topic} that is not handled: {jsonMessageBody}");
                            }

                        }

                    }
                    else if (e.Topic.StartsWith($"{_shadowTopic}/get"))
                    {
                        Debug.WriteLine($"Reached {_shadowTopic}/get");
                        if (e.Topic.IndexOf("/rejected") > 0)
                        {
                            _mqttBrokerStatus.Status = Status.ShadowUpdateError;
                            _mqttBrokerStatus.Message = jsonMessageBody;
                            StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                        }
                        else if (e.Topic.IndexOf("/accepted") > 0)
                        {
                            _shadow = (Shadow)JsonConvert.DeserializeObject(jsonMessageBody, typeof(Shadow)); // new Shadow(jsonMessageBody);
                            //_shadow = new Shadow(_uniqueId, jsonMessageBody); //TODO: Shadow (auto deserialize from JSON)
                            _shadowReceived = true;
                            _mqttBrokerStatus.Status = Status.ShadowReceived;
                            _mqttBrokerStatus.Message = jsonMessageBody;
                            StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(jsonMessageBody)) //!!! AWS is so wasteful... this cost money !!!
                            {
                                Debug.WriteLine($"Received an empty message on: {e.Topic}");
                            }
                            else
                            {
                                Debug.WriteLine($"Received a message on: {e.Topic} that is not handled: {jsonMessageBody}");
                            }

                        }
                    }
                    else if (e.Topic.StartsWith(_deviceMessageTopic))
                    {
                        string messageTopic = e.Topic.Substring(_deviceMessageTopic.Length);
                        _mqttBrokerStatus.Status = Status.MessageReceived;
                        _mqttBrokerStatus.Message = $"{messageTopic}/{jsonMessageBody}";
                        StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                        CloudToDeviceMessage?.Invoke(this, new CloudToDeviceMessageEventArgs(jsonMessageBody, messageTopic));
                    }
                    else //Other (unknown) topic message received!
                    {
                        _mqttBrokerStatus.Status = Status.IoTCoreWarning;
                        _mqttBrokerStatus.Message = $"Unknown topic or message: {e.Topic} :: {jsonMessageBody}";

                        Debug.WriteLine($"Received unknown message on topic: {e.Topic}:.. {jsonMessageBody}");

                        StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in event: {ex}");
                    _mqttBrokerStatus.Status = Status.InternalError;
                    _mqttBrokerStatus.Message = ex.ToString();
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
                }
            }
            else
            {
                Debug.WriteLine($"MqttClient subscribed topic {e.Topic}: Message received was empty!");
            }
        }

        private void ClientMqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            if (_waitForConfirmation.Count == 0)
            {
                return;
            }

            // Making sure the object will not be added or removed in this loop
            lock (_lock)
            {
                foreach (ConfirmationStatus status in _waitForConfirmation)
                {
                    if (status.ResponseId == e.MessageId)
                    {
                        status.Received = true;
                        // messages are unique
                        return;
                    }
                }
            }
        }

        private void ClientConnectionClosed(object sender, EventArgs e)
        {
            _mqttBrokerStatus.Status = Status.Disconnected;
            _mqttBrokerStatus.Message = string.Empty;
            StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_mqttBrokerStatus));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_mqttc != null)
            {
                // Making sure we unregister to events
                _mqttc.MqttMsgPublishReceived -= ClientMqttMsgReceived;
                _mqttc.MqttMsgPublished -= ClientMqttMsgPublished;
                _mqttc.ConnectionClosed -= ClientConnectionClosed;
                // Closing and waiting for the connection to be properly closed
                Close();
                while (_mqttc.IsConnected)
                {
                    Thread.Sleep(100);

                }

                // Cleaning
                GC.SuppressFinalize(_mqttc);
                _mqttc = null;
            }
        }
    }
}
