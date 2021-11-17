using System.Reflection;
using GraphQL.Resolvers;

namespace GraphQL.DI
{
    internal class PropertyResolver : IFieldResolver
    {
        private readonly PropertyInfo _property;
        public PropertyResolver(PropertyInfo property)
        {
            _property = property;
        }

        public object? Resolve(IResolveFieldContext context)
            => _property.GetValue(context.Source);
    }

}
