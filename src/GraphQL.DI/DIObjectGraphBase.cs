using System.Security.Claims;
using GraphQL.Execution;
using GraphQL.Instrumentation;
using GraphQL.Types;
using GraphQLParser.AST;

namespace GraphQL.DI;

/// <summary>
/// This is a required base type of all DI-created graph types. <see cref="DIObjectGraphBase"/> may be
/// used if the <see cref="IResolveFieldContext.Source"/> type is <see cref="object"/>.
/// </summary>
public abstract class DIObjectGraphBase<TSource> : IDIObjectGraphBase<TSource>, IResolveFieldContext<TSource>
{
    /// <summary>
    /// The <see cref="IResolveFieldContext"/> for the current field.
    /// </summary>
    public IResolveFieldContext Context { get; private set; } = null!;

    /// <inheritdoc cref="IResolveFieldContext.Source"/>
    public TSource Source => (TSource)Context.Source!;

    /// <inheritdoc cref="IResolveFieldContext.CancellationToken"/>
    public CancellationToken RequestAborted => Context.CancellationToken;

    /// <inheritdoc cref="IProvideUserContext.UserContext"/>
    public IDictionary<string, object?> UserContext => Context.UserContext;

    /// <inheritdoc cref="IResolveFieldContext.User"/>
    public ClaimsPrincipal? User => Context.User;

    /// <inheritdoc cref="IResolveFieldContext.Metrics"/>
    public Metrics Metrics => Context.Metrics;

    IResolveFieldContext IDIObjectGraphBase.Context { get => Context; set => Context = value; }
    GraphQLField IResolveFieldContext.FieldAst => Context.FieldAst;
    FieldType IResolveFieldContext.FieldDefinition => Context.FieldDefinition;
    IObjectGraphType IResolveFieldContext.ParentType => Context.ParentType;
    IResolveFieldContext IResolveFieldContext.Parent => Context.Parent!;
    IDictionary<string, ArgumentValue>? IResolveFieldContext.Arguments => Context.Arguments;
    object? IResolveFieldContext.RootValue => Context.RootValue;
    object IResolveFieldContext.Source => Context.Source!;
    ISchema IResolveFieldContext.Schema => Context.Schema;
    GraphQLDocument IResolveFieldContext.Document => Context.Document;
    GraphQLOperationDefinition IResolveFieldContext.Operation => Context.Operation;
    Validation.Variables IResolveFieldContext.Variables => Context.Variables;
    CancellationToken IResolveFieldContext.CancellationToken => Context.CancellationToken;
    ExecutionErrors IResolveFieldContext.Errors => Context.Errors;
    IEnumerable<object> IResolveFieldContext.Path => Context.Path;
    IEnumerable<object> IResolveFieldContext.ResponsePath => Context.ResponsePath;
    IServiceProvider IResolveFieldContext.RequestServices => Context.RequestServices!;
    IExecutionArrayPool IResolveFieldContext.ArrayPool => Context.ArrayPool;
    IDictionary<string, DirectiveInfo>? IResolveFieldContext.Directives => Context.Directives;
    Dictionary<string, (GraphQLField Field, FieldType FieldType)>? IResolveFieldContext.SubFields => Context.SubFields;
    IReadOnlyDictionary<string, object?> IResolveFieldContext.InputExtensions => Context.InputExtensions;
    IDictionary<string, object?> IResolveFieldContext.OutputExtensions => Context.OutputExtensions;
}

/// <summary>
/// This is a required base type of all DI-created graph types. <see cref="DIObjectGraphBase{TSource}"/> may be
/// used when the <see cref="IResolveFieldContext.Source"/> type is not <see cref="object"/>.
/// </summary>
public abstract class DIObjectGraphBase : DIObjectGraphBase<object>
{
}
