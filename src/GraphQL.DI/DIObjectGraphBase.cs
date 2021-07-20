using System;
using System.Collections.Generic;
using System.Threading;
using GraphQL.Execution;
using GraphQL.Instrumentation;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <summary>
    /// This is a required base type of all DI-created graph types. <see cref="DIObjectGraphBase"/> may be
    /// used if the <see cref="IResolveFieldContext.Source"/> type is <see cref="object"/>.
    /// </summary>
    //this class is a placeholder for future support of properties or methods on the base class
    public abstract class DIObjectGraphBase<TSource> : IDIObjectGraphBase<TSource>, IResolveFieldContext<TSource>
    {
        //this would be an ideal spot to put public readonly fields for the resolving query, such as Schema, Metrics, Executor, and so on, rather than being inside the ResolveFieldContext instance.
        //this could only contain fields that are not unique to a resolving field (such as Source), so as to not break multithreading support
        //with DI, any objects necessary could be brought in via dependency injection (such as Schema), so they really don't need to be in here

        /// <summary>
        /// The <see cref="IResolveFieldContext"/> for the current field.
        /// </summary>
        public IResolveFieldContext Context { get; private set; } = null!;

        /// <inheritdoc cref="IResolveFieldContext.Source"/>
        public TSource Source => (TSource)Context.Source;

        /// <inheritdoc cref="IResolveFieldContext.CancellationToken"/>
        public CancellationToken RequestAborted => Context.CancellationToken;

        /// <inheritdoc cref="IProvideUserContext.UserContext"/>
        public IDictionary<string, object> UserContext => Context.UserContext;

        /// <inheritdoc cref="IResolveFieldContext.Metrics"/>
        public Metrics Metrics => Context.Metrics;

        IResolveFieldContext IDIObjectGraphBase.Context { get => Context; set => Context = value; }
        Field IResolveFieldContext.FieldAst => Context.FieldAst;
        FieldType IResolveFieldContext.FieldDefinition => Context.FieldDefinition;
        IObjectGraphType IResolveFieldContext.ParentType => Context.ParentType;
        IResolveFieldContext IResolveFieldContext.Parent => Context.Parent;
        IDictionary<string, ArgumentValue> IResolveFieldContext.Arguments => Context.Arguments;
        object IResolveFieldContext.RootValue => Context.RootValue;
        object IResolveFieldContext.Source => Context.Source;
        ISchema IResolveFieldContext.Schema => Context.Schema;
        Document IResolveFieldContext.Document => Context.Document;
        Operation IResolveFieldContext.Operation => Context.Operation;
        Variables IResolveFieldContext.Variables => Context.Variables;
        CancellationToken IResolveFieldContext.CancellationToken => Context.CancellationToken;
        ExecutionErrors IResolveFieldContext.Errors => Context.Errors;
        IEnumerable<object> IResolveFieldContext.Path => Context.Path;
        IEnumerable<object> IResolveFieldContext.ResponsePath => Context.ResponsePath;
        Dictionary<string, Field> IResolveFieldContext.SubFields => Context.SubFields;
        IDictionary<string, object> IResolveFieldContext.Extensions => Context.Extensions;
        IServiceProvider IResolveFieldContext.RequestServices => Context.RequestServices;
        IExecutionArrayPool IResolveFieldContext.ArrayPool => Context.ArrayPool;
    }

    /// <summary>
    /// This is a required base type of all DI-created graph types. <see cref="DIObjectGraphBase{TSource}"/> may be
    /// used when the <see cref="IResolveFieldContext.Source"/> type is not <see cref="object"/>.
    /// </summary>
    public abstract class DIObjectGraphBase : DIObjectGraphBase<object>, IDIObjectGraphBase<object>
    {
    }
}
