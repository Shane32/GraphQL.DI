using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks a method's (field's) return value to be an ID graph type, or
    /// marks a parameter's (query argument's) input value to be an ID graph type.
    /// </summary>
    //perhaps this should apply to ReturnValue rather than Method
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class IdAttribute : Attribute
    {
        /// <inheritdoc cref="IdAttribute"/>
        public IdAttribute()
        {
        }
    }
}
