using System;
using System.Collections;

namespace nanoFramework.AwsIoT.Shadows
{
    //TODO: this is sooo similar to ShadowState, they should probably be the same!
    public class ShadowMetadata
    {

        public ShadowMetadata()
        {
            desired = new Hashtable();
            reported = new Hashtable();
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        /// <param name="_shadowState">Hashtable for the state properties</param>
        public ShadowMetadata(Hashtable _shadowMetadata)
        {
            desired = (Hashtable)_shadowMetadata["desired"];
            reported = (Hashtable)_shadowMetadata["reported"];
        }

#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific

        public Hashtable desired { get; set; }
        public Hashtable reported { get; set; }

#pragma warning restore IDE1006 // Naming Styles
    }
}
