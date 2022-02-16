using System;
using System.Diagnostics;
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

        VaisalaCL31()
        {
            sensor = new SerialPortRS485(termination: false);

            sensor.Port.BaudRate = 2400;
            sensor.Port.DataBits = 7;
            sensor.Port.Parity = System.IO.Ports.Parity.Even;
            sensor.Port.StopBits = System.IO.Ports.StopBits.One;
            sensor.Port.Handshake = System.IO.Ports.Handshake.None;


            sensor.Port.DataReceived += Port_DataReceived;

            sensor.Port.Open();
        }

        private static void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            Debug.WriteLine(e.ToString());
        }

        private void DecodeMessage(string message)
        {
            var tempStringArray = message.Split(' ');
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
