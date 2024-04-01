namespace OrgPalThreeDemo.Peripherals.LufftShm31
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
        /// <summary>
        /// Set the Block Heating Mode.
        /// </summary>
        SetBlockHeatingMode = 9,
        /// <summary>
        /// Set the Window Heating Mode.
        /// </summary>
        SetWindowHeatingMode,
        /// <summary>
        /// Enable or Disable External Heating Control
        /// </summary>
        SetExternalHeatingMode,
        /// <summary>
        /// Enable or Disable automatic defrost cycle after power on.
        /// </summary>
        SetDefrostingModeAfterPowerOn,
        /// <summary>
        /// Set the Reference Height Measurement in milimeters.
        /// </summary>
        SetReferenceHeight,
        /// <summary>
        /// Set the tilt angle in degrees.
        /// </summary>
        SetTiltAngle,
        /// <summary>
        /// Set whether to use the reference angle or the accelerometer for calculations.
        /// </summary>
        SetTiltAngleMode,
        /// <summary>
        /// Set the time to ignore height changes that exceed the maximum difference.
        /// </summary>
        SetSnowHeightChangeTimeAcceptance = 16,
        /// <summary>
        /// Set the maximum snow height change difference between two measurements.
        /// </summary>
        SetSnowHightChangeMaxDiffAcceptance,
        /// <summary>
        /// Set the laser operating mode.
        /// </summary>
        SetLaserOperatingMode,
        /// <summary>
        /// Set the laser measurement interval.
        /// </summary>
        SetLaserMeasurementInterval = 19,
    }
}
