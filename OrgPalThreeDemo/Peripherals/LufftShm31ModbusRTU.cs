using Iot.Device.Modbus.Client;

namespace OrgPalThreeDemo.Peripherals
{
    public class LufftShm31ModbusRTU
    {
        public enum SensorAction
        {
            /// <summary>
            /// Initiate a reboot of the sensor.
            /// </summary>
            Reboot = 0,
            /// <summary>
            /// Start performing measurement operations.
            /// </summary>
            MeasurementStart,
            /// <summary>
            /// Stop performing measurement operations.
            /// </summary>
            MeasurementStop,
            /// <summary>
            /// Turn the laser on permanently (e.g. for installation alignment).
            /// </summary>
            LaserOnAlways,
            /// <summary>
            /// Put the laser back into its normal operating mode.
            /// </summary>
            LaserResumeSchedule,
            /// <summary>
            /// Perform calibration of the height using the tilt angle from the gyroscope.
            /// </summary>
            CalibrateAll,
            /// <summary>
            /// Perform calibration of the height using the reference angle (ignore gyroscope).
            /// </summary>
            CalibrateHeightOnly,
            /// <summary>
            /// Start a defrost process.
            /// </summary>
            DefrostStart,
            /// <summary>
            /// Stop a defrost process.
            /// </summary>
            DefrostStop = 8,
        }

        public enum ParameterRegisterAddress
        {
            SetBlockHeatingMode = 9,
            SetWindowHeatingMode,
            SetExternalHeatingMode,
            SetDefrostingMode,
            SetReferenceHeight,
            SetTiltAngle,
            SetTiltAngleMode,
            SetSnowHeightChangeTimeAcceptance = 16,
            SetSnowHightChangeMaxDiffAcceptance,
            SetLaserOperatingMode,
            SetLaserMeasurementInterval = 19,
        }

        public enum HeatingMode
        {
            Off = 0,
            On_12Volts,
            On_24Volts,
            Defrosting_12Volts,
            Defrosting_24Volts,
            HeatingDisabled,
            VoltageControlError,
            Unavailable = 7,
        }

        public enum DeviceErrorCode
        {
            LaserSignalTooWeak = 15,
            LaserSignalTooStrong,
            LaserBackgroundLightLevelTooStrong,
            LaserMeasurementDisturbed,
            LaserTimeoutOff,
            LaserCommsInterface,
            LaserCommsResponse,
            LaserTemperatureTooLow,
            LaserTemperatureTooHigh,
            HardwareEepromChecksumIncorrect = 31,
            LaserEepromChecksumIncorrect,
            LaserAPD = 51,
            LaserCurrent,
            MathematicsDivisionByZero,
            LaserHardware,
            GeneralHardware,
            GeneralHardwareInterface = 61,
            SerialParity,
            SerialOverflow,
            SerialFraming,
            EvaluationFailure,
            MeasurementCancelled,
            TelegramNotAvailable,
            // Evaluation Routine:
            ReadSettingsDataFailed = 70,
            ReadLaserDataFailed,
            ReadLaserTemperatureFailed,
            ReadBlockTemperatureFailed,
            ReadOutsideTemperatureFailed,
            ReadLaserDistanceFailed,
            GyroVectorLength,
            ReferenceAngleInvalid,
            SignalDivision,
            SignalTooSmall,
            SignalTooLarge,
            NoAngleCorrection,
            ChannelAverageCountOverflow,
            RingBufferInitialization = 83,
        }

        public enum InputRegisterAddress
        {
            // Status Information registers
            DeviceIdentification = 0,
            DeviceStatusLower,
            DeviceStatusUpper,
            BlockHeatingState,
            WindowHeatingState,
            BlockTemperatureStatus,
            AmbientTemperatureStatus,
            LaserTemperatureStatus,
            TiltAngleStatus,
            SnowHeightStatus,
            DistanceStatus,
            NormalizedSignalStatus,
            //reserved_12
            //reserved_13
            ErrorCode = 14,
            ErrorCode_Current,
            AccumulatedOpperatingTimeLower,
            AccumulatedOpperatingTimeUpper,
            SystemTimeLower,
            SystemTimeUpper,
            // Standard data set registers (metric units)
            SnowHeightMillimeter_Current,
            BlockTemperatureDegC_Current,
            AmbientTemperatureDegC_Current,
            LaserTemperatureDegC_Current,
            NormalizedSignal_Metric,
            TiltAngle_Metric_Current,
            ErrorCode_Metric,
            //reserved_27
            //reserved_28
            //reserved_29
            // Standard data set registers (imperial units)
            SnowHeightInches_Current = 30,
            BlockTemperatureDegF_Current,
            AmbientTemperatureDegF_Current,
            LaserTemperatureDegF_Current,
            NormalizedSignal_Imperial,
            TiltAngle_Imperial_Current,
            ErrorCode_Imperial,
            //reserved_37
            //reserved_38
            //reserved_39
            // Distance Registers
            //...
            // Temperature Registers (metric units)
            //...
            // Temperature Registers (imperial units)
            //...
            // Angles registers
            //...
            AngleX = 89,
            AngleY,
            AngleZ,
            TiltAngleReference,
            //reserved_93
            //reserved_94
            // Logic and normalized values registers
            SnowFlag = 95,
            //...
            // Service Channels
            //...
            OperatingVoltage = 116,
            //reserved_118
            //reserved_119
        }

        public enum StatusCode
        {
            Success = 0,
            UnknownCommand = 16,
            InvalidParameter = 17,
            InvalidChannel = 36,
            DeviceBusy = 40, // initialization / calibration in progress
            DisplayRangeOffsetOverflow = 80,
            DisplayRangeOffsetOverflow2 = 81,
            MeasurementRangeOverflow = 82,
            MeasurementRangeOverflow2 = 83,
            MeasurementDataReadError = 84,
            AmbientConditionsError = 85
        }

        //private const ushort VALUE_ERROR_UINT16 = ushort.MaxValue; // 65530
        //private const short VALUE_ERROR_INT16 = short.MaxValue; // 32767

        private const short ACTION_APPLY = 12871;

        public byte DeviceId { get; private set; }

        private readonly ModbusClient client;

        public LufftShm31ModbusRTU(string port = "COM3", byte deviceId = 1)
        {
            DeviceId = deviceId;
            client = new(port, 19200);
            client.ReadTimeout = client.WriteTimeout = 500;
        }

        public short[] ReadRegistersRaw()
        {
            // read and return the info and standard measurements
            return client.ReadInputRegisters(DeviceId, 0, 27);
            // TODO: add the snow flag:
            //short[] regsRead = client.ReadInputRegisters(DeviceId, 95, 1);
        }

        public void PerformAction(SensorAction actionRegister)
        {
            client.WriteSingleRegister(DeviceId, (ushort)actionRegister, ACTION_APPLY);
        }

        public void ApplyParameter(ParameterRegisterAddress parameterRegister, short value)
        {
            client.WriteSingleRegister(DeviceId, (ushort)parameterRegister, value);
        }

        public float ApplyScaleFactor(short regValue, short scaleFactor)
        {
            return (regValue / scaleFactor);
        }
    }
}
