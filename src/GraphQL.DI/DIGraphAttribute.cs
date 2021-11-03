using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks a method's return graph type to be a specified DI graph type.
    /// Useful when the return type cannot be inferred (often when it is of type <see cref="object"/>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class DIGraphAttribute : Attribute
    {
        /// <summary>
        /// Marks a method's return graph type to be a specified DI graph type.
        /// </summary>
        /// <param name="graphBaseType">A type that inherits <see cref="DIObjectGraphBase"/>.</param>
        public DIGraphAttribute(Type graphBaseType)
        {
            GraphBaseType = graphBaseType;
        }

        /// <summary>
        /// The DI graph type that inherits <see cref="DIObjectGraphBase"/>.
        /// </summary>
        public Type GraphBaseType { get; }
    }
}
