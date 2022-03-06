using System;
using System.Text;
using System.Threading;
using System.Device.Gpio;
using System.Device.I2c;
//using System.Diagnostics;

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
        const byte I2C_LCD_ADDRESS_DEFAULT = 0x27; // 0X27 on other models depending of soldered a0,a1,a2


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

        private I2cDevice _displayViaI2C;
        private readonly I2cConnectionSettings config;

        private byte backlightval = LCD_NOBACKLIGHT;
        private GpioPin _lcdPowerPin;


        public bool PowerState
        {
            get { return backlightval == LCD_BACKLIGHT; }

            set
            {
                if (value)//turn it on
                {
                    _lcdPowerPin.Write(PinValue.High);
                    backlightval = LCD_BACKLIGHT;
                }
                else //turn it off
                {
                    _lcdPowerPin.Write(PinValue.Low);
                    backlightval = LCD_NOBACKLIGHT;
                }

                WriteByte(0); //TODO: what is this for?!
            }
        }


        public CharacterDisplay(int busId = Pinout.I2cBus.I2C3, byte deviceAddress = I2C_LCD_ADDRESS_MAIN)
        {
            _lcdPowerPin = new GpioController().OpenPin(Pinout.GpioPin.POWER_LCD_ON_OFF, PinMode.Output);
            _lcdPowerPin.Write(PinValue.High); // on by default!

            config = new I2cConnectionSettings(busId, deviceAddress, I2cBusSpeed.FastMode);


            // Thread.Sleep(250);//small break to make the LCD startup better in some cases helps boot up without sensor

            _displayViaI2C = I2cDevice.Create(config);

            var result = _displayViaI2C.WriteByte(0); //Write command 0 and see if it is acknowledged.
            if (result.Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
            {
                _displayViaI2C.Dispose();
                config = new I2cConnectionSettings(busId, I2C_LCD_ADDRESS_DEFAULT, I2cBusSpeed.FastMode); ;// the other default address
                _displayViaI2C = I2cDevice.Create(config); // the other default address);

                result = _displayViaI2C.WriteByte(0); //Write command 0 and see if it is acknowledged.
                if (result.Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
                {
                    throw new Exception("Character Display Not Found!");
                }
            }

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

            //LoadDefaultCustomChars();
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


        public void CreateCustomChar(byte location, byte[] charmap)
        {
            //Fill the first 8 CGRAM with custom characters
            location &= 0x7;
            SendCommand((byte)((byte)LcdInstruction.SetCGRAMAddress | (location << 3)));
            for (int i = 0; i < 7; i++)
            {
                SendData(charmap[i]);
            }
        }


        public void Update(string line1, string line2)
        {
            while (line1.Length < 16)
            {
                line1 += " "; //Remove previous chars that might have been there
            }
            while (line2.Length < 16)
            {
                line2 += " "; //Remove previous chars that might have been there
            }
            Update(line1 + line2);
        }

        public void Update(string text) //, byte line = 0) //TODO: does not handle empty lines or CRLF!!
        {
            int lines = text.Length > 16 ? 2 : 1;
            if (lines < 2)
            {
                foreach (byte b in Encoding.UTF8.GetBytes(text))
                {
                    Send(b, RegSelectBit); //, true);
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
                            Send(b, RegSelectBit); //, true);
                        }
                    }
                    else
                    {
                        foreach (byte b in Encoding.UTF8.GetBytes(text.Substring(16)))
                        {
                            Send(b, RegSelectBit); //, true);
                        }
                    }
                }
            }
        }


        public void RefreshText(string text) //, byte line = 0)
        {
            Clear();
            Update(text); //, line);
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

        //void LoadCustomCharacter(byte char_num, byte[] rows)
        //{
        //    CreateCustomChar(char_num, rows);
        //}

        void WriteByte(byte dat)
        {
            try
            {
                _displayViaI2C.Write(new byte[] { dat });
            }
            catch
            {
                //Debug.WriteLine("Error writing I2C data!");
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
            PowerState = false;
            _lcdPowerPin.Write(PinValue.Low);

            if (_lcdPowerPin != null)
            {
                _lcdPowerPin.Dispose();
                _lcdPowerPin = null;
            }

            if (_displayViaI2C != null)
                _displayViaI2C.Dispose();
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~CharacterDisplay()
        {
            Dispose(false);
        }

        //public void PrintChar(byte custChar)
        //{
        //    SendData(3);
        //}

        //void LoadDefaultCustomChars()
        //{
        //    byte[] bell = { 0x4, 0xe, 0xe, 0xe, 0x1f, 0x0, 0x4 };
        //    byte[] note = { 0x2, 0x3, 0x2, 0xe, 0x1e, 0xc, 0x0 };
        //    byte[] clock = { 0x0, 0xe, 0x15, 0x17, 0x11, 0xe, 0x0 };
        //    byte[] heart = { 0x0, 0xa, 0x1f, 0x1f, 0xe, 0x4, 0x0 };
        //    byte[] duck = { 0x0, 0xc, 0x1d, 0xf, 0xf, 0x6, 0x0 };
        //    byte[] check = { 0x0, 0x1, 0x3, 0x16, 0x1c, 0x8, 0x0 };
        //    byte[] cross = { 0x0, 0x1b, 0xe, 0x4, 0xe, 0x1b, 0x0 };
        //    byte[] retarrow = { 0x1, 0x1, 0x5, 0x9, 0x1f, 0x8, 0x4 };

        //    CreateCustomChar(0, bell);
        //    CreateCustomChar(1, note);
        //    CreateCustomChar(2, clock);
        //    CreateCustomChar(3, heart);
        //    CreateCustomChar(4, duck);
        //    CreateCustomChar(5, check);
        //    CreateCustomChar(6, cross);
        //    CreateCustomChar(7, retarrow);
        //}
    }
}