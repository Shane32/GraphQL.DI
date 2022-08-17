using GraphQL.Types;

namespace GraphQL.DI;

internal class DIGraphTypeMappingProvider : IGraphTypeMappingProvider
{
    private readonly Dictionary<Type, Type> _typeDictionary;

    public DIGraphTypeMappingProvider(Dictionary<Type, Type> typeDictionary)
    {
        _typeDictionary = typeDictionary;
    }

    public Type? GetGraphTypeFromClrType(Type clrType, bool isInputType, Type? preferredGraphType)
        => !isInputType && _typeDictionary.TryGetValue(clrType, out var graphType) ? graphType : preferredGraphType;
}
