namespace OrgPalThreeDemo.Peripherals.LufftShm31
{
    public enum HeatingModeState
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
        OpperationUnavailable = 7,
    }
}
