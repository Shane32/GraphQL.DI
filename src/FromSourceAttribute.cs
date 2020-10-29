using System;

namespace GraphQL.DI
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class FromSourceAttribute : Attribute
    {
    }
}
