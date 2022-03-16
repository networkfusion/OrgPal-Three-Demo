using System;
using System.Device.Gpio;

namespace OrgPal.Three
{
    public class Keypad : IDisposable
    {
        readonly GpioPin _keypadPin1;
        readonly GpioPin _keypadPin2;
        readonly GpioPin _keypadPin3;
        readonly GpioPin _keypadPin4;

        public Keypad()
        {
            GpioController gpioController = new GpioController();
            _keypadPin1 = gpioController.OpenPin(Pinout.GpioPin.KEY_PIN1);
            _keypadPin2 = gpioController.OpenPin(Pinout.GpioPin.KEY_PIN2);
            _keypadPin3 = gpioController.OpenPin(Pinout.GpioPin.KEY_PIN3);
            _keypadPin4 = gpioController.OpenPin(Pinout.GpioPin.KEY_PIN4);

        }

        public void Dispose()
        {
            _keypadPin1.Dispose();
            _keypadPin2.Dispose();
            _keypadPin3.Dispose();
            _keypadPin4.Dispose();
        }
    }
}
