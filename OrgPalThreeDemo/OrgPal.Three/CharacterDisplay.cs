using System;
using System.Text;
using System.Threading;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using Iot.Device.Pcx857x;

namespace OrgPal.Three
{
    /// <summary>
    /// LCD Character Display
    /// </summary>
    /// <remarks>
    /// Based on an I2C controller https://github.com/dotnet/iot/tree/main/src/devices/Pcx857x
    /// But actual display (via I2C) is a 1602A! https://www.openhacks.com/uploadsproductos/eone-1602a1.pdf -- https://github.com/k-moskwa/kmAvrLedBar/blob/327e662199ec53ddb2edf2fbe96dba01f4ce4d25/src/kmLCD/kmLiquidCrystal.c
    /// </remarks>
    public class CharacterDisplay : IDisposable
    {
        // For PCF8574 chip, I2C address range: 0x38 - 0x3F (Dec:   56-63)
        // For PCF8574T chip, I2C address range: 0x20-0x27  (Dec:    32-38)
        const byte I2C_LCD_ADDRESS_MAIN = 0x3F;
        const byte I2C_LCD_ADDRESS_DEFAULT = 0x27; // 0X27 on other models depnding of soldered a0,a1,a2


        /// <summary>
        /// Command
        /// </summary>
        [Flags]
        private enum LcdInstruction
        {
            None = 0x00,
            /// <summary>
            /// Write "0x20" to DDRAM and set DDRAM address to "0x00" from AC
            /// </summary>
            ClearDisplay = 0x01, // Delay 1.52ms
            /// <summary>
            /// Set DDRAM address to "0x00" from AC and return cursor to its original position if shifted.
            /// The contents of DDRAM are not changed.
            /// </summary>
            ReturnHome = 0x02, // Delay 1.52ms
            /// <summary>
            /// Sets cursor move direction and specifies display shift.
            /// These operations are performed during data write and read.
            /// </summary>
            /// <remarks>
            /// 0x01 = Shift Entire Display On
            /// 0x02 = Move Right, shift left (off would be move left, shift right)
            /// </remarks>
            EntryModeSet = 0x04, // Delay 37us
            /// <summary>
            /// Sets the display mode
            /// </summary>
            /// <remarks>
            /// As flags:
            /// 0x00 = Entire display off???
            /// 0x01 = Cursor position on
            /// 0x02 = Cursor On
            /// 0x04 = Entire display on
            /// </remarks>
            DisplayMode = 0x08, // Delay 37us
            /// <summary>
            /// Set cursor moving and display shift control bit, and direction,
            /// without changing DDRAM data.
            /// </summary>
            /// <remarks>
            /// 0x04 = R/L
            /// 0x08 = S/C
            /// </remarks>
            CursorDisplayShift = 0x10, // Delay 37us
            /// <summary>
            /// Sets display function
            /// </summary>
            /// <remarks>
            /// 0x04 = Font Size is 5x11 (not set is 5x8)
            /// 0x08 = Number of lines is 2 ( not set is 1)
            /// 0x10 = interface data is 8 (not set is 4 bits)
            /// </remarks>
            FunctionSet = 0x20, // Delay 37us
            /// <summary>
            /// Set CGRAM in  address counter
            /// </summary>
            SetCGRAMAddress = 0x40, // Delay 37us
            /// <summary>
            /// Set DDRAM in  address counter
            /// </summary>
            SetDDRAMAddress = 0x80, // Delay 37us
            /// <summary>
            /// Checks whether the display is currently busy.
            /// The content of address counter can also be read.
            /// </summary>
            /// <remarks>
            /// 0x80 = ReadBusy flag
            /// 0x40-0x01 = Address counter
            /// </remarks>
            ReadBusyFlagAndAddress = 0x100, // Delay 0us
            /// <summary>
            /// Write data to internal RAM (DDRAM/CGRAM)
            /// </summary>
            /// <remarks>
            /// First 8 bytes of RAM, e.g.
            /// 0x00 to 0x80
            /// </remarks>
            WriteDataToRam = 0x200, // Delay 37us
            /// <summary>
            /// Read data from internal RAM (DDRAM/CGRAM).
            /// </summary>
            /// <remarks>
            /// First 8 bytes of RAM, e.g.
            /// 0x00 to 0x80
            /// </remarks>
            ReadDataFromRam = 0x300, // Delay 37us
        }

        //////////// TODO: sort below out! ////////////

        /// <summary>
        /// LCD entry mode
        /// </summary>
        [Flags]
        private enum LcdEntryMode : byte
        {
            ShiftEntireDisplayOn = 0x01,
            MoveRightShiftLeft = 0x02,
        }

        /// <summary>
        /// LCD display mode
        /// </summary>
        [Flags]
        private enum LcdDisplayMode : byte
        {

            None = 0x00, // Entire display off???
            CursorPositionOn = 0x01, // Default is off
            CursorOn = 0x02, // Default is off
            EntireDisplayOn = 0x04, // Default is off
        }

        /// <summary>
        /// LCD function set
        /// </summary>
        [Flags]
        private enum LcdFuctionSet : byte
        {
            None = 0x00,
            FontSize5x11 = 0x04, // Default is 5x8
            TwoLines = 0x08, // Default is 1 line
            EightBitInterface = 0x10, // Default is 4 bits
        }


        //TODO: I2C values for GPIO???
        // flags for backlight control
        const byte LCD_NOBACKLIGHT = 0x00; //0x00
        const byte LCD_BACKLIGHT = 0b00001000;  // Turn On Backlight //0x08

        const byte EnableBit = 0b00000100;  // Enable bit
        //const byte Rw = 0b00000010;  // Read/Write bit
        const byte RegSelectBit = 0b00000001;  // Register select bit

        private readonly I2cDevice _i2cDevice;
        private byte backlightval = LCD_NOBACKLIGHT;
        private GpioPin lcdPowerOnOff;

        private readonly Pcf8574 _displayController;

        public bool BacklightOn
        {
            get { return backlightval == LCD_BACKLIGHT; }

            set
            {
                if (value)//turn it on
                {
                    lcdPowerOnOff.Write(PinValue.High);
                    backlightval = LCD_BACKLIGHT;
                }
                else //turn it off
                {
                    lcdPowerOnOff.Write(PinValue.Low);
                    backlightval = LCD_NOBACKLIGHT;
                }

                WriteByte(0);
            }
        }


        public CharacterDisplay(int i2cBusId = Pinout.I2CBus.I2C3, byte i2cAddress = I2C_LCD_ADDRESS_MAIN)
        {
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
                    throw new Exception("DeviceNotFound: I2C_DisplayController"); //possibily worth custom exceptions i.e. https://docs.microsoft.com/en-us/dotnet/standard/exceptions/how-to-create-user-defined-exceptions 
                }
            }

            _displayController = new Pcf8574(_i2cDevice);
            //using LcdInterface _interface = new Pcf8574(_i2cDevice);
            //var lcd = new Lcd1602(registerSelectPin: 0, enablePin: 2, dataPins: new int[] { 4, 5, 6, 7 }, backlightPin: 3, readWritePin: 1, controller: new GpioController((PinNumberingScheme.Logical, _displayController));
            //using Hd44780 lcd = new Lcd1602(_i2cDevice);
            //lcd.BacklightOn = true;

            //put the LCD into 4 bit mode (guessing I2C here?!)
            // we start in 8bit mode, try to set 4 bit mode
            Write4bits(0x03 << 4);
            Thread.SpinWait(5); // wait min 4.1ms
            Write4bits(0x03 << 4);
            Thread.SpinWait(5); // wait min 4.1ms
            Write4bits(0x03 << 4);
            Thread.Sleep(150); //min 150 ms
            // finally, set to 4-bit interface
            Write4bits(0x02 << 4);

            // set # lines, font size, etc.
            SendCommand((byte)LcdInstruction.FunctionSet | (byte)LcdFuctionSet.TwoLines ); // Default font5x8, 4bit interface

            // turn the display on with no cursor or blinking default
            //turning it on does not turn on the backlight
            SendCommand((byte)LcdInstruction.DisplayMode | (byte)LcdDisplayMode.EntireDisplayOn); //Default no cursor, no cursor blink

            // Initialize to default text direction (for roman languages)
            SendCommand((byte)LcdInstruction.EntryModeSet | (byte)LcdEntryMode.MoveRightShiftLeft); // Default Shift display off

            Clear();
        }


        public void Clear()
        {
            SendCommand((byte)LcdInstruction.ClearDisplay);// clear display, set cursor position to zero
            Thread.SpinWait(2);  // command needs time!
        }


        public void SetCursorPosition(byte col, byte row)
        {
            int[] row_offsets = new[] { 0x00, 0x40, 0x14, 0x54 };
            if (row > 1)
                row = 0;//default to first row

            SendCommand((byte)((byte)LcdInstruction.SetDDRAMAddress | (col + row_offsets[row])));
        }


        public void Update(string line1, string line2)
        {
            while (line1.Length < 16)
            {
                line1 = $"{line1} "; //Add spaces char to replace previous chars that might have been there
            }
            while (line2.Length < 16)
            {
                line2 = $"{line2} "; //Add spaces char to replace previous chars that might have been there
            }
            Update($"{line1.Substring(0, line1.Length)}{line2.Substring(0, line2.Length)}");
        }

        public void Update(string text)
        {
            int lines = text.Length > 16 ? 2 : 1;
            if (lines < 2)
            {
                foreach (byte b in Encoding.UTF8.GetBytes(text))
                {
                    Send(b, RegSelectBit);
                }
            }
            else
            {
                for (byte i = 0; i < lines; i++)
                {
                    SetCursorPosition(0, i);

                    if (i == 0)
                    {
                        foreach (byte b in Encoding.UTF8.GetBytes(text.Substring(0, 16)))
                        {
                            Send(b, RegSelectBit);
                        }
                    }
                    else
                    {
                        foreach (byte b in Encoding.UTF8.GetBytes(text.Substring(16)))
                        {
                            Send(b, RegSelectBit);
                        }
                    }
                }
            }
        }


        public void RefreshText(string text)
        {
            Clear();
            Update(text);
        }


        void SendCommand(byte value)
        {
            Send(value, 0); //mode 0 is for command
        }


        void SendData(byte value)
        {
            Send(value, RegSelectBit);
        }

        void Send(byte value, byte mode) //, bool isData = false)
        {
            byte highnib = (byte)(value & 0xf0);
            byte lownib = (byte)((value << (byte)4) & 0xf0);
            Write4bits((byte)(highnib | mode | backlightval));
            Write4bits((byte)(lownib | mode | backlightval));
            Thread.SpinWait(1);
        }

        void Write4bits(byte value)
        {
            WriteByte(value);
            PulseEnable(value);
        }

        void PulseEnable(byte data)
        {
            WriteByte((byte)(data | EnableBit | backlightval));  // En high
            Thread.SpinWait(1);       // enable pulse must be >450ns
            WriteByte((byte)(data & ~EnableBit | backlightval)); // En low
            Thread.SpinWait(1);      // commands need > 37us to settle
        }

        void WriteByte(byte dat)
        {
            try
            {
                _displayController.WriteByte(dat);
            }
            catch
            {
                Debug.WriteLine("Error writing lcdController data!");
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
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            Clear();
            BacklightOn = false;
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

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~CharacterDisplay()
        {
            Dispose(false);
        }

    }
}