// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using nanoFramework.Json;

namespace nanoFramework.AwsIoT.Devices.Shared
{
    /// <summary>
    /// Twin Representation.
    /// </summary>
    public class Shadow
    {
        /// <summary>
        /// Creates an instance of <see cref="Twin"/>.
        /// </summary>
        public Shadow()
        {
            Properties = new ShadowProperties();
        }

        /// <summary>
        /// Creates an instance of <see cref="Twin"/>.
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        public Shadow(string deviceId) : this()
        {
            DeviceId = deviceId;
        }

        /// <summary>
        /// Creates an instance of <see cref="Twin"/>.
        /// </summary>
        /// <param name="deviceId">Device Id.</param>
        /// <param name="jsonTwin">The json twin.</param>
        public Shadow(string deviceId, string jsonShadow)
        {
            DeviceId = deviceId;
            Hashtable props = (Hashtable)JsonConvert.DeserializeObject(jsonShadow, typeof(Hashtable));
            Properties = new ShadowProperties((Hashtable)props["desired"], (Hashtable)props["reported"]);
        }

        /// <summary>
        /// Creates an instance of <see cref="Twin"/>.
        /// </summary>
        /// <param name="twinProperties">The twin properties.</param>
        public Shadow(ShadowProperties shadowProperties)
        {
            Properties = shadowProperties;
        }

        /// <summary>
        /// Gets and sets the <see cref="Twin"/> Id.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// The DTDL model Id of the device.
        /// </summary>
        /// <remarks>
        /// The value will be null for a non-pnp device.
        /// The value will be null for a pnp device until the device connects and registers with the model Id.
        /// </remarks>
        public string ModelId { get; set; }

        /// <summary>
        /// Gets and sets the <see cref="Twin"/> properties.
        /// </summary>
        public ShadowProperties Properties { get; set; }

        /// <summary>
        /// Twin's Version
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// Gets the Twin as a JSON string.
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson()
        {
            Hashtable ser = new();
            ser.Add("properties", Properties);

            if (!string.IsNullOrEmpty(ModelId))
            {
                ser.Add("modelid", ModelId);
            }

            if (!string.IsNullOrEmpty(DeviceId))
            {
                ser.Add("deviceid", DeviceId);
            }

            if (Version != 0)
            {
                ser.Add("$version", Version);
            }

            return JsonConvert.SerializeObject(ser);
        }
    }
}
