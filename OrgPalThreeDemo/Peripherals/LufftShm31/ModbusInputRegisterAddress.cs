namespace OrgPalThreeDemo.Peripherals.LufftShm31
{
    public enum ModbusInputRegisterAddress
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
}
