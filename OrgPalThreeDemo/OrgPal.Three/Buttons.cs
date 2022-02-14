using System;
using System.Diagnostics;
using Iot.Device.Button;
using System.Device.Gpio;

namespace OrgPal.Three
{
    public class Buttons : IDisposable
    {
        private static GpioPin _muxWakeButtonFlowControl;
        private static GpioButton _userButton;
        private static GpioButton _wakeButton;
        private static GpioButton _diagnosticButton;
        // TODO: is loader button a thing?



        public Buttons()
        {
            //// the buttons are multiplexed so that the board can be woken up by user, or by the RTC
            //// so to get that interrupt to fire you need to do this:
            //// TODO: work out which buttons and why!
            _muxWakeButtonFlowControl = new GpioController().OpenPin(Pinout.GpioPin.MUX_EXT_BUTTON_WAKE_PE4);
            _muxWakeButtonFlowControl.SetPinMode(PinMode.Output);
            _muxWakeButtonFlowControl.Write(PinValue.High);
            //_muxWakeButtonFlowControl.ValueChanged += MuxWakeButtonFlowControl_ValueChanged;

            _userButton = new GpioButton(buttonPin: Pinout.GpioPin.BUTTON_USER_BOOT1_PK7);
            _userButton.Press += _userButton_Press;

            _wakeButton = new GpioButton(buttonPin: Pinout.GpioPin.BUTTON_WAKE_PA0);
            _wakeButton.Press += WakeButton_Press;

            _diagnosticButton = new GpioButton(buttonPin: Pinout.GpioPin.BUTTON_DIAGNOSTIC_PB7);
            _diagnosticButton.Press += DiagnosticButton_Press;
        }


        //private static void MuxWakeButtonFlowControl_ValueChanged(object sender, PinValueChangedEventArgs e)
        //{
        //    Debug.WriteLine("Handle MuxWakeButton Flow...!");
        //}

        private static void WakeButton_Press(object sender, EventArgs e)
        {
            Debug.WriteLine("Wake Button pressed...!"); //Flow -Should be MUX?-...!
        }

        
        private static void _userButton_Press(object sender, EventArgs e)
        {
            //TODO: this event seems to fire endlessly (probably MUX)?!
            Debug.WriteLine("USER/BOOT1 button pressed...!");

            //Thread lcdShowThread = new Thread(new ThreadStart(LcdUpdate_Thread));
            //lcdShowThread.Start();
        }

        private static void DiagnosticButton_Press(object sender, EventArgs e)
        {
            Debug.WriteLine("Diagnostic Button pressed...!");
        }

        public void Dispose()
        {
            _userButton.Dispose();
            _wakeButton.Dispose();
            _diagnosticButton.Dispose();
            _muxWakeButtonFlowControl.Dispose();
        }
    }
}
