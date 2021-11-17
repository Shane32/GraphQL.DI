using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <summary>
    /// An automatically-generated graph type for public properties and methods on the specified data model.
    /// </summary>
    public class AutoObjectGraphType<TSourceType> : ObjectGraphType<TSourceType>
    {
        /// <summary>
        /// Gets or sets whether fields added to this graph type will default to running concurrently.
        /// </summary>
        protected bool DefaultConcurrent { get; set; } = false;

        /// <summary>
        /// Gets or sets whether concurrent fields added to this graph type will default to running in a dedicated service scope.
        /// </summary>
        protected bool DefaultCreateScope { get; set; } = false;

        /// <summary>
        /// Creates a GraphQL type from <typeparamref name="TSourceType"/>.
        /// </summary>
        public AutoObjectGraphType()
        {
            GraphTypeHelper.ConfigureGraph<TSourceType>(this, GetDefaultName);

            //check if there is a default concurrency setting
            var concurrentAttribute = typeof(TSourceType).GetCustomAttribute<ConcurrentAttribute>();
            if (concurrentAttribute != null) {
                DefaultConcurrent = true;
                DefaultCreateScope = concurrentAttribute.CreateNewScope;
            }

            GraphTypeHelper.AddFields(this, CreateFieldTypeList());
        }

        /// <summary>
        /// Returns a list of <see cref="FieldType"/> instances representing the fields ready to be
        /// added to the graph type.
        /// Sorts the list if specified by <see cref="SortMembers"/>.
        /// </summary>
        protected virtual IEnumerable<FieldType> CreateFieldTypeList()
        {
            //scan for public members
            var properties = GetPropertiesToProcess();
            var methods = GetMethodsToProcess();
            var fieldTypeList = new List<FieldType>(properties.Count() + methods.Count());
            foreach (var property in properties) {
                var fieldType = ProcessProperty(property);
                if (fieldType != null)
                    fieldTypeList.Add(fieldType);
            }
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
        /// Returns the default name assigned to the graph, or <see langword="null"/> to leave the default setting set by the <see cref="GraphType"/> constructor.
        /// </summary>
        protected virtual string? GetDefaultName()
        {
            //if this class is inherited, do not set default name
            if (GetType() != typeof(AutoObjectGraphType<TSourceType>))
                return null;

            //without this code, the name would default to AutoObjectGraphType_1
            var name = typeof(TSourceType).Name.Replace('`', '_');
            if (name.EndsWith("Model", StringComparison.InvariantCulture))
                name = name.Substring(0, name.Length - "Model".Length);
            return name;
        }

        /// <summary>
        /// Indicates that the fields should be added to the graph type alphabetically.
        /// </summary>
        public static bool SortMembers { get; set; } = true;

        /// <summary>
        /// Returns a list of properties that should have fields created for them.
        /// </summary>
        protected virtual IEnumerable<PropertyInfo> GetPropertiesToProcess()
        {
            var props = typeof(TSourceType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead);
            return props;
        }

        /// <summary>
        /// Returns a list of methods (<see cref="MethodInfo"/> instances) on the <typeparamref name="TSourceType"/> class to be
        /// converted into field definitions.
        /// </summary>
        protected virtual IEnumerable<MethodInfo> GetMethodsToProcess()
        {
            var methods = typeof(TSourceType).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(x => !x.ContainsGenericParameters && !x.IsSpecialName);
            return methods;
        }

        /// <summary>
        /// Processes the specified property and returns a <see cref="FieldType"/>
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual FieldType? ProcessProperty(PropertyInfo property)
        {
            var fieldType = GraphTypeHelper.CreateField(
                property,
                () => InferGraphType(ApplyAttributes(GetTypeInformation(property))));
            if (fieldType == null)
                return null;
            //create the field resolver
            fieldType.Resolver = new PropertyResolver(property);
            //return the field
            return fieldType;
        }

        /// <inheritdoc cref="ReflectionExtensions.GetNullabilityInformation(ParameterInfo)"/>
        protected virtual IEnumerable<(Type Type, Nullability Nullable)> GetNullabilityInformation(PropertyInfo parameter)
            => parameter.GetNullabilityInformation();

        /// <summary>
        /// Analyzes a property and returns a <see cref="TypeInformation"/>
        /// struct containing type information necessary to select a graph type.
        /// </summary>
        protected virtual TypeInformation GetTypeInformation(PropertyInfo propertyInfo)
            => GraphTypeHelper.GetTypeInformation(propertyInfo, false, GetNullabilityInformation);

        /// <summary>
        /// Apply <see cref="RequiredAttribute"/>, <see cref="OptionalAttribute"/>, <see cref="RequiredListAttribute"/>,
        /// <see cref="OptionalListAttribute"/>, <see cref="IdAttribute"/> and <see cref="DIGraphAttribute"/> over
        /// the supplied <see cref="TypeInformation"/>.
        /// Override this method to enforce specific graph types for specific CLR types, or to implement custom
        /// attributes to change graph type selection behavior.
        /// </summary>
        protected virtual TypeInformation ApplyAttributes(TypeInformation typeInformation)
            => typeInformation.ApplyAttributes(typeInformation.MemberInfo is PropertyInfo propertyInfo ? propertyInfo : typeInformation.IsInputType ? (ICustomAttributeProvider)typeInformation.ParameterInfo! : typeInformation.MemberInfo);

        /// <summary>
        /// Returns a GraphQL input type for a specified CLR type
        /// </summary>
        protected virtual Type InferGraphType(TypeInformation typeInformation)
            => typeInformation.InferGraphType();

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
                resolveFieldContextParameter => GraphTypeHelper.GetSourceExpression(resolveFieldContextParameter, typeof(TSourceType)),
                CreateUnscopedResolver,
                CreateScopedResolver,
                DefaultConcurrent,
                DefaultCreateScope);

            return field;
        }

        /// <summary>
        /// Analyzes a method argument or return value and returns a <see cref="TypeInformation"/>
        /// struct containing type information necessary to select a graph type.
        /// </summary>
        protected virtual TypeInformation GetTypeInformation(ParameterInfo parameterInfo, bool isInputArgument)
            => GraphTypeHelper.GetTypeInformation(parameterInfo, isInputArgument, GetNullabilityInformation);

        /// <inheritdoc cref="ReflectionExtensions.GetNullabilityInformation(ParameterInfo)"/>
        protected virtual IEnumerable<(Type Type, Nullability Nullable)> GetNullabilityInformation(ParameterInfo parameter)
            => parameter.GetNullabilityInformation();

        /// <summary>
        /// Processes a parameter of a method, returning an expression based upon <see cref="IResolveFieldContext"/>, and
        /// optionally returns a query argument to be added to the field. <paramref name="usesServices"/> can be set to
        /// <see langword="true"/> to indicate that the parameter uses the service provider and may need to run within
        /// a scoped service provider for concurrent use.
        /// </summary>
        protected virtual QueryArgument? ProcessParameter(MethodInfo method, ParameterInfo param, ParameterExpression resolveFieldContextParameter, out bool usesServices, out Expression expr)
            => GraphTypeHelper.ProcessParameter<TSourceType>(
                method,
                param,
                resolveFieldContextParameter,
                GetServiceProviderExpression,
                GetServiceExpression,
                param => InferGraphType(ApplyAttributes(GetTypeInformation(param, true))),
                out usesServices,
                out expr);

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
    }
}
