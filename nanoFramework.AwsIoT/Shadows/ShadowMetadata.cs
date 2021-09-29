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

            //desired = new ShadowPropertyCollection();
            //reported = new ShadowPropertyCollection();
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ShadowState"/>
        /// </summary>
        /// <param name="_shadowState">Hashtable for the state properties</param>
        public ShadowMetadata(Hashtable _shadowState)
        {
            //desired = new ShadowPropertyCollection((Hashtable) _shadowState["desired"]);
            //reported = new ShadowPropertyCollection((Hashtable) _shadowState["reported"]);
            desired = (Hashtable)_shadowState["desired"];
            reported = (Hashtable)_shadowState["reported"];
        }

#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific

        public Hashtable desired { get; set; }
        public Hashtable reported { get; set; }

        //public ShadowPropertyCollection desired { get; set; }
        //public ShadowPropertyCollection reported { get; set; }

#pragma warning restore IDE1006 // Naming Styles
    }
}
