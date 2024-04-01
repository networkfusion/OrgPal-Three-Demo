namespace OrgPalThreeDemo.Peripherals.LufftShm31
{
    /// <summary>
    /// The status codes for the SHM 31 snow depth sensor can be retrieved in the UMB channels 4100
    /// and 4101.
    /// </summary>
    /// <remarks>
    /// The codes are based on those of the SHM 30 snow depth sensor and have been specifically
    /// expanded.
    /// </remarks>
    public enum DeviceErrorCode
    {
        /// <summary>
        /// Laser: Signal too weak; distance too short.
        /// </summary>
        LaserSignalTooWeak = 15,
        /// <summary>
        /// Laser: Signal too strong (mirror reflection effect).
        /// </summary>
        LaserSignalTooStrong = 16,
        /// <summary>
        /// Laser: Background light level too strong.
        /// </summary>
        LaserBackgroundLightLevelTooStrong = 17,
        /// <summary>
        /// Laser: Measurement disturbed (precipitation, movement, etc.).
        /// </summary>
        LaserMeasurementDisturbed = 18,
        /// <summary>
        /// Laser switched off due to too many timeouts.
        /// </summary>
        LaserTimeoutOff = 19,
        /// <summary>
        /// Laser communication error (unknown command).
        /// </summary>
        LaserCommsInterfaceCommand = 20,
        /// <summary>
        /// Laser communication error (interface).
        /// </summary>
        LaserCommsInterfaceError = 21,
        /// <summary>
        /// Laser communication error (invalid response).
        /// </summary>
        LaserCommsResponseInvalid = 22,
        /// <summary>
        /// Laser temperature below -15°C.
        /// </summary>
        LaserTemperatureTooLow = 23,
        /// <summary>
        /// Laser temperature above +50°C.
        /// </summary>
        LaserTemperatureTooHigh = 24,
        /// <summary>
        /// Hardware error; EEPROM checksum incorrect (sensor must be sent in for repair).
        /// </summary>
        HardwareEepromChecksumIncorrect = 31,
        /// <summary>
        /// Laser: Hardware error; EEPROM checksum incorrect (sensor must be sent in for repair).
        /// </summary>
        LaserEepromChecksumIncorrect = 32,
        /// <summary>
        /// Laser: APD power failure (scattered light or hardware error).
        /// </summary>
        LaserApdPowerFailure = 51,
        /// <summary>
        /// Laser current too high; defective laser (sensor must be sent in for repair).
        /// </summary>
        LaserCurrentTooHigh = 52,
        /// <summary>
        /// Mathematics (division by 0).
        /// </summary>
        MathematicsDivisionByZero = 53,
        /// <summary>
        /// Laser: Hardware error (sensor must be sent in for repair).
        /// </summary>
        LaserHardware = 54,
        /// <summary>
        /// Hardware error (sensor must be sent in for repair).
        /// </summary>
        GeneralHardware = 55,
        /// <summary>
        /// Hardware error in the interface.
        /// </summary>
        GeneralHardwareInterface = 61,
        /// <summary>
        /// Incorrect value in the interface communication (SIO parity error).
        /// </summary>
        SerialParity = 62,
        /// <summary>
        /// SIO overflow; check time for output signals in application software.
        /// </summary>
        SerialOverflow = 63,
        /// <summary>
        /// SIO framing error; serial interface parameter not set correctly to 8N1.
        /// </summary>
        SerialFraming = 64,
        /// <summary>
        /// Evaluation routine: In some cases, measurements in the calculation interval were
        /// ignored because they would have exceeded the maximum permitted change in snow depth.
        /// </summary>
        EvaluationFailure = 65,
        /// <summary>
        /// Evaluation routine: The most recently valid snow depth was output, as all
        /// measurements in the calculation interval would have exceeded the maximum
        /// permitted snow depth change.
        /// </summary>
        EvaluationFailure2 = 66,
        /// <summary>
        /// Measurement was cancelled by Measurement Enable ('MEN') command.
        /// </summary>
        MeasurementCancelled = 67,
        /// <summary>
        /// No valid telegram available yet (e.g. after starting the measurement with 'MST').
        /// </summary>
        TelegramNotAvailable =68,
        /// <summary>
        /// Evaluation routine could not read settings.
        /// </summary>
        ReadSettingsDataFailed = 70,
        /// <summary>
        /// Evaluation routine has not received any data from the laser.
        /// </summary>
        ReadLaserDataFailed = 71,
        /// <summary>
        /// Evaluation routine has no valid laser temperature values.
        /// </summary>
        ReadLaserTemperatureFailed = 72,
        /// <summary>
        /// Evaluation routine has no valid block temperature values
        /// </summary>
        ReadBlockTemperatureFailed = 73,
        /// <summary>
        /// Evaluation routine has no valid outside temperature values.
        /// </summary>
        ReadOutsideTemperatureFailed = 74,
        /// <summary>
        /// Evaluation routine has no valid laser distance measurement values.
        /// </summary>
        ReadLaserDistanceFailed = 75,
        /// <summary>
        /// Evaluation routine: G-sensor vector is an invalid length.
        /// </summary>
        GyroVectorLength = 76,
        /// <summary>
        /// Evaluation routine is using the reference angle, as the current angle from accelerometer is invalid.
        /// </summary>
        ReferenceAngleInvalid = 77,
        /// <summary>
        /// Evaluation routine: Signal calibration: signal_high <= signal_low
        /// </summary>
        SignalDivision = 78,
        /// <summary>
        /// Evaluation routine: Signal calibration: Signal too small.
        /// </summary>
        SignalTooSmall = 79,
        /// <summary>
        /// Evaluation routine: Signal calibration: Signal too large.
        /// </summary>
        SignalTooLarge = 80,
        /// <summary>
        /// Evaluation routine: Signal calibration: no angle correction, as angle > 90 degrees.
        /// </summary>
        NoAngleCorrection = 81,
        /// <summary>
        /// Evaluation routine: channel_average_count too large.
        /// </summary>
        ChannelAverageCountOverflow = 82,
        /// <summary>
        /// Evaluation routine could not initialise ring buffer for avg / min / max channels.
        /// </summary>
        RingBufferInitialization = 83,
    }
}
