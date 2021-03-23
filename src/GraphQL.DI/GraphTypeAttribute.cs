using System;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks a method's (field's) return value to be the specified GraphQL type, or
    /// marks a parameter's (query argument's) input value to be the specified GraphQL type.
    /// </summary>
    //perhaps this should apply to ReturnValue rather than Method
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class GraphTypeAttribute : Attribute
    {
        /// <inheritdoc cref="GraphTypeAttribute"/>
        public GraphTypeAttribute(Type graphType)
        {
            Type = graphType;
        }

        /// <summary>
        /// Returns the graph type specified for the method (field) or parameter (query argument).
        /// </summary>
        public Type Type { get; }
    }
}
