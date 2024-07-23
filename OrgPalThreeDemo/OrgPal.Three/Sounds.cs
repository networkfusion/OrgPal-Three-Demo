using System.Threading;
using System.Device.Pwm;

namespace OrgPal.Three
{
    public static class Sounds
    {
        public static bool PlayDefaultSound()
        {
            int[] melody = new int[]{
            330, 330, 330, 262, 330, 392
            };


            int[] noteDurations = new int[]{
            8, 4, 4, 8, 4, 2
            };

            // start a thread to play a sound on the buzzer
            new Thread(() =>
            {
                var buzzer = PwmChannel.CreateFromPin(Pinout.GpioPin.PWM_SPEAKER_PH12);

                buzzer.DutyCycle = 0.5;

                for (var i = 0; i < melody.Length; i++)
                {
                    buzzer.Frequency = melody[i]*2;

                    buzzer.Start();
                    Thread.Sleep(noteDurations[i]*40);
                    buzzer.Stop();
                    Thread.Sleep(noteDurations[i] * 5);
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
