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
            desired = new Hashtable();
            reported = new Hashtable();

            ////TODO: we need to get the following working (rather than the above!).
            //desired = new ShadowPropertyCollection();
            //reported = new ShadowPropertyCollection();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        /// <param name="_shadowState">Hashtable for the state properties</param>
        public ShadowState(Hashtable _shadowState) //or does this need to be a propertycollection!
        {
            desired = (Hashtable)_shadowState["desired"];
            reported = (Hashtable)_shadowState["reported"];

            ////TODO: we need to get the following working (rather than the above!).
            //desired = new ShadowPropertyCollection((Hashtable) _shadowState["desired"]);
            //reported = new ShadowPropertyCollection((Hashtable) _shadowState["reported"]);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        /// <param name="shadowStateJson">The shadow state as a JSON string</param>
        public ShadowState(string shadowStateJson)
        {
            //Hashtable _shadowState = (Hashtable) JsonConvert.DeserializeObject(shadowStateJson, typeof(Hashtable));
            //desired = new ShadowPropertyCollection((Hashtable) _shadowState["desired"]);
            //reported = new ShadowPropertyCollection((Hashtable) _shadowState["reported"]);
        }



        ///// <summary>
        ///// Initializes a new instance of <see cref="ShadowState"/>
        ///// </summary>
        ///// <param name="desired">Hashtable for the desired properties</param>
        ///// <param name="reported">Hashtable for the reported properties</param>
        //public ShadowState(Hashtable shadowStateDesired, Hashtable shadowStateReported) //or should this be a property collection?
        //{
        //    desired = shadowStateDesired;
        //    reported = shadowStateReported;
        //}

#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific

        ///// <summary>
        ///// Gets and sets the <see cref="Shadow"/> desired properties.
        ///// </summary>
        public Hashtable desired { get; set; }
        //public ShadowPropertyCollection desired { get; set; }

        ///// <summary>
        ///// Gets and sets the <see cref="Shadow"/> reported properties.
        ///// </summary>
        public Hashtable reported { get; set; }
        //public ShadowPropertyCollection reported { get; set; }

#pragma warning restore IDE1006 // Naming Styles

    }
}
