using System;
using System.Collections;

namespace OrgPalThreeDemo.Peripherals.LufftShm31
{
    public enum ModbusInputRegisterType : byte
    {
        /// <summary>
        /// Status Information registers.
        /// </summary>
        StatusInformation,
        /// <summary>
        /// Standard data registers (metric units).
        /// </summary>
        StandardMetric,
        /// <summary>
        /// Standard data set registers (imperial units).
        /// </summary>
        StandardImperial,
        /// <summary>
        /// Distance registers.
        /// </summary>
        Distance,
        /// <summary>
        /// Temperature registers (metric units).
        /// </summary>
        TemperaturesMetric,
        /// <summary>
        /// Temperature registers (imperial units).
        /// </summary>
        TemperaturesImperial,
        /// <summary>
        /// Angles registers.
        /// </summary>
        Angles,
        /// <summary>
        /// Logic and normalized values registers.
        /// </summary>
        LogicAndNormalizedValues,
        /// <summary>
        /// Service Channel registers.
        /// </summary>
        ServiceChannels,
        /// <summary>
        /// The Register type was not recognised or above the allowed value.
        /// </summary>
        Unknown = 0xFF

    }

    public enum ModbusRegisterValueType : byte
    {
        /// <summary>
        /// The value is an unsigned short (uint16)
        /// </summary>
        UnsignedShort,
        /// <summary>
        /// The value is a signed short (int16)
        /// </summary>
        /// <remarks>
        /// Might need conversion.
        /// </remarks>
        SignedShort,
        /// <summary>
        /// The value is the upper half of a Unt32
        /// </summary>
        PartalUIntUpper16,
        /// <summary>
        /// The value is the lower half of a Unt32
        /// </summary>
        PartialUIntLower16,
        /// <summary>
        /// The value type was unknown or above the allowed value.
        /// </summary>
        Unknown = 0xFF
    }

    public enum ModbusInputRegisterAddress : byte
    {
        // Status Information registers
        SI_DeviceIdentification = 0, // value = High byte: device subtype, Low byte: software version
        SI_DeviceStatusLower,
        SI_DeviceStatusUpper,
        SI_BlockHeatingState, // value = HeatingModeState
        SI_WindowHeatingState, // value = HeatingModeState
        SI_BlockTemperatureStatus, // value = StatusCode
        SI_AmbientTemperatureStatus, // value = StatusCode
        SI_LaserTemperatureStatus, // value = StatusCode
        SI_TiltAngleStatus, // value = StatusCode
        SI_SnowHeightStatus, // value = StatusCode
        SI_DistanceStatus, // value = StatusCode
        SI_NormalizedSignalStatus, // value = StatusCode
        SI_Reserved_12,
        SI_Reserved_13,
        SI_ErrorCode = 14, // value = DeviceErrorCode
        SI_ErrorCode_Current, // value = DeviceErrorCode
        SI_AccumulatedOpperatingTimeLower,
        SI_AccumulatedOpperatingTimeUpper,
        SI_SystemTimeLower,
        SI_SystemTimeUpper,
        // Standard data registers (metric units)
        SDM_SnowHeightMillimeter_Current = 20, // value = signed short
        SDM_BlockTemperatureDegC_Current, // value = signed short scaled by 10
        SDM_AmbientTemperatureDegC_Current, // value = signed short scaled by 10
        SDM_LaserTemperatureDegC_Current, // value = signed short scaled by 10
        SDM_NormalizedSignal_Metric,
        SDM_TiltAngle_Metric_Current, // value = signed short scaled by 10
        SDM_ErrorCode_Metric,
        SDM_Reserved_27,
        SDM_Reserved_28,
        SDM_Reserved_29,
        // Standard data set registers (imperial units)
        SDI_SnowHeightInches_Current = 30, // value = signed short scaled by 20
        SDI_BlockTemperatureDegF_Current, // value = signed short scaled by 10
        SDI_AmbientTemperatureDegF_Current, // value = signed short scaled by 10
        SDI_LaserTemperatureDegF_Current, // value = signed short scaled by 10
        SDI_NormalizedSignal_Imperial,
        SDI_TiltAngle_Imperial_Current, // value = signed short scaled by 10
        SDI_ErrorCode_Imperial,
        SDI_Reserved_37,
        SDI_Reserved_38,
        SDI_Reserved_39,
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
        D_SnowHeight_millimeter_HighRes, //value = unsigned short scaled by 10
        D_Reserved_54,
        // Temperature Registers (metric units)
        TM_BlockTemperatureDegC_Current = 55, // value = signed short scaled by 10
        TM_BlockTemperatureDegC_Minimum, // value = signed short scaled by 10
        TM_BlockTemperatureDegC_Maximum, // value = signed short scaled by 10
        TM_BlockTemperatureDegC_Average, // value = signed short scaled by 10
        TM_AmbientTemperatureDegC_Current, // value = signed short scaled by 10
        TM_AmbientTemperatureDegC_Minimum, // value = signed short scaled by 10
        TM_AmbientTemperatureDegC_Maximum, // value = signed short scaled by 10
        TM_AmbientTemperatureDegC_Average, // value = signed short scaled by 10
        TM_LaserTemperatureDegC_Current, // value = signed short scaled by 10
        TM_LaserTemperatureDegC_Minimum, // value = signed short scaled by 10
        TM_LaserTemperatureDegC_Maximum, // value = signed short scaled by 10
        TM_LaserTemperatureDegC_Average, // value = signed short scaled by 10
        TM_Reserved_67,
        TM_Reserved_68,
        TM_Reserved_69,
        // Temperature Registers (imperial units)
        TI_BlockTemperatureDegF_Current = 70, // value = signed short scaled by 10
        TI_BlockTemperatureDegF_Minimum, // value = signed short scaled by 10
        TI_BlockTemperatureDegF_Maximum, // value = signed short scaled by 10
        TI_BlockTemperatureDegF_Average, // value = signed short scaled by 10
        TI_AmbientTemperatureDegF_Current, // value = signed short scaled by 10
        TI_AmbientTemperatureDegF_Minimum, // value = signed short scaled by 10
        TI_AmbientTemperatureDegF_Maximum, // value = signed short scaled by 10
        TI_AmbientTemperatureDegF_Average, // value = signed short scaled by 10
        TI_LaserTemperatureDegF_Current, // value = signed short scaled by 10
        TI_LaserTemperatureDegF_Minimum, // value = signed short scaled by 10
        TI_LaserTemperatureDegF_Maximum, // value = signed short scaled by 10
        TI_LaserTemperatureDegF_Average, // value = signed short scaled by 10
        TI_Reserved_82,
        TI_Reserved_83,
        TI_Reserved_84,
        // Angles registers
        A_AngleTilt_Current = 85, // value = signed short scaled by 10
        A_AngleTilt_Minimum, // value = signed short scaled by 10
        A_AngleTilt_Maximum, // value = signed short scaled by 10
        A_AngleTilt_Average, // value = signed short scaled by 10
        A_AngleX_Current = 89, // value = signed short scaled by 10
        A_AngleY_Current, // value = signed short scaled by 10
        A_AngleZ_Current, // value = signed short scaled by 10
        A_TiltAngleReference, // value = signed short scaled by 10
        A_Reserved_93,
        A_Reserved_94,
        // Logic and normalized values registers
        LNV_SnowFlag = 95,
        LNV_Reserved_96, // TODO: Note: this is missing in the manual!
        LNV_NormalizedSignal_Current = 97,
        LNV_NormalizedSignal_Minimum,
        LNV_NormalizedSignal_Maximum,
        LNV_NormalizedSignal_Average,
        LNV_Reserved_101,
        LNV_Reserved_102,
        LNV_Reserved_103,
        LNV_Reserved_104,
        // Service Channel Registers
        SC_BlockHeatingState = 105, // value = HeatingModeState
        SC_InternalTemperatureDegC_NTC, // value = signed short scaled by 10
        SC_Reserved_107,
        SC_BlockHeatingDefrostTime_Seconds,
        SC_WindowHeatingState,
        SC_ExternalTemperatureDegC_NTC = 110, // value = signed short scaled by 10
        SC_Reserved_111,
        SC_WindowHeatingDefrostTime_Seconds = 112,
        SC_LaserGainCode = 113,
        SC_LaserSignalIntensity_uV = 114, // value = signed short scaled by 0.1
        SC_LaserDistance_Millimeter = 115,
        SC_LaserTemperatureDegC = 116, // value = signed short scaled by 10
        SC_OperatingVoltage = 117, // value = signed short scaled by 10
        SC_Reserved_118,
        SC_Reserved_119,
        /// <summary>
        /// The register is unknown or above the allowed value.
        /// </summary>
        Unknown = 0xFF
    }

    /// <summary>
    /// The valid range of a register value.
    /// </summary>
    public class ModbusRegisterValueRange
    {
        // requires int as the value might be short or ushort, but by default should use ushort.
        public int MinimumValue { get; set; } = ushort.MinValue;
        public int MaximumValue { get; set; } = ushort.MaxValue;

    }

    public class ModbusInputRegister
    {
        public ModbusInputRegisterAddress RegisterAddress { get; set; } = 0;
        public ModbusInputRegisterType RegisterType { get; set; } = ModbusInputRegisterType.Unknown;
        public ModbusRegisterValueType ValueType { get; set; } = ModbusRegisterValueType.UnsignedShort;
        public float ValueScaleFactor { get; set; } = 0.0f;
        public ModbusRegisterValueRange ValueRange { get; set; } = new ModbusRegisterValueRange();
        // public int ValueUnitsType { get; set; } // TODO: add units (mm, inch, etc.).

    }

    public class ModbusInputRegisters
    {
        public Hashtable Shm31InputRegisters;

        // TODO: add all the registers.
        public ModbusInputRegisters()
        {
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SI_DeviceIdentification,
                new ModbusInputRegister {
                    RegisterAddress = ModbusInputRegisterAddress.SI_DeviceIdentification,
                    RegisterType = ModbusInputRegisterType.StatusInformation
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SI_DeviceStatusLower,
                new ModbusInputRegister {
                    RegisterAddress = ModbusInputRegisterAddress.SI_DeviceStatusLower,
                    RegisterType = ModbusInputRegisterType.StatusInformation,
                    ValueType = ModbusRegisterValueType.PartialUIntLower16
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SI_DeviceStatusUpper,
                new ModbusInputRegister {
                    RegisterAddress = ModbusInputRegisterAddress.SI_DeviceStatusUpper,
                    RegisterType = ModbusInputRegisterType.StatusInformation,
                    ValueType = ModbusRegisterValueType.PartalUIntUpper16
                });

            // ...
            // Standard Data Metric
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDM_SnowHeightMillimeter_Current,
                new ModbusInputRegister {
                    RegisterAddress = ModbusInputRegisterAddress.SDM_SnowHeightMillimeter_Current,
                    RegisterType = ModbusInputRegisterType.StandardMetric,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -16000, MaximumValue = 16000 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDM_BlockTemperatureDegC_Current,
                new ModbusInputRegister {
                    RegisterAddress = ModbusInputRegisterAddress.SDM_BlockTemperatureDegC_Current,
                    RegisterType = ModbusInputRegisterType.StandardMetric,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueScaleFactor = 10,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -400, MaximumValue = 1000 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDM_AmbientTemperatureDegC_Current,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDM_AmbientTemperatureDegC_Current,
                    RegisterType = ModbusInputRegisterType.StandardMetric,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueScaleFactor = 10,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -500, MaximumValue = 1000 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDM_LaserTemperatureDegC_Current,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDM_LaserTemperatureDegC_Current,
                    RegisterType = ModbusInputRegisterType.StandardMetric,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueScaleFactor = 10,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -600, MaximumValue = 800 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDM_NormalizedSignal_Metric,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDM_NormalizedSignal_Metric,
                    RegisterType = ModbusInputRegisterType.StandardMetric,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = 0, MaximumValue = 255 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDM_TiltAngle_Metric_Current,
                new ModbusInputRegister {
                    RegisterAddress = ModbusInputRegisterAddress.SDM_TiltAngle_Metric_Current,
                    RegisterType = ModbusInputRegisterType.StandardMetric,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueScaleFactor = 10,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -1800, MaximumValue = 1800 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDM_ErrorCode_Metric,
                new ModbusInputRegister {
                    RegisterAddress = ModbusInputRegisterAddress.SDM_ErrorCode_Metric,
                    RegisterType = ModbusInputRegisterType.StandardMetric,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = 0, MaximumValue = 255 }
                });
            // Standard Data Imperial
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDI_SnowHeightInches_Current,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDI_SnowHeightInches_Current,
                    RegisterType = ModbusInputRegisterType.StandardImperial,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueScaleFactor = 20,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -12598, MaximumValue = 12598 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDI_BlockTemperatureDegF_Current,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDI_BlockTemperatureDegF_Current,
                    RegisterType = ModbusInputRegisterType.StandardImperial,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueScaleFactor = 10,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -400, MaximumValue = 2120 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDI_AmbientTemperatureDegF_Current,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDI_AmbientTemperatureDegF_Current,
                    RegisterType = ModbusInputRegisterType.StandardImperial,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueScaleFactor = 10,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -580, MaximumValue = 2120 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDI_LaserTemperatureDegF_Current,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDI_LaserTemperatureDegF_Current,
                    RegisterType = ModbusInputRegisterType.StandardImperial,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueScaleFactor = 10,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -760, MaximumValue = 1760 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDI_NormalizedSignal_Imperial,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDI_NormalizedSignal_Imperial,
                    RegisterType = ModbusInputRegisterType.StandardImperial,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = 0, MaximumValue = 255 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDI_TiltAngle_Imperial_Current,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDI_TiltAngle_Imperial_Current,
                    RegisterType = ModbusInputRegisterType.StandardImperial,
                    ValueType = ModbusRegisterValueType.SignedShort,
                    ValueScaleFactor = 10,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = -1800, MaximumValue = 1800 }
                });
            Shm31InputRegisters.Add((ushort)ModbusInputRegisterAddress.SDI_ErrorCode_Imperial,
                new ModbusInputRegister
                {
                    RegisterAddress = ModbusInputRegisterAddress.SDI_ErrorCode_Imperial,
                    RegisterType = ModbusInputRegisterType.StandardImperial,
                    ValueRange = new ModbusRegisterValueRange { MinimumValue = 0, MaximumValue = 255 }
                });
            // ...
            // Service channels
            // ...
        }
    }
}
