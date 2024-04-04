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
        /// <summary>
        /// Measurement variable (+offset) is outside the set display range.
        /// </summary>
        DisplayRangeOffsetOverflow = 80,
        /// <summary>
        /// Measurement variable (+offset) is outside the set display range.
        /// </summary>
        DisplayRangeOffsetOverflow2 = 81,
        /// <summary>
        /// Measurement value (physical) is outside the measuring range (e.g. ADC over range).
        /// </summary>
        MeasurementRangeOverflow = 82,
        /// <summary>
        /// Measurement value (physical) is outside the measuring range (e.g. ADC over range).
        /// </summary>
        MeasurementRangeOverflow2 = 83,
        /// <summary>
        /// Error in measurement data or no valid data available.
        /// </summary>
        MeasurementDataReadError = 84,
        /// <summary>
        /// Device / sensor is unable to perform valid measurement due to ambient conditions.
        /// </summary>
        AmbientConditionsError = 85,
        /// <summary>
        /// Unknown or above the allowed value.
        /// </summary>
        UnknownError
    }
}
