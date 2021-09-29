// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using nanoFramework.Json;

namespace nanoFramework.AwsIoT.Shadows
{
    /// <summary>
    /// Shadow Representation.
    /// </summary>
    /// <remarks>As per: <see cref="https://docs.aws.amazon.com/iot/latest/developerguide/device-shadow-document.html"/></remarks>
    public class Shadow
    {

        /// <summary>
        /// Creates an instance of <see cref="Shadow"/>.
        /// </summary>
        public Shadow()
        {
            state = new ShadowState();
            metadata = new ShadowMetadata();
            version = 0;
            clienttoken = string.Empty; //need to account for this later!
            timestamp = 0; //(int)System.DateTime.UtcNow.ToUnixTimeSeconds(); //should probably be 1970!
        }

        ///// <summary>
        ///// Creates an instance of <see cref="Shadow"/>.
        ///// </summary>
        ///// <param name="clientToken">Client Token.</param>
        ///// <remarks>The client token can be no longer than 64 bytes.</remarks>
        //public Shadow(string clientToken) : this()
        //{
        //    clienttoken = clientToken;
        //    state = new ShadowState();
        //    metadata = new ShadowMetadata(); //TODO: doubt this is needed, but better not being null!
        //}


        public Shadow(string shadowJsonString)
        {
            Hashtable _shadow = (Hashtable)JsonConvert.DeserializeObject(shadowJsonString, typeof(Hashtable));

            state = new ShadowState((Hashtable)_shadow["state"]);
            metadata = new ShadowMetadata((Hashtable)_shadow["metadata"]);
            version = (int) _shadow["version"]; //could be null
            clienttoken = (string) _shadow["clienttoken"]; //this could be null, or a named shadow?!
            timestamp = (int)_shadow["timestamp"];

        }

#pragma warning disable IDE1006 // Naming Styles, disabled due to being Json specific
        /// <summary>
        /// Gets and sets the <see cref="Shadow"/>  state properties.
        /// </summary>
        public ShadowState state { get; set; }

        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> metadata properties.
        /// </summary>
        public ShadowMetadata metadata { get; set; }

        /// <summary>
        /// Shadow's Version
        /// </summary>
        public int version { get; set; }

        /// <summary>
        /// Shadow's Client Token
        /// </summary>
        public string clienttoken { get; set; } //This is optional (may return null)!

        /// <summary>
        /// Shadow's Timestamp
        /// </summary>
        public int timestamp { get; set; } //TODO: should return datetime! and should actually be a float!

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets the Shadow as a JSON string.
        /// </summary>
        /// <param name="fullJsonString"> Optional: Only returns a partial json string for use with updates. unless specified.</param>
        /// <returns>JSON string</returns>
        public string ToJson(bool partialShadow = true)
        {
            //if (partialShadow)
            //{
                //Hashtable ser = new Hashtable();
                //ser.Add("state", state);

                //if (!string.IsNullOrEmpty(clienttoken))
                //{
                //    ser.Add("clienttoken", clienttoken);
                //}

                //return JsonConvert.SerializeObject(ser);

                //TODO: The following is a workaround (and hacky at that)!
                var shadowStringHeader = @"{""state"":""reported""" + JsonConvert.SerializeObject(state.reported);
                var shadowStringBody = string.Empty;
                if (!string.IsNullOrEmpty(clienttoken)) //not sure about this one!
                {
                    shadowStringBody = $",clientToken:{ clienttoken}";
                }
                var shadowStringFooter = "}";
                return shadowStringHeader + shadowStringBody + shadowStringFooter;
            //}
            //else
            //{
                //JsonConvert.SerializeObject(this);
            //}
        }
    }
}
