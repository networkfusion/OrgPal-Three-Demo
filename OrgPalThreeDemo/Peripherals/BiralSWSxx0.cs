using System;
using System.IO.Ports;

namespace OrgPalThreeDemo.Peripherals
{

    /// <summary>
    /// Biral SWSxx0 sensor
    /// </summary>
    /// <remarks> 
    /// Based on manual: https://www.biral.com/wp-content/uploads/2018/02/SWS-Manual-105223.08B.pdf
    /// </remarks>
    public class BiralSWSxx0
    {
        /// <summary>
        /// SWS sensor model
        /// </summary>
        public enum SwsModel
        {
            SWS050,
            SWS100,
            SWS200,
            SWS250
        }

        private readonly SwsModel _model;
        private readonly SerialPort _sensorPort;


        public BiralSWSxx0(string port, SwsModel model)
        {
            //TODO: need to handle the interface type... RS422-485 or RS232... and maybe relay
            _model = model;
            //FIXME: By default the sensor uses 96008N1, so lets ignore that for now!
            _sensorPort = new SerialPort(port);
            _sensorPort.Open();
            //TODO: send a reset command + read and return state.
            //TODO: get serial number?!

        }


        public Telemetry ReadSensor()
        {
            //FIXME: if it is in auto send mode, the buffer might be building!
            var receivedMessage = _sensorPort.ReadLine();

            switch (_model)
            {
                case SwsModel.SWS050:
                    throw new Exception("Unsupported Model");
                case SwsModel.SWS100:
                    return DecodeTelemetry(receivedMessage);
                case SwsModel.SWS200:
                case SwsModel.SWS250:
                    throw new Exception("Unsupported Model");
                default:
                    throw new Exception("Unknown Model");
            }
        }

        public class Telemetry
        {
            public string Date { get; set; }
            public string Time { get; set; }
            public string Model { get; set; }
            public string IdentificationNumber { get; set; }
            public string AveragingPeriodInSeconds { get; set; }
            public string ValueAverageInKM { get; set; }
            public double WaterPrecipitation { get; set; } = double.NaN;
            public string PresentWeatherCode { get; set; }
            public double Temperature { get; set; } = double.NaN;
            public string ValueInstantInKm { get; set; }

        }

        private static Telemetry DecodeTelemetry(string rawMessage)
        {
            //< Date >,< Time >,SWS100,NNN,XXX,AA.AA KM, BB.BBB,CC,DD.D C, EE.EE KM, FFF,< GGG.GG >< cs > crlf
            var message = rawMessage.Split(','); // FIXME: we might need to trim spaces from the the params!
            var telemetry = new Telemetry();
            telemetry.Date = message[0]; // TODO: use DateTime.Parse
            telemetry.Time = message[1]; // TODO: DateTime.Parse
            telemetry.Model = message[2];
            telemetry.IdentificationNumber = message[3];
            telemetry.AveragingPeriodInSeconds = message[4];
            telemetry.ValueAverageInKM = message[5];
            //telemetry.WaterPrecipitation  = message[6]; // Not used in the SWS100. Set to 99.999. //FIXME: check the model and set it to NaN!
            telemetry.PresentWeatherCode = message[7];
            //telemetry.Temperature  = message[8]; // Not used in the SWS100. Set to 99.9 C. //FIXME: check the model and set it to NaN!
            telemetry.ValueInstantInKm = message[9];

            return telemetry;
        }


        public static string VerbosePresentWeatherCode(string pwCode) //TODO: use int input?
        {
            // Present weather codes. From WMO Table 4680 (Automatic Weather Station).

            switch (pwCode)
            {
                case "XX": // Not Ready (first 5 measurement periods from restart).
                    return "Not Ready";
                case "00":
                    return "No Significant Weather Observed";
                case "04":
                    return "Haze or smoke";
                case "30":
                    return "Fog";
                case "40":
                    return "Indeterminate precipitation type";
                case "50":
                    return "Drizzle";
                case "60":
                    return "Rain";
                case "70":
                    return "Snow";
                default:
                    return "Unknown Code!!!";
            }
        }

        public class Status
        {
            public char ResetState { get; set; }
            public char WindowState { get; set; }
            public char SelfTestState { get; set; }
        }

        private static Status DecodeStatus(string statusStr)
        {
            // FFF
            // |||
            // ||O = Other self test values ok; X = other self test faults exist;
            // |O = Window not contamintated; X = Window cleaning recommended;  F = Window cleaning required!
            // T = test mode; O = not reset since last 'R?'; X = sensor reset since last 'R?' ;

            var statusChars = statusStr.ToCharArray();
            var sensorStatus = new Status();
            switch (statusChars[0])
            {
                //case 'O':
                //case 'T': //probably factory only
                //case 'X':
                default:
                    sensorStatus.ResetState = statusChars[0];
                    break;
            }

            switch (statusChars[1])
            {
                //case 'O':
                //case 'F':
                //case 'X':
                //case 'S': // ALS-2 only
                default:
                    sensorStatus.WindowState = statusChars[1];
                    break;
            }

            switch (statusChars[2])
            {
                //case 'O':
                //case 'X':
                //case 'F': //250 only
                //case 'B': //250 only
                default:
                    sensorStatus.SelfTestState = statusChars[2];
                    break;
            }
            return sensorStatus;

        }

        public void SelfTestResponse(string strResponse)
        {
            // <space>100,2.509,24.1,12.3,5.01,12.5,00.00,00.00,100,105,107,00,00,00,+021.0,4063
            //HeaterState
            //InternalReferenceVoltage
            //SupplyVoltage
            //InternalOperatingVoltage1
            //InternalOperatingVoltage2
            //InternalOperatingVoltage3
            //ForwardScatterBackgroundbrightness
            //TransmitPowerMonitor
            //ForwardReceiverMonitor
            //BackReceiverMonitor
            //TransmitterWindowContamination
            //BackReceiverContamintation
            //Temperature
            //AdcInteruptsPerSecond


        }

        

        public static class SendCommands
        {
            /// <summary>
            /// Query precipitation accumulation amount.
            /// </summary>
            public const string QueryPrecipAccAmount = "A?";

            /// <summary>
            /// Clear accumulated precipitation.
            /// </summary>
            public const string ClearPrecipTotalAmount = "AC";

            /// <summary>
            /// Query RS485 address.
            /// </summary>
            public const string QueryAddress = "ADR?";

            /// <summary>
            /// Set RS485 address.
            /// </summary>
            /// <remarks>
            /// Range 00-99
            /// </remarks>
            public const string SetAddress = "ADR"; //ADRxx

            // TODO: add other commands!
        }
    }
}
