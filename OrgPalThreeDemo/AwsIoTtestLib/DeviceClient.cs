// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using nanoFramework.AwsIoT.Devices.Shared;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace nanoFramework.AwsIoT.Devices.Client
{
    /// <summary>
    /// AWS IoT Core Client SDK for .NET nanoFramework using MQTT
    /// </summary>
    public class DeviceClient : IDisposable
    {
        const string ShadowTopicPrefix = "$aws/things/";
        const string shadowTopicPostFix = "/shadow";

        //const string ShadowReportedPropertiesTopic = "$iothub/shadow/PATCH/properties/reported/";
        //const string ShadowDesiredPropertiesTopic = "$iothub/shadow/GET/";
        //const string DirectMethodTopic = "$iothub/methods/POST/";

        private readonly string _iotCoreName; // FQDN
        private readonly string _deviceId; //Otherwise known as the Thing Name.
        const int _mqttsPort = 8883; //Default MQTTS port.
        private readonly X509Certificate2 _clientCert; //ClientRsaSha256Crt and ClientRsaKey
        private readonly X509Certificate _awsRootCACert;
        //private readonly string _privateKey;

        private MqttClient _mqttc;
        private readonly IoTCoreStatus _ioTCoreStatus = new IoTCoreStatus();
        private readonly ArrayList _methodCallback = new ArrayList();
        private readonly ArrayList _waitForConfirmation = new ArrayList();
        private readonly object _lock = new object();
        private Timer _timerTokenRenew;

        private readonly string _telemetryTopic;
        //private readonly string _deviceMessageTopic;

        private Shadow _shadow;
        private bool _shadowReceived;


        /// <summary>
        /// Device shadow updated event.
        /// </summary>
        public event ShadowUpdated ShadowUpated;

        /// <summary>
        /// Status change event.
        /// </summary>
        public event StatusUpdated StatusUpdated;

        ///// <summary>
        ///// Cloud to device message received event.
        ///// </summary>
        //public event CloudToDeviceMessage CloudToDeviceMessage;

        ///// <summary>
        ///// Creates an <see cref="DeviceClient"/> class.
        ///// </summary>
        ///// <param name="iotCoreName">The AWS IoT Core fully quilified domain name (example: <instance>.<region>.amazonaws.com)</param>
        ///// <param name="deviceId">The device ID which is the name of your device.</param>
        ///// <param name="sasKey">One of the SAS Key either primary, either secondary.</param>
        ///// <param name="qosLevel">The default quality level delivery for the MQTT messages, default to the lower quality</param>
        ///// <param name="awsRootCert">Azure certificate for the connection to Azure IoT Hub</param>
        //public DeviceClient(string iotCoreName, string deviceId, string sasKey, MqttQoSLevel qosLevel = MqttQoSLevel.AtMostOnce, X509Certificate awsRootCert = null)
        //{
        //    _clientCert = null;
        //    _privateKey = null;
        //    _iotCoreName = iotCoreName;
        //    _deviceId = deviceId;
        //    //_sasKey = sasKey;
        //    _telemetryTopic = $"devices/{_deviceId}/messages/events/";
        //    _ioTCoreStatus.Status = Status.Disconnected;
        //    _ioTCoreStatus.Message = string.Empty;
        //    _deviceMessageTopic = $"devices/{_deviceId}/messages/devicebound/";
        //    QosLevel = qosLevel;
        //    _awsRootCACert = awsRootCert;
        //}

        /// <summary>
        /// Creates an <see cref="DeviceClient"/> class.
        /// </summary>
        /// <param name="iotCoreName">The AWS IoT Core fully quilified domain name (example: <instance>.<region>.amazonaws.com)</param>
        /// <param name="deviceId">The device (thing) ID which is the unique name of your device.</param>
        /// <param name="clientCert">The certificate used to connect the device to the MQTT broker (containing both the public and private key).</param>
        /// <param name="qosLevel">The default quality of service level for the delivery of MQTT messages, (defaults to the lowest quality)</param>
        /// /// <param name="awsRootCert">The Root (AWS) certificate for the connection to AWS IoT Core</param>
        public DeviceClient(string iotCoreName, string deviceId, X509Certificate2 clientCert, MqttQoSLevel qosLevel = MqttQoSLevel.AtMostOnce, X509Certificate awsRootCert = null)
        {
            _clientCert = clientCert;
            //_privateKey = Convert.ToBase64String(clientCert.PrivateKey);
            _iotCoreName = iotCoreName;
            _deviceId = deviceId;
            _telemetryTopic = $"devices/{_deviceId}/messages/events/"; //TODO: should we make this configurable?!
            _ioTCoreStatus.Status = Status.Disconnected;
            _ioTCoreStatus.Message = string.Empty;
            //_deviceMessageTopic = $"devices/{_deviceId}/messages/devicebound/"; //TODO: should we make this configurable?!
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
        public IoTCoreStatus IoTCoreStatus => new IoTCoreStatus(_ioTCoreStatus);

        /// <summary>
        /// The default quality of service level.
        /// </summary>
        public MqttQoSLevel QosLevel { get; set; }

        /// <summary>
        /// True if the device connected sucessfully.
        /// </summary>
        public bool IsConnected => (_mqttc != null) && _mqttc.IsConnected;

        /// <summary>
        /// Open the connection with AWS IoT Core. This will connect AWS IoT Core (via MQTT) to the device.
        /// </summary>
        /// <returns>True for a successful connection</returns>
        public bool Open()
        {
            // Creates an MQTT Client with default TLS port 8883 using TLS 1.2 protocol
            _mqttc = new MqttClient(
                _iotCoreName,
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
                _deviceId,
                "",
                "",
                false, //TODO: what does "willretain" actually mean!
                MqttQoSLevel.ExactlyOnce,
                false, "device/will/topic/",
                "MQTT client unexpectedly disconnected",
                true, //TODO: this "should" handle persistant connections, and should be configurable?!
                60
                );

            if (_mqttc.IsConnected)
            {
                _mqttc.Subscribe(
                    new[] {
                        //$"devices/{_deviceId}/messages/devicebound/#",
                        $"{ ShadowTopicPrefix }{_deviceId}{ shadowTopicPostFix }/#", // "$iothub/shadow/#",
                        //"$iothub/methods/POST/#"
                    },
                    new[] {
                        //MqttQoSLevel.AtLeastOnce,
                        MqttQoSLevel.AtLeastOnce,
                        //MqttQoSLevel.AtLeastOnce
                    }
                );

                _ioTCoreStatus.Status = Status.Connected;
                _ioTCoreStatus.Message = string.Empty;
                StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                // We will renew after 10 minutes before just in case
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
                    //$"devices/{_deviceId}/messages/devicebound/#",
                    $"{ ShadowTopicPrefix }{_deviceId}{ shadowTopicPostFix }/#", // "$iothub/shadow/#",
                    //"$iothub/methods/POST/#"
                    });
                _mqttc.Disconnect();
                // Make sure all get disconnected, cleared 
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

            var topic = $"{ShadowTopicPrefix}{_deviceId}{shadowTopicPostFix}/get";
            if (namedShadow != string.Empty)
            {
                topic = $"{ShadowTopicPrefix}{_deviceId}{shadowTopicPostFix}/name/{namedShadow}/get";
            }

            _mqttc.Publish(topic, Encoding.UTF8.GetBytes(""), MqttQoSLevel.AtLeastOnce, false);

            while (!_shadowReceived && !cancellationToken.IsCancellationRequested)
            {
                cancellationToken.WaitHandle.WaitOne(200, true);
            }

            return _shadowReceived ? _shadow : null;
        }

        /// <summary>
        /// Update the device shadow reported properties.
        /// </summary>
        /// <param name="reported">The reported properties.</param>
        /// <param name="cancellationToken">A cancellation token. If you use the default one, the confirmation of delivery will not be awaited.</param>
        /// <returns>True for successful message delivery.</returns>
        public bool UpdateReportedProperties(ShadowCollection reported, CancellationToken cancellationToken = default, string namedShadow = "")
        {
            var topic = $"{ShadowTopicPrefix}{_deviceId}{shadowTopicPostFix}/update";
            if (namedShadow != string.Empty)
            {
                topic = $"{ShadowTopicPrefix}{_deviceId}{shadowTopicPostFix}/name/{namedShadow}/update";
            }

            string shadow = reported.ToJson();
            Debug.WriteLine($"update shadow: {shadow}");
            var rid = _mqttc.Publish(topic, Encoding.UTF8.GetBytes(shadow), MqttQoSLevel.AtLeastOnce, false);
            _ioTCoreStatus.Status = Status.ShadowUpdated;
            _ioTCoreStatus.Message = string.Empty;
            StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));

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
            try
            { //TODO: need to revisit this https://docs.aws.amazon.com/iot/latest/developerguide/device-shadow-mqtt.html#update-documents-pub-sub-topic to understand the full implementation!
                string message = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);

                if (e.Topic.StartsWith($"{ShadowTopicPrefix}{_deviceId}{shadowTopicPostFix}/update/accepted"))  //($"$iothub/shadow/res/204")) //TODO: what about named shadows!
                {
                    _ioTCoreStatus.Status = Status.ShadowUpdateReceived;
                    _ioTCoreStatus.Message = string.Empty;
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                }
                else if (e.Topic.StartsWith($"{ShadowTopicPrefix}{_deviceId}{shadowTopicPostFix}/update/")) //("$iothub/shadow/"))
                {
                    if (e.Topic.IndexOf("rejected/") > 0) //if (e.Topic.IndexOf("res/400/") > 0 || e.Topic.IndexOf("res/404/") > 0 || e.Topic.IndexOf("res/500/") > 0)
                    {
                        _ioTCoreStatus.Status = Status.ShadowUpdateError;
                        _ioTCoreStatus.Message = string.Empty;
                        StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                    }
                    else if (e.Topic.IndexOf("delta/") > 0) //(e.Topic.StartsWith("$iothub/shadow/PATCH/properties/desired/")) //Or should this be "document"?!
                    {
                        ShadowUpated?.Invoke(this, new ShadowUpdateEventArgs(new ShadowCollection(message)));
                        _ioTCoreStatus.Status = Status.ShadowUpdateReceived;
                        _ioTCoreStatus.Message = string.Empty;
                        StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                    }
                    else //TODO: is this handling "documents"
                    {
                        if ((message.Length > 0) && !_shadowReceived)
                        {
                            // skip if already received in this session                         
                            try
                            {
                                _shadow = new Shadow(_deviceId, message);
                                _shadowReceived = true;
                                _ioTCoreStatus.Status = Status.ShadowReceived;
                                _ioTCoreStatus.Message = message;
                                StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Exception receiving the shadows: {ex}");
                                _ioTCoreStatus.Status = Status.InternalError;
                                _ioTCoreStatus.Message = ex.ToString();
                                StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                            }
                        }
                    }
                }
                //else if (e.Topic.StartsWith(DirectMethodTopic))
                //{
                //    const string C9PatternMainStyle = "<<Main>$>g__";
                //    string method = e.Topic.Substring(DirectMethodTopic.Length);
                //    string methodName = method.Substring(0, method.IndexOf('/'));
                //    int rid = Convert.ToInt32(method.Substring(method.IndexOf('=') + 1));
                //    _ioTCoreStatus.Status = Status.DirectMethodCalled;
                //    _ioTCoreStatus.Message = $"{method}/{message}";
                //    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                //    foreach (MethodCallback mt in _methodCallback)
                //    {
                //        string mtName = mt.Method.Name;
                //        if (mtName.Contains(C9PatternMainStyle))
                //        {
                //            mtName = mtName.Substring(C9PatternMainStyle.Length);
                //            mtName = mtName.Substring(0, mtName.IndexOf('|'));
                //        }
                //        if (mtName == methodName)
                //        {
                //            try
                //            {
                //                var res = mt.Invoke(rid, message);
                //                _mqttc.Publish($"$iothub/methods/res/200/?$rid={rid}", Encoding.UTF8.GetBytes(res), MqttQoSLevel.AtLeastOnce, false);
                //            }
                //            catch (Exception ex)
                //            {
                //                _mqttc.Publish($"$iothub/methods/res/504/?$rid={rid}", Encoding.UTF8.GetBytes($"{{\"Exception:\":\"{ex}\"}}"), MqttQoSLevel.AtLeastOnce, false);
                //            }
                //        }
                //    }
                //}
                //else if (e.Topic.StartsWith(_deviceMessageTopic))
                //{
                //    string messageTopic = e.Topic.Substring(_deviceMessageTopic.Length);
                //    _ioTCoreStatus.Status = Status.MessageReceived;
                //    _ioTCoreStatus.Message = $"{messageTopic}/{message}";
                //    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                //    CloudToDeviceMessage?.Invoke(this, new CloudToDeviceMessageEventArgs(message, messageTopic));
                //}
                //else if (e.Topic.StartsWith("$iothub/clientproxy/"))
                //{
                //    _ioTCoreStatus.Status = Status.Disconnected;
                //    _ioTCoreStatus.Message = message;
                //    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                //}
                //else if (e.Topic.StartsWith("$iothub/logmessage/Info"))
                //{
                //    _ioTCoreStatus.Status = Status.IoTCoreInformation;
                //    _ioTCoreStatus.Message = message;
                //    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                //}
                //else if (e.Topic.StartsWith("$iothub/logmessage/HighlightInfo"))
                //{
                //    _ioTCoreStatus.Status = Status.IoTCoreHighlightInformation;
                //    _ioTCoreStatus.Message = message;
                //    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                //}
                //else if (e.Topic.StartsWith("$iothub/logmessage/Error"))
                //{
                //    _ioTCoreStatus.Status = Status.IoTCoreError;
                //    _ioTCoreStatus.Message = message;
                //    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                //}
                //else if (e.Topic.StartsWith("$iothub/logmessage/Warning"))
                //{
                //    _ioTCoreStatus.Status = Status.IoTCoreWarning;
                //    _ioTCoreStatus.Message = message;
                //    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                //}

                //else //TODO: Other message type callback or throw?!
                //{
                //    _ioTCoreStatus.Status = Status.MessageReceived;
                //    _ioTCoreStatus.Message = e.Message.ToString();
                //    Debug.WriteLine(e.Message.ToString());
                //    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                //}

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in event: {ex}");
                _ioTCoreStatus.Status = Status.InternalError;
                _ioTCoreStatus.Message = ex.ToString();
                StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
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
            _ioTCoreStatus.Status = Status.Disconnected;
            _ioTCoreStatus.Message = string.Empty;
            StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
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
