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
        SerialPortRS485 sensor = new SerialPortRS485();
        int probeAddress;

        public VaisalaHMP155(int connector = 0, int busAddress = 0, int baudrate = 4800)
        {
            probeAddress = busAddress;
            sensor = new SerialPortRS485(connector);

            sensor.Port.BaudRate = baudrate;
            sensor.Port.DataBits = 7;
            sensor.Port.Parity = Parity.Even;
            sensor.Port.StopBits = StopBits.One;
            sensor.Port.Handshake = Handshake.None;

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
            Debug.WriteLine("HMP155 serial port opened!");

            //TODO: support poll mode?
        }

        private void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort serialDevice = (SerialPort)sender;
            DecodeMessage(serialDevice.ReadLine().TrimEnd(new char[] {'\r','\n' }));
        }

        void DecodeMessage(string message)
        {
            //TODO: handle message. (default format).
            // "RH= 33.0 %RH T= 22.1 'C"
            Debug.WriteLine(message);
        }

        void SendCommand()
        {
            //R (set temporary run)
            //S (end run mode)
            //INTV (the interval to send data)
            //...
        }

        public void Dispose()
        {

            sensor.Dispose();
        }
    }

}
