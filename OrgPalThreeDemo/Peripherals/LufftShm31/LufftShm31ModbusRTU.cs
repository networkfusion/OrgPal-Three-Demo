using Iot.Device.Modbus.Client;
using Iot.Device.Modbus.Util;
using System;

namespace OrgPalThreeDemo.Peripherals.LufftShm31
{
    /// <summary>
    /// Lufft SHM31 Modbus RTU
    /// </summary>
    /// <remarks>
    /// Snow depth sensor.
    /// Manufacturer: OTT Hydromet (Lufft, Jenoptik).
    /// Manual: https://www.lufft.com/download/manual-lufft-shm31-en/
    /// </remarks>
    public class LufftShm31ModbusRTU
    {

        private const short ACTION_APPLY = 12871;

        public byte DeviceId { get; private set; }

        private readonly ModbusClient client;

        public LufftShm31ModbusRTU(string port = "COM3", byte deviceId = 1, int baudRate = 19200)
        {
            DeviceId = deviceId;
            client = new(port, baudRate);
            client.ReadTimeout = client.WriteTimeout = 500;
        }

        public short[] ReadAllRegistersRaw()
        {
            // TODO: is there a way to check if the read has happened successfully!? probably returns null...
            // read and return all (0..119) registers on the device.
            return client.ReadInputRegisters(DeviceId, 0, 120); // TODO: use the enum count.
        }

        public short[] ReadNormalRegistersRaw()
        {
            // TODO: is there a way to check if the read has happened successfully!? probably returns null...
            // read and return the info and standard measurements
            return client.ReadInputRegisters(DeviceId, 0, 27);
            // TODO: add the snow flag:
            //short[] regsRead = client.ReadInputRegisters(DeviceId, 95, 1);
        }

        public bool PerformAction(SensorAction actionRegister)
        {
            if ((ushort)actionRegister < 9)
            {
                return client.WriteSingleRegister(DeviceId, (ushort)actionRegister, ACTION_APPLY);
            }
            return false; // it involved a register that required a settable value.
        }

        public bool PerformAction(SensorAction actionRegister, ushort value)
        {
            if (value >= short.MaxValue)
            {
                // FIXME: certain values might need ushort (like height change acceptance time)
                // TODO: apply twos complement and write it raw?

                return false;
            }

            if ((ushort)actionRegister > 8) // make sure this is a settable register
            {
                var res = client.WriteSingleRegister(DeviceId, (ushort)actionRegister, (short)value);
                if (!res)
                {
                    res = PerformAction(SensorAction.InitiateReboot);
                }
                return res;
            }
            return false;
        }

        public float AdjustValue_ScaleFactor(short regValue, short scaleFactor)
        {
            return regValue / scaleFactor;
        }

        // convert short to ushort for most regs.

        public uint GetValueAsUInt32(ushort lowerValue, ushort upperValue)
        {
            // TODO: switch to Iot.Device.Modbus.Util
            uint value = upperValue;
            value <<= 16;
            value |= lowerValue;
            return value;
        }
    }
}
