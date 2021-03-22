using System;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks a class (graph type), method (field) or parameter (query argument) with additional metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public class MetadataAttribute : Attribute
    {
        /// <inheritdoc cref="MetadataAttribute"/>
        public MetadataAttribute(string key, object value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the metadata key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the metadata value.
        /// </summary>
        public object Value { get; set; }
    }
}
