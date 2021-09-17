// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Diagnostics;
using nanoFramework.Json;

namespace nanoFramework.Aws.IoTCore.Shadows
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
            State = new ShadowState();
        }

        /// <summary>
        /// Creates an instance of <see cref="Shadow"/>.
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        public Shadow(string deviceId) : this()
        {
            DeviceId = deviceId;
        }

        /// <summary>
        /// Creates an instance of <see cref="Shadow"/>.
        /// </summary>
        /// <param name="deviceId">Device Id.</param>
        /// <param name="jsonShadow">The json shadow.</param>
        public Shadow(string deviceId, string jsonShadow)
        {
            DeviceId = deviceId;
            Debug.WriteLine($"shadow was: {jsonShadow}");
            try
            {
                Hashtable shadowState = (Hashtable)JsonConvert.DeserializeObject(jsonShadow, typeof(Hashtable));
                Debug.WriteLine($"Decoded hashtable");
                State = new ShadowState((Hashtable)shadowState["desired"], (Hashtable)shadowState["reported"]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to decode hashtable");
                Debug.WriteLine($"Reason: {ex.ToString()}");
            }
            State = null;

        }

        /// <summary>
        /// Creates an instance of <see cref="Shadow"/>.
        /// </summary>
        /// <param name="shadowProperties">The shadow properties.</param>
        public Shadow(ShadowState shadowState)
        {
            State = shadowState;
        }

        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> Id.
        /// </summary>
        public string DeviceId { get; set; }


        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> properties.
        /// </summary>
        public ShadowState State { get; set; }

        /// <summary>
        /// Shadow's Version
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// Shadow's Client Token
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// Shadow's Timestamp
        /// </summary>
        public string TimeStamp { get; set; } //TODO: should be datetime!

        /// <summary>
        /// Gets the Shadow as a JSON string.
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson()
        {
            Hashtable ser = new();
            ser.Add("state", State);

            //if (!string.IsNullOrEmpty(DeviceId))
            //{
            //    ser.Add("deviceid", DeviceId);
            //}

            if (!string.IsNullOrEmpty(ClientToken))
            {
                ser.Add("clientToken", ClientToken);
            }

            if (Version != 0)
            {
                ser.Add("version", Version);
            }

            if (!string.IsNullOrEmpty(TimeStamp))
            {
                ser.Add("timestamp", TimeStamp);
            }

            return JsonConvert.SerializeObject(ser);
        }
    }
}
