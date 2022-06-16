// Copyright (c) NetworkFusion. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/*
    This program targets (and is tested against) firmware  ORGPAL_PALTHREE-1.8.0.323
    `nanoff --masserase --update --target ORGPAL_PALTHREE --fwversion 1.8.0.323`
    Future firmware (or nuget updates) might break it!!!

    Known Issues:
        * `System.IO.Streams` reports a conflict. There are no changes, so just ignore it!
*/


// These defines allow this program to be built for multiple targets. Make sure to use the one you need
#define ORGPAL_THREE //Comment this out for any other STM32 target!

// These defines allow for "features" that are not ready for primetime!
// #define ALPHA_FEATURE_FLAG
// #define BETA_FEATURE_FLAG

// #define ALPHA_FEATURE_FLAG_CL31 // Commented out as needs more work (and inherent issues with STM32 boards)
// #define BETA_FEATURE_THERMISTOR //Commented out as sometimes causes PRT to be null for some reason!

using nanoFramework.Json;
using nanoFramework.Runtime.Native;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using nanoFramework.System.IO.FileSystem; //used for removable device events.
using System.IO;
using nanoFramework.Aws.IoTCore.Devices;
using OrgPalThreeDemo.TempDebugHelpers;
using OrgPalThreeDemo.Networking;
using nanoFramework.Aws.IoTCore.Devices.Shadows;
using System.Text;
using Microsoft.Extensions.Logging;
using nanoFramework.Logging;
using nanoFramework.Logging.Debug;
using nanoFramework.Networking;
using nanoFramework.Hardware.Stm32;
// TODO: add logging to find out why it does not work when debugger is not attached!
//using nanoFramework.Logging.Stream; //should probably be only when orgpal?

#if ORGPAL_THREE
using OrgPal.Three;
#endif

namespace OrgPalThreeDemo
{
    public class Program : IDisposable
    {
#if ORGPAL_THREE
        private static Buttons palthreeButtons;
        private static OnboardAdcDevice palthreeInternalAdc;
        private static Lcd palthreeDisplay;
        private static AdcExpansionBoard palAdcExpBoard;
#endif
        private bool _disposed;
        private static string _serialNumber = string.Empty;

        private static DateTime startTime = DateTime.UtcNow;
        private static uint messagesSent = 0;
        public const int shadowSendInterval = 1000 * 60 * 60; // 60 minutes, since delta can be sent as and when neccessary 
        public const int telemetrySendInterval = 1000 * 60 * 10; // 10 minute...

        private static Timer sendTelemetryTimer; // Dont GC
        private static Timer sendShadowTimer; // Dont GC

        //private static ILoggerFactory _loggerFactory;
        private static ILogger _logger;

        public static void Main()
        {
            MacAddressHelper.CheckSetDefaultMac();

            _logger = new DebugLogger("debugLogger");

            if (Debugger.IsAttached)
            {
                //_loggerFactory = (ILoggerFactory)new DebugLoggerFactory();
                LogDispatcher.LoggerFactory = new DebugLoggerFactory();
            }
            else
            {
                // TODO: Dont bother with logging, but we should (potentially) redirect to a file!
                //TODO: Cannot actually use this yet as storage is not setup!
                //var _stream = new FileStream("D:\\logging.txt", FileMode.Open, FileAccess.ReadWrite);
                //LogDispatcher.LoggerFactory = new StreamLoggerFactory(_stream);
            }

            //_logger.MinLogLevel = LogLevel.Trace;
            _logger.LogInformation($"{SystemInfo.TargetName} AWS MQTT Demo.");
            _logger.LogInformation("");

#if ORGPAL_THREE

#if ALPHA_FEATURE_FLAG_CL31
            new Thread(() =>
            {
                var sensorCL31 = new OrgPalThreeDemo.Peripherals.VaisalaCL31();
                sensorCL31.Open(); // This seems to take too long (given its low baud rate and message size)...
                Thread.Sleep(Timeout.Infinite);
            });
#endif



            palthreeButtons = new Buttons();
            palthreeInternalAdc = new OnboardAdcDevice();
            palAdcExpBoard = new AdcExpansionBoard();

            palthreeDisplay = new Lcd();

            palthreeDisplay.Output.Clear();
            palthreeDisplay.Output.WriteLine("Initializing:");
            palthreeDisplay.Output.WriteLine("Please Wait...");
#endif

            try
            {
                // Word has it that string builder is slower on nF! (if required, suppress S1643)!
                //var sb = new StringBuilder();
                foreach (byte b in Utilities.UniqueDeviceId) //STM32 devices only!
                {
                    //sb.Append(b.ToString("X2")); //Generates a unique ID for the device.
                    _serialNumber += b.ToString("X2"); 
                }
                //_serialNumber = sb.ToString();
            }
            catch (Exception)
            {
                _serialNumber = Guid.NewGuid().ToString();
            }

#if ORGPAL_THREE
            palthreeDisplay.Output.Clear();
            palthreeDisplay.Output.WriteLine("Device S/N,");
            palthreeDisplay.Output.WriteLine($"{_serialNumber}");
            _logger.LogInformation($"Device Serial number: {_serialNumber}");
            _logger.LogInformation("");
            Thread.Sleep(1000);

#endif

            _logger.LogInformation($"Time before network available: {DateTime.UtcNow.ToString("o")}");

            var netConnected = false;
            int netConnectionAttempt = 0;
            while (!netConnected)
            {
#if ORGPAL_THREE
                palthreeDisplay.Output.Clear();
                palthreeDisplay.Output.WriteLine("Initializing:");
                palthreeDisplay.Output.WriteLine($"Network... {netConnectionAttempt}");
                Thread.Sleep(1000);
#endif
                _logger.LogInformation($"Network Connection Attempt: {netConnectionAttempt}");
                netConnected = SetupNetwork();
                if (!netConnected)
                {
                    netConnectionAttempt += 1;
                    Thread.Sleep(1000);
                }
            }

            startTime = DateTime.UtcNow; //set now because the clock might have been wrong before ntp is checked.

            _logger.LogInformation($"Time after network available: {startTime.ToString("o")}");
            _logger.LogInformation("");


#if ORGPAL_THREE
            palthreeDisplay.Output.Clear();
            palthreeDisplay.Output.WriteLine("Initializing:");
            palthreeDisplay.Output.WriteLine("Storage");
#endif
            // add event handlers for Removable Device insertion and removal
            StorageEventManager.RemovableDeviceInserted += StorageEventManager_RemovableDeviceInserted;
            StorageEventManager.RemovableDeviceRemoved += StorageEventManager_RemovableDeviceRemoved;

            while (AwsIotCore.MqttConnector.RootCA == null || AwsIotCore.MqttConnector.ClientRsaSha256Crt == string.Empty || AwsIotCore.MqttConnector.ClientRsaKey == string.Empty)
            {
                try
                {
                    ReadStorage(); // We cannot start without valid Certs!
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.Message.ToString());
                }
            }


            var mqttConnected = false;
            int mqttConnectionAttempt = 0;
            while (!mqttConnected)
            {
#if ORGPAL_THREE
                palthreeDisplay.Output.Clear();
                palthreeDisplay.Output.WriteLine("Initializing:");
                palthreeDisplay.Output.WriteLine($"MQTT... {mqttConnectionAttempt}");
#endif
                _logger.LogInformation($"MQTT Connection Attempt: {mqttConnectionAttempt}");
                mqttConnected = SetupMqtt();
                if (!mqttConnected)
                {
                    mqttConnectionAttempt += 1;
                    Thread.Sleep(1000);
                }
            }

#if ORGPAL_THREE
            palthreeDisplay.Output.Clear();
            palthreeDisplay.Output.WriteLine("Initializing:");
            palthreeDisplay.Output.WriteLine("MQTT Timers...");
#endif
            SetupMqttMessageTimers();

#if ORGPAL_THREE
            palthreeDisplay.Output.Clear();
            palthreeDisplay.Output.WriteLine("Initializing:");
            palthreeDisplay.Output.WriteLine("Finished!");

            Thread lcdUpdateThread = new Thread(new ThreadStart(LcdUpdate_Thread));
            lcdUpdateThread.Start();
#endif

            Thread.Sleep(Timeout.Infinite);
        }

#if ORGPAL_THREE
        private static void LcdUpdate_Thread() //TODO: backlight timeout should be in driver!
        {
            for( ; ; )
            {
                //palthreeDisplay.Display.BacklightOn = true;
                CycleDisplay();
                //palthreeDisplay.Display.BacklightOn = false;
                //Thread.Sleep(2000); //TODO: arbitary value... what should the update rate be?!
            }
        }

        private static void CycleDisplay()
        {
            try
            {
                const int cycleDelay = 2000;  //TODO: arbitary value... what should the update rate be?!
                //TODO: create a menu handler to scroll through the display!
                palthreeDisplay.Output.Clear();
                palthreeDisplay.Output.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")}"); //Time shortened to fit on display (excludes seconds)
                palthreeDisplay.Output.WriteLine($"{System.Net.NetworkInformation.IPGlobalProperties.GetIPAddress()}");
                Thread.Sleep(cycleDelay);

                palthreeDisplay.Output.Clear();

                palthreeDisplay.Output.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")}"); //Time shortened to fit on display (excludes seconds)
                palthreeDisplay.Output.WriteLine($"*{IpHelper.GetWanAddressIpv4()}");
                Thread.Sleep(cycleDelay);

                palthreeDisplay.Output.Clear();
                palthreeDisplay.Output.WriteLine($"PCB Temp: {palthreeInternalAdc.GetPcbTemperature().ToString("n2")}C");
                palthreeDisplay.Output.WriteLine($"Voltage: {palthreeInternalAdc.GetBatteryUnregulatedVoltage().ToString("n2")}VDC");
                Thread.Sleep(cycleDelay);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message.ToString());
            }
        }

#endif

        private static bool SetupNetwork()
        {
            CancellationTokenSource cs = new CancellationTokenSource(5000); //5 seconds.
                                                                            // We are using TLS and it requires valid date & time (so we should set the option to true, but SNTP is run in the background, and setting it manually causes issues for the moment!!!)
                                                                            // Although setting it to false seems to cause a worse issue. Let us fix this by using a managed class instead.

            try
            {

                _logger.LogInformation("Waiting for network up and IP address...");
                var success = NetworkHelper.SetupAndConnectNetwork(requiresDateTime: true, token: cs.Token);


                if (!success)
                {
                    _logger.LogWarning($"Failed to receive an IP address and/or valid DateTime. Error: {NetworkHelper.Status}.");
                    if (NetworkHelper.HelperException != null)
                    {
                        _logger.LogWarning($"Failed to receive an IP address and/or valid DateTime. Error: {NetworkHelper.HelperException}.");
                    }
                    _logger.LogInformation("It is likely a DateTime problem, so we will now try to set it using a managed helper class!");

                    success = Rtc.SetSystemTime(ManagedNtpClient.GetNetworkTime());
                    if (success)
                    {
#if ORGPAL_THREE
                        palthreeDisplay.Output.Clear();
                        palthreeDisplay.Output.WriteLine("DT SET USING:");
                        palthreeDisplay.Output.WriteLine("MANAGED SMTP");
                        Thread.Sleep(1000);
#endif
                        _logger.LogInformation("Retrived DateTime using Managed NTP Helper class...");
                    }
                    else
                    {
#if ORGPAL_THREE
                        palthreeDisplay.Output.Clear();
                        palthreeDisplay.Output.WriteLine("DT SET USING:");
                        palthreeDisplay.Output.WriteLine("UNMANAGED SMTP");
                        Thread.Sleep(1000);
#endif
                        _logger.LogWarning("Failed to Retrive DateTime (or IP Address)!");
                    }

                    _logger.LogInformation($"IP = {System.Net.NetworkInformation.IPGlobalProperties.GetIPAddress()}");
                    _logger.LogInformation($"RTC = {DateTime.UtcNow}");

#if ORGPAL_THREE
                    palthreeDisplay.Output.Clear();
                    palthreeDisplay.Output.WriteLine("No Network:");
                    palthreeDisplay.Output.WriteLine($"DT... {DateTime.UtcNow}");
                    Thread.Sleep(2000);
                    palthreeDisplay.Output.Clear();
                    palthreeDisplay.Output.WriteLine("No Network:");
                    palthreeDisplay.Output.WriteLine($"IP... {System.Net.NetworkInformation.IPGlobalProperties.GetIPAddress()}");
                    Thread.Sleep(2000);
#endif
                }

                return success;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message.ToString());

#if ORGPAL_THREE
                palthreeDisplay.Output.Clear();
                palthreeDisplay.Output.WriteLine("Net catch!:");
                palthreeDisplay.Output.WriteLine($"RTC... {DateTime.UtcNow}");
                Thread.Sleep(2000);
                palthreeDisplay.Output.Clear();
                palthreeDisplay.Output.WriteLine("Net catch!:");
                palthreeDisplay.Output.WriteLine($"IP... {System.Net.NetworkInformation.IPGlobalProperties.GetIPAddress()}");
                Thread.Sleep(2000);
#endif

                return false;
            }

        }

        static bool SetupMqtt()
        {
            //TODO: make sure we can use device flashed certs (before storage) https://github.com/nanoframework/Samples/blob/main/samples/SSL/SecureClient/Program.cs
            _logger.LogInformation("Program: Connecting to MQTT broker... : ");
            try
            {
                ////Handle cases where this method is called when MQTT is already connected!
                //if (AwsIotCore.MqttConnector.Client.IsConnected) //perhaps != null instead?
                //{
                //    AwsIotCore.MqttConnector.Client.Close();
                //    AwsIotCore.MqttConnector.Client.CloudToDeviceMessage -= Client_CloudToDeviceMessageReceived;
                //    AwsIotCore.MqttConnector.Client.StatusUpdated -= Client_StatusUpdated;
                //    AwsIotCore.MqttConnector.Client.ShadowUpdated -= Client_ShadowUpdated;
                //    AwsIotCore.MqttConnector.Client.Dispose();
                //}

                X509Certificate caCert = new X509Certificate(AwsIotCore.MqttConnector.RootCA); //commented out as alternative: //Resources.GetBytes(Resources.BinaryResources.AwsCAroot)); //should this be in secure storage, or is it fine where it is?
                X509Certificate2 clientCert = new X509Certificate2(AwsIotCore.MqttConnector.ClientRsaSha256Crt, AwsIotCore.MqttConnector.ClientRsaKey, ""); //make sure to add a correct pfx certificate


                AwsIotCore.MqttConnector.Client = new MqttConnectionClient(AwsIotCore.MqttConnector.Host, AwsIotCore.MqttConnector.ThingName, clientCert, MqttConnectionClient.QoSLevel.AtLeastOnce, caCert);

                bool success = AwsIotCore.MqttConnector.Client.Open("nanoframework/device");
                _logger.LogInformation($"{success}");
                if (!success)
                {
                    return false;
                }


                // Register to messages received:
                AwsIotCore.MqttConnector.Client.CloudToDeviceMessage += Client_CloudToDeviceMessageReceived;
                AwsIotCore.MqttConnector.Client.StatusUpdated += Client_StatusUpdated;
                AwsIotCore.MqttConnector.Client.ShadowUpdated += Client_ShadowUpdated;

                Thread.Sleep(1000); //ensure that we are ready (and connected)???
                _logger.LogInformation("");
                _logger.LogInformation($"Attempting to get AWS IOT shadow... Result was: ");
                
                var shadow = AwsIotCore.MqttConnector.Client.GetShadow(new CancellationTokenSource(30000).Token);
                if (shadow != null)
                {
                    _logger.LogInformation("Success!");
                    DecodeShadowAsHashtable(shadow);
                    //Debug.WriteLine($"Desired:  {shadow.state.desired.ToJson()}");
                    //Debug.WriteLine($"Reported:  {shadow.state.reported.ToJson()}");

                    _logger.LogInformation($"Converted shadow to a json (string):");
                    _logger.LogInformation("------------------");
                    Debug.WriteLine($"{shadow.ToJson()}");
                    //_logger.LogInformation($"{shadow.ToJson()}"); //TODO: Throws exception!
                    _logger.LogInformation("------------------");
                    _logger.LogInformation("");
                }
                else
                {
                    _logger.LogWarning("Failed!");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message.ToString());
                return false;
            }

        }

        private static void DecodeShadowAsHashtable(Shadow shadow)
        {
            _logger.LogInformation("Decoded shadow as Hashtable was:");
            _logger.LogInformation("------------------");
            _logger.LogInformation("state.desired:");
            DebugHelper.DumpHashTable(shadow.state.desired, 1);
            _logger.LogInformation("state.reported:");
            DebugHelper.DumpHashTable(shadow.state.reported, 1);
            _logger.LogInformation("metadata.desired:");
            DebugHelper.DumpHashTable(shadow.metadata.desired, 1);
            _logger.LogInformation("metadata.reported:");
            DebugHelper.DumpHashTable(shadow.metadata.reported, 1);
            _logger.LogInformation($"timestamp={shadow.timestamp}");
            _logger.LogInformation($"as ISO date: {DateTime.FromUnixTimeSeconds(shadow.timestamp).ToString("o")}");
            _logger.LogInformation($"version={shadow.version}");
            _logger.LogInformation($"clienttoken={shadow.clienttoken}");
            _logger.LogInformation("------------------");
            _logger.LogInformation("");
        }

        private static void Client_ShadowUpdated(object sender, ShadowUpdateEventArgs e)
        {
            //TODO: check against the last received shadow (or something)!
            _logger.LogInformation("Program: Received a shadow update!");
            //_logger.LogInformation("------------------");
            //DecodeShadow(e.Shadow);
            //_logger.LogInformation("------------------");
        }

        private static void Client_StatusUpdated(object sender, StatusUpdatedEventArgs e)
        {
            //TODO: handle it properly!
            Debug.WriteLine($"Program: Received a status update: {e.Status.State}"); //TODO: preferabily as a string?!
            //_logger.LogInformation($"Program: Received a status update: {e.Status.State}"); //TODO: Throws exception!
            if (!string.IsNullOrEmpty(e.Status.Message))
            {
                Debug.WriteLine($" with message {e.Status.Message}"); //TODO: does this need converting?
                //_logger.LogInformation($" with message {e.Status.Message}"); //TODO: Throws exception!
            }
        }

        static void SetupMqttMessageTimers()
        {
            while (DateTime.UtcNow.Second != 0) //TODO: this is a workaround to align to NTP second zero.
            {
                Thread.SpinWait(1);
            }

            sendTelemetryTimer = new Timer(TelemetryTimerCallback, null, 0, telemetrySendInterval);
            sendShadowTimer = new Timer(ShadowTimerCallback, null, 0, shadowSendInterval);
        }

        static void ShadowTimerCallback(object state)
        {
            new Thread(() =>
            {
                try
                {
                    var shadowReportedState = new AwsIotCore.DeviceMessageSchemas.ShadowStateProperties
                    {
                        operatingSystem = "nanoFramework",
                        platform = SystemInfo.TargetName,
                        cpu = SystemInfo.Platform,
                        serialNumber = $"SN_{_serialNumber}", //TODO: "SN" should not be needed! but might help in the long run anyway?!
                        bootTimestamp = startTime,
                        endpointAddressIpv4 = $"{IpHelper.GetWanAddressIpv4()}"

                    };

                    _logger.LogInformation($"Updating shadow reported properties... "); //wait for result before writeline.
                                                                                        //TODO: this should be worked out as part of the shadow property collection?!
                    const string shadowUpdateHeader = "{\"state\":{\"reported\":";
                    const string shadowUpdateFooter = "}}";
                    string shadowJson = $"{shadowUpdateHeader}{JsonConvert.SerializeObject(shadowReportedState)}{shadowUpdateFooter}";
                    Debug.WriteLine($"  With json: {shadowJson}");
                    //_logger.LogInformation($"  With json: {shadowJson}"); //TODO: Throws exception!
                    var shadow = new Shadow(shadowJson);
                    bool updateResult = AwsIotCore.MqttConnector.Client.UpdateReportedState(shadow);

                    _logger.LogInformation($"Result was: {!updateResult}"); //Received == false (inverted for UI).

                }
                catch (Exception ex)
                {

                    _logger.LogInformation($"error sending shadow: {ex}");
                    SetupMqtt();
                }
            }).Start();
        }

        static uint IncrementTelemtryMessageSentCount()
        {
            if (messagesSent < uint.MaxValue)
            {
                return messagesSent += 1;
            }
            else
            {
                return messagesSent = 0;
            }
        }

        static void TelemetryTimerCallback(object state)
        {
            new Thread(() =>
            {
                try
                {

                    var statusTelemetry = new AwsIotCore.DeviceMessageSchemas.TelemetryMessage
                    {
                        serialNumber = $"SN_{_serialNumber}", //TODO: "SN" should not be needed! but might help in the long run anyway?!
                        sendTimestamp = DateTime.UtcNow,
                        messageNumber = IncrementTelemtryMessageSentCount(),
                        memoryFreeBytes = nanoFramework.Runtime.Native.GC.Run(false),
#if ORGPAL_THREE
                        batteryVoltage = palthreeInternalAdc.GetBatteryUnregulatedVoltage(),
                        enclosureTemperatureCelsius = palthreeInternalAdc.GetPcbTemperature(),
                        mcuTemperatureCelsius = palthreeInternalAdc.GetMcuTemperature(),
                        airTemperatureCelsius = palAdcExpBoard.GetTemperatureFromPT100(),
#if BETA_FEATURE_THERMISTOR
                        thermistorTemperatureCelsius = palAdcExpBoard.GetTemperatureFromThermistorNTC1000()
#endif
#endif
                    };

                    string sampleData = JsonConvert.SerializeObject(statusTelemetry);
                    AwsIotCore.MqttConnector.Client.SendMessage(sampleData); // ($"{AwsMqtt.ThingName}/data", Encoding.UTF8.GetBytes(sampleData), MqttQoSLevel.AtMostOnce, false);

                    Debug.WriteLine("Message sent: " + sampleData);
                    //_logger.LogInformation("Message sent: " + sampleData); //TODO: Throws exception!
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Message sent ex: {ex}");
                    SetupMqtt();
                }
            }).Start();
        }

        static void Client_CloudToDeviceMessageReceived(object sender, CloudToDeviceMessageEventArgs e)
        {
            try
            {
                _logger.LogInformation($"Command and Control Message received: {e.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Message received ex: " + ex);
                SetupMqtt();
            }
        }

        private static void ReadStorage(string path = "") //TODO: this can fail on redeploy or exiting debug!!!
        {
            // in nanoFramework, currently the drive letters are fixed, being:
            // D: SD Card
            // E: USB Mass Storage Device
            // I: Internal

            if (string.IsNullOrEmpty(path)) //Generally only "should" happen on initialization.
            {
                // list all removable drives
                var removableDrives = Directory.GetLogicalDrives(); //TODO: FEEDBACK... I Cannot help but feel (since we are no longer attached to UWP, that this should be `SD:` and `USB:`

                //TODO: Better handle no removable MSD avaliable?!
                if (removableDrives.Length == 0) // Arrays are counted by Length. (not a collection!)
                {
                    _logger.LogError("NO REMOVABLE STORAGE DEVICE FOUND");
                    // FIXME: what should we do?!
                }
                foreach (var drive in removableDrives)
                {
                    _logger.LogInformation($"Found logical drive {drive}");
                    path = drive;
                    if (path.StartsWith("D")) break; // Always use the SDcard as the default device.
                }
            }

            if (!string.IsNullOrEmpty(path))
            {
                _logger.LogInformation("Reading storage...");

                // get folders on 1st removable device
                var foldersOnDevice = Directory.GetDirectories(path);

                foreach (var folder in foldersOnDevice)
                {
                    _logger.LogInformation($"Found folder: {folder}");
                }

                // get files on the root of the 1st removable device
                var filesInDevice = Directory.GetFiles(path);

                //TODO: in certain cases it would be helpful to support File.ReadAllText -- https://zetcode.com/csharp/file/ (Helper lib??)
                foreach (var file in filesInDevice)
                {
                    _logger.LogInformation($"Found file: {file}");
                    //TODO: we should really check if certs are in the mcu flash before retreiving them from the filesystem (SD).
                    if (file.Contains(".crt"))
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            var buffer = new byte[fs.Length];
                            fs.Read(buffer, 0, (int)fs.Length);
                            AwsIotCore.MqttConnector.ClientRsaSha256Crt = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                            fs.Flush();
                        }
                        //Should load into secure storage (somewhere) and delete file on removable device?
                    }
                    if (file.Contains(".der"))
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            AwsIotCore.MqttConnector.RootCA = new byte[fs.Length];
                            fs.Read(AwsIotCore.MqttConnector.RootCA, 0, (int)fs.Length);
                            fs.Flush();
                        }
                        //Should load into secure storage (somewhere) and delete file on removable device?
                    }
                    if (file.Contains("key"))
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            var buffer = new byte[fs.Length];
                            fs.Read(buffer, 0, (int)fs.Length);
                            AwsIotCore.MqttConnector.ClientRsaKey = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                            fs.Flush();
                        }

                        //Should load into secure storage (somewhere) and delete file on removable device?
                    }
                    if (file.Contains("mqttconfig.json"))
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            var success = false;
                            while (!success)
                            {
                                try
                                {
                                    var buffer = new byte[fs.Length];
                                    fs.Read(buffer, 0, (int)fs.Length);

                                    MqttConfigFileSchema config = (MqttConfigFileSchema)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer, 0, buffer.Length), typeof(MqttConfigFileSchema));
                                    AwsIotCore.MqttConnector.Host = config.Url;
                                    if (config.Port != null)
                                    {
                                        AwsIotCore.MqttConnector.Port = int.Parse(config.Port);
                                    }
                                    if (!string.IsNullOrEmpty(config.ThingName))
                                    {
                                        AwsIotCore.MqttConnector.ThingName = config.ThingName;
                                    }
                                    else
                                    {
                                        AwsIotCore.MqttConnector.ThingName = _serialNumber;
                                    }
                                    fs.Flush();
                                    success = true;
                                }
                                catch (Exception)
                                {
                                    //TODO: sometimes a json deserialize exception happens. For the moment, just try again.
                                    _logger.LogWarning("Failed to decode MqttConfig.json, Retrying...");
                                }
                            }
                        }
                    }
                }

                //TODO: if the certs are on the SD, they should be loaded into (secure) mcu storage (and delete file on removable device)?
                _logger.LogInformation(""); //finished loading files.

                //TODO: shadow should be informed of Storage devices and content
                //TODO: return MQTTconfigChanged...
            }

        }


        private static void StorageEventManager_RemovableDeviceRemoved(object sender, RemovableDeviceEventArgs e)
        {
            _logger.LogInformation($"Removable Device Event: @ \"{e.Path}\" was removed.");
        }

        private static void StorageEventManager_RemovableDeviceInserted(object sender, RemovableDeviceEventArgs e)
        {
            _logger.LogInformation($"Removable Device Event: @ \"{e.Path}\" was inserted.");

            // var mqttConfigChange = ReadStorage(e.Path);
            //if (mqttConfigChange)
            //{
            //  SetupMqtt(restore: true); //should help handle events and Reconnect/close/open
            //}

        }

        /// <summary>
        /// Releases unmanaged resources
        /// and optionally release managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                //should dispose of SD and Char display (at least!)
#if ORGPAL_THREE
                palthreeDisplay.Dispose();
                palAdcExpBoard.Dispose();
                palthreeInternalAdc.Dispose();
                palthreeButtons.Dispose();
#endif
                //System.IO.FileStream -- Dispose?? (we are already (using))!
                StorageEventManager.RemovableDeviceInserted -= StorageEventManager_RemovableDeviceInserted;
                StorageEventManager.RemovableDeviceRemoved -= StorageEventManager_RemovableDeviceRemoved;

                sendTelemetryTimer.Dispose();

                sendShadowTimer.Dispose();

                AwsIotCore.MqttConnector.Client.CloudToDeviceMessage -= Client_CloudToDeviceMessageReceived;
                AwsIotCore.MqttConnector.Client.StatusUpdated -= Client_StatusUpdated;
                AwsIotCore.MqttConnector.Client.ShadowUpdated -= Client_ShadowUpdated;
                AwsIotCore.MqttConnector.Client.Close();
                AwsIotCore.MqttConnector.Client.Dispose();

                Sntp.Stop();
                LogDispatcher.LoggerFactory.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {

            if (!_disposed)
            {
                Dispose(true);
                _disposed = true;
            }
            System.GC.SuppressFinalize(this);
        }

        ~Program()
        {
            Dispose(false);
        }
    }
}
