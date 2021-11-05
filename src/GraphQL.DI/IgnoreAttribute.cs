using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks a method (field) to be skipped when constructing the graph type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IgnoreAttribute : NameAttribute
    {
        /// <inheritdoc cref="IgnoreAttribute"/>
        public IgnoreAttribute() : base(null!)
        {
        }
    }
}
