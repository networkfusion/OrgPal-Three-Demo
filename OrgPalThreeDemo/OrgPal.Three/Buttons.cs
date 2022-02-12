using System;
using Iot.Device.Button;

namespace OrgPal.Three
{
    public class Buttons : IDisposable
    {
        //private static GpioController gpioController;

        private static GpioButton _userButton;
        //private static GpioButton _muxWakeButtonFlowControl;
        //private static GpioButton _wakeButton;

        public Buttons()
        {
            //gpioController = new GpioController();

            _userButton = new GpioButton(buttonPin: Pinout.GpioPin.BUTTON_USER_BOOT1_PK7);
            //_userButton = gpioController.OpenPin(Pinout.GpioPin.BUTTON_USER_BOOT1_PK7); //TODO: perhaps should use IoT.Devices.Button
            //_userButton.SetPinMode(PinMode.Input); //TODO: we definitely need to debounce this!
            //_userButton.ValueChanged += User_Boot1_Button_ValueChanged;

            ////the buttons are multiplexed so that the board can be woken up by user, or by the RTC
            ////so to get that interrupt to fire you need to do this:
            //// TODO: work out which buttons and why!
            //_muxWakeButtonFlowControl = gpioController.OpenPin(Pinout.GpioPin.MUX_EXT_BUTTON_WAKE_PE4);
            //_muxWakeButtonFlowControl.SetPinMode(PinMode.Output);
            //_muxWakeButtonFlowControl.Write(PinValue.High);
            //_muxWakeButtonFlowControl.ValueChanged += MuxWakeButtonFlowControl_ValueChanged;

            ////_wakeButton = gpioController.OpenPin(Pinout.GpioPin.BUTTON_WAKE_PA0);
            ////_wakeButton.SetPinMode(PinMode.Input);
            ////_wakeButton.ValueChanged += WakeButton_ValueChanged;
        }


        //private static void MuxWakeButtonFlowControl_ValueChanged(object sender, PinValueChangedEventArgs e)
        //{
        //    Debug.WriteLine("Handle MuxWakeButton Flow...!");
        //}

        //private static void WakeButton_ValueChanged(object sender, PinValueChangedEventArgs e)
        //{
        //    Debug.WriteLine("Handle WakeFlow -Should be MUX?-...!");
        //}

        //TODO: this event seems to fire endlessly (probably MUX>?!
        //private static void User_Boot1_Button_ValueChanged(object sender, PinValueChangedEventArgs e)
        //{
        //    if (e.ChangeType == PinEventTypes.Rising) //button pressed.
        //    {
        //        Debug.WriteLine("USER/BOOT1 button pressed...!");
        //        Thread lcdShowThread = new Thread(new ThreadStart(LcdUpdate_Thread));
        //        lcdShowThread.Start();
        //    }
        //}

        public void Dispose()
        {
            _userButton.Dispose();
            //_muxWakeButtonFlowControl.Dispose();
            //_wakeButton.Dispose();
        }
    }
}
