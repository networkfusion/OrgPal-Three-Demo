// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

//using nanoFramework.Json;
using System.Collections;

namespace nanoFramework.AwsIoT.Shadows
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
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        /// <param name="shadowState">Hashtable for the shadow state</param>
        public ShadowState(Hashtable shadowState) //or should this be a property collection?
        {
            if (shadowState["desired"] != null)
            {
                desired = (Hashtable)shadowState["desired"];
            }

            if (shadowState["reported"] != null)
            {
                reported = (Hashtable)shadowState["reported"];
            }
        }

#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific

        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> desired properties.
        /// </summary>
        public Hashtable desired { get; set; } = new Hashtable();

        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> reported properties.
        /// </summary>
        public Hashtable reported { get; set; } = new Hashtable();

#pragma warning restore IDE1006 // Naming Styles

    }
}
