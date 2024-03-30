using Iot.Device.Modbus.Client;
using Iot.Device.Modbus.Util;
using System;

namespace OrgPalThreeDemo.Peripherals
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
        public enum SensorAction
        {
            /// <summary>
            /// Initiate a reboot of the sensor.
            /// </summary>
            InitiateReboot = 0,
            /// <summary>
            /// Start performing measurement operations.
            /// </summary>
            StartMeasurements,
            /// <summary>
            /// Stop performing measurement operations.
            /// </summary>
            StopMeasurements,
            /// <summary>
            /// Turn the laser on permanently (e.g. for installation alignment).
            /// </summary>
            TurnLaserOnPermanently,
            /// <summary>
            /// Put the laser back into its normal operating mode (after TurnLaserOnPermanently).
            /// </summary>
            ResumeNormalLaserSchedule,
            /// <summary>
            /// Perform calibration of the height using the tilt angle from the gyroscope.
            /// </summary>
            InitiateFullCalibration,
            /// <summary>
            /// Perform calibration of the height using the reference angle (ignore gyroscope).
            /// </summary>
            InitiateHeightCalibration,
            /// <summary>
            /// Start a defrost process.
            /// </summary>
            InitiateDefrost,
            /// <summary>
            /// Stop a defrost process.
            /// </summary>
            StopDefrosting = 8,

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
            /// <summary>
            /// The heating is OFF.
            /// </summary>
            Off = 0,
            /// <summary>
            /// The heating is ON using 12 VDC power input.
            /// </summary>
            On_12Volts,
            /// <summary>
            /// The heating is ON using 24 VDC power input.
            /// </summary>
            On_24Volts,
            /// <summary>
            /// The heating is ON and currently defrosting using 12 VDC power input.
            /// </summary>
            Defrosting_12Volts,
            /// <summary>
            /// The heating is ON and currently defrosting using 24 VDC power input.
            /// </summary>
            Defrosting_24Volts,
            /// <summary>
            /// The heating mode is disabled.
            /// </summary>
            HeatingDisabled,
            /// <summary>
            /// There was an internal voltage control error.
            /// </summary>
            VoltageControlError,
            /// <summary>
            /// The operation is unavailable due to either incorrect configuration or temperature values.
            /// </summary>
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
            SI_DeviceIdentification = 0, // value = High byte: device subtype, Low byte: software version
            SI_DeviceStatusLower,
            SI_DeviceStatusUpper,
            SI_BlockHeatingState, // value = HeatingMode
            SI_WindowHeatingState, // value = HeatingMode
            SI_BlockTemperatureStatus, // value = StatusCode
            SI_AmbientTemperatureStatus, // value = StatusCode
            SI_LaserTemperatureStatus, // value = StatusCode
            SI_TiltAngleStatus, // value = StatusCode
            SI_SnowHeightStatus, // value = StatusCode
            SI_DistanceStatus, // value = StatusCode
            SI_NormalizedSignalStatus, // value = StatusCode
            Reserved_12,
            Reserved_13,
            SI_ErrorCode = 14, // value = DeviceErrorCode
            SI_ErrorCode_Current, // value = DeviceErrorCode
            SI_AccumulatedOpperatingTimeLower,
            SI_AccumulatedOpperatingTimeUpper,
            SI_SystemTimeLower,
            SI_SystemTimeUpper,
            // Standard data registers (metric units)
            SDM_SnowHeightMillimeter_Current, // value = signed short
            SDM_BlockTemperatureDegC_Current, // value = signed short scaled by 10
            SDM_AmbientTemperatureDegC_Current, // value = signed short scaled by 10
            SDM_LaserTemperatureDegC_Current, // value = signed short scaled by 10
            SDM_NormalizedSignal_Metric,
            SDM_TiltAngle_Metric_Current, // value = signed short scaled by 10
            SDM_ErrorCode_Metric,
            Reserved_27,
            Reserved_28,
            Reserved_29,
            // Standard data set registers (imperial units)
            SDI_SnowHeightInches_Current = 30, // value = signed short scaled by 20
            SDI_BlockTemperatureDegF_Current, // value = signed short scaled by 10
            SDI_AmbientTemperatureDegF_Current, // value = signed short scaled by 10
            SDI_LaserTemperatureDegF_Current, // value = signed short scaled by 10
            SDI_NormalizedSignal_Imperial,
            SDI_TiltAngle_Imperial_Current, // value = signed short scaled by 10
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
            D_SnowHeight_Inches_Current, // value = signed short scaled by 20
            D_SnowHeight_Inches_Minimum, // value = signed short scaled by 20
            D_SnowHeight_Inches_Maximum, // value = signed short scaled by 20
            D_SnowHeight_Inches_Average, // value = signed short scaled by 20
            D_Calibrated_Inches_Current, // value = signed short scaled by 20
            D_Raw_Inches_Current, // value = signed short scaled by 20
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
            Reserved_96, // TODO: Note: this is missing in the manual!
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
            /// <summary>
            /// The operation was successful.
            /// </summary>
            Success = 0,
            /// <summary>
            /// The command was unknown.
            /// </summary>
            UnknownCommand = 16,
            /// <summary>
            /// The parameter was invalid.
            /// </summary>
            InvalidParameter = 17,
            /// <summary>
            /// The channel was invalid.
            /// </summary>
            InvalidChannel = 36,
            /// <summary>
            /// The device is busy with initialization or calibration processes.
            /// </summary>
            DeviceBusy = 40,
            DisplayRangeOffsetOverflow = 80,
            DisplayRangeOffsetOverflow2 = 81,
            MeasurementRangeOverflow = 82,
            MeasurementRangeOverflow2 = 83,
            MeasurementDataReadError = 84,
            AmbientConditionsError = 85
        }

        private const short ACTION_APPLY = 12871;

        public byte DeviceId { get; private set; }

        private readonly ModbusClient client;

        public LufftShm31ModbusRTU(string port = "COM3", byte deviceId = 1)
        {
            DeviceId = deviceId;
            client = new(port, 19200);
            client.ReadTimeout = client.WriteTimeout = 500;
        }

        public short[] ReadAllRegistersRaw()
        {
            // TODO: is there a way to check if the read has happened successfully!? probably returns null...
            // read and return all (0..119) registers on the device.
            return client.ReadInputRegisters(DeviceId, 0, 120);
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
                return false; // FIXME: certain values might need ushort (like height change acceptance time)
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
            return (regValue / scaleFactor);
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
