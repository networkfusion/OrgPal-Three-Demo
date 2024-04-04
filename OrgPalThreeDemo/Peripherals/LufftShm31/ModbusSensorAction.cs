namespace OrgPalThreeDemo.Peripherals.LufftShm31
{
    public enum ModbusSensorAction : byte
    {
        /// <summary>
        /// Initiate a reboot of the sensor.
        /// </summary>
        InitiateReboot = 0,
        /// <summary>
        /// Start performing measurement operations.
        /// </summary>
        StartMeasurements = 1,
        /// <summary>
        /// Stop performing measurement operations.
        /// </summary>
        StopMeasurements = 2,
        /// <summary>
        /// Turn the laser on permanently (e.g. for installation alignment).
        /// </summary>
        TurnLaserOnPermanently = 3,
        /// <summary>
        /// Put the laser back into its normal operating mode (after TurnLaserOnPermanently).
        /// </summary>
        ResumeNormalLaserSchedule = 4,
        /// <summary>
        /// Perform calibration of the height using the tilt angle from the gyroscope.
        /// </summary>
        InitiateFullCalibration = 5,
        /// <summary>
        /// Perform calibration of the height using the reference angle (ignore gyroscope).
        /// </summary>
        InitiateHeightCalibration = 6,
        /// <summary>
        /// Start a defrost process.
        /// </summary>
        InitiateDefrost = 7,
        /// <summary>
        /// Stop a defrost process.
        /// </summary>
        StopDefrosting = 8,
        /// <summary>
        /// Set the Block Heating Mode.
        /// </summary>
        SetBlockHeatingMode = 9,
        /// <summary>
        /// Set the Window Heating Mode.
        /// </summary>
        SetWindowHeatingMode = 10,
        /// <summary>
        /// Enable or Disable External Heating Control
        /// </summary>
        SetExternalHeatingMode = 11,
        /// <summary>
        /// Enable or Disable automatic defrost cycle after power on.
        /// </summary>
        SetDefrostingModeAfterPowerOn = 12,
        /// <summary>
        /// Set the Reference Height Measurement in milimeters.
        /// </summary>
        SetReferenceHeight = 13,
        /// <summary>
        /// Set the tilt angle in degrees.
        /// </summary>
        SetTiltAngle = 14,
        /// <summary>
        /// Set whether to use the reference angle or the accelerometer for calculations.
        /// </summary>
        SetTiltAngleMode = 15,
        /// <summary>
        /// Set the time to ignore height changes that exceed the maximum difference.
        /// </summary>
        SetSnowHeightChangeTimeAcceptance = 16,
        /// <summary>
        /// Set the maximum snow height change difference between two measurements.
        /// </summary>
        SetSnowHightChangeMaxDiffAcceptance = 17,
        /// <summary>
        /// Set the laser operating mode.
        /// </summary>
        SetLaserOperatingMode = 18,
        /// <summary>
        /// Set the laser measurement interval.
        /// </summary>
        SetLaserMeasurementInterval = 19
    }
}
