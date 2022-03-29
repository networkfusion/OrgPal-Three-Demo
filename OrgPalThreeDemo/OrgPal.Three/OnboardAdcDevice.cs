using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Windows.Devices.Adc;

namespace OrgPal.Three
{
    public class OnboardAdcDevice : IDisposable
    {
        private bool _disposed;
        private AdcChannel adcVBAT;
        private AdcChannel adcTemp;
        //private AdcChannel adc420mA;

        /// <summary>
        /// Returns the value of the 12V battery voltage coming in the system 
        /// </summary>
        /// <returns></returns>
        public double GetBatteryUnregulatedVoltage()
        {
            float voltage = 0;

            if (adcVBAT == null)
            {
                AdcController adc1 = AdcController.GetDefault();
                adcVBAT = adc1.OpenChannel(Pinout.AdcChannel.ADC1_IN8_VBAT);
            }

            var average = 0;
            for (byte i = 0; i < 5; i++)
            {
                average += adcVBAT.ReadValue();

                Thread.Sleep(50);//pause to stabilize
            }

            try
            {
                average /= 5;

                //maximumValue = 4095;
                //analogReference = 3300;
                //VBat = 0.25 x VIN adc count
                //float voltage = ((3300 * average) / 4096)* 4;

                voltage = ((3300 * average) / 4096) * 0.004f;

                voltage += 0.25f;//small offset calibration factor for board to even drop on measure
            }
            catch
            {
                Debug.WriteLine("OnboardAdcDevice: GetBatteryUnregulatedVoltage failed!");
            }

            return voltage;
        }

        public double GetTemperatureOnBoard()
        {
            AdcController adc1 = AdcController.GetDefault();
            adcTemp = adc1.OpenChannel(Pinout.AdcChannel.ADC1_IN13_TEMP);

            double tempInCent = 0;

            try
            {
                var maximumValue = 4095.0;
                var analogReference = 3300.0;
                double adcTempCalcValue = (analogReference * adcTemp.ReadValue()) / maximumValue;
                tempInCent = ((13.582f - Math.Sqrt(184.470724f + (0.01732f * (2230.8f - adcTempCalcValue)))) / (-0.00866f)) + 30;
                // double tempInF = ((9f / 5f) * tempInCent) + 32f;
            }
            catch
            {
                Debug.WriteLine("OnboardAdcDevice: GetTemperatureOnBoard failed!");
            }

            return tempInCent;

        }

        public float GetMcuTemperature()
        {
            AdcController adc1 = AdcController.GetDefault();
            adcTemp = adc1.OpenChannel(Pinout.AdcChannel.ADC_CHANNEL_SENSOR);
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
