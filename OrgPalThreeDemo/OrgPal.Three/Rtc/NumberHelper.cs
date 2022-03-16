using System;

namespace Iot.Device.Rtc
{

    public static class NumberHelper
    {

        public static int Bcd2Dec(byte bcd) => ((bcd >> 4) * 10) + (bcd % 16);

        public static byte Bcd2Bin(byte val) { return (byte)(val - 6 * (val >> 4)); }

        public static byte Bin2Bcd(byte val) { return (byte)(val + 6 * (val / 10)); }

        public static byte Dec2Bcd(int val) { return (byte)((val / 10 * 16) + (val % 10)); }
    }
}
