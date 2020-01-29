using PalThree;
using System;
using System.Text;
using System.Threading;
using Windows.Devices.Adc;

namespace OrgPalThreeDemo.Drivers
{
    public class OnboardDevices
    {
        private AdcChannel adcVBAT;
        private AdcChannel adcTemp;
        //private AdcChannel adc420mA;

        /// <summary>
        /// Returns the value of the 12V battery voltage coming in the system 
        /// </summary>
        /// <returns></returns>
        public float GetBatteryUnregulatedVoltage()
        {
            float voltage = 0;

            if (adcVBAT == null)
            {
                AdcController adc1 = AdcController.GetDefault();
                adcVBAT = adc1.OpenChannel(PalThreePins.AdcChannel.ADC1_IN8_VBAT);
            }

            int average = 0;
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
            }

            return voltage;
        }

        public float GetTemperatureOnBoard()
        {
            AdcController adc1 = AdcController.GetDefault();
            adcTemp = adc1.OpenChannel(PalThreePins.AdcChannel.ADC1_IN13_TEMP);

            float tempInCent = 0;

            try
            {
                var maximumValue = 4095;
                var analogReference = 3300;
                float adcTempCalcValue = (analogReference * adcTemp.ReadValue()) / maximumValue;
                tempInCent = ((13.582f - Math.Sqrt(184.470724f + (0.01732f * (2230.8f - adcTempCalcValue)))) / (-0.00866f)) + 30;
                // float tempInF = ((9f / 5f) * tempInCent) + 32f;
            }
            catch { }

            return tempInCent;

        }



    //    RTCScheduller = new PCF85263();
    //    startUp = RTCScheduller.GetDateTime();
    //        IsUserWakeUP = !RTCScheduller.IsAlarmActive();
    //        RTCScheduller.ClearAlarmInterrupt();//always clear interrupt to prevent random power issues

    //        if (startUp.Year< 2019)//set date/time default 
    //        {
    //            startUp = new DateTime(2019, 11, 1);
    //    RTCScheduller.SetDateTime(startUp);
    //        }

    //RTCScheduller.Dispose();
    //        RTCScheduller = null;
    }
}
