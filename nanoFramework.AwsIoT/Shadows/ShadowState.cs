// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using nanoFramework.Json;
using System.Collections;

namespace nanoFramework.AwsIoT.Shadows
{
    /// <summary>
    /// Represents <see cref="ShadowState"/> properties
    /// </summary>
    public class ShadowState
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        public ShadowState()
        {
            desired = new ShadowStatePropertyCollection();
            reported = new ShadowStatePropertyCollection();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        /// <param name="desired">Hashtable for the desired properties</param>
        /// <param name="reported">Hashtable for the reported properties</param>
        public ShadowState(string tmp_state) :this()
        {
            Hashtable _shadowState = (Hashtable)JsonConvert.DeserializeObject(tmp_state, typeof(Hashtable));
            desired = new ShadowStatePropertyCollection(_shadowState["desired"].ToString()); //TODO: this will be interesting!
            reported = new ShadowStatePropertyCollection(_shadowState["reported"].ToString()); //TODO: this will be interesting!
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        /// <param name="desired">Hashtable for the desired properties</param>
        /// <param name="reported">Hashtable for the reported properties</param>
        public ShadowState(Hashtable tmp_desired, Hashtable tmp_reported)
        {
            desired = new ShadowStatePropertyCollection(tmp_desired);
            reported = new ShadowStatePropertyCollection(tmp_reported);
        }


        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> desired properties.
        /// </summary>
        public ShadowStatePropertyCollection desired { get; set; }

        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> reported properties.
        /// </summary>
        public ShadowStatePropertyCollection reported { get; set; }


    }
}
