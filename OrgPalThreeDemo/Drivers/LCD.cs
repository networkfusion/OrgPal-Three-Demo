// Adapted from Micro Liquid Crystal Library http://microliquidcrystal.codeplex.com

using System;
using System.Text;
using System.Threading;
using System.Device.Gpio;
using Windows.Devices.I2c;

namespace PalThree
{

    public class LCD : IDisposable
    {
        //For PCF8574T chip, I2C address range: 0x20-0x27  (Dec:    32-38)
        // For PCF8574 chip, I2C address range: 0x38 - 0x3F (Dec:   56-63)

        const byte LCD_ADDRESS = 0x3F;// 0X27 on other models depnding of soldered a0,a1,a2

        // commands
        const byte LCD_CLEARDISPLAY = 0x01;
        const byte LCD_ENTRYMODESET = 0x04;
        const byte LCD_DISPLAYCONTROL = 0x08;
        const byte LCD_FUNCTIONSET = 0x20;
        const byte LCD_SETCGRAMADDR = 0x40;
        const byte LCD_SETDDRAMADDR = 0x80;

        // flags for display entry mode
        const byte LCD_ENTRYLEFT = 0x02;
        const byte LCD_ENTRYSHIFTDECREMENT = 0x00;

        // flags for display on/off control
        const byte LCD_DISPLAYON = 0x04;
        const byte LCD_CURSOROFF = 0x00;
        const byte LCD_BLINKOFF = 0x00;

        // flags for function set
        const byte LCD_4BITMODE = 0x00;
        const byte LCD_2LINE = 0x08;
        const byte LCD_5x8DOTS = 0x00;

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


        public LCD(string I2CId = PalThreePins.I2cBus.I2C3)
        {
            lcdPowerOnOff = PalHelper.GpioPort(PalThreePins.GpioPin.POWER_LCD_ON_OFF, PinMode.Output, PinValue.High);

            config = new I2cConnectionSettings(LCD_ADDRESS)// the slave's address
            {
                BusSpeed = I2cBusSpeed.FastMode,
                SharingMode = I2cSharingMode.Shared
            };

            // For PCF8574T chip, I2C address range: 0x20-0x27  (Dec:    32-38)
            // For PCF8574 chip, I2C address range: 0x38 - 0x3F (Dec:   56-63)
            //var i2cList = PalHelper.FindDevices(I2CId, 0x20, 0x3F);
            //if (i2cList.Count == 1)
            //    config.SlaveAddress = (byte)i2cList[0];

            // Thread.Sleep(250);//small break to make the LCD startup better in some cases helps boot up without sensor

            I2C = I2cDevice.FromId(I2CId, config);

            byte[] cmdBuffer = new byte[1] { 0 };
            var result = I2C.WritePartial(cmdBuffer);
            if (result.Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
            {
                I2C.Dispose();
                config.SlaveAddress = 0x27;// the other default address
                I2C = I2cDevice.FromId(I2CId, config);
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
            SendCommand((byte)(LCD_FUNCTIONSET | LCD_4BITMODE | LCD_2LINE | LCD_5x8DOTS));

            // turn the display on with no cursor or blinking default
            //turning it on does not turn on the backlight
            SendCommand((byte)(LCD_DISPLAYCONTROL | LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF));

            // Initialize to default text direction (for roman languages)
            SendCommand((byte)(LCD_ENTRYMODESET | LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT));

            Clear();

            //LoadDefaultCustomChars();
        }


        public void Clear()
        {
            SendCommand(LCD_CLEARDISPLAY);// clear display, set cursor position to zero
            Thread.Sleep(5);  // command needs time!
        }


        public void SetCursorPosition(byte col, byte row)
        {
            int[] row_offsets = new[] { 0x00, 0x40, 0x14, 0x54 };
            if (row > 1)
                row = 0;//default to first row

            SendCommand((byte)(LCD_SETDDRAMADDR | (col + row_offsets[row])));
        }


        public void CreateCustomChar(byte location, byte[] charmap)
        {
            //Fill the first 8 CGRAM with custom characters
            location &= 0x7;
            SendCommand((byte)(LCD_SETCGRAMADDR | (location << 3)));
            for (int i = 0; i < 7; i++)
            {
                SendData(charmap[i]);
            }
        }


        public void Display(string text, byte line = 0)
        {
            int lines = text.Length > 16 ? 2 : 1;
            if (lines < 2)
            {
                foreach (byte b in Encoding.UTF8.GetBytes(text))
                {
                    Send(b, Rs, true);
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
                            Send(b, Rs, true);
                        }
                    }
                    else
                    {
                        foreach (byte b in Encoding.UTF8.GetBytes(text.Substring(16)))
                        {
                            Send(b, Rs, true);
                        }
                    }
                }
            }
        }


        public void ShowText(string text, byte line = 0)
        {
            this.Clear();
            Display(text, line);
        }


        void SendCommand(byte value)
        {
            Send(value, 0);//mode 0 is for command
        }


        void SendData(byte value)
        {
            Send(value, Rs);
        }

        void Send(byte value, byte mode, bool isData = false)
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

        void LoadCustomCharacter(byte char_num, byte[] rows)
        {
            CreateCustomChar(char_num, rows);
        }

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


        public void Dispose()
        {
            if (I2C != null)
                I2C.Dispose();

            lcdPowerOnOff.Write(PinValue.Low);
            lcdPowerOnOff.Dispose();
            lcdPowerOnOff = null;
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