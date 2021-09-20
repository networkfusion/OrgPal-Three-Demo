// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
            desired = new Hashtable(); // ShadowCollection();
            reported = new Hashtable(); // ShadowCollection();
        }

        ///// <summary>
        ///// Initializes a new instance of <see cref="ShadowState"/>
        ///// </summary>
        ///// <param name="desired">Hashtable for the desired properties</param>
        ///// <param name="reported">Hashtable for the reported properties</param>
        //public ShadowState(Hashtable tmp_desired, Hashtable tmp_reported)
        //{
        //    desired = new ShadowCollection(tmp_desired);
        //    reported = new ShadowCollection(tmp_reported);
        //}

        ///// <summary>
        ///// Gets and sets the <see cref="Shadow"/> desired properties.
        ///// </summary>
        //public ShadowCollection desired { get; set; }

        ///// <summary>
        ///// Gets and sets the <see cref="Shadow"/> reported properties.
        ///// </summary>
        //public ShadowCollection reported { get; set; }

#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific

        public Hashtable desired { get; set; }
        public Hashtable reported { get; set; }

#pragma warning restore IDE1006 // Naming Styles
    }
}
