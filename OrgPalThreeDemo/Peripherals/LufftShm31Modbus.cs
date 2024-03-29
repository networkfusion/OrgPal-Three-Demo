using Iot.Device.Modbus.Client;

namespace OrgPalThreeDemo.Peripherals
{
    public class LufftShm31Modbus
    {
        public enum Actions
        {
            SensorReboot = 1,
            MeasurementStart = 2,
            MeasurementStop = 3,
            LaserOn = 4,
            LaserOff = 5,
            CalibrateAll = 6,
            CalibrateHeight = 7,
            DefrostStart = 8,
            DefrostStop = 9,
        }

        private const short STATUS_DEVICE_NOT_READY = 40;

        private const short ACTION_APPLY = 12871;

        public byte DeviceId { get; set; }

        ModbusClient client;

        public LufftShm31Modbus()
        {
            DeviceId = 1;
            client = new("COM3", 19200);
            client.ReadTimeout = client.WriteTimeout = 500;
        }

        public short[] ReadRegisters()
        {
            // read and return the info and standard measurements
            return client.ReadInputRegisters(DeviceId, 1, 30);
            // TODO: add the snow flag:
            //short[] regsRead = client.ReadInputRegisters(DeviceId, 96, 1);
        }

        public void PerformAction(Actions actionRegister)
        {
            client.WriteSingleRegister(DeviceId, (ushort)actionRegister, ACTION_APPLY);
        }
    }
}
