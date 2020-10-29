using System;

namespace GraphQL.DI
{
    //perhaps this should apply to ReturnValue instead of Method
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RequiredAttribute : Attribute
    {
        public RequiredAttribute() { }
    }
}
