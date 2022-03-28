using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using Iot.Device.Pcx857x;
using Iot.Device.CharacterLcd;

namespace OrgPal.Three
{
    /// <summary>
    /// LCD Character Display
    /// </summary>
    /// <remarks>
    /// Based on an I2C controller https://github.com/dotnet/iot/tree/main/src/devices/Pcx857x
    /// But actual display (via I2C) is a 1602A! https://www.openhacks.com/uploadsproductos/eone-1602a1.pdf -- https://github.com/k-moskwa/kmAvrLedBar/blob/327e662199ec53ddb2edf2fbe96dba01f4ce4d25/src/kmLCD/kmLiquidCrystal.c
    /// </remarks>
    public class OrgPalThreeLcd : IDisposable
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

        private readonly Pcf8574 _displayController;

        public LcdConsole Display { get; private set; }
        public OrgPalThreeLcd(int i2cBusId = Pinout.I2CBus.I2C3, byte i2cAddress = I2C_LCD_ADDRESS_MAIN)
        {

            //TODO: this (should) be initialised by the Pcf8574 controller!
            lcdPowerOnOff = new GpioController().OpenPin(Pinout.GpioPin.POWER_LCD_ON_OFF, PinMode.Output);
            lcdPowerOnOff.Write(PinValue.High);

            var _i2cConfig = new I2cConnectionSettings(i2cBusId, i2cAddress, I2cBusSpeed.FastMode);


            // Thread.Sleep(250);//small break to make the LCD startup better in some cases helps boot up without sensor

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

            _displayController = new Pcf8574(_i2cDevice);
            //using LcdInterface _interface = new Pcf8574(_i2cDevice);
            using LcdInterface lcdInterface = LcdInterface.CreateI2c(_i2cDevice, false);
            //var lcd = new Lcd1602(registerSelectPin: 0, enablePin: 2, dataPins: new int[] { 4, 5, 6, 7 }, backlightPin: 3, readWritePin: 1, controller: new GpioController((PinNumberingScheme.Logical, _displayController));
            using Hd44780 lcd = new Lcd1602(lcdInterface);
            lcd.UnderlineCursorVisible = false;
            lcd.BacklightOn = true;

            Debug.WriteLine("Display initialized.");
            Display = new LcdConsole(lcd, "A00"); //using default char encoding.

            // Ensure the display is blank before doing stuff!
            lcd.Clear();
        }



        public void Update(string line1, string line2)
        {
            //TODO: this function should be deprecated now!!!
            Display.Clear();
            Display.WriteLine(line1);
            Display.WriteLine(line2);
        }



        /// <inheritdoc/>
        public void Dispose()
        {
            // Cleanup
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Display.Dispose();


                lcdPowerOnOff.Write(PinValue.Low);

                if (lcdPowerOnOff != null)
                {
                    lcdPowerOnOff.Dispose();
                    lcdPowerOnOff = null;
                }

                if (_displayController != null)
                    _displayController.Dispose();

                if (_i2cDevice != null)
                    _i2cDevice.Dispose();
            }
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~OrgPalThreeLcd()
        {
            Dispose(false);
        }

    }
}