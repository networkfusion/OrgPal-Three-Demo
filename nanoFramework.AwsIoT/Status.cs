// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace nanoFramework.AwsIot //TODO: improve for AWS IoT.
{
    /// <summary>
    /// Connection and event status.
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
        /// Shadow deleted.
        /// </summary>
        ShadowDeleted,

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

        //TODO: map the following exceptions?!
        ///// Amazon.IotData.Model.
        ///// <exception cref="InternalFailureException">
        ///// An unexpected error has occurred.
        ///// </exception>
        ///// <exception cref="InvalidRequestException">
        ///// The request is not valid.
        ///// </exception>
        ///// <exception cref="MethodNotAllowedException">
        ///// The specified combination of HTTP verb and URI is not supported.
        ///// </exception>
        ///// <exception cref="ResourceNotFoundException">
        ///// The specified resource does not exist.
        ///// </exception>
        ///// <exception cref="ServiceUnavailableException">
        ///// The service is temporarily unavailable.
        ///// </exception>
        ///// <exception cref="ThrottlingException">
        ///// The rate exceeds the limit.
        ///// </exception>
        ///// <exception cref="UnauthorizedException">
        ///// You are not authorized to perform this operation.
        ///// </exception>
        ///// <exception cref="UnsupportedDocumentEncodingException">
        ///// The document encoding is not supported.
        ///// </exception>
    }
}
