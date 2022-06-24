using OrgPal.Three;
using System;
using System.Diagnostics;
using System.IO.Ports;

namespace OrgPalThreeDemo.Peripherals
{
    /// <summary>
    /// Vaisala HMP155 humidity sensor
    /// </summary>
    /// <remarks> 
    /// Based on manual: https://www.vaisala.com/sites/default/files/documents/HMP155-User-Guide-in-English-M210912EN.pdf
    /// </remarks>
    public class VaisalaHMP155 : IDisposable
    {
        readonly SerialPortRS485 sensor;
        public int probeAddress { get; set; } = 0;

        public VaisalaHMP155(int connector = 0)
        {
            sensor = new SerialPortRS485(connector);

            sensor.Port.BaudRate = 4800;
            sensor.Port.DataBits = 7;
            sensor.Port.Parity = Parity.Even;
            sensor.Port.StopBits = StopBits.One;
            sensor.Port.Handshake = Handshake.None;

        }

        public void Close()
        {
            if (sensor.Port.IsOpen)
            {
                sensor.Port.Close();
            }
            sensor.Port.DataReceived -= Port_DataReceived;
        }

        public void Open()
        {
            if (sensor.Port.IsOpen)
            {
                sensor.Port.Close();
            }
            sensor.Port.Open();
            Debug.WriteLine("HMP155 serial port opened!");

            //TODO: support poll mode?

            sensor.Port.WriteLine("smode stop"); //or is this just "S"
            sensor.Port.WriteLine("?"); //Get the device info
            while (sensor.Port.BytesToRead > 0)
            {
                Debug.WriteLine(sensor.Port.ReadLine());
            }
            //sensor.Port.WriteLine("open");
            sensor.Port.WriteLine("intv 15 s");
            Debug.WriteLine(sensor.Port.ReadLine());
            sensor.Port.WriteLine("smode run"); // or should it just be "R"

            // We should possibily disable the transmit line now (unless we are in poll mode)?!

            sensor.Port.DataReceived += Port_DataReceived;
        }

        private void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort serialDevice = (SerialPort)sender;
            DecodeMessage(serialDevice.ReadLine().TrimEnd(new char[] {'\r','\n' }));
        }

        private void DecodeMessage(string message)
        {
            //TODO: handle message. (default format).
            // "RH= 33.0 %RH T= 22.1 'C"
            Debug.WriteLine(message);
            if (message.StartsWith("RH="))
            {
                // starts with "RH=" and ends with "%RH", strip those chars, then convert to a double?!
                var humidity = message.Substring(2, 6).Trim(' '); // Temporary "workaround?!
                                                                  // starts with "T=" and ends with "'C", strip those chars, then convert to a double?!
                var temperature = message.Substring(14, 6).Trim(' '); // Temporary "workaround?!
                Debug.WriteLine($"RH:{humidity}, T:{temperature}");
            }
        }


        public void Dispose()
        {

            sensor.Dispose();
        }
    }

}
