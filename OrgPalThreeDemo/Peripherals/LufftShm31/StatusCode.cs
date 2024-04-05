namespace OrgPalThreeDemo.Peripherals.LufftShm31
{
    public enum StatusCode : byte
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        Success = 0,
        /// <summary>
        /// The command was unknown.
        /// </summary>
        UnknownCommand = 16, // 0x10
        /// <summary>
        /// The parameter was invalid.
        /// </summary>
        InvalidParameter = 17, // 0x11
        /// <summary>
        /// The channel was invalid.
        /// </summary>
        InvalidChannel = 36, // 0x24
        /// <summary>
        /// The device is busy with initialization or calibration processes.
        /// </summary>
        DeviceBusy = 40, // 0x28
        /// <summary>
        /// Measurement variable (+offset) is outside the set display range.
        /// </summary>
        DisplayRangeOffsetOverflow = 80, // 0x50
        /// <summary>
        /// Measurement variable (+offset) is outside the set display range.
        /// </summary>
        DisplayRangeOffsetOverflow2 = 81, // 0x51
        /// <summary>
        /// Measurement value (physical) is outside the measuring range (e.g. ADC over range).
        /// </summary>
        MeasurementRangeOverflow = 82, // 0x52
        /// <summary>
        /// Measurement value (physical) is outside the measuring range (e.g. ADC over range).
        /// </summary>
        MeasurementRangeOverflow2 = 83, // 0x53
        /// <summary>
        /// Error in measurement data or no valid data available.
        /// </summary>
        MeasurementDataReadError = 84, // 0x54
        /// <summary>
        /// Device / sensor is unable to perform valid measurement due to ambient conditions.
        /// </summary>
        AmbientConditionsError = 85, // 0x55
        /// <summary>
        /// Unknown or above the allowed value.
        /// </summary>
        UnknownError
    }
}
