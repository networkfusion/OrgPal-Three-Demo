using System;
using System.Device.Gpio;

namespace OrgPal.Three
{
    public class Keypad : IDisposable
    {
        private bool _disposed;
        readonly GpioPin _keypadPin1;
        readonly GpioPin _keypadPin2;
        readonly GpioPin _keypadPin3;
        readonly GpioPin _keypadPin4;

        public Keypad()
        {
            GpioController gpioController = new GpioController();
            _keypadPin1 = gpioController.OpenPin(Pinout.GpioPin.KEYPAD_PORT.KEY_PIN1);
            _keypadPin2 = gpioController.OpenPin(Pinout.GpioPin.KEYPAD_PORT.KEY_PIN2);
            _keypadPin3 = gpioController.OpenPin(Pinout.GpioPin.KEYPAD_PORT.KEY_PIN3);
            _keypadPin4 = gpioController.OpenPin(Pinout.GpioPin.KEYPAD_PORT.KEY_PIN4);

        }

        /// <summary>
        /// Releases unmanaged resources
        /// and optionally release managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            _keypadPin1.Dispose();
            _keypadPin2.Dispose();
            _keypadPin3.Dispose();
            _keypadPin4.Dispose();

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
