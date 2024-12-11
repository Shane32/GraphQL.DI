using System.Reflection;
using GraphQL.DI;

namespace GraphQL;

/// <summary>
/// Provides extension methods to configure GraphQL.NET services within a dependency injection framework.
/// </summary>
public static class GraphQLDIBuilderExtensions
{
    /// <summary>
    /// Performs the following:
    /// <list type="bullet">
    /// <item>
    /// Registers <see cref="DIObjectGraphType{TDIGraph}"/> and
    /// <see cref="DIObjectGraphType{TDIGraph, TSource}"/> as generic types.
    /// </item>
    /// <item>
    /// Scans the calling assembly for classes that implement <see cref="IDIObjectGraphBase{TSource}"/>
    /// and registers them as transients within the DI container.
    /// </item>
    /// <item>
    /// Scans the calling assembly for classes that implement <see cref="IDIObjectGraphBase{TSource}"/> and
    /// registers clr type mappings on the schema between that <see cref="DIObjectGraphType{TDIGraph, TSource}"/>
    /// (constructed from that class and its source type), and the source type.
    /// Skips classes where the source type is <see cref="object"/>, or where the class is marked with
    /// the <see cref="DoNotMapClrTypeAttribute"/>, or where another graph type would be automatically mapped
    /// to the specified type, or where a graph type has already been registered to the specified clr type.
    /// </item>
    /// </list>
    /// </summary>
    public static IGraphQLBuilder AddDI(this IGraphQLBuilder builder)
        => AddDI(builder, Assembly.GetCallingAssembly());

    /// <summary>
    /// Performs the following:
    /// <list type="bullet">
    /// <item>
    /// Registers <see cref="DIObjectGraphType{TDIGraph}"/> and
    /// <see cref="DIObjectGraphType{TDIGraph, TSource}"/> as generic types.
    /// </item>
    /// <item>
    /// Scans the specified assembly for classes that implement <see cref="IDIObjectGraphBase{TSource}"/>
    /// and registers them as transients within the DI container.
    /// </item>
    /// <item>
    /// Scans the specified assembly for classes that implement <see cref="IDIObjectGraphBase{TSource}"/> and
    /// registers clr type mappings on the schema between that <see cref="DIObjectGraphType{TDIGraph, TSource}"/>
    /// (constructed from that class and its source type), and the source type.
    /// Skips classes where the source type is <see cref="object"/>, or where the class is marked with
    /// the <see cref="DoNotMapClrTypeAttribute"/>, or where another graph type would be automatically mapped
    /// to the specified type, or where a graph type has already been registered to the specified clr type.
    /// </item>
    /// </list>
    /// </summary>
    public static IGraphQLBuilder AddDI(this IGraphQLBuilder builder, Assembly assembly)
    {
        return builder
            .AddDIGraphTypes()
            .AddDIGraphBases(assembly)
            .AddDIClrTypeMappings(assembly);
    }
}
