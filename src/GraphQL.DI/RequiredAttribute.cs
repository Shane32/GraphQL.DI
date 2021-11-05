using System;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks a method's (field's) return value as always non-null, or
    /// marks a parameter's (query argument's) input value to be required.
    /// </summary>
    //perhaps this should apply to ReturnValue instead of Method
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class RequiredAttribute : Attribute
    {
    }
}
