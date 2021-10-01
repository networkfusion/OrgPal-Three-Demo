﻿// Copyright (c) .Net Foundation. All rights reserved.
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
        }


        public Shadow(string shadowJsonString)
        {
            Hashtable _shadow = (Hashtable)JsonConvert.DeserializeObject(shadowJsonString, typeof(Hashtable)); //TODO: could probably just decode to this now?!

            if (_shadow["state"] != null)
            {
                state = new ShadowState((Hashtable)_shadow["state"]);
            }
            if (_shadow["metadata"] != null)
            {
                metadata = new ShadowMetadata((Hashtable)_shadow["metadata"]);
            }
            if (_shadow["version"] != null)
            {
                version = (int)_shadow["version"]; //could be null
            }
            if (_shadow["clienttoken"] != null)
            {
                clienttoken = (string)_shadow["clienttoken"]; //this could be null, or a named shadow?!
            }
            if (_shadow["timestamp"] != null)
            {
                timestamp = (int)_shadow["timestamp"];
            }

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
        public string clienttoken { get; set; }

        /// <summary>
        /// Shadow's Timestamp
        /// </summary>
        public int timestamp { get; set; } //technically this should be a long?!

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets the Shadow as a JSON string.
        /// </summary>
        /// <param name="fullJsonString"> Optional: Only returns a partial json string for use with updates. unless specified.</param>
        /// <returns>JSON string</returns>
        public string ToJson(bool updateShadow = true)
        {
            if (updateShadow)
            {
                //if (state.reported != null)
                //{
                //    Hashtable serShadow = new Hashtable();
                //    Hashtable serState = new Hashtable();

                //    serState.Add("reported", state.reported);
                //    serShadow.Add("state", serState);
                //    if (!string.IsNullOrEmpty(clienttoken))
                //    {
                //        serShadow.Add("clienttoken", clienttoken);
                //    }
                //    var shadow = JsonConvert.SerializeObject(serShadow);
                //    return shadow;
                //}

                //TODO: The following is a workaround (and hacky at that)!
                var shadowStringHeader = @"{""state"":""reported""" + JsonConvert.SerializeObject(state.reported);
                var shadowStringBody = string.Empty;
                if (!string.IsNullOrEmpty(clienttoken)) //not sure about this one!
                {
                    shadowStringBody = $",clientToken:{ clienttoken}";
                }
                var shadowStringFooter = "}";
                return shadowStringHeader + shadowStringBody + shadowStringFooter;
            }
            else
            {
                JsonConvert.SerializeObject(this);
            }
            return @"{""shadow"" : ""Serialization-Error""}"; //technically unreachable?!
        }
    }
}
