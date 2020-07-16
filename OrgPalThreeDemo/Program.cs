using nanoFramework.Json;
using nanoFramework.Networking;
using nanoFramework.Runtime.Native;
using PalThree;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Windows.Devices.Gpio;
using Windows.Storage;
using Windows.Storage.Streams;

namespace OrgPalThreeDemo
{
    public class Program
    {
        private static GpioController gpioController;

        //private static GpioPin _muxFlowControl;
        private static GpioPin _wakeButton;
        //private static GpioPin _userButton;
        private static Drivers.OnboardDevices palthree;
        private static LCD lcd;
        private static MCP342x adcPalSensor;

        private static string _serialNumber;

        private static DateTime startTime = DateTime.UtcNow;
        private static int messagesSent = 0;


        private static string _awsHost = string.Empty; //make sure to add your AWS endpoint and region. Stored in mqttconfig.json (make sure it is stored on the root of the SD card)
        //{"Url" : "<endpoint>-ats.iot.<region>.amazonaws.com"}
        private static int _awsPort = 8883; //add your port to mqttconfig.json if different from the default
                                            //{"Port" : "8883"}
        private static string _thingName; ////add your "thing" to mqttconfig.json if different from the devices SerialNumber

        //private static readonly string clientId = Guid.NewGuid().ToString(); //This should really be persisted across reboots, but an auto generated GUID is fine for testing.
        private static string clientRsaSha256Crt = string.Empty; //Device Certificate copied from AWS (make sure it is stored on the root of the SD card)
        private static string clientRsaKey = string.Empty; //Device private key copied from AWS (make sure it is stored on the root of the SD card)
        private static byte[] rootCA;
        private static MqttClient client;
        private static AwsShadow shadow;


        public static void Main()
        {
            Debug.WriteLine("OrgPal Three Demo!");

            palthree = new Drivers.OnboardDevices();
            adcPalSensor = new MCP342x();

            gpioController = GpioController.GetDefault();


            //we have multiplexed these buttons so that the board can wake up by user, or by the RTC
            //so to get that interrupt to fire you need to do this
            //_muxFlowControl = gpioController.OpenPin(PalThreePins.GpioPin.MUX_EXT_BUTTON_WAKE_PE4);
            //_muxFlowControl.SetDriveMode(GpioPinDriveMode.Output);
            //_muxFlowControl.Write(GpioPinValue.High);
            //_muxFlowControl.ValueChanged += MuxFlowControl_ValueChanged;

            //_userButton = gpioController.OpenPin(PalThreePins.GpioPin.BUTTON_USER_WAKE_PE6);
            //_userButton.SetDriveMode(GpioPinDriveMode.Input);
            //_userButton.ValueChanged += UserButton_ValueChanged;

            _wakeButton = gpioController.OpenPin(PalThreePins.GpioPin.BUTTON_WAKE_PA0);
            _wakeButton.SetDriveMode(GpioPinDriveMode.Input);
            _wakeButton.ValueChanged += WakeButton_ValueChanged;

            lcd = new LCD();

            lcd.BacklightOn = true;
            lcd.Display("Please Wait...");

            Thread.Sleep(1000);

            lcd.Display($"Voltage: {palthree.GetBatteryUnregulatedVoltage().ToString("n2")} \n Temp: {palthree.GetTemperatureOnBoard().ToString("n2")}", 0);

            Thread.Sleep(5000);
            lcd.BacklightOn = false;


            foreach (byte b in nanoFramework.Hardware.Stm32.Utilities.UniqueDeviceId)
            {
                _serialNumber += b.ToString("X2");
            }

            // add event handlers for Removable Device insertion and removal
            StorageEventManager.RemovableDeviceInserted += StorageEventManager_RemovableDeviceInserted;
            StorageEventManager.RemovableDeviceRemoved += StorageEventManager_RemovableDeviceRemoved;
            ReadStorage();

            SetupNetwork();

            startTime = DateTime.UtcNow; //set now because the clock might have been wrong before ntp is checked.

            Debug.WriteLine($"Start Time: {startTime}");

            var connected = false;
            while (!connected)
            {
                connected = SetupMqtt();
            }


            Thread.Sleep(Timeout.Infinite);
        }

        //private static void MuxFlowControl_ValueChanged(object sender, GpioPinValueChangedEventArgs e)
        //{
        //    Debug.WriteLine("Handle Mux Flow...!");
        //}

        private static void WakeButton_ValueChanged(object sender, GpioPinValueChangedEventArgs e)
        {
            if (e.Edge == GpioPinEdge.RisingEdge)
            {
                lcd.BacklightOn = true;
                lcd.Display($"Voltage: {palthree.GetBatteryUnregulatedVoltage().ToString("n2")} \n Temp: {palthree.GetTemperatureOnBoard().ToString("n2")}", 0);

                Thread.Sleep(5000);
                lcd.BacklightOn = false;
            }
        }

        //private static void UserButton_ValueChanged(object sender, GpioPinValueChangedEventArgs e)
        //{
        //    if (e.Edge == GpioPinEdge.RisingEdge)
        //    {
        //        lcd.BacklightOn = true;
        //        lcd.Display($"Voltage: {palthree.GetBatteryUnregulatedVoltage().ToString("n2")} \n Status: {palthree.GetTemperatureOnBoard().ToString("n2")}", 0);

        //        Thread.Sleep(5000);
        //        lcd.BacklightOn = false;
        //    }
        //}

        private static void SetupNetwork()
        {
            // if we are using TLS it requires valid date & time
            NetworkHelpers.SetupAndConnectNetwork(true);

            Debug.WriteLine("Waiting for network up and IP address...");
            NetworkHelpers.IpAddressAvailable.WaitOne();

            Debug.WriteLine("Waiting for valid Date & Time...");
            NetworkHelpers.DateTimeAvailable.WaitOne();
        }

        static bool SetupMqtt()
        {
            X509Certificate caCert = new X509Certificate(rootCA); //commented out as MDP changes mean resources dont currently work//Resources.GetBytes(Resources.BinaryResources.AwsCAroot)); //should this be in secure storage, or is it fine where it is?
            X509Certificate2 clientCert = new X509Certificate2(clientRsaSha256Crt, clientRsaKey, ""); //make sure to add a correct pfx certificate

            try
            {
                client = new MqttClient(_awsHost, _awsPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);

                // register to message received 
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                client.Connect(_thingName);

                //shadow = new AwsShadow(_thingName, ref client);
                //Thread shadowThread = new Thread(new ThreadStart(ShadowLoop));
                //shadowThread.Start();

                // subscribe to the topic with QoS 1
                client.Subscribe(new string[] { "devices/nanoframework/sys" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                Thread telemetryThread = new Thread(new ThreadStart(TelemetryLoop));
                telemetryThread.Start();
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
                var shadowTelemetry = new StatusMessage();
                shadowTelemetry.operatingSystem = "nanoFramework"; //move to shadow
                shadowTelemetry.platform = SystemInfo.TargetName; //move to shadow
                shadowTelemetry.cpu = SystemInfo.Platform; //move to shadow
                shadowTelemetry.bootTimestamp = startTime; //move to shadow

                string shadowData = JsonConvert.SerializeObject(shadowTelemetry);
                shadow.UpdateThingShadow(shadowData);

                Debug.WriteLine("Shadow sent: " + shadowData);

                Thread.Sleep(600000); //1 hour
            }
        }

        static void TelemetryLoop()
        {
            for ( ; ; )
            {
                var statusTelemetry = new StatusMessage();
                statusTelemetry.operatingSystem = "nanoFramework"; //move to shadow
                statusTelemetry.platform = SystemInfo.TargetName; //move to shadow
                statusTelemetry.cpu = SystemInfo.Platform; //move to shadow
                statusTelemetry.serialNumber = _serialNumber;
                statusTelemetry.sendTimestamp = DateTime.UtcNow;
                statusTelemetry.bootTimestamp = startTime; //move to shadow
                statusTelemetry.messageNumber = messagesSent += 1;
                statusTelemetry.batteryVoltage = palthree.GetBatteryUnregulatedVoltage();
                statusTelemetry.enclosureTemperature = palthree.GetTemperatureOnBoard();
                statusTelemetry.memoryFree = nanoFramework.Runtime.Native.GC.Run(false);
                statusTelemetry.mcuTemperature = palthree.GetMcuTemperature();
                statusTelemetry.airTemperature = adcPalSensor.GetTemperatureFromPT100();

                string sampleData = JsonConvert.SerializeObject(statusTelemetry);
                client.Publish($"devices/nanoframework/{_thingName}/data", Encoding.UTF8.GetBytes(sampleData), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

                Debug.WriteLine("Message sent: " + sampleData);

                Thread.Sleep(60000); //1 minute
            }
        }

        static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string Message = new string(Encoding.UTF8.GetChars(e.Message));
            Debug.WriteLine("Message received: " + Message);
        }

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
                    if (file.FileType == "crt")
                    {
                        //clientRsaSha256Crt = FileIO.ReadText(f); //Currently doesnt work with nf!
                        //workaround...
                        var buffer = FileIO.ReadBuffer(file);
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            clientRsaSha256Crt = dataReader.ReadString(buffer.Length);
                        }
                        
                        //Should load into secure storage (somewhere) and delete file on removable device?
                    }
                    if (file.FileType == "der")
                    {
                        var buffer = FileIO.ReadBuffer(file);
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            rootCA = new byte[buffer.Length];
                            dataReader.ReadBytes(rootCA);
                        }
                    }
                    if (file.FileType == "key")
                    {
                        //clientRsaKey = FileIO.ReadText(f); //Currently doesnt work with nf!
                        //workaround...
                        var buffer = FileIO.ReadBuffer(file);
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            clientRsaKey = dataReader.ReadString(buffer.Length);
                        }
                        
                        //Should load into secure storage (somewhere) and delete file on removable device?
                    }
                    if (file.Name == "mqttconfig.json")
                    {
                        var buffer = FileIO.ReadBuffer(file);
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            MqttConfig config = (MqttConfig)JsonConvert.DeserializeObject(dataReader, typeof(MqttConfig));
                            _awsHost = config.Url;
                            //_awsPort = config.Port;
                            if (config.ThingName != string.Empty || config.ThingName != null)
                            {
                                _thingName = config.ThingName;
                            }
                            else
                            {
                                _thingName = _serialNumber;
                            }
                        }

                        //Should load into secure storage (somewhere) and delete file on removable device?
                    }
                }
            }

        }


        private static void StorageEventManager_RemovableDeviceRemoved(object sender, RemovableDeviceEventArgs e)
        {
            Debug.WriteLine($"Removable Device @ \"{e.Path}\" removed.");
        }

        private static void StorageEventManager_RemovableDeviceInserted(object sender, RemovableDeviceEventArgs e)
        {
            Debug.WriteLine($"Removable Device @ \"{e.Path}\" inserted.");
            ReadStorage();
        }
    }
}
