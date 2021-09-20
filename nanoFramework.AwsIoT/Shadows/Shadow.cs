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

        ///// <summary>
        ///// Creates an instance of <see cref="Shadow"/>.
        ///// </summary>
        //public Shadow()
        //{
        //    State = new ShadowState();
        //}

        ///// <summary>
        ///// Creates an instance of <see cref="Shadow"/>.
        ///// </summary>
        ///// <param name="deviceId">Device Id</param>
        //public Shadow(string deviceId) : this()
        //{
        //    DeviceId = deviceId;
        //}

        ///// <summary>
        ///// Creates an instance of <see cref="Shadow"/>.
        ///// </summary>
        ///// <param name="deviceId">Device Id.</param>
        ///// <param name="jsonShadow">The json shadow.</param>
        //public Shadow(string deviceId, string jsonShadow) //We are parsing the json as a string (rather than a serialized class, so we have to decode it!)
        //{
        //    DeviceId = deviceId;
        //    Debug.WriteLine($"shadow was: {jsonShadow}");
        //    try
        //    {
        //        // TODO: THE OBVIOUS FACT IS THIS CLASS SHOULD ALREADY BE POPULATED VIA SERIALIZATION!
        //        Hashtable shadowContent = (Hashtable)JsonConvert.DeserializeObject(jsonShadow, typeof(Hashtable));
        //        Debug.WriteLine($"Decoded shadow as hashtable");

        //        Version = (long)JsonConvert.DeserializeObject(shadowContent["version"].ToString(), typeof(long));
        //        TimeStamp = (string)JsonConvert.DeserializeObject(shadowContent["timestamp"].ToString(), typeof(string));
        //        //ClientToken is optional!
        //        if (shadowContent["clienttoken"] != null)
        //        {
        //            ClientToken = (string)JsonConvert.DeserializeObject(shadowContent["clienttoken"].ToString(), typeof(string));
        //        }

        //        Hashtable shadowState = (Hashtable)JsonConvert.DeserializeObject(shadowContent["state"].ToString(), typeof(Hashtable));
        //        Hashtable shadowDesired = (Hashtable)JsonConvert.DeserializeObject(shadowState["desired"].ToString(), typeof(Hashtable));
        //        Hashtable shadowReported = (Hashtable)JsonConvert.DeserializeObject(shadowState["reported"].ToString(), typeof(Hashtable));
        //        State = new ShadowState(shadowDesired, shadowReported);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Failed to decode hashtable");
        //        Debug.WriteLine($"Reason: {ex}");
        //        State = null;
        //    }

        //}

        ///// <summary>
        ///// Creates an instance of <see cref="Shadow"/>.
        ///// </summary>
        ///// <param name="shadowProperties">The shadow properties.</param>
        //public Shadow(ShadowState shadowState)
        //{
        //    State = shadowState;
        //}

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
        public int timestamp { get; set; } //TODO: should be datetime!

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets the Shadow as a JSON string.
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson() //TODO: may want to return a partial object, especially when sending a shadow update!
        {
            //TODO: need help from ShadowCollection here, through inspitation of DebugHelper!
            Hashtable serializedShadow = new Hashtable();
            serializedShadow.Add("state", state); //TODO: currently this throws an exception!

            //serializedShadow.Add("metadata", metadata);  //probably dont want to include this regardless!

            //if (!string.IsNullOrEmpty(DeviceId)) //this would be the named shadow, but probably only used in the url (or part of the client token?
            //{
            //    serializedShadow.Add("deviceid", DeviceId);
            //}

            //if (!string.IsNullOrEmpty(clienttoken)) //not sure about this one!
            //{
            //    serializedShadow.Add("clientToken", clienttoken);
            //}

            //if (version != 0) //not sure about this one!
            //{
            //    serializedShadow.Add("version", version);
            //}

            //if (timestamp != 0) //not sure about this one!
            //{
            //    serializedShadow.Add("timestamp", timestamp);
            //}

            return JsonConvert.SerializeObject(serializedShadow);
        }
    }
}
