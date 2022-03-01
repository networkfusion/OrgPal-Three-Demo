//using System;
//using System.Diagnostics;
//using System.IO.Ports;
//using System.Text;
//using OrgPal.Three;

//namespace OrgPalThreeDemo.Peripherals
//{
//    public class VaisalaCL31 : IDisposable
//    {
//        private SerialPortRS485 sensor;

//        // This should be a struct!
//        public string cloudLayer1 = string.Empty;
//        public string cloudLayer2 = string.Empty;
//        public string cloudLayer3 = string.Empty;
//        public string verticalVis = string.Empty;
//        public string backscatter = string.Empty;
//        public string status = string.Empty;
//        // This should be a struct!

//        public VaisalaCL31()
//        {
//            sensor = new SerialPortRS485();

//            sensor.Port.BaudRate = 2400;
//            sensor.Port.DataBits = 7;
//            sensor.Port.Parity = System.IO.Ports.Parity.Even;
//            sensor.Port.StopBits = System.IO.Ports.StopBits.One;
//            sensor.Port.Handshake = System.IO.Ports.Handshake.None;

//            sensor.Port.WatchChar = (char)0x0A;
//        }

//        public void Close()
//        {
//            sensor.Port.DataReceived -= Port_DataReceived;
//            if (sensor.Port.IsOpen)
//            {
//                sensor.Port.Close();
//            }
//        }

//        public void Open()
//        {
//            sensor.Port.DataReceived += Port_DataReceived;
//            if (sensor.Port.IsOpen)
//            {
//                sensor.Port.Close();
//            }
//            sensor.Port.Open();
//            Debug.WriteLine("CL31 serial port opened!");
//        }

//        private void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
//        {
//            try
//            {
//                SerialPort serialDevice = (SerialPort)sender;

//                while (serialDevice.BytesToRead > 0)
//                {
//                    var byteRead = serialDevice.ReadByte();

//                    if (byteRead == 0x00)
//                    {
//                        Debug.WriteLine("-NULL-");
//                    }
//                    else if (byteRead == 0x01)
//                    {
//                        Debug.WriteLine("-SOH-");
//                    }
//                    else if (byteRead == 0x02)
//                    {
//                        Debug.WriteLine("-STX-");
//                    }
//                    else if (byteRead == 0x03)
//                    {
//                        Debug.WriteLine("-ETX-"); // Only hit!!!
//                    }
//                    else if (byteRead == 0x04)
//                    {
//                        Debug.WriteLine("-EOT-");
//                    }
//                    else
//                    {
//                        Debug.Write(((char)byteRead).ToString());
//                    }

//                }

//            }
//            catch (Exception ex)
//            {

//                Debug.WriteLine(ex.ToString());
//            }
//        }

//        private void DecodeMessage(string message)
//        {
//            var tempStringArray = message.Split('\u0004');
//            cloudLayer1 = "hello";
//            cloudLayer2 = "world";
//        }

//        public void Dispose()
//        {
//            cloudLayer1 = null;
//            cloudLayer2 = null;
//            cloudLayer3 = null;
//            verticalVis = null;
//            backscatter = null;
//            status = null;
//            sensor.Dispose();
//        }
//    }
//}
