namespace OrgPalThreeDemo.Peripherals.LufftShm31
{
    public enum HeatingMode
    {
        /// <summary>
        /// Turn the heating off.
        /// </summary>
        Off = 0,
        /// <summary>
        /// Set the heating mode to automatic.
        /// </summary>
        Automatic = 1,
        /// <summary>
        /// Start a defrost operation.
        /// </summary>
        StartDefrosting = 2,
        /// <summary>
        /// Stop a defrost opperation.
        /// </summary>
        StopDefrosting = 3
    }

    public enum HeatingModeState
    {
        /// <summary>
        /// The heating is OFF.
        /// </summary>
        Off = 0,
        /// <summary>
        /// The heating is ON using 12 VDC power input.
        /// </summary>
        On_12Volts = 1,
        /// <summary>
        /// The heating is ON using 24 VDC power input.
        /// </summary>
        On_24Volts = 2,
        /// <summary>
        /// The heating is ON and currently defrosting using 12 VDC power input.
        /// </summary>
        Defrosting_12Volts = 3,
        /// <summary>
        /// The heating is ON and currently defrosting using 24 VDC power input.
        /// </summary>
        Defrosting_24Volts = 4,
        /// <summary>
        /// The heating mode is disabled.
        /// </summary>
        HeatingDisabled = 5,
        /// <summary>
        /// There was an internal voltage control error.
        /// </summary>
        VoltageControlError = 6,
        /// <summary>
        /// The operation is unavailable due to either incorrect configuration or temperature values.
        /// </summary>
        OpperationUnavailable = 7,
    }
}
