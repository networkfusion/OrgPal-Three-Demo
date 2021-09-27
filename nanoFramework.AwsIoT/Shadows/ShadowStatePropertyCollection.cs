// Copyright (c) .Net Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using nanoFramework.Json;
using System.Collections;

namespace nanoFramework.AwsIoT.Shadows
{
    /// <summary>
    /// Represents a collection of state (or metadata) properties for <see cref="ShadowState"/>.
    /// </summary>
    public class ShadowStatePropertyCollection : IEnumerable
    {
        internal const string VersionName = "$version"; //Used for versioning the collection.
        private readonly Hashtable _shadowStateProperty;

        /// <summary>
        /// Creates an empty <see cref="ShadowStatePropertyCollection"/>.
        /// </summary>
        public ShadowStatePropertyCollection() : this(string.Empty)
        {
        }

        /// <summary>
        /// Creates a <see cref="ShadowStatePropertyCollection"/> using the given JSON fragments for the body.
        /// </summary>
        /// <param name="shadowJson">JSON fragment containing the shadow data.</param>        
        public ShadowStatePropertyCollection(string shadowJson)
        {
            _shadowStateProperty = string.IsNullOrEmpty(shadowJson) ? new() : (Hashtable)JsonConvert.DeserializeObject(shadowJson, typeof(Hashtable));
        }

        /// <summary>
        /// Creates a <see cref="ShadowStatePropertyCollection"/> using the given JSON fragments for the body.
        /// </summary>
        /// <param name="shadow">The JSON hashtable.</param>
        public ShadowStatePropertyCollection(Hashtable shadow)
        {
            _shadowStateProperty = shadow ?? new();
        }

        /// <summary>
        /// Gets the version of the <see cref="ShadowStatePropertyCollection"/>.
        /// </summary>
        public long Version
        {
            get
            {
                try
                {
                    int ver = (int)_shadowStateProperty[VersionName];
                    return ver;
                }
                catch
                {
                    return default(long);
                }
            }
        }

        /// <summary>
        /// Gets the count of properties in the Collection.
        /// </summary>
        public int Count
        {
            get
            {
                int count = _shadowStateProperty.Count;
                if ((count > 0) && Contains(VersionName))
                {
                    count--;
                }

                return count;
            }
        }

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
                    return _shadowStateProperty[propertyName];
                }
                catch
                {
                    return null;
                }
            }

            set => _shadowStateProperty[propertyName] = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonConvert.SerializeObject(_shadowStateProperty);
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
            _shadowStateProperty.Add(property, value);
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
                var obj = _shadowStateProperty[propertyName];
                return true;
            }
            catch
            {
                // That means it doesn't exist
                return false;
            }
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator() => _shadowStateProperty as IEnumerator;
    }
}
