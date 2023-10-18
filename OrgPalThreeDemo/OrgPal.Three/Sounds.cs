using System.Threading;
using System.Device.Pwm;

namespace OrgPal.Three
{
    public static class Sounds
    {
        public static bool PlayDefaultSound()
        {

            // start a thread to play a sound on the buzzer
            new Thread(() =>
            {
                var freq = 2000;
                var delta = 250;
                var lengthInSeconds = 3;

                var buzzer = PwmChannel.CreateFromPin(Pinout.GpioPin.PWM_SPEAKER_PH12);

                buzzer.DutyCycle = 0.5;

                for (short i = 0; i < lengthInSeconds; i++)
                {
                    buzzer.Frequency = freq;

                    buzzer.Start();
                    Thread.Sleep(500);
                    buzzer.Stop();

                    freq += delta;

                    buzzer.Frequency = freq;
                    buzzer.Start();
                    Thread.Sleep(500);
                    buzzer.Stop();

                    if (freq < 1000 || freq > 3000)
                        delta *= -1;
                }


                buzzer.Stop();
                buzzer.Dispose();
                buzzer = null;
            }
            ).Start();

            return true;
        }

    }
}
