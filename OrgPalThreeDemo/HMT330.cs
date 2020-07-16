using System;
using System.Text;
//using Windows.Devices.Uart;

namespace OrgPalThreeDemo
{
    /// <summary>
    /// https://www.vaisala.com/sites/default/files/documents/HMT330%20User%27s%20Guide%20in%20English%20M210566EN.pdf
    /// </summary>
    public class HMT330
    {

        HMT330(string port)
        {

        }


        /// <summary>
        /// Opens the port to communicate with the HMT330 user port
        /// </summary>
        public void Open()
        {
            //baud 4800
            //Parity.Even
            //DataBits.Seven
            //StopBits.One
            //FlowControl.None
            //
        }

        public void Run()
        {
            //R<CR>
        }

        public void Stop()
        {
            //S<CR>
        }

        public string OneShot()
        {
            //SEND<CR>

            return "";
        }
    }
}
