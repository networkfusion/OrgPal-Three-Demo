// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Diagnostics;
using nanoFramework.Json;

namespace nanoFramework.AwsIoT.Shadows
{
    /// <summary>
    /// Shadow Representation.
    /// </summary>
    public class Shadow
    {
        // https://docs.aws.amazon.com/iot/latest/developerguide/device-shadow-document.html

        /// <summary>
        /// Creates an instance of <see cref="Shadow"/>.
        /// </summary>
        public Shadow()
        {
            state = new ShadowState();
        }

        ///// <summary>
        ///// Creates an instance of <see cref="Shadow"/>.
        ///// </summary>
        ///// <param name="deviceId">Device Id</param>
        //public Shadow(string deviceId) : this()
        //{
        //    DeviceId = deviceId;
        //}

        /// <summary>
        /// Creates an instance of <see cref="Shadow"/>.
        /// </summary>
        /// <param name="deviceId">Device Id.</param>
        /// <param name="jsonShadow">The json shadow.</param>
        public Shadow(string clientToken, string jsonShadow) //We are parsing the json as a string (rather than a serialized class, so we have to decode it!)
        {
            clienttoken = clienttoken;
            Hashtable shadowStateProps = (Hashtable)JsonConvert.DeserializeObject(jsonShadow, typeof(Hashtable));
            state = new ShadowState((Hashtable)shadowStateProps["desired"], (Hashtable)shadowStateProps["reported"]);

        }

        /// <summary>
        /// Creates an instance of <see cref="Shadow"/>.
        /// </summary>
        /// <param name="shadowProperties">The shadow properties.</param>
        public Shadow(ShadowState shadowState)
        {
            state = shadowState;
        }

        ///// <summary>
        ///// Gets and sets the <see cref="Shadow"/> Id.
        ///// </summary>
        //public string DeviceId { get; set; }

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
        /// <returns>JSON string</returns>
        /// <remarks>Only returns a partial json string for use with updates.</remarks>
        public string ToJson()
        {
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
        }
    }
}
