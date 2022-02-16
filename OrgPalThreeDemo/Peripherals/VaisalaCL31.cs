using System;
using System.Diagnostics;
using System.IO.Ports;
using OrgPal.Three;

namespace OrgPalThreeDemo.Peripherals
{
    public class VaisalaCL31 : IDisposable
    {
        private SerialPortRS485 sensor;

        // This should be a struct!
        public string cloudLayer1 = string.Empty;
        public string cloudLayer2 = string.Empty;
        public string cloudLayer3 = string.Empty;
        public string verticalVis = string.Empty;
        public string backscatter = string.Empty;
        public string status = string.Empty;
        // This should be a struct!

        public VaisalaCL31()
        {
            sensor = new SerialPortRS485();

            sensor.Port.BaudRate = 2400;
            sensor.Port.DataBits = 7;
            sensor.Port.Parity = System.IO.Ports.Parity.Even;
            sensor.Port.StopBits = System.IO.Ports.StopBits.One;
            sensor.Port.Handshake = System.IO.Ports.Handshake.None;

            sensor.Port.WatchChar = "\u0004";
        }

        public void Close()
        {
            sensor.Port.DataReceived -= Port_DataReceived;
            if (sensor.Port.IsOpen)
            {
                sensor.Port.Close();
            }
        }

        public void Open()
        {
            sensor.Port.DataReceived += Port_DataReceived;
            if (sensor.Port.IsOpen)
            {
                sensor.Port.Close();
            }
            sensor.Port.Open();
            Debug.WriteLine("CL31 serial port opened!");
        }

        private void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort serialDevice = (SerialPort)sender;
            if (e.EventType == SerialData.Chars)
            {
                Debug.WriteLine("rx chars");
            }
            else if (e.EventType == SerialData.WatchChar)
            {
                Debug.WriteLine("rx watch char");
            }

            // need to make sure that there is data to be read, because
            // the event could have been queued several times and data read on a previous call
            if (serialDevice.BytesToRead > 0)
            {
                byte[] buffer = new byte[serialDevice.BytesToRead];

                var bytesRead = serialDevice.Read(buffer, 0, buffer.Length);

                Debug.WriteLine("Read completed: " + bytesRead + " bytes were read from " + serialDevice.PortName + ".");

                string temp = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.WriteLine("String: >>" + temp + "<< ");
            }
        }

        private void DecodeMessage(string message)
        {
            var tempStringArray = message.Split('\u0004');
            cloudLayer1 = "hello";
            cloudLayer2 = "world";
        }

        public void Dispose()
        {
            cloudLayer1 = null;
            cloudLayer2 = null;
            cloudLayer3 = null;
            verticalVis = null;
            backscatter = null;
            status = null;
            sensor.Dispose();
        }
    }
}
