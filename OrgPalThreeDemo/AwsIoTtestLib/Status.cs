// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace nanoFramework.AwsIoT.Devices.Client
{
    /// <summary>
    /// Azure IoT Hub status.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Connection happened.
        /// </summary>
        Connected,

        /// <summary>
        /// Disconnection happened.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Shadow has been updated.
        /// </summary>
        ShadowUpdated,

        /// <summary>
        /// Error updating the shadows.
        /// </summary>
        ShadowUpdateError,

        /// <summary>
        /// Shadow received.
        /// </summary>
        ShadowReceived,

        /// <summary>
        /// Shadow update received.
        /// </summary>
        ShadowUpdateReceived,

        /// <summary>
        /// IoT Hub Error.
        /// </summary>
        IoTCoreError,

        /// <summary>
        /// IoT Hub Warning.
        /// </summary>
        IoTCoreWarning,

        /// <summary>
        /// IoT Hub Information.
        /// </summary>
        IoTCoreInformation,

        /// <summary>
        /// IoT Hub Highlight Information.
        /// </summary>
        IoTCoreHighlightInformation,

        /// <summary>
        /// Internal SDK error.
        /// </summary>
        InternalError,

        /// <summary>
        /// Message received.
        /// </summary>
        MessageReceived,

        /// <summary>
        /// A direct method has been called.
        /// </summary>
        DirectMethodCalled,
    }
}
