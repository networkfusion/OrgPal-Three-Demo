// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using nanoFramework.Json;

namespace nanoFramework.AwsIoT.Devices.Shared
{
    /// <summary>
    /// Shadow Representation.
    /// </summary>
    public class Shadow
    {
        /// <summary>
        /// Creates an instance of <see cref="Shadow"/>.
        /// </summary>
        public Shadow()
        {
            State = new ShadowProperties();
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
            Hashtable props = (Hashtable)JsonConvert.DeserializeObject(jsonShadow, typeof(Hashtable));
            State = new ShadowProperties((Hashtable)props["desired"], (Hashtable)props["reported"]);
        }

        /// <summary>
        /// Creates an instance of <see cref="Shadow"/>.
        /// </summary>
        /// <param name="shadowProperties">The shadow properties.</param>
        public Shadow(ShadowProperties shadowProperties)
        {
            State = shadowProperties;
        }

        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> Id.
        /// </summary>
        public string DeviceId { get; set; }


        /// <summary>
        /// Gets and sets the <see cref="Shadow"/> properties.
        /// </summary>
        public ShadowProperties State { get; set; }

        /// <summary>
        /// Shadow's Version
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// Shadow's Client Token
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// Gets the Shadow as a JSON string.
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson()
        {
            Hashtable ser = new();
            ser.Add("state", State); //TODO: should this be "state"? https://docs.aws.amazon.com/iot/latest/developerguide/device-shadow-document.html

            //if (!string.IsNullOrEmpty(DeviceId))
            //{
            //    ser.Add("deviceid", DeviceId);
            //}

            if (!string.IsNullOrEmpty(ClientToken)) //TODO: uncommend when ready!
            {
                ser.Add("clientToken", ClientToken);
            }

            if (Version != 0)
            {
                ser.Add("version", Version);
            }

            return JsonConvert.SerializeObject(ser);
        }
    }
}
