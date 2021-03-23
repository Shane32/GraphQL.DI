using System;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks an parameter (query argument) as being pulled via <see cref="IResolveFieldContext.RequestServices"/>.
    /// For concurrent fields specified to run in a separate scope, the service will be pulled from the dedicated
    /// service scope created for that field resolver.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class FromSourceAttribute : Attribute
    {
    }
}
