﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using nanoFramework.AwsIoT.Devices.Client;
using System;

namespace nanoFramework.AwsIoT.Devices.Shared
{
    /// <summary>
    /// Delegate for Twin updated.
    /// </summary>
    /// <param name="sender">The <see cref="DeviceClient"/> sender.</param>
    /// <param name="e">The Twin updated event arguments.</param>
    public delegate void ShadowUpdated(object sender, ShadowUpdateEventArgs e);

    /// <summary>
    /// Twin updated event arguments.
    /// </summary>
    public class ShadowUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for Shadow updated event arguments.
        /// </summary>
        /// <param name="shadow">The twin collection.</param>
        public ShadowUpdateEventArgs(ShadowCollection shadow)
        {
            AwsShadow = shadow;
        }

        /// <summary>
        /// Twin collection.
        /// </summary>
        public ShadowCollection AwsShadow { get; set; }
    }
}