using System;
using System.Collections;

namespace nanoFramework.AwsIoT.Shadows
{
    //TODO: this is sooo similar to ShadowState, they should probably be the same!
    public class ShadowMetadata
    {

#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific

        public Hashtable desired { get; set; }
        public Hashtable reported { get; set; }

#pragma warning restore IDE1006 // Naming Styles
    }
}
