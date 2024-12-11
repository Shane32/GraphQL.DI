using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.DI;

/// <summary>
/// Wraps a <see cref="DIObjectGraphBase"/> graph type for use with GraphQL. This class should be registered as a singleton
/// within your dependency injection provider.
/// </summary>
public class DIObjectGraphType<TDIGraph> : DIObjectGraphType<TDIGraph, object> where TDIGraph : IDIObjectGraphBase<object>
{
}

/// <summary>
/// Wraps a <see cref="DIObjectGraphBase{TSource}"/> graph type for use with GraphQL. This class should be registered as a singleton
/// within your dependency injection provider.
/// </summary>
public class DIObjectGraphType<TDIGraph, TSource> : AutoRegisteringObjectGraphType<TSource>
    where TDIGraph : IDIObjectGraphBase<TSource>
{
    /// <inheritdoc/>
    protected override void ConfigureGraph()
    {
        // do not configure attributes set on TSource
        // base.ConfigureGraph();

        // configure attributes set on DIObject instead
        var name = typeof(TDIGraph).GraphQLName();
        if (name.EndsWith("Graph", StringComparison.Ordinal) && name.Length > 5)
            name = name.Substring(0, name.Length - 5);
        Name = name;
        Description ??= typeof(TDIGraph).Description();
        DeprecationReason ??= typeof(TDIGraph).ObsoleteMessage();
        var attributes = typeof(TDIGraph).GetCustomAttributes<GraphQLAttribute>();
        foreach (var attr in attributes) {
            attr.Modify(this);
        }
    }

    // only process methods declared directly on TDIGraph -- not anything declared on TSource
    /// <inheritdoc/>
    protected override IEnumerable<MemberInfo> GetRegisteredMembers()
    {
        // only methods are supported
        var methods = typeof(TDIGraph).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(x =>
                !x.ContainsGenericParameters &&               // exclude methods with open generics
                !x.IsSpecialName &&                           // exclude methods generated for properties
                x.ReturnType != typeof(void) &&               // exclude methods which do not return a value
                x.ReturnType != typeof(Task) &&               // exclude methods which do not return a value
                x.GetBaseDefinition() == x &&                 // exclude methods which override an inherited class' method (e.g. GetHashCode)
                                                              // exclude methods generated for record types: bool Equals(TSourceType)
                !(x.Name == "Equals" && !x.IsStatic && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(TDIGraph) && x.ReturnType == typeof(bool)) &&
                x.Name != "<Clone>$");                        // exclude methods generated for record types
        return methods;
    }

    // each field resolver will build a new instance of DIObject
    /// <inheritdoc/>
    protected override LambdaExpression BuildMemberInstanceExpression(MemberInfo memberInfo)
    {
        // use an explicit type here rather than simply LambdaExpression
        var func = typeof(IDisposable).IsAssignableFrom(typeof(TDIGraph))
            ? ((IResolveFieldContext context) => MemberInstanceDisposableFunc(context))
            : (Expression<Func<IResolveFieldContext, TDIGraph>>)((IResolveFieldContext context) => MemberInstanceFunc(context));
        return func;
    }

    /// <inheritdoc/>
    private static TDIGraph MemberInstanceFunc(IResolveFieldContext context)
    {
        // create a new instance of DIObject, filling in any constructor arguments from DI
        var graph = ActivatorUtilities.GetServiceOrCreateInstance<TDIGraph>(context.RequestServices ?? ThrowMissingRequestServicesException());
        // set the context
        graph.Context = context;
        // return the object
        return graph;

        static IServiceProvider ThrowMissingRequestServicesException() => throw new MissingRequestServicesException();
    }

    /// <inheritdoc/>
    private static TDIGraph MemberInstanceDisposableFunc(IResolveFieldContext context)
    {
        // pull DIObject from dependency injection, as it is disposable and must be managed by DI
        var graph = (context.RequestServices ?? throw new MissingRequestServicesException()).GetService<TDIGraph>()
            ?? throw new InvalidOperationException($"Could not resolve an instance of {typeof(TDIGraph).Name} from the service provider. DI graph types that implement IDisposable must be registered in the service provider.");
        // set the context
        graph.Context = context;
        // return the object
        return graph;
    }

    /// <inheritdoc/>
    protected override ArgumentInformation GetArgumentInformation<TParameterType>(FieldType fieldType, ParameterInfo parameterInfo)
    {
        var typeInformation = GetTypeInformation(parameterInfo);
        var argumentInfo = new ArgumentInformation(parameterInfo, typeof(TSource), fieldType, typeInformation);
        if (argumentInfo.ParameterInfo.ParameterType == typeof(IServiceProvider)) {
            argumentInfo.SetDelegate(context => context.RequestServices ?? throw new MissingRequestServicesException());
        }
        if (argumentInfo.ParameterInfo.Name == "source" && argumentInfo.ParameterInfo.ParameterType == typeof(TSource)) {
            argumentInfo.SetDelegate(context => (TSource?)context.Source);
        }
        argumentInfo.ApplyAttributes();
        return argumentInfo;
    }
}
