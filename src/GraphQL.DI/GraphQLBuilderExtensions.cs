using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <summary>
    /// Provides extension methods to configure GraphQL.NET services within a dependency injection framework.
    /// </summary>
    public static class GraphQLBuilderExtensions
    {
        /// <summary>
        /// Registers <see cref="DIObjectGraphType{TDIGraph}"/> and
        /// <see cref="DIObjectGraphType{TDIGraph, TSource}"/> as generic types.
        /// </summary>
        public static IGraphQLBuilder AddDIGraphTypes(this IGraphQLBuilder builder)
        {
            builder.Services.TryRegister(typeof(DIObjectGraphType<>), typeof(DIObjectGraphType<>), ServiceLifetime.Transient);
            builder.Services.TryRegister(typeof(DIObjectGraphType<,>), typeof(DIObjectGraphType<,>), ServiceLifetime.Transient);
            return builder;
        }

        /// <summary>
        /// Scans the calling assembly for classes that implement <see cref="IDIObjectGraphBase{TSource}"/> and
        /// registers clr type mappings on the schema between that <see cref="DIObjectGraphType{TDIGraph, TSource}"/>
        /// (constructed from that class and its source type), and the source type.
        /// Skips classes where the source type is <see cref="object"/>, or where the class is marked with
        /// the <see cref="DoNotMapClrTypeAttribute"/>, or where another graph type would be automatically mapped
        /// to the specified type, or where a graph type has already been registered to the specified clr type.
        /// </summary>
        public static IGraphQLBuilder AddDIClrTypeMappings(this IGraphQLBuilder builder)
            => AddDIClrTypeMappings(builder, Assembly.GetCallingAssembly());

        /// <summary>
        /// Scans the specified assembly for classes that implement <see cref="IDIObjectGraphBase{TSource}"/> and
        /// registers clr type mappings on the schema between that <see cref="DIObjectGraphType{TDIGraph, TSource}"/>
        /// (constructed from that class and its source type), and the source type.
        /// Skips classes where the source type is <see cref="object"/> or where the class is marked with
        /// the <see cref="DoNotMapClrTypeAttribute"/>.
        /// </summary>
        public static IGraphQLBuilder AddDIClrTypeMappings(this IGraphQLBuilder builder, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && typeof(IDIObjectGraphBase).IsAssignableFrom(x))
                .Select<Type, (Type DIGraphType, Type? SourceType)>(x => {
                    var iface = x.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDIObjectGraphBase<>));
                    return (x, iface?.GetGenericArguments()[0]);
                })
                .Where(x => x.SourceType != null && x.SourceType != typeof(object) && !x.DIGraphType.IsDefined(typeof(DoNotMapClrTypeAttribute)))
                .Select<(Type DIGraphType, Type? SourceType), (Type ClrType, Type GraphType)>(x => (x.SourceType!, typeof(DIObjectGraphType<,>).MakeGenericType(x.DIGraphType, x.SourceType!)))
                .ToDictionary(x => x.ClrType, x => x.GraphType);

            if (types.Count == 0)
                return builder;

            builder.AddGraphTypeMappingProvider(new DIGraphTypeMappingProvider(types));

            return builder;
        }
    }
}
