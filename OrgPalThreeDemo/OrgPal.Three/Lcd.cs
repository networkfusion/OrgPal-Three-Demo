using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
//using Iot.Device.Pcx857x;
using Iot.Device.CharacterLcd;

namespace OrgPal.Three
{
    /// <summary>
    /// LCD Character Display
    /// </summary>
    /// <remarks>
    /// Based on an I2C controller https://github.com/dotnet/iot/tree/main/src/devices/Pcx857x
    /// But actual display (via I2C) is a 1602A! https://www.openhacks.com/uploadsproductos/eone-1602a1.pdf -- https://github.com/k-moskwa/kmAvrLedBar/blob/327e662199ec53ddb2edf2fbe96dba01f4ce4d25/src/kmLCD/kmLiquidCrystal.c
    /// Although Pcf8574 only (seems) to be redundant at this point! 
    /// </remarks>
    public class Lcd : IDisposable
    {
        /// <summary>
        /// For a `PCF8574`, the I2C address range is: 0x38 - 0x3F (Dec: 56 - 63)
        /// </summary>
        const byte I2C_LCD_ADDRESS_MAIN = 0x3F;

        /// <summary>
        /// For a `PCF8574T`, I2C address range is: 0x20 - 0x27  (Dec: 32 - 38)
        /// </summary>
        const byte I2C_LCD_ADDRESS_DEFAULT = 0x27; // 0X27 on other models depnding of soldered a0,a1,a2

        private readonly I2cDevice _i2cDevice;
        private GpioPin lcdPowerOnOff { get; set; }

        //private readonly Pcf8574 _displayController;

        public LcdConsole Output { get; private set; }

        public Lcd(int i2cBusId = Pinout.I2CBus.I2C3, byte i2cAddress = I2C_LCD_ADDRESS_MAIN)
        {

            //TODO: this (should) be initialised by the Pcf8574 controller?!
            lcdPowerOnOff = new GpioController().OpenPin(Pinout.GpioPin.POWER_LCD_ON_OFF, PinMode.Output);
            lcdPowerOnOff.Write(PinValue.High);

            var _i2cConfig = new I2cConnectionSettings(i2cBusId, i2cAddress, I2cBusSpeed.FastMode);

            _i2cDevice = I2cDevice.Create(_i2cConfig);

            var result = _i2cDevice.WriteByte(0x00); //Write command 0 and see if it is acknowledged.
            if (result.Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
            {
                _i2cDevice.Dispose();
                _i2cConfig = new I2cConnectionSettings(i2cBusId, I2C_LCD_ADDRESS_DEFAULT, I2cBusSpeed.FastMode); // the other default address
                _i2cDevice = I2cDevice.Create(_i2cConfig);

                result = _i2cDevice.WriteByte(0x00); //Write command 0 and see if it is acknowledged.
                if (result.Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
                {
                    Debug.WriteLine("No Character LCD controller found");
                    throw new ArgumentNullException("DeviceNotFound: I2C_DisplayController"); //possibily worth custom exceptions i.e. https://docs.microsoft.com/en-us/dotnet/standard/exceptions/how-to-create-user-defined-exceptions 
                }
            }

            //_displayController = new Pcf8574(_i2cDevice);
            //using LcdInterface lcdInterface = LcdInterface.CreatePcx857x(_displayController);
            using LcdInterface lcdInterface = LcdInterface.CreateI2c(_i2cDevice, false);
            using Hd44780 lcd = new Lcd1602(lcdInterface);
            lcd.UnderlineCursorVisible = false;

            Debug.WriteLine("Display initialized.");
            Output = new LcdConsole(lcd, "A00"); //using default char encoding.

            // Ensure the display is blank before doing stuff!
            Output.Clear();
        }



        /// <summary>
        /// Releases unmanaged resources
        /// and optionally release managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Output.Dispose();


                lcdPowerOnOff.Write(PinValue.Low);

                if (lcdPowerOnOff != null)
                {
                    lcdPowerOnOff.Dispose();
                    lcdPowerOnOff = null;
                }

                //if (_displayController != null)
                //    _displayController.Dispose();

                if (_i2cDevice != null)
                    _i2cDevice.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Cleanup
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~Lcd()
        {
            Dispose(false);
        }

    }
}