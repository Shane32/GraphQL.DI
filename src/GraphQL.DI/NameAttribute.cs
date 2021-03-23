using System;
using GraphQL.Conversion;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks a class (graph type), method (field) or parameter (query argument) with a specified GraphQL name.
    /// Note that the specified name will be translated by the schema's <see cref="INameConverter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NameAttribute : Attribute
    {
        /// <inheritdoc cref="NameAttribute"/>
        public NameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Returns the GraphQL name of the class (graph type), method (field), or parameter (query argument).
        /// </summary>
        public string Name { get; }
    }
}
