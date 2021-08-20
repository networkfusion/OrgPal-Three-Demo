// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;

namespace nanoFramework.AwsIoT.Devices.Shared
{
    /// <summary>
    /// Represents <see cref="Shadow"/> properties
    /// </summary>
    public class ShadowState
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        public ShadowState()
        {
            Desired = new ShadowCollection();
            Reported = new ShadowCollection();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        /// <param name="desired">Hashtable for the desired properties</param>
        /// <param name="reported">Hashtable for the reported properties</param>
        public ShadowState(Hashtable desired, Hashtable reported)
        {
            Desired = new ShadowCollection(desired);
            Reported = new ShadowCollection(reported);
        }

        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> desired properties.
        /// </summary>
        public ShadowCollection Desired { get; set; }

        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> reported properties.
        /// </summary>
        public ShadowCollection Reported { get; set; }
    }
}

