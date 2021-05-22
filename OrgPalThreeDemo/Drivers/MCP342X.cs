using System;
using System.Diagnostics;
using System.Threading;
using System.Device.I2c;

namespace PalThree
{
    /// <summary>
    /// A Class to manage the MicroChip MCP342x, 18-Bit Multi-Channel delta-sigma ADC series with
    /// I²C Interface and On-Board Reference
    /// </summary>
    /// <remarks>
    /// Can be used with MCP3422/3/4/6/7/8.
    /// </remarks>
    public class MCP342x : IDisposable
    {
        private bool disposed = false;
        const ushort TIME_TO_LEAVE_STANDBY_MODE = 200;

        /// <summary>
        /// Resolution Selection affects convertion time (more bits, slower conversion)
        /// </summary>
        public enum MCP342xResolution : byte
    {
        /// <summary>
        /// 240 SPS (By default on power-up)
        /// </summary>
        TwelveBits,
        /// <summary>
        /// 60 SPS
        /// </summary>
        FourteenBits,
        /// <summary>
        /// 15 SPS
        /// </summary>
        SixteenBits,
        /// <summary>
        /// 3.75 SPS
        /// </summary>
        EighteenBits
    }

    /// <summary>
    /// Use One-Shot if very low power consuption is required
    /// </summary>
    public enum MCP342xConversionMode : byte
    {
        /// <summary>
        /// The device performs a single conversion and enters a low current standby mode automatically until it
        /// receives another conversion command. This reduces current consumption greatly during idle periods.
        /// </summary>
        OneShot,
        /// <summary>
        /// The conversion takes place continuously at the set conversion speed. See Resolution. (By default on power-up)
        /// </summary>
        Continuous
    }
    /// <summary>
    /// MCP3422 and MCP3423 devices have two differential input channels and the MCP3424 has four differential input channels
    /// </summary>
    public enum MCP342xChannel : byte
    {
        /// <summary>
        /// MCP3422/3/4 (By default on power-up)
        /// </summary>
        One,
        /// <summary>
        /// MCP3422/3/4
        /// </summary>
        Two,
        /// <summary>
        /// MCP3424
        /// </summary>
        Three,
        /// <summary>
        /// MCP3424
        /// </summary>
        Four
    }
    /// <summary>
    /// Selects the gain factor for the input signal before the analog-to-digital conversion takes place
    /// </summary>
    public enum MCP342xPGA_Gain : byte
    {
        /// <summary>
        /// By default on power-up Gain = x1
        /// </summary>
        One,
        Two,
        Four,
        Eight
    }

        I2cDevice i2cBus = null;

        readonly ushort address;

        // By default on power-up
        private MCP342xPGA_Gain gain = MCP342xPGA_Gain.One;
        private MCP342xChannel channel = MCP342xChannel.One;
        private MCP342xConversionMode conversionMode = MCP342xConversionMode.Continuous; // default
        private MCP342xResolution resolution;

        private bool isConfigRegisterOk = true;
        private bool isEndOfConversion = false;
        private bool hasSampleError = false;
        private readonly float[] lsbValues = new float[] { 0.001f, 0.00025f, 0.0000625f, 0.000015625f };
        private readonly int[] conversionTimeMilliSecs = new int[] { 5, 20, 70, 270 };
        private float lsbVolts = 0.0000625f; // resolution = 16-bit 
        private float gainDivisor = 1.0f;
        private int maxADCValue;
        //float ADCResolution = 65536.0f;//16 bit
        //float vRef = 2.048f;//2.048f for the default internal reference

        // Used to read raw data
        //int dataMask = 0x0fff; // resolution = 12-bit 
        int dataMask = (1 << (12 + (int)MCP342xResolution.SixteenBits * 2)) - 1; //resolution 16-bits
        Timer timer = null;

        public MCP342x(int I2CId = PalThreePins.I2cBus.I2C2, ushort i2cAddress = 0x68)
        {
            address = i2cAddress;

            Resolution = MCP342xResolution.SixteenBits;

            var settings = new I2cConnectionSettings(I2CId, address, I2cBusSpeed.StandardMode);// the slave's address

            // Adress Options 0x68, 0x6A, 0x6C or 0x6E
            //var i2cList = PalHelper.FindDevices(I2CId, 0x68, 0x6E);
            //if (i2cList.Count == 1)
            //    settings.SlaveAddress = (byte)i2cList[0];

            i2cBus = I2cDevice.Create(settings);

            TemperatureCoefficient = 1.250f; //not sure why this is better... 0c correct 30c 0.5c off...
            //TemperatureCoefficient = 0.385f;// 0.00385 Ohms/Ohm/ºC
        }

        public float NTC_A { get; set; }
        public float NTC_B { get; set; }
        public float NTC_C { get; set; }

        public float TemperatureCoefficient
        {
            get;
            set;
        }

        /// <summary>
        /// Write value to configuration register
        /// </summary>
        /// <param name="value">RDY C1 C0 /O_C S1 S0 G1 G0 = 00010000 (default)</param>
        public void ConfigDevice(byte value)
        {
            WriteConfRegister(value);
        }

        /// <summary>
        /// Get or Set Resolution Selection Bit
        /// </summary>
        /// <remarks>
        /// Get :  0 => 240 SPS (12 bits) (Default), 1 => 60 SPS (14 bits), 2 => 15 SPS (16 bits), 3 => 3.75 SPS (18 bits)
        /// </remarks>
        public MCP342xResolution Resolution
        {
            get
            {
                return resolution;
            }

            set
            {
                resolution = value;
                dataMask = (1 << (12 + (int)resolution * 2)) - 1;
                maxADCValue = 1 << (11 + (int)resolution * 2);
                lsbVolts = lsbValues[(ushort)resolution];
                isConfigRegisterOk = false;
            }
        }

        /// <summary>
        /// Get or Set PGA Gain Selection Bits
        /// </summary>
        /// <remarks>
        /// Get :  0 => x1 (Default), 1 => x2, 2 => x4, 3 => x8
        /// </remarks>
        public MCP342xPGA_Gain Gain
        {
            get
            {
                return gain;
            }
            set
            {
                gain = value;
                gainDivisor = ((float)Math.Pow(2.0, (float)gain));
                isConfigRegisterOk = false;
            }
        }


        /// <summary>
        /// Get or Set Channel Selection Bits
        /// </summary>
        /// <remarks>
        /// MCP3422 and MCP3423 : CH1 or CH2 - MCP3424 : CH1, CH2, CH3 or CH4
        /// </remarks>
        public MCP342xChannel Channel
        {
            get
            {
                return channel;
            }
            set
            {
                channel = value;
                isConfigRegisterOk = false;
            }
        }


        /// <summary>
        /// Get or Set Conversion Mode
        /// </summary>
        /// <remarks>
        /// One-Shot Conversion mode or Continuous Conversion mode
        /// </remarks>
        public MCP342xConversionMode Mode
        {
            get
            {
                return conversionMode;
            }
            set
            {
                conversionMode = value;
                isConfigRegisterOk = false;
            }
        }

        /// <summary>
        /// Gets the resistance value.
        /// </summary>
        /// <returns></returns>
        public float GetResistance()
        {
            return ReadVolts() * 1000;
        }


        public int GetChannelADCValue()
        {
            return ReadChannel();
        }


        public double GetTemperatureFromThermistorNTC1000()
        {
            Channel = MCP342xChannel.Two;//NTC Thermistor is only avaialble on CH2+
            Mode = MCP342xConversionMode.Continuous;

            //Vishay NTCALUG01A103F specs
            //B25/85-value  3435 to 4190 K 
            NTC_A = 0.0011415995549162692f;
            NTC_B = 0.000232134233116422f;
            NTC_C = 9.520031026040015e-8f;

            //calculate temperature from resistance
            double volts = 0;
            //do 3 readings just to make sure sampling is good
            for (int i = 0; i < 3; i++)
            {
                volts = ReadVolts();
                Thread.Sleep(100);
            }

            double thermistorResistance = volts * 10000f / (3.3f - volts);//3.3 V for thermistor, 10K resistor/thermistor
            double lnR = Math.Log(thermistorResistance);
            double Tk = 1 / (NTC_A + NTC_B * lnR + NTC_C * (lnR * lnR * lnR));
            double tempC = Tk - 273.15f;
            return tempC;

        }


        /// <summary>
        /// Gets the temperature value for a PT100 probe.
        /// </summary>
        /// <remarks>
        /// 3-wire PT100 connection on the PalSensors Rev B and later board
        /// Connect the single color wire to both  EX+ and SIG+  (bridge with cable or connector or jumper)
        /// Connect the 2 same color wires to the EX- and SIG- individually
        /// </remarks>
        /// <returns></returns>
        public double GetTemperatureFromPT100()
        {
            Channel = MCP342xChannel.One;//pt100 is only avaialble on CH1+/CH1-

            Mode = MCP342xConversionMode.Continuous;
            ReadChannel();

            Thread.Sleep(10); //let the ADC settle

            //float tempValue = 0;
            ////do 2-3 readings just to make sure sampling is good
            //for (int i = 0; i < 2; i++)
            //{
            double tempValue = GetTemperature() + TemperatureCoefficient;
                
            //}
            //put the IC in low power mode, thus set on OneShot and read to write config
            Mode = MCP342xConversionMode.OneShot;
            ReadChannel(); //ensure we are back in oneshot mode.

            return tempValue;
        }

        private double GetTemperature()
        {
            const float RTD_ALPHA = 3.9083e-3F;
            const float RTD_BETA = -5.775e-7F;
            const float a2 = 2.0F * RTD_BETA;
            const float bSq = RTD_ALPHA * RTD_ALPHA;

            float c = 1.0F - GetResistance() / (int)100; //100 for PT100, 1000 for PT1000
            float d = bSq - 2.0F * a2 * c;
            return (-RTD_ALPHA + Math.Sqrt(d)) / a2;
        }


        private int ReadChannel()
        {
            float dataRaw = 0;
            byte value = 0x10;

            if (!isConfigRegisterOk)
            {
                value = (byte)((ushort)gain +
                                   ((ushort)resolution << 2) +
                                   ((ushort)conversionMode << 4) +
                                   ((ushort)channel << 5));
                ConfigDevice(value);
                isConfigRegisterOk = true;
            }

            if (conversionMode == MCP342xConversionMode.OneShot)
            {
                value |= 0x80; // Start a single conversion
                ConfigDevice(value);
                timer = new Timer(new TimerCallback(NoSample), null, TIME_TO_LEAVE_STANDBY_MODE, -1);
            }
            else
            {
                timer = new Timer(new TimerCallback(NoSample), null, conversionTimeMilliSecs[(ushort)resolution], -1);
            }

            if (resolution == MCP342xResolution.EighteenBits)
            {
                do
                {
                    dataRaw = DataReadRaw(4); // Read three data bytes plus the data register
                }
                while (!isEndOfConversion && !hasSampleError);
            }
            else
            {
                do
                {
                    dataRaw = DataReadRaw(3); // Read two data bytes plus the data register 
                } while (!isEndOfConversion && !hasSampleError);
            }

            if (hasSampleError)
            {
                timer.Dispose();
                hasSampleError = false;
                //throw new System.IO.IOException("No sample on " + _sla);
            }
            timer.Dispose();
            isEndOfConversion = false;

            return (int)dataRaw;
        }

        /// <summary>
        /// Reads the selected input channel and converts to voltage units
        /// </summary>
        /// <returns>Voltage representated as a float</returns>
        public float ReadVolts()
        {
            float adcCount = ReadChannel();

            Debug.WriteLine("rawADC: " + adcCount.ToString() + " GainDiv: " + gainDivisor);
            if (adcCount > maxADCValue)
                adcCount -= maxADCValue * 2;

            float volts = (adcCount * lsbVolts) / gainDivisor;

            if (channel == MCP342xChannel.Three)
            {
                volts *= 2.5f;
                if (volts > 3.9)
                    volts += 0.085f;//add offset for board
            }
            else if (channel == MCP342xChannel.Four)
            {
                volts *= 25;
                if (volts > 3.9)
                    volts += 0.085f;//add offset for board
            }

            return volts;
        }

        private void NoSample(object state)
        {
            hasSampleError = true;
        }

        /// <summary>
        /// Reads the raw counts from the ADC
        /// </summary>
        /// <param name="number">Number of bytes to read (four if 18-bit conversion else three) </param>
        /// <returns>Result of 12, 14, 16 or 18-bit conversion</returns>
        private float DataReadRaw(byte number)
        {
            byte[] inbuffer = new byte[number];
            i2cBus.Read(inbuffer);

            if ((inbuffer[number - 1] & 0x80) == 0) // End of conversion
            {
                isEndOfConversion = true;

                int data;
                if (resolution == MCP342xResolution.EighteenBits)
                    data = ((inbuffer[0]) << 16) + ((inbuffer[1]) << 8) + inbuffer[2];
                else
                    data = ((inbuffer[0]) << 8) + inbuffer[1];

                return data &= dataMask;
            }
            else
            {
                isEndOfConversion = false;
                return 0; // return something
            }
        }

        private void WriteConfRegister(byte value)
        {
            byte[] outbuffer = { value };
            i2cBus.Write(outbuffer);
        }


        public void Dispose()
        {

            if (!disposed)
                return;

            i2cBus.Dispose();
            i2cBus = null;
            disposed = true;
        }
    }
}
