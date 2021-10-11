//using System;
//using Windows.Devices.I2c;

//namespace OrgPal.Three
//{
//    /// <summary>
//    /// Driver for RTC PCF85263 with key functions implemented for time, alarm and some system functions.
//    /// </summary>
//    public class PCF85263 : IDisposable
//    {
//        private I2cDevice I2C;
//        byte ADDRESS = 0x51;
//        byte REG_SECONDS = 0x01;
//        byte REG_ALARM_ENABLE = 0x10;
//        byte REG_OFFSET = 0x24; // tuning
//        byte REG_OSC_25 = 0x25; // oscillator register
//        byte REG_BATT_26 = 0x26; // battery switch
//        byte REG_IO_27 = 0x27; // Pin IO register
//        byte REG_FUNC_28 = 0x28; // function register
//        byte REG_INTA_ENABLES = 0x29; //interrupt control register
//        byte REG_FLAGS_2B = 0x2B; // flag status register
//        byte REG_STOP_2E = 0x2E; // stop enable
//        byte REG_RESET_2F = 0x2F; // software reset control

//        byte OS_BIT = 7;
//        byte CLKPM_BIT_7 = 7;
//        byte INTA_OUT_BIT_1 = 1;

//        byte ALARM1_SECONDS_BIT = 0;
//        byte ALARM1_MINUTES_BIT = 1;
//        byte ALARM1_HOURS_BIT = 2;
//        byte ALARM1_DAYS_BIT = 3;
//        byte ALARM1_MONTHS_BIT = 4;
//        byte ALARM1_AF1_FLAG_BIT5 = 5;
//        byte ALARM1_SECONDS_08 = 0x08;
//        byte ALARM1_MINUTES_09 = 0x09;
//        byte ALARM2_MINUTES = 0x0D;
//        byte INTA_A1IEA_BIT_4 = 4;
//        byte INTA_ILPA_BIT_7 = 7;
//        byte INTA_PIEA_BIT = 6;
//        byte INTA_PI_SECOND_BIT = 5;
//        byte INTA_PI_MINUTE_BIT = 6;
//        byte OSC_HIGH_CAPACITANCE_BIT = 1;
//        byte OSC_LOW_CAPACITANCE_BIT = 0;

//        public PCF85263()
//        {
//            var settings = new I2cConnectionSettings(ADDRESS)
//            {
//                BusSpeed = I2cBusSpeed.FastMode,
//                SharingMode = I2cSharingMode.Shared
//            };

//            I2C = I2cDevice.FromId(PalThreePins.I2cBus.I2C3, settings);

//           //DisableClockOutEnableInterruptA();
//           //DisableBatterySwitch();
//        }

//        public void Dispose()
//        {
//            if (I2C != null)
//                I2C.Dispose();
//        }

//        public DateTime GetDateTime()
//        {
//            WriteByte(REG_SECONDS);

//            byte[] dtNow = new byte[7];
//            I2C.Read(dtNow);

//            byte ss = PalHelper.Bcd2Bin(dtNow[0].ClearBit(7));//do not use last bit is for OS
//            byte mm = PalHelper.Bcd2Bin(dtNow[1].ClearBit(7));//do not use last bit is for EMON
//            byte hh = PalHelper.Bcd2Bin(dtNow[2]);
//            byte d = PalHelper.Bcd2Bin(dtNow[3]);
//            byte wd = dtNow[4];// skip weekdays
//            byte m = PalHelper.Bcd2Bin(dtNow[5]);
//            int y = PalHelper.Bcd2Bin(dtNow[6]) + 2000;

//            if (m == 0 || d == 0)
//                return DateTime.MinValue;
//            else
//                return new DateTime(y, m, d, hh, mm, ss);
//        }


//        public void SetDateTime(DateTime dt)
//        {
//            byte[] sb = new byte[8] { REG_SECONDS,  // start at location 1 for seconds
//                                   PalHelper.DecToBcd(dt.Second),
//                                   PalHelper.DecToBcd(dt.Minute),
//                                   PalHelper.DecToBcd(dt.Hour),
//                                   PalHelper.DecToBcd(dt.Day),
//                                   PalHelper.DecToBcd(0), // skip weekdays
//                                   PalHelper.DecToBcd(dt.Month),
//                                   PalHelper.DecToBcd(dt.Year - 2000)
//                                   };
//            I2C.Write(sb);
//        }


//        public void DisableClockOutEnableInterruptA()
//        {
//            // Disable CLK and enable interrupts on CLK/INTA pin
//            SetRegisterBit(REG_IO_27, CLKPM_BIT_7);
//            SetRegisterBit(REG_IO_27, INTA_OUT_BIT_1);
//        }


//        /// <summary>
//        /// Set alarm for seconds/min/hour/day/month, a full calandar like alarm
//        /// </summary>
//        /// <param name="alarmDateTime">Date and time to set the alarm, must be in teh future from present RTC time.</param>
//        /// <returns>True if alarm set, False if the alarm date is in the past from the RTC time.</returns>
//        public bool SetAlarm(DateTime alarmDateTime)
//        {
//            if (alarmDateTime < GetDateTime())
//                return false;

//            ClearAlarmInterrupt();

//            Debug.WriteLine("Alarm set: " + alarmDateTime.ToString());

//            byte[] sb = new byte[6] { ALARM1_SECONDS_08,  // start at location 1 for alarm seconds
//                                   PalHelper.DecToBcd(alarmDateTime.Second),
//                                   PalHelper.DecToBcd(alarmDateTime.Minute),
//                                   PalHelper.DecToBcd(alarmDateTime.Hour),
//                                   PalHelper.DecToBcd(alarmDateTime.Day),
//                                   PalHelper.DecToBcd(alarmDateTime.Month)
//                                   };

//            I2C.Write(sb);

//            EnableAlarm();

//            return true;
//        }

//        /// <summary>
//        /// Returns the alarm if any, for month, day, hour, min seconds. The year part is just picked to be current year but
//        /// alarm will fire every time for that month and the date/time.
//        /// </summary>
//        /// <returns>Date and time of alarm if set, or DateTime.MinValue if alarm is not set.</returns>
//        public DateTime GetAlarm()
//        {
//            WriteByte(ALARM1_SECONDS_08);

//            byte[] dtNow = new byte[6];
//            I2C.Read(dtNow);
//            byte ss = PalHelper.Bcd2Bin(dtNow[0]);
//            byte mm = PalHelper.Bcd2Bin(dtNow[1]);
//            byte hh = PalHelper.Bcd2Bin(dtNow[2]);
//            byte d = PalHelper.Bcd2Bin(dtNow[3]);
//            byte m = PalHelper.Bcd2Bin(dtNow[4]);
//            int y = PalHelper.Bcd2Bin(dtNow[5]) + 2000;

//            try
//            {
//                if (m == 0 || d == 0)
//                    return DateTime.MinValue;
//                else
//                    return new DateTime(DateTime.UtcNow.Year, m, d, hh, mm, ss);
//            }
//            catch {
//                return DateTime.MinValue;
//            }
//        }

//        /// <summary>
//        /// Enables the alarm, and the interrupts for the Alarm 1, for month/day/hour/min/sec always.
//        /// </summary>
//        void EnableAlarm()
//        {
//            DisableClockOutEnableInterruptA();

//            // mode = (1 << ALARM1_MINUTES_BIT) | (1 << ALARM1_HOURS_BIT) enables hour/minute of alarm1
//            byte mode = (byte)((1 << ALARM1_SECONDS_BIT) | (1 << ALARM1_MINUTES_BIT) | (1 << ALARM1_HOURS_BIT) |
//                (1 << ALARM1_DAYS_BIT) | (1 << ALARM1_MONTHS_BIT));

//            WriteRegister(REG_ALARM_ENABLE, mode);

//            SetRegisterBit(REG_INTA_ENABLES, INTA_A1IEA_BIT_4);
//            //set interrupt to permanent low
//            SetRegisterBit(REG_INTA_ENABLES, INTA_ILPA_BIT_7);
//        }


//        /// <summary>
//        /// Clears the alarm flag bit 5 and also disables clockout and enables interrupt for Alarm.
//        /// </summary>
//        public void ClearAlarmInterrupt()
//        {
//            ClearRegisterBit(REG_FLAGS_2B, ALARM1_AF1_FLAG_BIT5);
//            DisableClockOutEnableInterruptA();
//        }

//        /// <summary>
//        /// Checks if the alarm flag bit 5 is set  indicating there is active alarm.
//        /// </summary>
//        /// <returns>True if bit 5 of flags redister is set otherwise false.</returns>
//        public bool IsAlarmActive()
//        {
//            byte tmp = ReadRegister(REG_FLAGS_2B);
//            return tmp.IsBitSet(ALARM1_AF1_FLAG_BIT5);
//        }


//        void SetPeriodicInterrupt(bool forSeconds = true)
//        {
//            if (forSeconds)
//            {
//                SetRegisterBit(REG_FUNC_28, INTA_PI_SECOND_BIT);
//            }
//            else
//            {
//                SetRegisterBit(REG_FUNC_28, INTA_PI_MINUTE_BIT);
//                Debug.WriteLine("Periodit INT per minute set: ");
//            }
//        }

//        /// <summary>
//        /// Enables period interrupt on the interrupt pin
//        /// </summary>
//        void EnablePeriodicInterrupt()
//        {
//            DisableClockOutEnableInterruptA();
//            SetRegisterBit(REG_INTA_ENABLES, INTA_PIEA_BIT);
//        }

//        void DisablePeriodicInterrupt()
//        {
//            ClearRegisterBit(REG_INTA_ENABLES, INTA_PIEA_BIT);
//        }

//        //void SetHourMode(byte hour)
//        //{
//        //    //DateTime time = getTime();
//        //    //DateTime then = Date.now();
//        //    StopClock();
//        //    // bit 5 = 12/24 hour mode, 1 = 12 hour mode 0 = 24 hour mode
//        //    byte mode = ReadRegister(REG_OSC_25);
//        //    switch (hour)
//        //    {
//        //        case 12:
//        //            mode |= 0x20;
//        //            break;

//        //        case 24:
//        //            mode &= 0xDF;
//        //            break;

//        //        default:
//        //            return;
//        //    }
//        //    WriteRegister(REG_OSC_25, mode);
//        //    //time += Date.now() - then; // adjust for lost time
//        //    //SetTime(time);
//        //}


//        public byte GetHourMode()
//        {
//            byte mode = ReadRegister(REG_OSC_25);
//            mode &= 0x20;

//            if (mode != 0)
//                return 12;
//            else
//                return 24;
//        }


//        public void EnableHundredths()
//        {
//            byte temp = ReadRegister(REG_FUNC_28);
//            // bit 7 on
//            temp |= 0x80;
//            WriteRegister(REG_FUNC_28, temp);
//        }

//        public void DisableHundredths()
//        {
//            byte temp = ReadRegister(REG_FUNC_28);
//            // bit 7 off
//            temp &= 0x7F;
//            WriteRegister(REG_FUNC_28, temp);
//        }

//        // set drive control for quartz series resistance, ESR, motional resistance, or Rs
//        // 100k ohms = 'normal', 60k ohms = 'low', 500k ohms = 'high'
//        void SetDriveControl(string drive)
//        {
//            // bit 3 - 2 = drive control, 00 = normal Rs 100k, 01 = low drive Rs 60k, 10 and 11 = high drive Rs 500k
//            byte bits = 0x00;
//            switch (drive)
//            {
//                case "low":
//                    bits = 0x04;
//                    break;

//                case "normal":
//                    bits = 0x00;
//                    break;

//                case "high":
//                    bits = 0x08;
//                    break;

//                default:
//                    throw new Exception("Invalid drive mode " + drive);
//            }
//            byte tmp = ReadRegister(REG_OSC_25);
//            tmp |= bits;
//            WriteRegister(REG_OSC_25, tmp);
//        }

//        // set load capacitance for quartz crystal in pF, valid values are 6, 7, or 12.5
//        void SetLoadCapacitance(float capacitance)
//        {
//            // bit 1 - 0 = load capacitance, 00 = 7.0pF, 01 = 6.0pF, 10 and 11 = 12.5pF
//            byte bits = 0x00;
//            switch (capacitance)
//            {
//                case 7:
//                    bits = 0x00;
//                    break;

//                case 6:
//                    bits = 0x01;
//                    break;

//                case 12.5:
//                    bits = 0x02;
//                    break;

//                default:
//                    throw new Exception("Invalid load capacitance.");
//            }
//            byte tmp = ReadRegister(REG_OSC_25);
//            tmp |= bits;
//            WriteRegister(REG_OSC_25, tmp);

//        }


//        // enable the battery switch circuitry
//        void EnableBatterySwitch()
//        {
//            byte tmp = ReadRegister(REG_BATT_26);
//            tmp &= 0x0F;
//            WriteRegister(REG_BATT_26, tmp);
//        }

//        // disable the battery switch circuitry
//        void DisableBatterySwitch()
//        {
//            byte tmp = ReadRegister(REG_BATT_26);
//            tmp |= 0x10;
//            WriteRegister(REG_BATT_26, tmp);
//        }

//        void StartClock()
//        {
//            WriteRegister(REG_STOP_2E, 0);
//        }

//        void ResetClock()
//        {
//            WriteRegister(REG_RESET_2F, 0x2c);
//        }

//        void SetRegisterBit(byte reg, byte bitNum)
//        {
//            byte ss = ReadRegister(reg);
//            ss = ss.SetBit(bitNum);
//            WriteRegister(reg, ss);
//        }

//        void ClearRegisterBit(byte reg, byte bitNum)
//        {
//            byte ss = ReadRegister(reg);
//            ss = ss.ClearBit(bitNum);
//            WriteRegister(reg, ss);
//        }

//        // mode = 0 for normal 7pF, 1 for low 6pF, 2 for high 12.5 pF
//        void SetOscillatorCapacitance(byte capVal = 0)
//        {
//            WriteRegister(REG_OSC_25, 0x2B);

//            //switch (capVal)
//            //{
//            //    case 0:// 7 pF
//            //    default:
//            //        ClearRegisterBit(REG_OSC_25, OSC_HIGH_CAPACITANCE_BIT);
//            //        ClearRegisterBit(REG_OSC_25, OSC_LOW_CAPACITANCE_BIT);
//            //        break;
//            //    case 1: //low 6 pF
//            //        ClearRegisterBit(REG_OSC_25, OSC_HIGH_CAPACITANCE_BIT);
//            //        SetRegisterBit(REG_OSC_25, OSC_LOW_CAPACITANCE_BIT);
//            //        break;
//            //    case 2: //high 12.5 pF
//            //        SetRegisterBit(REG_OSC_25, OSC_HIGH_CAPACITANCE_BIT);
//            //        ClearRegisterBit(REG_OSC_25, OSC_LOW_CAPACITANCE_BIT);
//            //        break;
//            //}
//        }

//        void ClearAllFlags()
//        {
//            WriteRegister(REG_FLAGS_2B, 0x00);
//        }

//        public void StopClock()
//        {
//            WriteRegister(REG_STOP_2E, 1);
//        }

//        byte ReadRegister(byte addrByte)
//        {
//            WriteByte(addrByte);
//            return ReadByte();
//        }

//        void WriteRegister(byte reg, byte cmd)
//        {
//            I2C.Write(new byte[] { reg, cmd });
//        }

//        void WriteByte(byte cmd)
//        {
//            I2C.Write(new byte[] { cmd });
//        }

//        byte ReadByte()
//        {
//            byte[] rb = new byte[1];
//            I2C.Read(rb);
//            return rb[0];
//        }

//        public void PrintAllRegisters()
//        {
//            for (byte i = 0; i < 48; i++)
//            {
//                PrintRegister(i);
//            }
//        }

//        public void PrintRegister(byte regAdd)
//        {
//            byte reg = ReadRegister(regAdd);
//            Debug.WriteLine("REG " + regAdd.ToString("X") + " " + PalHelper.ByteToBitsString(reg));
//        }

//    }

//}
