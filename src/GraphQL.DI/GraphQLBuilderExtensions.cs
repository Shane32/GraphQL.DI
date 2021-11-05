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
        /// Registers <see cref="AutoInputObjectGraphType{TSourceType}"/>, <see cref="DIObjectGraphType{TDIGraph}"/> and
        /// <see cref="DIObjectGraphType{TDIGraph, TSource}"/> as generic types.
        /// </summary>
        public static IGraphQLBuilder AddDIGraphTypes(this IGraphQLBuilder builder)
        {
            builder.TryRegister(typeof(AutoInputObjectGraphType<>), typeof(AutoInputObjectGraphType<>), ServiceLifetime.Transient);
            builder.TryRegister(typeof(DIObjectGraphType<>), typeof(DIObjectGraphType<>), ServiceLifetime.Transient);
            builder.TryRegister(typeof(DIObjectGraphType<,>), typeof(DIObjectGraphType<,>), ServiceLifetime.Transient);
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
        /// Skips classes where the source type is <see cref="object"/>, or where the class is marked with
        /// the <see cref="DoNotMapClrTypeAttribute"/>, or where another graph type would be automatically mapped
        /// to the specified type, or where a graph type has already been registered to the specified clr type.
        /// </summary>
        public static IGraphQLBuilder AddDIClrTypeMappings(this IGraphQLBuilder builder, Assembly assembly)
        {
            var typesAlreadyMapped = new HashSet<Type>(
                assembly.GetDefaultClrTypeMappings()
                    .Where(x => x.GraphType.IsOutputType())
                    .Select(x => x.ClrType));

            var types = assembly.GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && typeof(IDIObjectGraphBase).IsAssignableFrom(x))
                .Select<Type, (Type DIGraphType, Type? SourceType)>(x => {
                    var iface = x.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDIObjectGraphBase<>));
                    return (x, iface?.GetGenericArguments()[0]);
                })
                .Where(x => x.SourceType != null && x.SourceType != typeof(object) && !x.DIGraphType.IsDefined(typeof(DoNotMapClrTypeAttribute)) && !typesAlreadyMapped.Contains(x.SourceType))
                .Select<(Type DIGraphType, Type? SourceType), (Type ClrType, Type GraphType)>(x => (x.SourceType!, typeof(DIObjectGraphType<,>).MakeGenericType(x.DIGraphType, x.SourceType!)))
                .ToList();

            if (types.Count == 0)
                return builder;

            builder.ConfigureSchema(schema => {
                var existingMappings = new HashSet<Type>(schema.TypeMappings.Where(x => x.graphType.IsOutputType()).Select(x => x.clrType));
                foreach (var type in types) {
                    if (!existingMappings.Contains(type.ClrType))
                        schema.RegisterTypeMapping(type.ClrType, type.GraphType);
                }
            });

            return builder;
        }

        /// <summary>
        /// Contains a list of types that are scanned for, from which a clr type mapping will be matched
        /// </summary>
        private static readonly Type[] _typesToRegister = new Type[]
            {
                typeof(ObjectGraphType<>),
                typeof(InputObjectGraphType<>),
                typeof(EnumerationGraphType<>),
            };

        /// <summary>
        /// Scans the specified assembly for classes that inherit from <see cref="ObjectGraphType{TSourceType}"/>,
        /// <see cref="InputObjectGraphType{TSourceType}"/>, or <see cref="EnumerationGraphType{TEnum}"/>, and
        /// returns a list of mappings between matched classes and the source type or underlying enum type.
        /// Skips classes where the source type is <see cref="object"/>, or where the class is marked with
        /// the <see cref="DoNotMapClrTypeAttribute"/>.
        /// </summary>
        private static List<(Type ClrType, Type GraphType)> GetDefaultClrTypeMappings(this Assembly assembly)
        {
            //create a list of type mappings
            var typeMappings = new List<(Type clrType, Type graphType)>();

            //loop through each type in the specified assembly
            foreach (var graphType in assembly.GetTypes()) {
                //skip types that are not graph types
                if (!typeof(IGraphType).IsAssignableFrom(graphType))
                    continue;

                //skip abstract types and interfaces
                if (graphType.IsAbstract || graphType.IsInterface)
                    continue;

                //skip types marked with the DoNotRegister attribute
                if (graphType.GetCustomAttributes(false).Any(y => y.GetType() == typeof(DoNotMapClrTypeAttribute)))
                    continue;

                //start with the base type
                var baseType = graphType.BaseType;
                while (baseType != null) {
                    //skip types marked with the DoNotRegister attribute
                    if (baseType.GetCustomAttributes(false).Any(y => y.GetType() == typeof(DoNotMapClrTypeAttribute)))
                        break;

                    //look for generic types that match our list above
                    if (baseType.IsConstructedGenericType && _typesToRegister.Contains(baseType.GetGenericTypeDefinition())) {
                        //get the base type
                        var clrType = baseType.GetGenericArguments()[0];

                        //as long as it's not of type 'object', register it
                        if (clrType != typeof(object))
                            typeMappings.Add((clrType, graphType));

                        //skip to the next type
                        break;
                    }

                    //look up the inheritance chain for a match
                    baseType = baseType.BaseType;
                }
            }

            //return the list of type mappings
            return typeMappings;
        }
    }
}
