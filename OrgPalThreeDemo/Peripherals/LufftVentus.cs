using System;

namespace OrgPalThreeDemo.Peripherals
{
    public class LufftVentus
    {
        internal class UmbBinary
        {
            //const byte SOH = 0x01;
            //const byte STX = 0x02;
            //const byte ETX = 0x03;
            //const byte EOT = 0x04;
            //// SOH | VER | TO (device ID | class) | FROM (Master unit) | len | STX | CMD | VER | CHANNEL | ETX | CHKSUM  | EOT      
            //// 01h | 10h | 01h 80h                | 01h F0h            | 04h | 02h | 23h | 10h | 64h 00h | 03h | 0Bh 54h | 04h 'this is a request for the temperature on channel 64, so need to deal with the checksum
            //const byte UMB_PROTOCOL_VERSION = 0x10; // V1.0
            //const byte UMB_DEVICE_TYPE_VENTUS = 0x01 + 0x80; // (device ID | class)
            //const byte UMB_MASTER_DEVICE = 0x01 + 0xF0; // (device | class (Master Device))
            //const byte UMB_CMD_LENGTH = 0x04; // This should remain consistant with UMB_PROTOCOL_VERSION V1.0
            //const byte UMB_REQUEST_TYPE = 0x23; // Defined as: the command for the online data request
            //const byte UMB_CHANNEL_WIND_AIR_TEMP = 0x64 + 0x00; //dec 100 (little endian)
            //const byte UMB_CHANNEL_WIND_SPEED_CURRENT_KTS = 0xF0 + 0x19; //dec 415 (little endian)
            //const byte UMB_CHANNEL_WIND_DIRECTION_CURRENT_DEG = 0xF4 + 0x01; // dec 500 (little endian)
            //const byte UMB_CHANNEL_WIND_GUST_3S_KTS = 0xCA + 0x10; //dec 458 (little endian)
            //const byte UMB_CHKSUM_TEST = 0x0B + 0x54; //Currently only for air temp channel
            //                                     //TODO: BytesToSend will need to be in a function or a Variable (Dim)
            //const byte[] BytesToSend = SOH + UMB_PROTOCOL_VERSION + UMB_DEVICE_TYPE_VENTUS + UMB_MASTER_DEVICE + UMB_CMD_LENGTH + STX + UMB_REQUEST_TYPE + UMB_PROTOCOL_VERSION + UMB_CHANNEL_WIND_AIR_TEMP + ETX + UMB_CHKSUM_TEST + EOT;
        }

        //Connection type
        // RS485-HalfDuplex
        // RS485-FullDuplex
        // SDI
        // MODBUS

        //RS485 119200 8N1 - Default

        //Protocol
        //UMB Binary
        //UMB ASCII - Depricated
        //NMEA
        //SDI-12
        //MODBUS RTU
        //MODBUS ASCII

        //public enum Status
        //{
        //    VoltageUpper = 4006, //ThresholdExceeded above 28VDC
        //    VoltageLower = 4007, //ThresholdExceeded below 20VDC
        //    HeatingLower = 4997,
        //    HeatingUpper = 4998
        //}

        //Properties
        //public int WindSpeed { get; set; }
        //public int WindDirection { get; set; }
        //public int WindMeasurementQuality { get; set; }
        //public int VirtualTemprature { get; set; }
        //public int AirPressure { get; set; }
        //public int AirDensity { get; set; }
        //public int Heating { get; set; }
        //public int CurrentStatus { get; set; }

        ////public int Humidity { get; set; } //This needs to be sent to the sensor for working out air density.


        //private void DecodeNmeaMessage
        //{

        //}

        //private void DecodeUmbBinaryMessage
        //{

        //}

        //private void DecodeUmbASCIIMessage
        //{

        //}


        //private class UmbBinaryMessage
        //{

        //}

        //private class NmeaMessage
        //{

        //}
    }
}
