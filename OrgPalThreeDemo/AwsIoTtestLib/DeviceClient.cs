// Copyright (c) Microsoft. All rights reserved.
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
    /// Azure IoT Client SDK for .NET nanoFramework using MQTT
    /// </summary>
    public class DeviceClient : IDisposable
    {
        const string ShadowReportedPropertiesTopic = "$iothub/twin/PATCH/properties/reported/";
        const string ShadowDesiredPropertiesTopic = "$iothub/twin/GET/";
        const string DirectMethodTopic = "$iothub/methods/POST/";

        private readonly string _iotCoreName;
        private readonly string _deviceId;
        //private readonly string _sasKey;
        private readonly string _telemetryTopic;
        private readonly X509Certificate2 _clientCert;
        private readonly string _deviceMessageTopic;
        private readonly string _privateKey;
        private Shadow _shadow;
        private bool _shadowReceived;
        private MqttClient _mqttc;
        private readonly IoTCoreStatus _ioTCoreStatus = new IoTCoreStatus();
        private readonly ArrayList _methodCallback = new ArrayList();
        private readonly ArrayList _waitForConfirmation = new ArrayList();
        private readonly object _lock = new object();
        private Timer _timerTokenRenew;
        private readonly X509Certificate _awsRootCACert;

        /// <summary>
        /// Device twin updated event.
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

        /// <summary>
        /// Creates an <see cref="DeviceClient"/> class.
        /// </summary>
        /// <param name="iotHubName">The Azure IoT name fully qualified (ex: youriothub.azure-devices.net)</param>
        /// <param name="deviceId">The device ID which is the name of your device.</param>
        /// <param name="sasKey">One of the SAS Key either primary, either secondary.</param>
        /// <param name="qosLevel">The default quality level delivery for the MQTT messages, default to the lower quality</param>
        /// <param name="awsRootCert">Azure certificate for the connection to Azure IoT Hub</param>
        public DeviceClient(string iotCoreName, string deviceId, string sasKey, MqttQoSLevel qosLevel = MqttQoSLevel.AtMostOnce, X509Certificate awsRootCert = null)
        {
            _clientCert = null;
            _privateKey = null;
            _iotCoreName = iotCoreName;
            _deviceId = deviceId;
            //_sasKey = sasKey;
            _telemetryTopic = $"devices/{_deviceId}/messages/events/";
            _ioTCoreStatus.Status = Status.Disconnected;
            _ioTCoreStatus.Message = string.Empty;
            _deviceMessageTopic = $"devices/{_deviceId}/messages/devicebound/";
            QosLevel = qosLevel;
            _awsRootCACert = awsRootCert;
        }

        /// <summary>
        /// Creates an <see cref="DeviceClient"/> class.
        /// </summary>
        /// <param name="iotHubName">The Aws IoT Core domainname fully qualified (ex: youriotinstance.azure-devices.net)</param>
        /// <param name="deviceId">The device ID which is the name of your device.</param>
        /// <param name="clientCert">The certificate to connect the device containing both public and private key.</param>
        /// <param name="qosLevel">The default quality level delivery for the MQTT messages, default to the lower quality</param>
        /// /// <param name="awsRootCert">AWS Root certificate for the connection to AWS IoT Core</param>
        public DeviceClient(string iotHubName, string deviceId, X509Certificate2 clientCert, MqttQoSLevel qosLevel = MqttQoSLevel.AtMostOnce, X509Certificate awsRootCert = null)
        {
            _clientCert = clientCert;
            _privateKey = Convert.ToBase64String(clientCert.PrivateKey);
            _iotCoreName = iotHubName;
            _deviceId = deviceId;
            //_sasKey = null;
            _telemetryTopic = $"devices/{_deviceId}/messages/events/";
            _ioTCoreStatus.Status = Status.Disconnected;
            _ioTCoreStatus.Message = string.Empty;
            _deviceMessageTopic = $"devices/{_deviceId}/messages/devicebound/";
            QosLevel = qosLevel;
            _awsRootCACert = awsRootCert;
        }

        /// <summary>
        /// The latest Twin received.
        /// </summary>
        public Shadow LastShadow => _shadow;

        /// <summary>
        /// The latests status.
        /// </summary>
        public IoTCoreStatus IoTCoreStatus => new IoTCoreStatus(_ioTCoreStatus);

        /// <summary>
        /// The default level quality.
        /// </summary>
        public MqttQoSLevel QosLevel { get; set; }

        /// <summary>
        /// True if the device connected
        /// </summary>
        public bool IsConnected => (_mqttc != null) && _mqttc.IsConnected;

        /// <summary>
        /// Open the connection with Aws IoT Core. This will connected AWS IoT Core to the device.
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            // Creates an MQTT Client with default port 8883 using TLS protocol
            _mqttc = new MqttClient(
                _iotCoreName,
                8883,
                true,
                _awsRootCACert,
                _clientCert,
                MqttSslProtocols.TLSv1_2);

            // Handler for received messages on the subscribed topics
            _mqttc.MqttMsgPublishReceived += ClientMqttMsgReceived;
            // Handler for publisher
            _mqttc.MqttMsgPublished += ClientMqttMsgPublished;
            // event when connection has been dropped
            _mqttc.ConnectionClosed += ClientConnectionClosed;

            // Now connect the device
            string key = ""; //_clientCert == null ? Helper.GetSharedAccessSignature(null, _sasKey, $"{_iotCoreName}/devices/{_deviceId}", new TimeSpan(24, 0, 0)) : _privateKey;
            _mqttc.Connect(
                _deviceId,
                $"{_iotCoreName}/{_deviceId}/api-version=2020-09-30",
                key,
                false,
                MqttQoSLevel.ExactlyOnce,
                false, "$iothub/twin/GET/?$rid=999",
                "Disconnected",
                true,
                60
                );

            if (_mqttc.IsConnected)
            {
                _mqttc.Subscribe(
                    new[] {
                        $"devices/{_deviceId}/messages/devicebound/#",
                        "$iothub/twin/#",
                        "$iothub/methods/POST/#"
                    },
                    new[] {
                        MqttQoSLevel.AtLeastOnce,
                        MqttQoSLevel.AtLeastOnce,
                        MqttQoSLevel.AtLeastOnce
                    }
                );

                _ioTCoreStatus.Status = Status.Connected;
                _ioTCoreStatus.Message = string.Empty;
                StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                // We will renew 10 minutes before just in case
                _timerTokenRenew = new Timer(TimerCallbackReconnect, null, new TimeSpan(23, 50, 0), TimeSpan.MaxValue);
            }

            return _mqttc.IsConnected;
        }

        /// <summary>
        /// Reconnect to Azure Iot Hub
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
        /// Close the connection with Azure IoT and disconnect the device.
        /// </summary>
        public void Close()
        {
            if (_mqttc.IsConnected)
            {
                _mqttc.Unsubscribe(new[] {
                    $"devices/{_deviceId}/messages/devicebound/#",
                    "$iothub/twin/#",
                    "$iothub/methods/POST/#"
                    });
                _mqttc.Disconnect();
                // Make sure all get disconnected, cleared 
                Thread.Sleep(1000);
            }

            _timerTokenRenew.Dispose();
        }

        /// <summary>
        /// Gets the twin.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The twin.</returns>
        /// <remarks>It is strongly recommended to use a cancellation token that can be canceled and manage this on the 
        /// caller code level. A reasonable time of few seconds is recommended with a retry mechanism.</remarks>
        public Shadow GetShadow(CancellationToken cancellationToken = default)
        {
            _shadowReceived = false;
            _mqttc.Publish($"{ShadowDesiredPropertiesTopic}?$rid={Guid.NewGuid()}", Encoding.UTF8.GetBytes(""), MqttQoSLevel.AtLeastOnce, false);

            while (!_shadowReceived && !cancellationToken.IsCancellationRequested)
            {
                cancellationToken.WaitHandle.WaitOne(200, true);
            }

            return _shadowReceived ? _shadow : null;
        }

        /// <summary>
        /// Update the twin reported properties.
        /// </summary>
        /// <param name="reported">The reported properties.</param>
        /// <param name="cancellationToken">A cancellation token. If you use the default one, the confirmation of delivery will not be awaited.</param>
        /// <returns>True for successful message delivery.</returns>
        public bool UpdateReportedProperties(ShadowCollection reported, CancellationToken cancellationToken = default)
        {
            string twin = reported.ToJson();
            Debug.WriteLine($"update twin: {twin}");
            var rid = _mqttc.Publish($"{ShadowReportedPropertiesTopic}?$rid={Guid.NewGuid()}", Encoding.UTF8.GetBytes(twin), MqttQoSLevel.AtLeastOnce, false);
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
        /// Send a message to Azure IoT.
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

        private void ClientMqttMsgReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                string message = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);

                if (e.Topic.StartsWith("$iothub/twin/res/204"))
                {
                    _ioTCoreStatus.Status = Status.ShadowUpdateReceived;
                    _ioTCoreStatus.Message = string.Empty;
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                }
                else if (e.Topic.StartsWith("$iothub/twin/"))
                {
                    if (e.Topic.IndexOf("res/400/") > 0 || e.Topic.IndexOf("res/404/") > 0 || e.Topic.IndexOf("res/500/") > 0)
                    {
                        _ioTCoreStatus.Status = Status.ShadowUpdateError;
                        _ioTCoreStatus.Message = string.Empty;
                        StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                    }
                    else if (e.Topic.StartsWith("$iothub/twin/PATCH/properties/desired/"))
                    {
                        ShadowUpated?.Invoke(this, new ShadowUpdateEventArgs(new ShadowCollection(message)));
                        _ioTCoreStatus.Status = Status.ShadowUpdateReceived;
                        _ioTCoreStatus.Message = string.Empty;
                        StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                    }
                    else
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
                                Debug.WriteLine($"Exception receiving the twins: {ex}");
                                _ioTCoreStatus.Status = Status.InternalError;
                                _ioTCoreStatus.Message = ex.ToString();
                                StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                            }
                        }
                    }
                }
                else if (e.Topic.StartsWith(DirectMethodTopic))
                {
                    const string C9PatternMainStyle = "<<Main>$>g__";
                    string method = e.Topic.Substring(DirectMethodTopic.Length);
                    string methodName = method.Substring(0, method.IndexOf('/'));
                    int rid = Convert.ToInt32(method.Substring(method.IndexOf('=') + 1));
                    _ioTCoreStatus.Status = Status.DirectMethodCalled;
                    _ioTCoreStatus.Message = $"{method}/{message}";
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                    foreach (MethodCallback mt in _methodCallback)
                    {
                        string mtName = mt.Method.Name;
                        if (mtName.Contains(C9PatternMainStyle))
                        {
                            mtName = mtName.Substring(C9PatternMainStyle.Length);
                            mtName = mtName.Substring(0, mtName.IndexOf('|'));
                        }
                        if (mtName == methodName)
                        {
                            try
                            {
                                var res = mt.Invoke(rid, message);
                                _mqttc.Publish($"$iothub/methods/res/200/?$rid={rid}", Encoding.UTF8.GetBytes(res), MqttQoSLevel.AtLeastOnce, false);
                            }
                            catch (Exception ex)
                            {
                                _mqttc.Publish($"$iothub/methods/res/504/?$rid={rid}", Encoding.UTF8.GetBytes($"{{\"Exception:\":\"{ex}\"}}"), MqttQoSLevel.AtLeastOnce, false);
                            }
                        }
                    }
                }
                else if (e.Topic.StartsWith(_deviceMessageTopic))
                {
                    string messageTopic = e.Topic.Substring(_deviceMessageTopic.Length);
                    _ioTCoreStatus.Status = Status.MessageReceived;
                    _ioTCoreStatus.Message = $"{messageTopic}/{message}";
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                    //CloudToDeviceMessage?.Invoke(this, new CloudToDeviceMessageEventArgs(message, messageTopic));
                }
                else if (e.Topic.StartsWith("$iothub/clientproxy/"))
                {
                    _ioTCoreStatus.Status = Status.Disconnected;
                    _ioTCoreStatus.Message = message;
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                }
                else if (e.Topic.StartsWith("$iothub/logmessage/Info"))
                {
                    _ioTCoreStatus.Status = Status.IoTCoreInformation;
                    _ioTCoreStatus.Message = message;
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                }
                else if (e.Topic.StartsWith("$iothub/logmessage/HighlightInfo"))
                {
                    _ioTCoreStatus.Status = Status.IoTCoreHighlightInformation;
                    _ioTCoreStatus.Message = message;
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                }
                else if (e.Topic.StartsWith("$iothub/logmessage/Error"))
                {
                    _ioTCoreStatus.Status = Status.IoTCoreError;
                    _ioTCoreStatus.Message = message;
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                }
                else if (e.Topic.StartsWith("$iothub/logmessage/Warning"))
                {
                    _ioTCoreStatus.Status = Status.IoTCoreWarning;
                    _ioTCoreStatus.Message = message;
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(_ioTCoreStatus));
                }
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
