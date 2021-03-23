using System;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks an parameter (query argument) as being pulled from <see cref="IResolveFieldContext.Source"/>.
    /// The type of this parameter must match the source type as specified in the generic type parameter of
    /// <see cref="DIObjectGraphBase{TSource}"/>. For <see cref="DIObjectGraphBase"/>, the type must be <see cref="object"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class FromServicesAttribute : Attribute
    {
    }
}
