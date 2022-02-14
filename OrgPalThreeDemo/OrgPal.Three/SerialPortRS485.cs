using System;
using System.IO.Ports;
using System.Device.Gpio;

namespace OrgPal.Three
{
    public class SerialPortRS485 : IDisposable
    {
        //GpioPin _receiverEnabled;
        //GpioPin _shutdownPort;
        //GpioPin _resistorOff; //Guessing this is the termination resistor!

        //SerialPort _port;

        //public enum ReceiverState
        //{
        //    On,
        //    Off
        //}

        //public enum PortState
        //{
        //    On,
        //    Off
        //}

        //public enum ResistorState
        //{
        //    On,
        //    Off
        //}


        public SerialPortRS485()
        {
            //var gpioController = new GpioController();
            //_receiverEnabled = gpioController.OpenPin(Pinout.GpioPin.RS485_RECEIVERENABLE_PI_12);
            //_shutdownPort = gpioController.OpenPin(Pinout.GpioPin.RS485_SHUTDOWN_PI_13);
            //_resistorOff = gpioController.OpenPin(Pinout.GpioPin.RS485_RESISTORONOFF_PI_14);

            //_port = new SerialPort(Pinout.UartPort.UART3_RS485);
        }

        public void Dispose()
        {
            //_receiverEnabled.Dispose();
            //_shutdownPort.Dispose();
            //_resistorOff.Dispose();

            //if (_port.IsOpen())
            //{ _port.Close(); }
            //_port.Dispose();
        }
    }
}
