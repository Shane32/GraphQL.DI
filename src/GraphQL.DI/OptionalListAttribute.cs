using System;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks a method's (field's) return value as nullable, or
    /// marks a parameter's (query argument's) input value to be optional.
    /// </summary>
    //perhaps this should apply to ReturnValue instead of Method
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class OptionalListAttribute : Attribute
    {
    }
}
