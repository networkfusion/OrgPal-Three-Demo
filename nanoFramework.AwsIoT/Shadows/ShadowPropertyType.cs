// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;

namespace nanoFramework.AwsIoT.Shadows
{
    /// <summary>
    /// Represents <see cref="Shadow"/> properties
    /// </summary>
    /// <remarks>
    /// Decodes State and Metadata types.
    /// </remarks>
    public class ShadowPropertyType
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShadowPropertyType"/>
        /// </summary>
        public ShadowPropertyType()
        {
            //TODO: we can use uppercase letters for properties if we initialise them here...
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShadowPropertyType"/>
        /// </summary>
        /// <param name="shadowProperty">Hashtable for the shadow property type</param>
        /// <remarks>
        /// Decodes State and Metadata
        /// </remarks>
        public ShadowPropertyType(Hashtable shadowProperty) //or should this be a property collection?
        {
            if (shadowProperty["desired"] != null)
            {
                desired = (Hashtable)shadowProperty["desired"];
            }

            if (shadowProperty["reported"] != null)
            {
                reported = (Hashtable)shadowProperty["reported"];
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
