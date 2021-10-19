using System;
using System.Text;
using System.Threading;
using System.Device.Gpio;
using System.Device.I2c;

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
        const byte LCD_ADDRESS_MAIN = 0x3F;
        const byte LCD_ADDRESS_DEFAULT = 0x27; // 0X27 on other models depnding of soldered a0,a1,a2


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
            /// DL: interface data is 8/4 bits
            /// </summary>
            /// <remarks>
            /// 0x08 = Number of lines is 2/1
            /// 0x10 = Font Size is 5x11/5x8
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
        /// Display entry mode
        /// </summary>
        [Flags]
        private enum LcdEntryMode : byte
        {
            ENTRYLEFT = 0x02,
            ENTRYSHIFTDECREMENT = 0x00,
        }

        /// <summary>
        /// display on/off control
        /// </summary>
        [Flags]
        private enum LcdCursor : byte
        {

            /// 0x00 = Entire display off???
            /// 0x01 = Cursor position on
            /// 0x02 = Cursor On
            /// 0x04 = Entire display on
            DISPLAYON = 0x04,
            CURSOROFF = 0x00,
            BLINKOFF = 0x00,
        }

        /// <summary>
        /// function set
        /// </summary>
        [Flags]
        private enum LcdFuctionSet : byte
        {
        _4BITMODE = 0x00,
        _2LINE = 0x08,
        _5x8DOTS = 0x00,
        }


        //TODO: I2C values for GPIO???
        // flags for backlight control
        const byte LCD_NOBACKLIGHT = 0x00;
        const byte LCD_BACKLIGHT = 0b00001000;  // Turn On Backlight
        const byte En = 0b00000100;  // Enable bit
        //const byte Rw = 0b00000010;  // Read/Write bit
        const byte Rs = 0b00000001;  // Register select bit

        private readonly I2cDevice I2C;
        private readonly I2cConnectionSettings config;
        private byte backlightval = LCD_NOBACKLIGHT;
        private GpioPin lcdPowerOnOff;


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


        public CharacterDisplay(int I2CId = Pinout.I2cBus.I2C3, byte mainAddress = LCD_ADDRESS_MAIN)
        {
            //lcdPowerOnOff = PalHelper.GpioPort(PalThreePins.GpioPin.POWER_LCD_ON_OFF, PinMode.Output, PinValue.High);
            lcdPowerOnOff = new GpioController().OpenPin(Pinout.GpioPin.POWER_LCD_ON_OFF, PinMode.Output);
            lcdPowerOnOff.Write(PinValue.High);

            config = new I2cConnectionSettings(I2CId, mainAddress, I2cBusSpeed.FastMode);


            // Thread.Sleep(250);//small break to make the LCD startup better in some cases helps boot up without sensor

            I2C = I2cDevice.Create(config);

            var result = I2C.WriteByte(0); //Write command 0 and see if it is acknowledged.
            if (result.Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
            {
                I2C.Dispose();
                config = new I2cConnectionSettings(I2CId, LCD_ADDRESS_DEFAULT, I2cBusSpeed.FastMode); ;// the other default address
                I2C = I2cDevice.Create(config);

                result = I2C.WriteByte(0); //Write command 0 and see if it is acknowledged.
                if (result.Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
                {
                    throw new Exception("No LCD found");
                }
            }

            //put the LCD into 4 bit mode
            // we start in 8bit mode, try to set 4 bit mode
            Write4bits(0x03 << 4);
            Thread.Sleep(5); // wait min 4.1ms
            Write4bits(0x03 << 4);
            Thread.Sleep(5); // wait min 4.1ms
            Write4bits(0x03 << 4);
            Thread.Sleep(150); //min 150 ms
            // finally, set to 4-bit interface
            Write4bits(0x02 << 4);

            // set # lines, font size, etc.
            SendCommand((byte)LcdInstruction.FunctionSet | (byte)LcdFuctionSet._4BITMODE | (byte)LcdFuctionSet._2LINE | (byte)LcdFuctionSet._5x8DOTS);

            // turn the display on with no cursor or blinking default
            //turning it on does not turn on the backlight
            SendCommand((byte)LcdInstruction.DisplayMode | (byte)LcdCursor.DISPLAYON | (byte)LcdCursor.CURSOROFF | (byte)LcdCursor.BLINKOFF);

            // Initialize to default text direction (for roman languages)
            SendCommand((byte)LcdInstruction.EntryModeSet | (byte)LcdEntryMode.ENTRYLEFT | (byte)LcdEntryMode.ENTRYSHIFTDECREMENT);

            Clear();

            //LoadDefaultCustomChars();
        }


        public void Clear()
        {
            SendCommand((byte)LcdInstruction.ClearDisplay);// clear display, set cursor position to zero
            Thread.Sleep(250);  // command needs time! (but possibly less than this...)
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
                line1 += " ";
            }
            while (line2.Length < 16)
            {
                line2 += " ";
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
                    Send(b, Rs); //, true);
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
                            Send(b, Rs); //, true);
                        }
                    }
                    else
                    {
                        foreach (byte b in Encoding.UTF8.GetBytes(text.Substring(16)))
                        {
                            Send(b, Rs); //, true);
                        }
                    }
                }
            }
        }


        public void RefreshText(string text) //, byte line = 0)
        {
            this.Clear();
            Update(text); //, line);
        }


        void SendCommand(byte value)
        {
            Send(value, 0);//mode 0 is for command
        }


        void SendData(byte value)
        {
            Send(value, Rs);
        }

        void Send(byte value, byte mode) //, bool isData = false)
        {
            byte highnib = (byte)(value & 0xf0);
            byte lownib = (byte)((value << (byte)4) & 0xf0);
            Write4bits((byte)(highnib | mode | backlightval));
            Write4bits((byte)(lownib | mode | backlightval));
            Thread.Sleep(1);
        }

        void Write4bits(byte value)
        {
            WriteByte(value);
            PulseEnable(value);
        }

        void PulseEnable(byte data)
        {
            WriteByte((byte)(data | En | backlightval));  // En high
            Thread.Sleep(1);       // enable pulse must be >450ns
            WriteByte((byte)(data & ~En | backlightval)); // En low
            Thread.Sleep(1);      // commands need > 37us to settle
        }

        //void LoadCustomCharacter(byte char_num, byte[] rows)
        //{
        //    CreateCustomChar(char_num, rows);
        //}

        void WriteByte(byte dat)
        {
            try
            {
                I2C.Write(new byte[] { dat });
            }
            catch
            {

            }
        }


        public void DisposeI2C()
        {
            if (I2C != null)
                I2C.Dispose();
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
            if (I2C != null)
                I2C.Dispose();

            lcdPowerOnOff.Write(PinValue.Low);
            lcdPowerOnOff.Dispose();
            lcdPowerOnOff = null;

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