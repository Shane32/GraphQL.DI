using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using GraphQL.MicrosoftDI;
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
    public class DIObjectGraphType<TDIGraph> : DIObjectGraphType<TDIGraph, object> where TDIGraph : DIObjectGraphBase<object> { }
    /// <summary>
    /// Wraps a <see cref="DIObjectGraphBase{TSource}"/> graph type for use with GraphQL. This class should be registered as a singleton
    /// within your dependency injection provider.
    /// </summary>
    public class DIObjectGraphType<TDIGraph, TSource> : ObjectGraphType<TSource> where TDIGraph : DIObjectGraphBase<TSource>
    {
        /// <summary>
        /// Initializes a new instance, configuring the <see cref="GraphType.Name"/>, <see cref="GraphType.Description"/>,
        /// <see cref="GraphType.DeprecationReason"/> and <see cref="MetadataProvider.Metadata"/> properties.
        /// </summary>
        public DIObjectGraphType()
        {
            var classType = typeof(TDIGraph);
            //allow default name / description / obsolete tags to remain if not overridden
            var nameAttribute = classType.GetCustomAttribute<NameAttribute>();
            if (nameAttribute != null)
                Name = nameAttribute.Name;
            //note: should probably take the default name from TDIGraph's name, rather than this type's name
            var descriptionAttribute = classType.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
                Description = descriptionAttribute.Description;
            var obsoleteAttribute = classType.GetCustomAttribute<ObsoleteAttribute>();
            if (obsoleteAttribute != null)
                DeprecationReason = obsoleteAttribute.Message;
            //pull metadata
            foreach (var metadataAttribute in classType.GetCustomAttributes<MetadataAttribute>())
                Metadata.Add(metadataAttribute.Key, metadataAttribute.Value);
            //check if there is a default concurrency setting
            var concurrentAttribute = classType.GetCustomAttribute<ConcurrentAttribute>();
            if (concurrentAttribute != null) {
                DefaultConcurrent = true;
                DefaultCreateScope = concurrentAttribute.CreateNewScope;
            }

            //give inherited classes a chance to mutate the field type list before they are added to the graph type list
            var fieldTypes = CreateFieldTypeList();
            if (fieldTypes != null)
                foreach (var fieldType in fieldTypes)
                    AddField(fieldType);
        }

        //grab some methods via reflection for us to use later
        private static readonly MethodInfo _getRequiredServiceMethod = typeof(ServiceProviderServiceExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(x => x.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService) && !x.IsGenericMethod);
        private static readonly MethodInfo _asMethod = typeof(ResolveFieldContextExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(x => x.Name == nameof(ResolveFieldContextExtensions.As) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(IResolveFieldContext));
        private static readonly MethodInfo _getArgumentMethod = typeof(ResolveFieldContextExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(x => x.Name == nameof(ResolveFieldContextExtensions.GetArgument) && x.IsGenericMethod);
        private static readonly PropertyInfo _sourceProperty = typeof(IResolveFieldContext).GetProperty(nameof(IResolveFieldContext.Source), BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo _requestServicesProperty = typeof(IResolveFieldContext).GetProperty(nameof(IResolveFieldContext.RequestServices), BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo _cancellationTokenProperty = typeof(IResolveFieldContext).GetProperty(nameof(IResolveFieldContext.CancellationToken), BindingFlags.Public | BindingFlags.Instance);

        /// <summary>
        /// Gets or sets whether fields added to this graph type will default to running concurrently.
        /// </summary>
        protected bool DefaultConcurrent { get; set; } = false;

        /// <summary>
        /// Gets or sets whether concurrent fields added to this graph type will default to running in a dedicated service scope.
        /// </summary>
        protected bool DefaultCreateScope { get; set; } = false;

        /// <summary>
        /// Returns a list of <see cref="DIFieldType"/> instances representing the fields ready to be
        /// added to the graph type.
        /// </summary>
        protected virtual List<DIFieldType> CreateFieldTypeList()
        {
            //scan for public members
            var methods = GetMethodsToProcess();
            var fieldTypeList = new List<DIFieldType>(methods.Count());
            foreach (var method in methods) {
                var fieldType = ProcessMethod(method);
                if (fieldType != null)
                    fieldTypeList.Add(fieldType);
            }
            return fieldTypeList;
        }

        /// <summary>
        /// Returns a list of methods (<see cref="MethodInfo"/> instances) on the <typeparamref name="TDIGraph"/> class to be
        /// converted into field definitions.
        /// </summary>
        protected virtual IEnumerable<MethodInfo> GetMethodsToProcess()
        {
            return typeof(TDIGraph).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(x => !x.ContainsGenericParameters);
        }

        /// <summary>
        /// Converts a specified method (<see cref="MethodInfo"/> instance) into a field definition.
        /// </summary>
        protected virtual DIFieldType? ProcessMethod(MethodInfo method)
        {
            //get the method name
            string methodName = method.Name;
            var methodNameAttribute = method.GetCustomAttribute<NameAttribute>();
            if (methodNameAttribute != null) {
                methodName = methodNameAttribute.Name;
            } else {
                if (methodName.EndsWith("Async") && method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)) {
                    methodName = methodName.Substring(0, methodName.Length - "Async".Length);
                }
            }
            if (methodName == null)
                return null; //ignore method if set to null

            //ignore method if it does not return a value
            if (method.ReturnType == typeof(void) || method.ReturnType == typeof(Task))
                return null;

            //scan the parameter list to create a list of arguments, and at the same time, generate the expressions to be used to call this method during the resolve function
            IFieldResolver resolver;
            var queryArguments = new List<QueryArgument>();
            bool concurrent = false;
            bool anyParamsUseServices = false;
            {
                var resolveFieldContextParameter = Expression.Parameter(typeof(IResolveFieldContext));
                var executeParams = new List<Expression>();
                foreach (var param in method.GetParameters()) {
                    var queryArgument = ProcessParameter(method, param, resolveFieldContextParameter, out bool isService, out Expression expr);
                    anyParamsUseServices |= isService;
                    if (queryArgument != null)
                        queryArguments.Add(queryArgument);
                    //add the constructed expression to the list to be used for creating the resolve function
                    executeParams.Add(expr);
                }
                //check if this is an async method
                var isAsync = typeof(Task).IsAssignableFrom(method.ReturnType);
                //define the resolve expression
                Expression exprResolve;
                if (method.IsStatic) {
                    //for static methods, no need to pull an instance of the class from the service provider
                    //just call the static method with the executeParams as the parameters
                    exprResolve = Expression.Call(method, executeParams.ToArray());
                } else {
                    //for instance methods, pull an instance of the class from the service provider
                    Expression exprGetService = GetInstanceExpression(resolveFieldContextParameter);
                    //then, call the method with the executeParams as the parameters
                    exprResolve = Expression.Call(exprGetService, method, executeParams.ToArray());
                }

                //determine if this should run concurrently with other resolvers
                //if it's an async static method that does not pull from services, then it's safe to run concurrently
                if (isAsync && method.IsStatic && !anyParamsUseServices) {
                    //mark this field as concurrent, so the execution strategy will run it asynchronously
                    concurrent = true;
                    //set the resolver to run the compiled resolve function
                    resolver = CreateUnscopedResolver(exprResolve, resolveFieldContextParameter);
                } else {
                    var methodConcurrent = method.GetCustomAttribute<ConcurrentAttribute>();
                    concurrent = DefaultConcurrent;
                    var scoped = DefaultCreateScope;
                    if (methodConcurrent != null) {
                        concurrent = methodConcurrent.Concurrent;
                        scoped = methodConcurrent.CreateNewScope;
                    }
                    //for methods that return a Task and are marked with the Concurrent attribute,
                    if (isAsync && concurrent) {
                        //mark this field as concurrent, so the execution strategy will run it asynchronously
                        concurrent = true;
                        //determine if a new DI scope is required
                        if (scoped) {
                            //the resolve function needs to create a scope,
                            //  then run the compiled resolve function (which creates an instance of the class),
                            //  then release the scope once the task has been awaited
                            resolver = CreateScopedResolver(exprResolve, resolveFieldContextParameter);
                        } else {
                            //just run the compiled resolve function, and count on the method to handle multithreading issues
                            resolver = CreateUnscopedResolver(exprResolve, resolveFieldContextParameter);
                        }
                    }
                    //for non-async methods, and instance methods that are not marked with the Concurrent attribute
                    else {
                        concurrent = false;
                        //just run the compiled resolve function
                        resolver = CreateUnscopedResolver(exprResolve, resolveFieldContextParameter);
                    }
                }
            }

            //process the method's attributes and add the field
            {
                //determine the graphtype of the field
                var graphTypeAttribute = method.GetCustomAttribute<GraphTypeAttribute>();
                Type? graphType = graphTypeAttribute?.Type;
                //infer the graphtype if it is not specified
                if (graphType == null) {
                    //determine if the field is required
                    var isNullable = GetNullability(method);
                    if (method.ReturnType.IsConstructedGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)) {
                        graphType = InferOutputGraphType(method.ReturnType.GetGenericArguments()[0], isNullable);
                    } else {
                        graphType = InferOutputGraphType(method.ReturnType, isNullable);
                    }
                }
                //load the description
                string? description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
                //load the deprecation reason
                string? obsoleteDescription = method.GetCustomAttribute<ObsoleteAttribute>()?.Message;
                //create the field
                var fieldType = new DIFieldType() {
                    Type = graphType,
                    Name = methodName,
                    Arguments = new QueryArguments(queryArguments.ToArray()),
                    Resolver = resolver,
                    Description = description,
                    Concurrent = concurrent,
                    DeprecationReason = obsoleteDescription,
                };
                //load the metadata
                foreach (var metaAttribute in method.GetCustomAttributes<MetadataAttribute>())
                    fieldType.WithMetadata(metaAttribute.Key, metaAttribute.Value);
                //return the field
                return fieldType;
            }

        }

        /// <summary>
        /// Returns a boolean indicating if the return value of a method is nullable.
        /// </summary>
        protected virtual bool GetNullability(MethodInfo method)
        {
            if (method.GetCustomAttribute<OptionalAttribute>() != null)
                return true;
            if (method.GetCustomAttribute<RequiredAttribute>() != null)
                return false;
            if (method.ReturnType.IsValueType)
                return method.ReturnType.IsConstructedGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>);

            Nullability nullable = Nullability.Unknown;

            // check the parent type first to see if there's a nullable context attribute set for it
            var parentType = method.DeclaringType;
            var attribute = parentType.CustomAttributes.FirstOrDefault(x =>
                x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute" &&
                x.ConstructorArguments.Count == 1 &&
                x.ConstructorArguments[0].ArgumentType == typeof(byte));
            if (attribute != null) {
                nullable = (Nullability)(byte)attribute.ConstructorArguments[0].Value;
            }

            // now check the method to see if there's a nullable context attribute set for it
            attribute = method.CustomAttributes.FirstOrDefault(x =>
                x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute" &&
                x.ConstructorArguments.Count == 1 &&
                x.ConstructorArguments[0].ArgumentType == typeof(byte));
            if (attribute != null) {
                nullable = (Nullability)(byte)attribute.ConstructorArguments[0].Value;
            }

            // now check the return type to see if there's a nullable attribute for it
            attribute = method.ReturnParameter.CustomAttributes.FirstOrDefault(x =>
                x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute" &&
                x.ConstructorArguments.Count == 1 &&
                (x.ConstructorArguments[0].ArgumentType == typeof(byte) ||
                x.ConstructorArguments[0].ArgumentType == typeof(byte[])));
            if (attribute != null && attribute.ConstructorArguments[0].ArgumentType == typeof(byte)) {
                nullable = (Nullability)(byte)attribute.ConstructorArguments[0].Value;
            }

            var nullabilityBytes = attribute?.ConstructorArguments[0].Value as byte[];
            var index = 0;
            nullable = Consider(method.ReturnType);
            return nullable != Nullability.NonNullable;

            Nullability Consider(Type t)
            {
                var g = t.IsGenericType ? t.GetGenericTypeDefinition() : null;
                if (g == typeof(Nullable<>))
                    return Nullability.Nullable;
                if (t.IsValueType)
                    return Nullability.NonNullable;
                if ((nullabilityBytes != null && nullabilityBytes[index] == (byte)Nullability.Nullable) || (nullabilityBytes == null && nullable == Nullability.Nullable))
                    return Nullability.Nullable;
                if (g == typeof(IDataLoaderResult<>) || g == typeof(Task<>)) {
                    index++;
                    return Consider(t.GenericTypeArguments[0]);
                }
                if (t == typeof(IDataLoaderResult))
                    return Nullability.Nullable;
                if (nullabilityBytes != null)
                    return (Nullability)nullabilityBytes[index];
                return nullable;
            }
        }

        private enum Nullability : byte
        {
            Unknown = 0,
            NonNullable = 1,
            Nullable = 2,
        }

        /// <summary>
        /// Returns a boolean indicating if the parameter value is nullable
        /// </summary>
        protected virtual bool GetNullability(MethodInfo method, ParameterInfo parameter)
        {
            if (parameter.GetCustomAttribute<OptionalAttribute>() != null)
                return true;
            if (parameter.GetCustomAttribute<RequiredAttribute>() != null)
                return false;
            if (parameter.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null)
                return false;
            if (parameter.ParameterType.IsValueType)
                return parameter.ParameterType.IsConstructedGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>);

            Nullability nullable = Nullability.Unknown;

            // check the parent type first to see if there's a nullable context attribute set for it
            var parentType = method.DeclaringType;
            var attribute = parentType.CustomAttributes.FirstOrDefault(x =>
                x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute" &&
                x.ConstructorArguments.Count == 1 &&
                x.ConstructorArguments[0].ArgumentType == typeof(byte));
            if (attribute != null) {
                nullable = (Nullability)(byte)attribute.ConstructorArguments[0].Value;
            }

            // now check the method to see if there's a nullable context attribute set for it
            attribute = method.CustomAttributes.FirstOrDefault(x =>
                x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute" &&
                x.ConstructorArguments.Count == 1 &&
                x.ConstructorArguments[0].ArgumentType == typeof(byte));
            if (attribute != null) {
                nullable = (Nullability)(byte)attribute.ConstructorArguments[0].Value;
            }

            // now check the parameter to see if there's a nullable attribute for it
            attribute = parameter.CustomAttributes.FirstOrDefault(x =>
                x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute" &&
                x.ConstructorArguments.Count == 1 &&
                (x.ConstructorArguments[0].ArgumentType == typeof(byte) ||
                x.ConstructorArguments[0].ArgumentType == typeof(byte[])));
            if (attribute != null && attribute.ConstructorArguments[0].ArgumentType == typeof(byte)) {
                nullable = (Nullability)(byte)attribute.ConstructorArguments[0].Value;
            }

            var nullabilityBytes = attribute?.ConstructorArguments[0].Value as byte[];
            var index = 0;
            nullable = Consider(parameter.ParameterType);
            return nullable != Nullability.NonNullable;

            Nullability Consider(Type t)
            {
                var g = t.IsGenericType ? t.GetGenericTypeDefinition() : null;
                if (g == typeof(Nullable<>))
                    return Nullability.Nullable;
                if (t.IsValueType)
                    return Nullability.NonNullable;
                if ((nullabilityBytes != null && nullabilityBytes[index] == (byte)Nullability.Nullable) || (nullabilityBytes == null && nullable == Nullability.Nullable))
                    return Nullability.Nullable;
                if (nullabilityBytes != null)
                    return (Nullability)nullabilityBytes[index];
                return nullable;
            }
        }

        /// <summary>
        /// Returns a GraphQL input type for a specified CLR type
        /// </summary>
        protected virtual Type InferInputGraphType(Type type, bool nullable)
        {
            return type.GetGraphTypeFromType(nullable, TypeMappingMode.InputType);
        }

        /// <summary>
        /// Returns a GraphQL output type for a specified CLR type
        /// </summary>
        protected virtual Type InferOutputGraphType(Type type, bool nullable)
        {
            return type.GetGraphTypeFromType(nullable, TypeMappingMode.OutputType);
        }

        /// <summary>
        /// Returns an expression that gets/creates an instance of <typeparamref name="TDIGraph"/> from a <see cref="IResolveFieldContext"/>.
        /// Defaults to the following:
        /// <code>context =&gt; context.RequestServices.GetRequiredService&lt;TDIGraph&gt;();</code>
        /// </summary>
        protected virtual Expression GetInstanceExpression(ParameterExpression resolveFieldContextParameter)
        {
            return GetServiceExpression(resolveFieldContextParameter, typeof(TDIGraph));
        }

        /// <summary>
        /// Returns an expression that returns an <see cref="IServiceProvider"/> from a <see cref="IResolveFieldContext"/>.
        /// Defaults to the following:
        /// <code>context =&gt; context.RequestServices</code>.
        /// </summary>
        protected virtual Expression GetServiceProviderExpression(ParameterExpression resolveFieldContextParameter)
        {
            //returns: context.RequestServices
            return Expression.Property(resolveFieldContextParameter, _requestServicesProperty);
        }

        /// <summary>
        /// Returns an expression that gets/creates an instance of the specified type from a <see cref="IResolveFieldContext"/>.
        /// Defaults to the following:
        /// <code>context =&gt; context.RequestServices.GetRequiredService&lt;T&gt;();</code>
        /// </summary>
        protected virtual Expression GetServiceExpression(ParameterExpression resolveFieldContextParameter, Type serviceType)
        {
            //returns: (serviceType)(context.RequestServices.GetRequiredService(serviceType))
            var serviceProvider = GetServiceProviderExpression(resolveFieldContextParameter);
            var type = Expression.Constant(serviceType ?? throw new ArgumentNullException(nameof(serviceType)));
            var getService = Expression.Call(_getRequiredServiceMethod, serviceProvider, type);
            var cast = Expression.Convert(getService, serviceType);
            return cast;
        }

        private readonly ConcurrentDictionary<Type, ConstructorInfo> _funcFieldResolverConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        /// <summary>
        /// Returns a field resolver for a specified delegate expression. Does not create a dedicated service scope for the delegate.
        /// </summary>
        protected virtual IFieldResolver CreateUnscopedResolver(Expression resolveExpression, ParameterExpression resolveFieldContextParameter)
        {
            var constructorInfo = _funcFieldResolverConstructors.GetOrAdd(resolveExpression.Type, t => typeof(FuncFieldResolver<>).MakeGenericType(t).GetConstructors().Single());
            var lambda = Expression.Lambda(resolveExpression, resolveFieldContextParameter).Compile();
            return (IFieldResolver)constructorInfo.Invoke(new[] { lambda });
        }

        private readonly ConcurrentDictionary<Type, ConstructorInfo> _scopedFieldResolverConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        private readonly ConcurrentDictionary<Type, ConstructorInfo> _scopedAsyncFieldResolverConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        /// <summary>
        /// Returns a field resolver for a specified delegate expression. The field resolver creates a dedicated service scope for the delegate.
        /// </summary>
        protected virtual IFieldResolver CreateScopedResolver(Expression resolveExpression, ParameterExpression resolveFieldContextParameter)
        {
            ConstructorInfo constructorInfo;
            if (typeof(Task).IsAssignableFrom(resolveExpression.Type)) {
                constructorInfo = _scopedAsyncFieldResolverConstructors.GetOrAdd(GetTaskType(resolveExpression.Type), t => typeof(ScopedAsyncFieldResolver<>).MakeGenericType(t).GetConstructors().Single());
            } else {
                constructorInfo = _scopedFieldResolverConstructors.GetOrAdd(resolveExpression.Type, t => typeof(ScopedFieldResolver<>).MakeGenericType(t).GetConstructors().Single());
            }
            var lambda = Expression.Lambda(resolveExpression, resolveFieldContextParameter).Compile();
            return (IFieldResolver)constructorInfo.Invoke(new[] { lambda });
        }

        private static Type GetTaskType(Type t)
        {
            while (t != null) {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>))
                    return t.GenericTypeArguments[0];
                t = t.BaseType;
            }
            throw new ArgumentOutOfRangeException(nameof(t), "Type does not inherit from Task<>.");
        }

        /// <summary>
        /// Processes a parameter of a method, returning an expression based upon <see cref="IResolveFieldContext"/>, and
        /// optionally returns a query argument to be added to the field. <paramref name="usesServices"/> can be set to
        /// <see langword="true"/> to indicate that the parameter uses the service provider and may need to run within
        /// a scoped service provider for concurrent use.
        /// </summary>
        protected virtual QueryArgument? ProcessParameter(MethodInfo method, ParameterInfo param, ParameterExpression resolveFieldContextParameter, out bool usesServices, out Expression expr)
        {
            usesServices = false;

            if (param.ParameterType == typeof(IResolveFieldContext)) {
                //if they are requesting the IResolveFieldContext, just pass it in
                //e.g. Func<IResolveFieldContext, IResolveFieldContext> = (context) => context;
                expr = resolveFieldContextParameter;
                //and do not add it as a QueryArgument
                return null;
            }
            if (param.ParameterType == typeof(CancellationToken)) {
                //return the cancellation token from the IResolveFieldContext parameter
                expr = Expression.MakeMemberAccess(resolveFieldContextParameter, _cancellationTokenProperty);
                //and do not add it as a QueryArgument
                return null;
            }
            if (param.ParameterType.IsConstructedGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IResolveFieldContext<>)) {
                //validate that constructed type matches TSource
                var genericType = param.ParameterType.GetGenericArguments()[0];
                if (!genericType.IsAssignableFrom(typeof(TSource)))
                    throw new InvalidOperationException($"Invalid {nameof(IResolveFieldContext)}<> type for method {method.Name}");
                //convert the IResolveFieldContext to the specified ResolveFieldContext<>
                var asMethodTyped = _asMethod.MakeGenericMethod(genericType);
                //e.g. Func<IResolveFieldContext, IResolveFieldContext<MyClass>> = (context) => context.As<MyClass>();
                expr = Expression.Call(asMethodTyped, resolveFieldContextParameter);
                //and do not add it as a QueryArgument
                return null;
            }
            if (param.GetCustomAttribute<FromSourceAttribute>() != null) {
                //validate that type matches TSource
                if (!param.ParameterType.IsAssignableFrom(typeof(TSource)))
                    throw new InvalidOperationException($"Invalid {nameof(IResolveFieldContext)}<> type for method {method.Name}");
                //retrieve the value and cast it to the specified type
                //e.g. Func<IResolveFieldContext, TSource> = (context) => (TSource)context.Source;
                expr = Expression.Convert(Expression.Property(resolveFieldContextParameter, _sourceProperty), param.ParameterType);
                //and do not add it as a QueryArgument
                return null;
            }
            if (param.ParameterType == typeof(IServiceProvider)) {
                //if they want the service provider, just pass it in
                //e.g. Func<ResolveFieldContext, IServiceProvider> = (context) => context.RequestServices;
                expr = GetServiceProviderExpression(resolveFieldContextParameter);
                //note that we have a parameter that pulls from the service provider
                usesServices = true;
                //and do not add it as a QueryArgument
                return null;
            }
            if (param.GetCustomAttribute<FromServicesAttribute>() != null) {
                //if they are pulling from a service context, pull that in
                //e.g. Func<IResolveFieldContext, IMyService> = (context) => (IMyService)AsyncServiceProvider.GetRequiredService(typeof(IMyService));
                expr = GetServiceExpression(resolveFieldContextParameter, param.ParameterType);
                //note that we have a parameter that pulls from the service provider
                usesServices = true;
                //and do not add it as a QueryArgument
                return null;
            }
            //pull the name attribute
            var nameAttribute = param.GetCustomAttribute<NameAttribute>();
            if (nameAttribute != null && nameAttribute.Name == null) {
                //name is set to null, so just fill with the default for this parameter and don't create a query argument
                //e.g. Func<ResolveFieldContext, int> = (context) => default(int);
                expr = Expression.Constant(param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType) : null, param.ParameterType);
                //and do not add it as a QueryArgument
                return null;
            }
            //otherwise, it's a query argument
            //initialize the query argument parameters

            //load the specified graph type
            var graphTypeAttribute = param.GetCustomAttribute<GraphTypeAttribute>();
            Type? graphType = graphTypeAttribute?.Type;
            //if no specific graphtype set, pull from registered graph type list
            if (graphType == null) {
                //determine if this query argument is required
                var nullable = GetNullability(method, param);
                graphType = InferInputGraphType(param.ParameterType, nullable);
            }

            //construct the query argument
            var argument = new QueryArgument(graphType) {
                Name = nameAttribute?.Name ?? param.Name,
                Description = param.GetCustomAttribute<DescriptionAttribute>()?.Description,
            };

            foreach (var metaAttribute in param.GetCustomAttributes<MetadataAttribute>())
                argument.Metadata.Add(metaAttribute.Key, metaAttribute.Value);

            //pull/create the default value
            object? defaultValue = null;
            if (param.IsOptional) {
                defaultValue = param.DefaultValue;
            } else if (param.ParameterType.IsValueType) {
                defaultValue = Activator.CreateInstance(param.ParameterType);
            }

            //construct a call to ResolveFieldContextExtensions.GetArgument, passing in the appropriate default value
            var getArgumentMethodTyped = _getArgumentMethod.MakeGenericMethod(param.ParameterType);
            //e.g. Func<IResolveFieldContext, int> = (context) => ResolveFieldContextExtensions.GetArgument<int>(context, argument.Name, defaultValue);
            expr = Expression.Call(getArgumentMethodTyped, resolveFieldContextParameter, Expression.Constant(argument.Name), Expression.Constant(defaultValue, param.ParameterType));

            //return the query argument
            return argument;
        }

    }
}
