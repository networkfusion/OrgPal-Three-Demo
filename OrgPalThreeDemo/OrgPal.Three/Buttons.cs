using System;
//using System.Device.Gpio;

namespace OrgPal.Three
{
    public class Buttons
    {
        //private static GpioController gpioController;

        //private static GpioPin _userButton;
        //private static GpioPin _muxWakeButtonFlowControl;
        //private static GpioPin _wakeButton;

        public Buttons()
        {
            //gpioController = new GpioController();

            //_userButton = gpioController.OpenPin(PalThreePins.GpioPin.BUTTON_USER_BOOT1_PK7); //TODO: perhaps should use IoT.Devices.Button
            //_userButton.SetPinMode(PinMode.Input); //TODO: we definitely need to debounce this!
            //_userButton.ValueChanged += User_Boot1_Button_ValueChanged;

            ////the buttons are multiplexed so that the board can be woken up by user, or by the RTC
            ////so to get that interrupt to fire you need to do this:
            //// TODO: work out which buttons and why!
            //_muxWakeButtonFlowControl = gpioController.OpenPin(PalThreePins.GpioPin.MUX_EXT_BUTTON_WAKE_PE4);
            //_muxWakeButtonFlowControl.SetPinMode(PinMode.Output);
            //_muxWakeButtonFlowControl.Write(PinValue.High);
            //_muxWakeButtonFlowControl.ValueChanged += MuxWakeButtonFlowControl_ValueChanged;

            ////_wakeButton = gpioController.OpenPin(PalThreePins.GpioPin.BUTTON_WAKE_PA0);
            ////_wakeButton.SetPinMode(PinMode.Input);
            ////_wakeButton.ValueChanged += WakeButton_ValueChanged;
        }

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
}
