using System;

namespace OrgPalThreeDemo.Peripherals
{
    public class LufftVentus
    {
        internal class UmbBinary
        {
            //could be defined as CHAR...
            const byte CHAR_SOH = 0x01;
            const byte CHAR_STX = 0x02;
            const byte CHAR_ETX = 0x03;
            const byte CHAR_EOT = 0x04;

            public class UmbBinaryMessage
            {
                //Message structure...
                // SOH | VER | TO (device ID | class) | FROM (Master unit) | len | STX | CMD | VER | CHANNEL | ETX | CHKSUM  | EOT      
                // 01h | 10h | 01h 80h                | 01h F0h            | 04h | 02h | 23h | 10h | 64h 00h | 03h | 0Bh 54h | 04h 'this is a request for the temperature on channel 64, so need to deal with the checksum
                const byte UMB_PROTOCOL_VERSION_V1_0 = 0x10; // V1.0
                
                const short DEFAULT_UMB_MASTER_DEVICE = 0x01 + 0x80; // From (device & class (Master unit))
                const byte DEFAULT_UMB_CMD_LENGTH = 0x04; // This should remain consistant with UMB_PROTOCOL_VERSION V1.0
                const byte DEFAULT_UMB_REQUEST_TYPE = 0x23; // Defined as: the command for the online data request

                public enum SensorType : short
                {
                    VENTUS = 0x01 + 0x80 // To (device ID & class)
                }

                public enum ParameterChannelType : short
                {
                    WIND_AIR_TEMP = 100, //0x64 + 0x00; //dec 100 (little endian)
                    WIND_SPEED_CURRENT_KTS = 415, //0xF0 + 0x19; //dec 415 (little endian)
                    WIND_DIRECTION_CURRENT_DEG = 500, //0xF4 + 0x01; // dec 500 (little endian)
                    WIND_GUST_3S_KTS = 458, // 0xCA + 0x10; //dec 458 (little endian)
            }

                public byte ProtocolVersion { get; set; }
                public short ToAddress { get; set; } //this should allow different sensors!
                public short FromAddress { get; set; }
                public byte Command { get; set; }
                public short Channel { get; set; }
            }

            //public void EncodeMessage()
            //{

            //    const short UMB_CHKSUM_TEST = 0x0B + 0x54; //Currently only for "ParameterChannelType.WIND_AIR_TEMP"
            //    //TODO: BytesToSend will need to be in a function or a Variable
            //    var BytesToSend = new byte[] { 
            //    CHAR_SOH,
            //    DEFAULT_UMB_PROTOCOL_VERSION_V1_0,
            //    SensorType.VENTUS, //the "TO" address
            //    FromAddress, //UMB_MASTER_DEVICE, 
            //    DEFAULT_UMB_CMD_LENGTH, 
            //    CHAR_STX, 
            //    UMB_REQUEST_TYPE,
            //    DEFAULT_UMB_PROTOCOL_VERSION_V1_0,
            //    ParameterChannelType.WIND_AIR_TEMP, 
            //    CHAR_ETX, 
            //    UMB_CHKSUM_TEST, 
            //    CHAR_EOT;
            //}

        }

        //Connection type
        public enum ConnectionType
        {
            RS485_HalfDuplex,
            RS485_FullDuplex,
            SDI,
            MODBUS
        }

        //RS485 119200 8N1 - Default

        //Protocol
        public enum Protocol
        {
            UMB_Binary,
            UMB_ASCII, //- Depricated
            NMEA,
            SDI12,
            MODBUS_RTU,
            MODBUS_ASCII
        }

        public enum Status
        {
            VoltageUpper = 4006, //ThresholdExceeded above 28VDC
            VoltageLower = 4007, //ThresholdExceeded below 20VDC
            HeatingLower = 4997,
            HeatingUpper = 4998
        }

        //Properties
        public int WindSpeed { get; set; }
        public int WindDirection { get; set; }
        public int WindMeasurementQuality { get; set; }
        public int VirtualTemprature { get; set; }
        public int AirPressure { get; set; }
        public int AirDensity { get; set; }
        public int Heating { get; set; }
        public int CurrentStatus { get; set; }

        public int Humidity { get; set; } //This needs to be sent to the sensor for working out air density.

        private void DecodeNmeaMessage()
        {

        }

        private void DecodeUmbBinaryMessage()
        {

        }

        private void DecodeUmbASCIIMessage()
        {

        }


        private class UmbBinaryMessage
        {

        }

        private class NmeaMessage
        {

        }
    }
}
