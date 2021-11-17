using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.DI
{
    /// <summary>
    /// Wraps a <see cref="DIObjectGraphBase"/> graph type for use with GraphQL. This class should be registered as a singleton
    /// within your dependency injection provider.
    /// </summary>
    public class DIObjectGraphType<TDIGraph> : DIObjectGraphType<TDIGraph, object> where TDIGraph : IDIObjectGraphBase<object>
    {
        /// <inheritdoc/>
        protected override string? GetDefaultName()
        {
            //if this class is inherited, do not set default name
            var thisType = GetType();
            if (thisType != typeof(DIObjectGraphType<TDIGraph>))
                return null;

            //without this code, the name would default to DIObjectGraphType_1
            var name = typeof(TDIGraph).Name.Replace('`', '_');
            if (name.EndsWith("Graph", StringComparison.InvariantCulture))
                name = name.Substring(0, name.Length - "Graph".Length);
            return name;
        }
    }

    /// <summary>
    /// Wraps a <see cref="DIObjectGraphBase{TSource}"/> graph type for use with GraphQL. This class should be registered as a singleton
    /// within your dependency injection provider.
    /// </summary>
    public class DIObjectGraphType<TDIGraph, TSource> : ObjectGraphType<TSource> where TDIGraph : IDIObjectGraphBase<TSource>
    {
        /// <summary>
        /// Initializes a new instance, configuring the <see cref="GraphType.Name"/>, <see cref="GraphType.Description"/>,
        /// <see cref="GraphType.DeprecationReason"/> and <see cref="MetadataProvider.Metadata"/> properties.
        /// </summary>
        public DIObjectGraphType()
        {
            GraphTypeHelper.ConfigureGraph<TDIGraph>(this, GetDefaultName);

            var classType = typeof(TDIGraph);

            //check if there is a default concurrency setting
            var concurrentAttribute = classType.GetCustomAttribute<ConcurrentAttribute>();
            if (concurrentAttribute != null) {
                DefaultConcurrent = true;
                DefaultCreateScope = concurrentAttribute.CreateNewScope;
            }

            //give inherited classes a chance to mutate the field type list before they are added to the graph type list
            GraphTypeHelper.AddFields(this, CreateFieldTypeList());
        }

        /// <summary>
        /// Returns the default name assigned to the graph, or <see langword="null"/> to leave the default setting set by the <see cref="GraphType"/> constructor.
        /// </summary>
        protected virtual string? GetDefaultName()
        {
            //if this class is inherited, do not set default name
            var thisType = GetType();
            if (thisType != typeof(DIObjectGraphType<TDIGraph, TSource>))
                return null;

            //without this code, the name would default to DIObjectGraphType_1
            var name = typeof(TDIGraph).Name.Replace('`', '_');
            if (name.EndsWith("Graph", StringComparison.InvariantCulture))
                name = name.Substring(0, name.Length - "Graph".Length);
            return name;
        }

        /// <summary>
        /// Gets or sets whether fields added to this graph type will default to running concurrently.
        /// </summary>
        protected bool DefaultConcurrent { get; set; } = false;

        /// <summary>
        /// Gets or sets whether concurrent fields added to this graph type will default to running in a dedicated service scope.
        /// </summary>
        protected bool DefaultCreateScope { get; set; } = false;

        /// <summary>
        /// Indicates that the fields should be added to the graph type alphabetically.
        /// </summary>
        public static bool SortMembers { get; set; } = true;

        /// <summary>
        /// Returns a list of <see cref="DIFieldType"/> instances representing the fields ready to be
        /// added to the graph type.
        /// Sorts the list if specified by <see cref="SortMembers"/>.
        /// </summary>
        protected virtual IEnumerable<DIFieldType> CreateFieldTypeList()
        {
            //scan for public members
            var methods = GetMethodsToProcess();
            var fieldTypeList = new List<DIFieldType>(methods.Count());
            foreach (var method in methods) {
                var fieldType = ProcessMethod(method);
                if (fieldType != null)
                    fieldTypeList.Add(fieldType);
            }
            if (SortMembers)
                return fieldTypeList.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
            return fieldTypeList;
        }

        /// <summary>
        /// Returns a list of methods (<see cref="MethodInfo"/> instances) on the <typeparamref name="TDIGraph"/> class to be
        /// converted into field definitions.
        /// </summary>
        protected virtual IEnumerable<MethodInfo> GetMethodsToProcess()
        {
            var methods = typeof(TDIGraph).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(x => !x.ContainsGenericParameters);
            return methods;
        }

        /// <summary>
        /// Converts a specified method (<see cref="MethodInfo"/> instance) into a field definition.
        /// </summary>
        protected virtual DIFieldType? ProcessMethod(MethodInfo method)
        {
            var field = GraphTypeHelper.CreateField(
                method,
                () => InferGraphType(ApplyAttributes(GetTypeInformation(method.ReturnParameter, false))));
            if (field == null)
                return null;

            GraphTypeHelper.ProcessMethod(
                method,
                field,
                (method, param, resolveFieldContextParameter) => {
                    var queryArgument = ProcessParameter(method, param, resolveFieldContextParameter, out var usesServices, out var expr);
                    return (queryArgument, usesServices, expr);
                },
                GetInstanceExpression,
                CreateUnscopedResolver,
                CreateScopedResolver,
                DefaultConcurrent,
                DefaultCreateScope);

            return field;
        }

        /// <summary>
        /// Apply <see cref="RequiredAttribute"/>, <see cref="OptionalAttribute"/>, <see cref="RequiredListAttribute"/>,
        /// <see cref="OptionalListAttribute"/>, <see cref="IdAttribute"/> and <see cref="DIGraphAttribute"/> over
        /// the supplied <see cref="TypeInformation"/>.
        /// Override this method to enforce specific graph types for specific CLR types, or to implement custom
        /// attributes to change graph type selection behavior.
        /// </summary>
        protected virtual TypeInformation ApplyAttributes(TypeInformation typeInformation)
            => typeInformation.ApplyAttributes(typeInformation.IsInputType ? (ICustomAttributeProvider)typeInformation.ParameterInfo! : typeInformation.MemberInfo);

        /// <inheritdoc cref="ReflectionExtensions.GetNullabilityInformation(ParameterInfo)"/>
        protected virtual IEnumerable<(Type Type, Nullability Nullable)> GetNullabilityInformation(ParameterInfo parameter)
            => parameter.GetNullabilityInformation();

        /// <summary>
        /// Analyzes a method argument or return value and returns a <see cref="TypeInformation"/>
        /// struct containing type information necessary to select a graph type.
        /// </summary>
        protected virtual TypeInformation GetTypeInformation(ParameterInfo parameterInfo, bool isInputArgument)
            => GraphTypeHelper.GetTypeInformation(parameterInfo, isInputArgument, GetNullabilityInformation);

        /// <summary>
        /// Returns a GraphQL input type for a specified CLR type
        /// </summary>
        protected virtual Type InferGraphType(TypeInformation typeInformation)
            => typeInformation.InferGraphType();

        //grab some methods via reflection for us to use later
        private static readonly MethodInfo _getOrCreateServiceMethod = typeof(ActivatorUtilities).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(x => x.Name == nameof(ActivatorUtilities.GetServiceOrCreateInstance) && !x.IsGenericMethod);
        private static readonly MethodInfo _setContextMethod = typeof(IDIObjectGraphBase).GetProperty(nameof(IDIObjectGraphBase.Context)).GetSetMethod();

        /// <summary>
        /// Returns an expression that gets/creates an instance of <typeparamref name="TDIGraph"/> from a <see cref="IResolveFieldContext"/>.
        /// Defaults to the following:
        /// <code>context =&gt; {<br/>
        /// var g = ActivatorUtilities.GetServiceOrCreateInstance&lt;T&gt;(context.RequestServices);<br/>
        /// ((IDIObjectGraphBase)g).Context = context;<br/>
        /// return g;<br/>
        /// }</code>
        /// </summary>
        protected virtual Expression GetInstanceExpression(ParameterExpression resolveFieldContextParameter)
        {
            var serviceType = typeof(TDIGraph);
            var serviceProvider = GetServiceProviderExpression(resolveFieldContextParameter);
            var type = Expression.Constant(serviceType ?? throw new ArgumentNullException(nameof(serviceType)));
            var getService = Expression.Call(_getOrCreateServiceMethod, serviceProvider, type);
            var cast = Expression.Convert(getService, serviceType);
            var variable = Expression.Parameter(serviceType);
            var setVariable = Expression.Assign(variable, cast);
            var setContext = Expression.Call(variable, _setContextMethod, resolveFieldContextParameter);
            var block = Expression.Block(serviceType, new ParameterExpression[] { variable }, new Expression[] { setVariable, setContext, variable });
            return block;
        }

        /// <summary>
        /// Returns an expression that returns an <see cref="IServiceProvider"/> from a <see cref="IResolveFieldContext"/>.
        /// Defaults to the following:
        /// <code>context =&gt; context.RequestServices</code>.
        /// </summary>
        protected virtual Expression GetServiceProviderExpression(ParameterExpression resolveFieldContextParameter)
            => GraphTypeHelper.GetServiceProviderExpression(resolveFieldContextParameter);

        /// <summary>
        /// Returns an expression that gets/creates an instance of the specified type from a <see cref="IResolveFieldContext"/>.
        /// Defaults to the following:
        /// <code>context =&gt; context.RequestServices.GetRequiredService&lt;T&gt;();</code>
        /// </summary>
        protected virtual Expression GetServiceExpression(ParameterExpression resolveFieldContextParameter, Type serviceType)
            => GraphTypeHelper.GetServiceExpression(resolveFieldContextParameter, serviceType);

        /// <summary>
        /// Returns a field resolver for a specified delegate expression. Does not create a dedicated service scope for the delegate.
        /// </summary>
        protected virtual IFieldResolver CreateUnscopedResolver(Expression resolveExpression, ParameterExpression resolveFieldContextParameter)
            => GraphTypeHelper.CreateUnscopedResolver(resolveExpression, resolveFieldContextParameter);

        /// <summary>
        /// Returns a field resolver for a specified delegate expression. The field resolver creates a dedicated service scope for the delegate.
        /// </summary>
        protected virtual IFieldResolver CreateScopedResolver(Expression resolveExpression, ParameterExpression resolveFieldContextParameter)
            => GraphTypeHelper.CreateScopedResolver(resolveExpression, resolveFieldContextParameter);

        /// <summary>
        /// Processes a parameter of a method, returning an expression based upon <see cref="IResolveFieldContext"/>, and
        /// optionally returns a query argument to be added to the field. <paramref name="usesServices"/> can be set to
        /// <see langword="true"/> to indicate that the parameter uses the service provider and may need to run within
        /// a scoped service provider for concurrent use.
        /// </summary>
        protected virtual QueryArgument? ProcessParameter(MethodInfo method, ParameterInfo param, ParameterExpression resolveFieldContextParameter, out bool usesServices, out Expression expr)
            => GraphTypeHelper.ProcessParameter<TSource>(
                method,
                param,
                resolveFieldContextParameter,
                GetServiceProviderExpression,
                GetServiceExpression,
                param => InferGraphType(ApplyAttributes(GetTypeInformation(param, true))),
                out usesServices,
                out expr);
    }
}
