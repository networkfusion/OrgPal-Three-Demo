using System;
using System.Diagnostics;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Mcp3428;

namespace PalThree
{

    /// <summary>
    /// A Class to manage the Adc Pal sensors Expansion Board
    /// </summary>
    /// <remarks>
    /// Based on an MCP3428.
    /// </remarks>
    public class AdcExpansionBoard : IDisposable
    {
        private bool disposed = false;
        Mcp3428 sensor;

        //private readonly float[] lsbValues = new float[] { 0.001f, 0.00025f, 0.0000625f, 0.000015625f };
        //private readonly int[] conversionTimeMilliSecs = new int[] { 5, 20, 70, 270 };
        //private float lsbVolts = 0.0000625f; // resolution = 16-bit 
        //private float gainDivisor = 1.0f;
        //private int maxADCValue;

        public enum Channel
        {
            One = 0,
            Two = 1,
            Three = 2,
            Four = 3
        }

        public AdcExpansionBoard(int I2CId = PalThreePins.I2cBus.I2C2, ushort i2cAddress = 0x68)
        {
            // Adress Options 0x68, 0x6A, 0x6C or 0x6E
            
            I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(I2CId, i2cAddress));
            sensor = new Mcp3428(i2cDevice, AdcMode.OneShot, AdcResolution.Bit16, AdcGain.X1);

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
        /// Gets the resistance value.
        /// </summary>
        /// <returns></returns>
        public double GetResistance(Channel channel)
        {
            return ReadVolts(channel) * 1000;
        }


        public double GetTemperatureFromThermistorNTC1000()
        {
            var channel = Channel.Two; //NTC Thermistor is only avaialble on CH2+
            sensor.Mode = AdcMode.Continuous;

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
                Thread.Sleep(100);
                volts = ReadVolts(channel);

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
            var channel = Channel.One; //pt100 is only avaialble on CH1+/CH1

            sensor.Mode = AdcMode.Continuous;
            sensor.ReadChannel((int)channel);

            Thread.Sleep(100); //let the ADC settle

            //float tempValue = 0;
            ////do 2-3 readings just to make sure sampling is good
            //for (int i = 0; i < 2; i++)
            //{
            double tempValue = GetTemperature(channel) + TemperatureCoefficient;

            //}
            //put the IC in low power mode, thus set on OneShot and read to write config
            sensor.Mode = AdcMode.OneShot;
            sensor.ReadChannel((int)channel); //ensure we are back in oneshot mode.

            return tempValue;
        }

        private double GetTemperature(Channel channel)
        {
            const double RTD_ALPHA = 3.9083e-3F;
            const double RTD_BETA = -5.775e-7F;
            const double a2 = 2.0F * RTD_BETA;
            const double bSq = RTD_ALPHA * RTD_ALPHA;

            double c = 1.0F - GetResistance(channel) / 100; //100 for PT100, 1000 for PT1000
            double d = bSq - 2.0F * a2 * c;
            return (-RTD_ALPHA + Math.Sqrt(d)) / a2;
        }


        /// <summary>
        /// Reads the selected input channel and converts to voltage units
        /// </summary>
        /// <returns>Voltage representated as a double</returns>
        public double ReadVolts(Channel channel)
        {
            double adcValue = sensor.ReadChannel((int)channel);
            Debug.WriteLine("rawADC: " + adcValue.ToString() + " GainDiv: " + sensor.InputGain);
            //if (adcValue > maxADCValue)
            //    adcValue -= maxADCValue * 2;

            //double volts = (adcValue * lsbVolts) / sensor.InputGain;

            //if (channel == 3)
            //{
            //    volts *= 2.5;
            //    if (volts > 3.9)
            //        volts += 0.085;//add offset for board
            //}
            //else if (channel == 4)
            //{
            //    volts *= 25;
            //    if (volts > 3.9)
            //        volts += 0.085;//add offset for board
            //}

            //return volts;

            return adcValue;
        }


        public void Dispose()
        {

            if (!disposed)
                return;

            sensor.Dispose();
            sensor = null;
            disposed = true;
        }
    }
}
