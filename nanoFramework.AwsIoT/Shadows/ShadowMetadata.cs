using System;
using System.Collections;

namespace nanoFramework.AwsIoT.Shadows
{
    //TODO: this is sooo similar to ShadowState, they should probably be the same!
    public class ShadowMetadata
    {

        public ShadowMetadata()
        {
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ShadowMetadata"/>
        /// </summary>
        /// <param name="_shadowState">Hashtable for the state properties</param>
        public ShadowMetadata(Hashtable shadowMetadata)
        {

            if (shadowMetadata["desired"] != null)
            {
                desired = (Hashtable)shadowMetadata["desired"];
            }

            if (shadowMetadata["reported"] != null)
            {
                reported = (Hashtable)shadowMetadata["reported"];
            }
        }

#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific

        public Hashtable desired { get; set; } = new Hashtable();
        public Hashtable reported { get; set; } = new Hashtable();

#pragma warning restore IDE1006 // Naming Styles
    }
}
