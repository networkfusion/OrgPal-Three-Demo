using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Adc;

namespace OrgPal.Three
{
    public class OnboardAdcDevice : IDisposable
    {
        private bool _disposed;
        private AdcChannel adcVBAT;
        private AdcChannel adcTemp;
        private AdcController adcController = new AdcController();
        //private AdcChannel adc420mA;

        // ADC constants
        private const int ANALOG_REF_VALUE = 3300;
        private const int MAX_ADC_VALUE = 4095;

        /// <summary>
        /// Read the power supply input voltage.
        /// </summary>
        /// <remarks>
        /// This should be 9-24VDC
        /// </remarks>
        /// <param name="samplesToTake">Number of samples to read for an average</param>
        /// <returns>The voltage as VDC.</returns>
        public double GetUnregulatedInputVoltage(byte samplesToTake = 5)
        {
            var voltage = 0f;

            if (adcVBAT == null)
            {
                adcVBAT = adcController.OpenChannel(Pinout.AdcChannel.ADC1_IN8_VBAT);
            }

            var average = 0;
            for (byte i = 0; i < samplesToTake; i++)
            {
                average += adcVBAT.ReadValue();

                Thread.Sleep(50); // pause to stabilize
            }

            try
            {
                average /= samplesToTake;

                //VBat = 0.25 x VIN adc count
                //float voltage = ((ANALOG_REF_VALUE * average) / MAX_ADC_VALUE)* 4;

                voltage = ((ANALOG_REF_VALUE * average) / MAX_ADC_VALUE) * 0.004f;

                voltage += 0.25f; // small offset calibration factor for board to even drop on measure
            }
            catch
            {
                Debug.WriteLine("OnboardAdcDevice: GetUnregulatedInputVoltage failed!");
            }

            return voltage;
        }

        /// <summary>
        /// Reads the board PCB temperature sensor value.
        /// </summary>
        /// <param name="celsius">Return celsius by default, otherwise f</param>
        /// <param name="samplesToTake">Number of samples to read for average</param>
        /// <returns>Temperature value.</returns>
        public double GetPcbTemperature(bool celsius = true)
        {
            adcTemp = adcController.OpenChannel(Pinout.AdcChannel.ADC1_IN13_TEMP);

            var tempInCent = 0.0d;


            try
            {
                double adcTempCalcValue = (ANALOG_REF_VALUE * adcTemp.ReadValue()) / MAX_ADC_VALUE;
                tempInCent = ((13.582f - Math.Sqrt(184.470724f + (0.01732f * (2230.8f - adcTempCalcValue)))) / (-0.00866f)) + 30f;
            }
            catch
            {
                Debug.WriteLine("OnboardAdcDevice: GetPcbTemperature failed!");
            }
            if (celsius)
            {
                return tempInCent;
            }
            else
            {
                return ((9f / 5f) * tempInCent) + 32f;
            }

        }

        public float GetMcuTemperature()
        {
            adcTemp = adcController.OpenChannel(Pinout.AdcChannel.ADC_CHANNEL_SENSOR);
            return adcTemp.ReadValue() / 100.00f;

            //https://www.st.com/resource/en/datasheet/stm32f769ni.pdf
            //https://electronics.stackexchange.com/questions/324321/reading-internal-temperature-sensor-stm32
            //const int ADC_TEMP_3V3_30C = 0x1FF0F44C //0x1FFF7A2C;
            //const int ADC_TEMP_3V3_110C = 0x1FF0 F44E //0x1FFF7A2E;
            //const float CALIBRATION_REFERENCE_VOLTAGE = 3.3F;
            //const float REFERENCE_VOLTAGE = 3.0F; // supplied with Vref+ or VDDA

            // scale constants to current reference voltage
            //float adcCalTemp30C = getRegisterValue(ADC_TEMP_3V3_30C) * (REFERENCE_VOLTAGE / CALIBRATION_REFERENCE_VOLTAGE);
            //float adcCalTemp110C = getRegisterValue(ADC_TEMP_3V3_110C) * (REFERENCE_VOLTAGE / CALIBRATION_REFERENCE_VOLTAGE);

            // return (adcTemp.ReadValue() - adcCalTemp30C)/(adcCalTemp110C - adcCalTemp30C) *(110.0F - 30.0F) + 30.0F);
        }



        //    RTCScheduler = new PCF85263();
        //    startUp = RTCScheduler.GetDateTime();
        //        IsUserWakeUP = !RTCScheduler.IsAlarmActive();
        //        RTCScheduller.ClearAlarmInterrupt();//always clear interrupt to prevent random power issues

        //        if (startUp.Year< 2019)//set date/time default 
        //        {
        //            startUp = new DateTime(2019, 11, 1);
        //    RTCScheduler.SetDateTime(startUp);
        //        }

        //RTCScheduler.Dispose();
        //        RTCScheduller = null;


        /// <summary>
        /// Releases unmanaged resources
        /// and optionally release managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            adcVBAT.Dispose();
            adcTemp.Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
