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
            SI_DeviceIdentification = 0,
            SI_DeviceStatusLower,
            SI_DeviceStatusUpper,
            SI_BlockHeatingState,
            SI_WindowHeatingState,
            SI_BlockTemperatureStatus,
            SI_AmbientTemperatureStatus,
            SI_LaserTemperatureStatus,
            SI_TiltAngleStatus,
            SI_SnowHeightStatus,
            SI_DistanceStatus,
            SI_NormalizedSignalStatus,
            Reserved_12,
            Reserved_13,
            SI_ErrorCode = 14,
            SI_ErrorCode_Current,
            SI_AccumulatedOpperatingTimeLower,
            SI_AccumulatedOpperatingTimeUpper,
            SI_SystemTimeLower,
            SI_SystemTimeUpper,
            // Standard data registers (metric units)
            SDM_SnowHeightMillimeter_Current,
            SDM_BlockTemperatureDegC_Current,
            SDM_AmbientTemperatureDegC_Current,
            SDM_LaserTemperatureDegC_Current,
            SDM_NormalizedSignal_Metric,
            SDM_TiltAngle_Metric_Current,
            SDM_ErrorCode_Metric,
            Reserved_27,
            Reserved_28,
            Reserved_29,
            // Standard data set registers (imperial units)
            SDI_SnowHeightInches_Current = 30,
            SDI_BlockTemperatureDegF_Current,
            SDI_AmbientTemperatureDegF_Current,
            SDI_LaserTemperatureDegF_Current,
            SDI_NormalizedSignal_Imperial,
            SDI_TiltAngle_Imperial_Current,
            SDI_ErrorCode_Imperial,
            Reserved_37,
            Reserved_38,
            Reserved_39,
            // Distance Registers
            D_SnowHeight_Millimeter_Current,
            D_SnowHeight_Millimeter_Minimum,
            D_SnowHeight_Millimeter_Maximum,
            D_SnowHeight_Millimeter_Average,
            D_Calibrated_Millimeter_Current,
            D_Raw_Millimeter_Current,
            D_SnowHeight_Inches_Current,
            D_SnowHeight_Inches_Minimum,
            D_SnowHeight_Inches_Maximum,
            D_SnowHeight_Inches_Average,
            D_Calibrated_Inches_Current,
            D_Raw_Inches_Current,
            D_ReferenceHeight_millimeter = 52,
            D_SnowHeight_millimeter_HighRes,
            Reserved_54,
            // Temperature Registers (metric units)
            TM_BlockTemperatureDegC_Current = 55,
            TM_BlockTemperatureDegC_Minimum,
            TM_BlockTemperatureDegC_Maximum,
            TM_BlockTemperatureDegC_Average,
            TM_AmbientTemperatureDegC_Current,
            TM_AmbientTemperatureDegC_Minimum,
            TM_AmbientTemperatureDegC_Maximum,
            TM_AmbientTemperatureDegC_Average,
            TM_LaserTemperatureDegC_Current,
            TM_LaserTemperatureDegC_Minimum,
            TM_LaserTemperatureDegC_Maximum,
            TM_LaserTemperatureDegC_Average,
            Reserved_67,
            Reserved_68,
            Reserved_69,
            // Temperature Registers (imperial units)
            TI_BlockTemperatureDegF_Current = 70,
            TI_BlockTemperatureDegF_Minimum,
            TI_BlockTemperatureDegF_Maximum,
            TI_BlockTemperatureDegF_Average,
            TI_AmbientTemperatureDegF_Current,
            TI_AmbientTemperatureDegF_Minimum,
            TI_AmbientTemperatureDegF_Maximum,
            TI_AmbientTemperatureDegF_Average,
            TI_LaserTemperatureDegF_Current,
            TI_LaserTemperatureDegF_Minimum,
            TI_LaserTemperatureDegF_Maximum,
            TI_LaserTemperatureDegF_Average,
            Reserved_82,
            Reserved_83,
            Reserved_84,
            // Angles registers
            A_AngleTilt_Current = 85,
            A_AngleTilt_Minimum,
            A_AngleTilt_Maximum,
            A_AngleTilt_Average,
            A_AngleX_Current = 89,
            A_AngleY_Current,
            A_AngleZ_Current,
            A_TiltAngleReference,
            Reserved_93,
            Reserved_94,
            // Logic and normalized values registers
            LNV_SnowFlag = 95,
            Reserved_96, // TODO: Note: this is not in the manual!
            LNV_NormalizedSignal_Current = 97,
            LNV_NormalizedSignal_Minimum,
            LNV_NormalizedSignal_Maximum,
            LNV_NormalizedSignal_Average,
            Reserved_101,
            Reserved_102,
            Reserved_103,
            Reserved_104,
            // Service Channel Registers
            SC_BlockHeatingState = 105,
            SC_InternalTemperatureDegC_NTC,
            Reserved_107,
            SC_BlockHeatingDefrostTime_Seconds,
            SC_WindowHeatingState,
            SC_ExternalTemperatureDegC_NTC = 110,
            Reserved_111,
            SC_WindowHeatingDefrostTime_Seconds = 112,
            SC_LaserGainCode = 113,
            SC_LaserSignalIntensity_uV = 114,
            SC_LaserDistance_Millimeter = 115,
            SC_LaserTemperatureDegC = 116,
            SC_OperatingVoltage = 117,
            Reserved_118,
            Reserved_119,
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
