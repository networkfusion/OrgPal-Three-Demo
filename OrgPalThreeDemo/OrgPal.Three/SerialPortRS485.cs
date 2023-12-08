using System;
using System.IO.Ports;
using System.Device.Gpio;

namespace OrgPal.Three
{
    public class SerialPortRS485 : IDisposable
    {
        private bool _disposed;
        private readonly GpioPin _receiveEnablePin; //Drive RE high to let the AutoDirection circuit control the receiver (Default low = off)
        private readonly GpioPin _driverEnablePin; //must be high for IC to be on, low turns off the IC (Default low = off)
        private readonly GpioPin _terminationResistorPin; //120ohm as per spec! (default low = off)
        private readonly GpioPin _portPowerPin;

        public SerialPort Port { get; set; }

        //public bool ReceiverEnabled { get; set; }
        //public bool PortEnabled { get; set; }
        //public bool TerminationResistorEnabled { get; set; }

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

        //public enum TerminationResistorState
        //{
        //    On,
        //    Off
        //}

        /// <summary>
        /// Creates an RS485 serial port.
        /// </summary>
        /// <param name="connector">
        /// 0 = This is the default port found on the top (left) green connector (pins 1 and 2). 
        /// 1 = Expansion Port 1, (right side of PCB)
        /// 2 = Expansion Port 2, (bottom center of PCB)
        /// </param>
        /// <param name="termination">
        /// Sets the 120ohm termination resistor state.
        /// Default is true.
        /// </param>
        /// <param name="receiverEnabled">
        /// Enables the receive line
        /// </param>
        /// <param name="transmitterEnabled">
        /// Enables the transmit (driver) line
        /// </param>
        public SerialPortRS485(int connector = 0, bool termination = true, bool receiverEnabled = true, bool transmitterEnabled = true)
        {
            var gpioController = new GpioController();

            switch (connector)
            {
                case 1:
                    // Expansion Port 1, (right side of PCB)
                    _driverEnablePin = gpioController.OpenPin(Pinout.GpioPin.IO_PORT0.IO_PORT0_PIN_6_PA4, PinMode.Output);
                    _receiveEnablePin = gpioController.OpenPin(Pinout.GpioPin.IO_PORT0.IO_PORT0_PIN_7_PH13, PinMode.Output);
                    _terminationResistorPin = gpioController.OpenPin(Pinout.GpioPin.IO_PORT0.IO_PORT0_PIN_8_PH14, PinMode.Output);
                    Port = new SerialPort(Pinout.UartPort.UART6_IO_PORT0);
                    break;
                case 2:
                    //Expansion Port 2, (bottom center of PCB)
                    _driverEnablePin = gpioController.OpenPin(Pinout.GpioPin.IO_PORT1.IO_PORT1_PIN_6_PK4, PinMode.Output);
                    _receiveEnablePin = gpioController.OpenPin(Pinout.GpioPin.IO_PORT1.IO_PORT1_PIN_7_PB8, PinMode.Output);
                    _terminationResistorPin = gpioController.OpenPin(Pinout.GpioPin.IO_PORT1.IO_PORT1_PIN_8_PB9, PinMode.Output);
                    Port = new SerialPort(Pinout.UartPort.UART7_IO_PORT1);
                    break;
                default:
                    // This is the default port found on the top (left) green connector (pins 1 and 2). 
                    _receiveEnablePin = gpioController.OpenPin(Pinout.GpioPin.RS485_RECEIVERENABLE_PI_12, PinMode.Output);
                    _driverEnablePin = gpioController.OpenPin(Pinout.GpioPin.RS485_SHUTDOWN_PI_13, PinMode.Output);
                    _terminationResistorPin = gpioController.OpenPin(Pinout.GpioPin.RS485_RESISTORONOFF_PI_14, PinMode.Output);
                    _portPowerPin = gpioController.OpenPin(Pinout.GpioPin.POWER_RS485_ON_OFF_PJ14, PinMode.Output);
                    Port = new SerialPort(Pinout.UartPort.UART3_RS485);
                    break;
            }

            // We are going to currently set all pins to high (by default) as most likely to work!
            if (transmitterEnabled) //TODO: is this actually the transmitter???
            {
                // Must be high for IC to be on, low turns off the IC
                _driverEnablePin.Write(PinValue.High); //rs485SHTD
            }

            if (receiverEnabled)
            {
                // Drive Receiver Enabled high to let the AutoDirection circuit control the receiver
                _receiveEnablePin.Write(PinValue.High);
            }

            if (termination)
            {
                //if node is at end of RS485 circuit, enable the 120Ohms resistor.
                _terminationResistorPin.Write(PinValue.High);
            }

            // may need to turn power on RS 485 on board, but may be already done in other places so only try to do it.
            if (_portPowerPin.Read() == PinValue.Low)
            {
                _portPowerPin.Write(PinValue.High);
            }

        }

        /// <summary>
        /// Releases unmanaged resources
        /// and optionally release managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            _receiveEnablePin.Dispose();
            //_receiveEnablePin = null;
            _driverEnablePin.Dispose();
            //_driverEnablePin = null;
            _terminationResistorPin.Dispose();
            //_terminationResistorPin = null;
            _portPowerPin.Dispose();
            //_portPowerPin = null;

            if (Port.IsOpen)
            {
                Port.Close();
            }
            Port.Dispose();
            Port = null;


        }

        public void Dispose()
        {

            if (!_disposed)
            {
                Dispose(true);
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
