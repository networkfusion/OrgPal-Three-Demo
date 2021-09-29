// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using nanoFramework.Json;
using System.Collections;

namespace nanoFramework.AwsIoT.Shadows
{
    /// <summary>
    /// Represents a collection of state (or metadata) properties for <see cref="Shadow"/>.
    /// </summary>
    public class ShadowPropertyCollection : IEnumerable
    {
        private readonly Hashtable _shadowPropertyCollection;

        /// <summary>
        /// Creates an empty <see cref="ShadowPropertyCollection"/>.
        /// </summary>
        public ShadowPropertyCollection() : this(string.Empty)
        { }

        /// <summary>
        /// Creates a <see cref="ShadowPropertyCollection"/> using the given JSON fragments for the body.
        /// </summary>
        /// <param name="shadowPropertiesJson">JSON fragment containing the shadow data.</param>        
        public ShadowPropertyCollection(string shadowPropertiesJson)
        {
            _shadowPropertyCollection = string.IsNullOrEmpty(shadowPropertiesJson) ? new() : (Hashtable)JsonConvert.DeserializeObject(shadowPropertiesJson, typeof(Hashtable));
        }

        /// <summary>
        /// Creates a <see cref="ShadowPropertyCollection"/> using the given JSON fragments for the body.
        /// </summary>
        /// <param name="shadow">The JSON hashtable.</param>
        public ShadowPropertyCollection(Hashtable shadowPropertiesCollection)
        {
            _shadowPropertyCollection = shadowPropertiesCollection ?? new();
        }


        /// <summary>
        /// Gets the count of properties in the Collection.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Property Indexer.
        /// </summary>
        /// <param name="propertyName">Name of the property to get.</param>
        /// <returns>Value for the given property name.</returns>

        public object this[string propertyName]
        {
            get
            {
                try
                {
                    return _shadowPropertyCollection[propertyName];
                }
                catch
                {
                    return null;
                }
            }

            set => _shadowPropertyCollection[propertyName] = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonConvert.SerializeObject(_shadowPropertyCollection);
        }

        /// <summary>
        /// Gets the Shadow State/Metadata Properties as a JSON string.
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson() => ToString();

        /// <summary>
        /// Add a new property.
        /// </summary>
        /// <param name="property">The property to add.</param>
        /// <param name="value">The value of the property.</param>
        public void Add(string property, object value)
        {
            _shadowPropertyCollection.Add(property, value);
        }

        /// <summary>
        /// Determines whether the specified property is present.
        /// </summary>
        /// <param name="propertyName">The property to locate.</param>
        /// <returns>True if the specified property is present; otherwise, false.</returns>
        public bool Contains(string propertyName)
        {
            try
            {
                var obj = _shadowPropertyCollection[propertyName];
                return true;
            }
            catch
            {
                // That means it doesn't exist
                return false;
            }
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator() => _shadowPropertyCollection as IEnumerator;
    }
}
