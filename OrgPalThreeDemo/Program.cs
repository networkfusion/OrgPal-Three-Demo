using nanoFramework.Json;
using nanoFramework.Runtime.Native;
using PalThree;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using nanoFramework.M2Mqtt.Messages; // Only required due to QoS level. Perhaps this should be inherited through the Aws lib?!
//using System.Device.Gpio;
using Windows.Storage;
using Windows.Storage.Streams;
using nanoFramework.AwsIot;
using OrgPalThreeDemo.TempDebugHelpers;

namespace OrgPalThreeDemo
{
    public class Program
    {
        //private static GpioController gpioController;

        //private static GpioPin _muxFlowControl;
        //private static GpioPin _wakeButton;
        //private static GpioPin _userButton;
        private static Drivers.OnboardDevices palthree;
        private static LCD lcd;
        private static MCP342x adcPalSensor;

        private static string _serialNumber;

        private static DateTime startTime = DateTime.UtcNow;
        private static int messagesSent = 0;
        public const int shadowSendInterval = 600000; //10 minutes...  TODO: increase shadow interval to 3600000 for 1 hour when happy!
        public const int telemetrySendInterval = 60000; //1 minute... TODO: does not take into account delays or execution time!

        public static void Main()
        {
            Debug.WriteLine($"{SystemInfo.TargetName} AWS MQTT Demo.");
            Debug.WriteLine();

            palthree = new Drivers.OnboardDevices();
            adcPalSensor = new MCP342x();

            //gpioController = new GpioController();


            //the buttons are multiplexed so that the board can wake up by user, or by the RTC
            //so to get that interrupt to fire you need to do this
            //_muxFlowControl = gpioController.OpenPin(PalThreePins.GpioPin.MUX_EXT_BUTTON_WAKE_PE4);
            //_muxFlowControl.SetPinMode(PinMode.Output);
            //_muxFlowControl.Write(PinValue.High);
            //_muxFlowControl.ValueChanged += MuxFlowControl_ValueChanged;

            //_userButton = gpioController.OpenPin(PalThreePins.GpioPin.BUTTON_USER_WAKE_PE6);
            //_userButton.SetPinMode(PinMode.Input);
            //_userButton.ValueChanged += UserButton_ValueChanged;

            //_wakeButton = gpioController.OpenPin(PalThreePins.GpioPin.BUTTON_WAKE_PA0);
            //_wakeButton.SetPinMode(PinMode.Input);
            //_wakeButton.ValueChanged += WakeButton_ValueChanged;

            lcd = new LCD
            {
                BacklightOn = true
            };
            lcd.Display("Please Wait...");

            //Thread.Sleep(1000);

            lcd.Display($"Voltage: {palthree.GetBatteryUnregulatedVoltage().ToString("n2")} \n PCB-Temp: {palthree.GetTemperatureOnBoard().ToString("n2")}"); //, 0);

            //Thread.Sleep(5000);
            //lcd.BacklightOn = false;


            foreach (byte b in nanoFramework.Hardware.Stm32.Utilities.UniqueDeviceId)
            {
                _serialNumber += b.ToString("X2"); //Generates a unique ID for the device.
            }

            // add event handlers for Removable Device insertion and removal
            StorageEventManager.RemovableDeviceInserted += StorageEventManager_RemovableDeviceInserted;
            StorageEventManager.RemovableDeviceRemoved += StorageEventManager_RemovableDeviceRemoved;
            ReadStorage();

            Debug.WriteLine($"Time before network available: {DateTime.UtcNow}");

            SetupNetwork();

            //while (DateTime.UtcNow.Year < 2021)
            //{
            //    Thread.Sleep(1000); //give time for native SNTP to be retrieved.
            //    Debug.WriteLine("Waiting for time to be set...");
            //}
            startTime = DateTime.UtcNow; //set now because the clock might have been wrong before ntp is checked.

            Debug.WriteLine($"Start Time: {startTime.ToString("yyyy-MM-dd HH:mm:ss")}");


            var connected = false;
            int connectionAttempt = 0;
            while (!connected)
            {
                Debug.WriteLine($"Connection Attempt {connectionAttempt}");
                connected = SetupMqtt();
                if (!connected)
                {
                    connectionAttempt += 1;
                    Thread.Sleep(1000);
                }
            }


            Thread.Sleep(Timeout.Infinite);
        }

        //private static void MuxFlowControl_ValueChanged(object sender, PinValueChangedEventArgs e)
        //{
        //    Debug.WriteLine("Handle Mux Flow...!");
        //}

        //private static void WakeButton_ValueChanged(object sender, PinValueChangedEventArgs e)
        //{
        //    if (lcd.BacklightOn == false)
        //    {
        //        //TODO: this has display corruption!!!
        //        if (e.ChangeType == PinEventTypes.Rising)
        //        {
        //            lcd.BacklightOn = true;
        //            lcd.Clear();
        //            lcd.Display($"Voltage: {palthree.GetBatteryUnregulatedVoltage().ToString("n2")} \n Temp: {palthree.GetTemperatureOnBoard().ToString("n2")}"); //, 0);

        //            Thread.Sleep(5000);
        //            lcd.BacklightOn = false;
        //        }
        //    }
        //}

        //private static void UserButton_ValueChanged(object sender, GpioPinValueChangedEventArgs e)
        //{

        //}

        private static void SetupNetwork()
        {
            CancellationTokenSource cs = new CancellationTokenSource(5000); //5 seconds.
            // We are using TLS and it requires valid date & time (so we should set the option to true, but SNTP is run in the background, and setting it manually causes issues for the moment!!!)
            // Although setting it to false seems to cause a worse issue. Let us fix this by using a managed class instead.
            Debug.WriteLine("Waiting for network up and IP address...");
            var success = NetworkHelper.WaitForValidIPAndDate(true, System.Net.NetworkInformation.NetworkInterfaceType.Ethernet, cs.Token);

            if (!success)
            {
                Debug.WriteLine($"Failed to receive an IP address and/or valid DateTime. Error: {NetworkHelper.ConnectionError.Error}.");
                if (NetworkHelper.ConnectionError.Exception != null)
                {
                    Debug.WriteLine($"Exception: {NetworkHelper.ConnectionError.Exception}");
                }
                Debug.WriteLine("It is likely a DateTime problem, so we will now try to set it using a managed helper class!");
                success = Rtc.SetSystemTime(ManagedNtpClient.GetNetworkTime());
                if (success)
                {
                    Debug.WriteLine("Retrived DateTime using Managed NTP Helper class...");
                }
                else
                {
                    Debug.WriteLine("Failed to Retrive DateTime (or IP Address)! Retrying...");
                    SetupNetwork();
                }
                Debug.WriteLine($"RTC = {DateTime.UtcNow}");
            }

        }

        static bool SetupMqtt()
        {
            try
            {
                X509Certificate caCert = new X509Certificate(AwsMqttConnector.RootCA); //commented out as MDP changes mean resources dont currently work//Resources.GetBytes(Resources.BinaryResources.AwsCAroot)); //should this be in secure storage, or is it fine where it is?
                X509Certificate2 clientCert = new X509Certificate2(AwsMqttConnector.ClientRsaSha256Crt, AwsMqttConnector.ClientRsaKey, ""); //make sure to add a correct pfx certificate


                AwsMqttConnector.Client = new MqttConnectionClient(AwsMqttConnector.Host, AwsMqttConnector.ThingName, clientCert, MqttQoSLevel.AtLeastOnce, caCert);

                AwsMqttConnector.Client.Open();


                Thread.Sleep(1000); //ensure that we are ready (and connected)???
                var shadow = AwsMqttConnector.Client.GetShadow(new CancellationTokenSource(30000).Token);
                if (shadow != null)
                {
                    Debug.WriteLine($"Get shadow result:");
                    //Debug.WriteLine($"Desired:  {shadow.state.desired.ToJson()}");
                    //Debug.WriteLine($"Reported:  {shadow.state.reported.ToJson()}");

                    Debug.WriteLine("state.desired:");
                    DebugHelper.DumpHashTable(shadow.state.desired, 1);
                    Debug.WriteLine("state.reported:");
                    DebugHelper.DumpHashTable(shadow.state.reported, 1);
                    Debug.WriteLine("metadata.desired:");
                    DebugHelper.DumpHashTable(shadow.metadata.desired, 1);
                    Debug.WriteLine("metadata.reported:");
                    DebugHelper.DumpHashTable(shadow.metadata.reported, 1);
                    Debug.WriteLine($"timestamp={DateTime.FromUnixTimeSeconds(shadow.timestamp)}");
                    Debug.WriteLine($"version={shadow.version}");
                    Debug.WriteLine($"clienttoken={shadow.clienttoken}");
                    Debug.WriteLine("");
                    Debug.WriteLine($"Get shadow as json (string):");
                    Debug.WriteLine($"...:  {shadow.ToJson()}"); //TODO: this currently throws an invalid cast exception.
                }

                // register to message received 
                //AwsMqtt.Client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                // subscribe to the topic with QoS 1
                //AwsMqtt.Client.Subscribe(new string[] { $"{AwsMqtt.ThingName}/sys" }, new MqttQoSLevel[] { MqttQoSLevel.AtMostOnce });
                Thread telemetryThread = new Thread(new ThreadStart(TelemetryLoop));
                telemetryThread.Start();


                Thread shadowThread = new Thread(new ThreadStart(ShadowLoop)); //TODO: currently throws exception on subscribe!
                shadowThread.Start();



                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message.ToString());
                return false;
            }
            



        }

        static void ShadowLoop()
        {
            for ( ; ; )
            {
                try
                {
                    var shadowReportedState = new DeviceMessageSchemas.ShadowStateProperties
                    {
                        operatingSystem = "nanoFramework",
                        platform = SystemInfo.TargetName,
                        cpu = SystemInfo.Platform,
                        serialNumber = "test-device", //"//$"SN{_serialNumber }", //TODO: "SN" should not be needed! and this seems to throw anyway!
                        bootTimestamp = startTime.ToUnixTimeSeconds()
                    };

                    Debug.Write($"Updating shadow reported properties... "); //wait for result before writeline.
                    //TODO: this should be worked out as part of the shadow property collection?!
                    const string shadowUpdateHeader = "{\"state\":{\"reported\":";
                    const string shadowUpdateFooter = "}}";
                    string shadowJson = $"{shadowUpdateHeader}{JsonConvert.SerializeObject(shadowReportedState)}{shadowUpdateFooter}";
                    bool updateResult = AwsMqttConnector.Client.UpdateReportedState(//new ShadowPropertyCollection(
                        shadowJson); //);

                    Debug.WriteLine($"{!updateResult}"); //Received == false (inverted for UI).

                }
                catch (Exception ex)
                {

                    Debug.WriteLine($"error sending shadow: {ex}");
                    SetupMqtt();
                }


                Thread.Sleep(shadowSendInterval);
            }
        }

        static void TelemetryLoop()
        {
            for ( ; ; )
            {
                try
                {
                    var statusTelemetry = new DeviceMessageSchemas.TelemetryMessage
                    {
                        serialNumber = _serialNumber,
                        sendTimestamp = DateTime.UtcNow,
                        messageNumber = messagesSent += 1,
                        batteryVoltage = (float)palthree.GetBatteryUnregulatedVoltage(),
                        enclosureTemperature = (float)palthree.GetTemperatureOnBoard(),
                        memoryFree = nanoFramework.Runtime.Native.GC.Run(false),
                        mcuTemperature = palthree.GetMcuTemperature(),
                        airTemperature = (float)adcPalSensor.GetTemperatureFromPT100()
                    };

                    string sampleData = JsonConvert.SerializeObject(statusTelemetry);
                    AwsMqttConnector.Client.SendMessage(sampleData); // ($"{AwsMqtt.ThingName}/data", Encoding.UTF8.GetBytes(sampleData), MqttQoSLevel.AtMostOnce, false);

                    Debug.WriteLine("Message sent: " + sampleData);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Message sent ex: " + ex);
                    SetupMqtt();
                }

                Thread.Sleep(telemetrySendInterval); //TODO: this should probably be a stopwatch to ensure timing consistancy!
            }
        }

        //static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        //{
        //    try
        //    {
        //        string Message = new string(Encoding.UTF8.GetChars(e.Message));
        //        Debug.WriteLine("Message received: " + Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Message received ex: " + ex);
        //        SetupMqtt();
        //    }

        //    //should we handle the shadow received messages here?!
        //}

        private static void ReadStorage()
        {
            // Get the logical root folder for all removable storage devices
            // in nanoFramework the drive letters are fixed, being:
            // D: SD Card
            // E: USB Mass Storage Device
            StorageFolder externalDevices = KnownFolders.RemovableDevices;

            // list all removable storage devices
            var removableDevices = externalDevices.GetFolders();

            if (removableDevices.Length > 0)
            {
                // get folders on 1st removable device
                var foldersInDevice = removableDevices[0].GetFolders();

                foreach (var folder in foldersInDevice)
                {
                    Debug.WriteLine(folder.DisplayName);
                }

                // get files on the root of the 1st removable device
                var filesInDevice = removableDevices[0].GetFiles();

                foreach(var file in filesInDevice)
                {
                    Debug.WriteLine(file.Name);
                    //TODO: we should really check if certs are in the mcu flash before retreiving them from the filesystem (SD).
                    if (file.FileType == "crt")
                    {
                        //clientRsaSha256Crt = FileIO.ReadText(f); //Currently doesnt work with nf!
                        //workaround...
                        var buffer = FileIO.ReadBuffer(file);
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            AwsMqttConnector.ClientRsaSha256Crt = dataReader.ReadString(buffer.Length);
                        }
                        
                        //Should load into secure storage (somewhere) and delete file on removable device?
                    }
                    if (file.FileType == "der")
                    {
                        var buffer = FileIO.ReadBuffer(file);
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            AwsMqttConnector.RootCA = new byte[buffer.Length];
                            dataReader.ReadBytes(AwsMqttConnector.RootCA);
                        }
                    }
                    if (file.FileType == "key")
                    {
                        //clientRsaKey = FileIO.ReadText(f); //Currently doesnt work with nf!
                        //workaround...
                        var buffer = FileIO.ReadBuffer(file);
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            AwsMqttConnector.ClientRsaKey = dataReader.ReadString(buffer.Length);
                        }
                        
                        //Should load into secure storage (somewhere) and delete file on removable device?
                    }
                    if (file.Name == "mqttconfig.json")
                    {
                        var buffer = FileIO.ReadBuffer(file);
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            readMqttConfig:
                            try
                            {
                                MqttConfigFileSchema config = (MqttConfigFileSchema)JsonConvert.DeserializeObject(dataReader, typeof(MqttConfigFileSchema));
                                AwsMqttConnector.Host = config.Url;
                                if (config.Port != null)
                                {
                                    AwsMqttConnector.Port = int.Parse(config.Port);
                                }
                                if (config.ThingName != string.Empty || config.ThingName != null)
                                {
                                    AwsMqttConnector.ThingName = config.ThingName;
                                }
                                else
                                {
                                    AwsMqttConnector.ThingName = _serialNumber;
                                }
                            }
                            catch (Exception)
                            {

                                goto readMqttConfig; //TODO: sometimes a json deserialize exception happens. For the moment, just try again.
                            }

                        }

                        //TODO: if they are on the SD, Should load into (secure) mcu storage and [delete file on removable device]?
                    }
                }
            }

        }


        private static void StorageEventManager_RemovableDeviceRemoved(object sender, RemovableDeviceEventArgs e)
        {
            Debug.WriteLine($"Removable Device Event: @ \"{e.Path}\" was removed.");
        }

        private static void StorageEventManager_RemovableDeviceInserted(object sender, RemovableDeviceEventArgs e)
        {
            Debug.WriteLine($"Removable Device Event: @ \"{e.Path}\" was inserted.");
            ReadStorage();
        }
    }
}
