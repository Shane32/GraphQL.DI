using System;
using System.Collections;
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
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.DI
{
    internal static class GraphTypeHelper
    {
        /// <summary>
        /// Configures the Name, Description, DeprecationRreason and metadata for a graph
        /// </summary>
        public static void ConfigureGraph<TSourceType>(IGraphType graphType, Func<string?> getDefaultNameFunc)
        {
            var classType = typeof(TSourceType);

            //allow default name / description / obsolete tags to remain if not overridden
            var nameAttribute = classType.GetCustomAttribute<NameAttribute>();
            if (nameAttribute != null)
                graphType.Name = nameAttribute.Name;
            else {
                var name = getDefaultNameFunc();
                if (name != null)
                    graphType.Name = name;
            }

            var descriptionAttribute = classType.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
                graphType.Description = descriptionAttribute.Description;
            var obsoleteAttribute = classType.GetCustomAttribute<ObsoleteAttribute>();
            if (obsoleteAttribute != null)
                graphType.DeprecationReason = obsoleteAttribute.Message;

            //pull metadata
            foreach (var metadataAttribute in classType.GetCustomAttributes<MetadataAttribute>())
                graphType.Metadata.Add(metadataAttribute.Key, metadataAttribute.Value);
        }

        /// <summary>
        /// Adds the specified fields to the specified graph.
        /// Skips null fields.
        /// </summary>
        public static void AddFields(IComplexGraphType graphType, IEnumerable<FieldType?>? fieldTypes)
        {
            if (fieldTypes != null)
                foreach (var fieldType in fieldTypes)
                    if (fieldType != null)
                        graphType.AddField(fieldType);
        }

        /// <summary>
        /// Returns a DIFieldType for the specified method, with the metadata set and Name, Type,
        /// Description, and DeprecationReason properties initialized.
        /// </summary>
        public static DIFieldType? CreateField(MethodInfo method, Func<Type> inferGraphType)
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

            //determine the graphtype of the field
            var graphTypeAttribute = method.GetCustomAttribute<GraphTypeAttribute>();
            Type? graphType = graphTypeAttribute?.Type;
            //infer the graphtype if it is not specified
            if (graphType == null) {
                graphType = inferGraphType();
            }
            //load the description
            string? description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
            //load the deprecation reason
            string? obsoleteDescription = method.GetCustomAttribute<ObsoleteAttribute>()?.Message;

            var fieldType = new DIFieldType {
                Name = methodName,
                Type = graphType,
                Description = description,
                DeprecationReason = obsoleteDescription,
            };

            //load the metadata
            foreach (var metaAttribute in method.GetCustomAttributes<MetadataAttribute>())
                fieldType.WithMetadata(metaAttribute.Key, metaAttribute.Value);

            return fieldType;
        }

        private const string ORIGINAL_EXPRESSION_PROPERTY_NAME = nameof(ORIGINAL_EXPRESSION_PROPERTY_NAME);

        /// <summary>
        /// Returns a DIFieldType for the specified property, with the metadata set and Name, Type,
        /// Description, and DeprecationReason properties initialized.
        /// </summary>
        public static DIFieldType? CreateField(PropertyInfo property, Func<Type> inferGraphType)
        {
            //get the field name
            string fieldName = property.Name;
            var fieldNameAttribute = property.GetCustomAttribute<NameAttribute>();
            if (fieldNameAttribute != null) {
                fieldName = fieldNameAttribute.Name;
            }
            if (fieldName == null)
                return null; //ignore field if set to null (or Ignore is specified)

            //determine the graphtype of the field
            var graphTypeAttribute = property.GetCustomAttribute<GraphTypeAttribute>();
            Type? graphType = graphTypeAttribute?.Type;
            //infer the graphtype if it is not specified
            if (graphType == null) {
                graphType = inferGraphType();
            }
            //load the description
            string? description = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
            //load the deprecation reason
            string? obsoleteDescription = property.GetCustomAttribute<ObsoleteAttribute>()?.Message;
            //create the field
            var fieldType = new DIFieldType {
                Name = fieldName,
                Type = graphType,
                Description = description,
                DeprecationReason = obsoleteDescription,
            };

            //set name of property
            fieldType.WithMetadata(ORIGINAL_EXPRESSION_PROPERTY_NAME, property.Name);
            //load the metadata
            foreach (var metaAttribute in property.GetCustomAttributes<MetadataAttribute>())
                fieldType.WithMetadata(metaAttribute.Key, metaAttribute.Value);
            //return the field
            return fieldType;
        }

        private static readonly Type[] _listTypes = new Type[] {
            typeof(IEnumerable<>),
            typeof(IList<>),
            typeof(List<>),
            typeof(ICollection<>),
            typeof(IReadOnlyCollection<>),
            typeof(IReadOnlyList<>),
            typeof(HashSet<>),
            typeof(ISet<>),
        };

        /// <summary>
        /// Analyzes a property and returns a <see cref="TypeInformation"/>
        /// struct containing type information necessary to select a graph type.
        /// </summary>
        public static TypeInformation GetTypeInformation(PropertyInfo propertyInfo, Func<PropertyInfo, IEnumerable<(Type Type, Nullability Nullable)>> getNullabilityInformationFunc)
        {
            var isList = false;
            var isNullableList = false;
            var typeTree = getNullabilityInformationFunc(propertyInfo);
            foreach (var type in typeTree) {
                if (type.Type.IsArray) {
                    //unwrap type and mark as list
                    isList = true;
                    isNullableList = type.Nullable != Nullability.NonNullable;
                    continue;
                }
                if (type.Type.IsGenericType) {
                    var g = type.Type.GetGenericTypeDefinition();
                    if (_listTypes.Contains(g)) {
                        //unwrap type and mark as list
                        isList = true;
                        isNullableList = type.Nullable != Nullability.NonNullable;
                        continue;
                    }
                }
                if (type.Type == typeof(IEnumerable) || type.Type == typeof(ICollection)) {
                    //assume list of nullable object
                    isList = true;
                    isNullableList = type.Nullable != Nullability.NonNullable;
                    break;
                }
                //found match
                var nullable = type.Nullable != Nullability.NonNullable;
                return new TypeInformation(propertyInfo, true, type.Type, nullable, isList, isNullableList, null);
            }
            //unknown type
            return new TypeInformation(propertyInfo, true, typeof(object), true, isList, isNullableList, null);
        }

        /// <summary>
        /// Analyzes a method argument or return value and returns a <see cref="TypeInformation"/>
        /// struct containing type information necessary to select a graph type.
        /// </summary>
        public static TypeInformation GetTypeInformation(ParameterInfo parameterInfo, bool isInputArgument, Func<ParameterInfo, IEnumerable<(Type Type, Nullability Nullable)>> getNullabilityInformationFunc)
        {
            var isOptionalParameter = parameterInfo.IsOptional;
            var isList = false;
            var isNullableList = false;
            var typeTree = getNullabilityInformationFunc(parameterInfo);
            foreach (var type in typeTree) {
                if (type.Type == typeof(IDataLoaderResult))
                    //assume type is nullable object
                    break;
                if (type.Type.IsArray) {
                    //unwrap type and mark as list
                    isList = true;
                    isNullableList = type.Nullable != Nullability.NonNullable;
                    continue;
                }
                if (type.Type.IsGenericType) {
                    var g = type.Type.GetGenericTypeDefinition();
                    if (g == typeof(IDataLoaderResult<>) || g == typeof(Task<>)) {
                        //unwrap type
                        continue;
                    }
                    if (_listTypes.Contains(g)) {
                        //unwrap type and mark as list
                        isList = true;
                        isNullableList = type.Nullable != Nullability.NonNullable;
                        continue;
                    }
                }
                if (type.Type == typeof(IEnumerable) || type.Type == typeof(ICollection)) {
                    //assume list of nullable object
                    isList = true;
                    isNullableList = type.Nullable != Nullability.NonNullable;
                    break;
                }
                //found match
                var nullable = type.Nullable != Nullability.NonNullable;
                if (isOptionalParameter) {
                    if (isList)
                        isNullableList = true;
                    else
                        nullable = true;
                }
                return new TypeInformation(parameterInfo, isInputArgument, type.Type, nullable, isList, isNullableList, null);
            }
            //unknown type
            if (isOptionalParameter && isList)
                isNullableList = true;
            return new TypeInformation(parameterInfo, isInputArgument, typeof(object), true, isList, isNullableList, null);
        }

        /// <summary>
        /// Converts a specified method (<see cref="MethodInfo"/> instance) into a field definition.
        /// </summary>
        public static void ProcessMethod(
            MethodInfo method,
            DIFieldType field,
            Func<MethodInfo, ParameterInfo, ParameterExpression, (QueryArgument? QueryArgument, bool UsesServices, Expression Expr)> processParameterFunc,
            Func<ParameterExpression, Expression> getInstanceExpressionFunc,
            Func<Expression, ParameterExpression, IFieldResolver> createUnscopedResolverFunc,
            Func<Expression, ParameterExpression, IFieldResolver> createScopedResolverFunc,
            bool defaultConcurrent,
            bool defaultCreateScope)
        {
            //scan the parameter list to create a list of arguments, and at the same time, generate the expressions to be used to call this method during the resolve function
            IFieldResolver resolver;
            var queryArguments = new List<QueryArgument>();
            bool concurrent = false;
            bool anyParamsUseServices = false;
            {
                var resolveFieldContextParameter = Expression.Parameter(typeof(IResolveFieldContext));
                var executeParams = new List<Expression>();
                foreach (var param in method.GetParameters()) {
                    var (queryArgument, isService, expr) = processParameterFunc(method, param, resolveFieldContextParameter);
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
                    Expression exprGetService = getInstanceExpressionFunc(resolveFieldContextParameter);
                    //then, call the method with the executeParams as the parameters
                    exprResolve = Expression.Call(exprGetService, method, executeParams.ToArray());
                }

                //determine if this should run concurrently with other resolvers
                //if it's an async static method that does not pull from services, then it's safe to run concurrently
                if (isAsync && method.IsStatic && !anyParamsUseServices) {
                    //mark this field as concurrent, so the execution strategy will run it asynchronously
                    concurrent = true;
                    //set the resolver to run the compiled resolve function
                    resolver = createUnscopedResolverFunc(exprResolve, resolveFieldContextParameter);
                } else {
                    var methodConcurrent = method.GetCustomAttribute<ConcurrentAttribute>();
                    concurrent = defaultConcurrent;
                    var scoped = defaultCreateScope;
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
                            resolver = createScopedResolverFunc(exprResolve, resolveFieldContextParameter);
                        } else {
                            //just run the compiled resolve function, and count on the method to handle multithreading issues
                            resolver = createUnscopedResolverFunc(exprResolve, resolveFieldContextParameter);
                        }
                    }
                    //for non-async methods, and instance methods that are not marked with the Concurrent attribute
                    else {
                        concurrent = false;
                        //just run the compiled resolve function
                        resolver = createUnscopedResolverFunc(exprResolve, resolveFieldContextParameter);
                    }
                }
            }

            //return the field
            field.Arguments = new QueryArguments(queryArguments);
            field.Concurrent = concurrent;
            field.Resolver = resolver;
        }

        private static readonly PropertyInfo _cancellationTokenProperty = typeof(IResolveFieldContext).GetProperty(nameof(IResolveFieldContext.CancellationToken), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo _asMethod = typeof(ResolveFieldContextExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(x => x.Name == nameof(ResolveFieldContextExtensions.As) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(IResolveFieldContext));
        private static readonly PropertyInfo _sourceProperty = typeof(IResolveFieldContext).GetProperty(nameof(IResolveFieldContext.Source), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo _getArgumentMethod = typeof(ResolveFieldContextExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(x => x.Name == nameof(ResolveFieldContextExtensions.GetArgument) && x.IsGenericMethod);

        /// <summary>
        /// Processes a parameter of a method, returning an expression based upon <see cref="IResolveFieldContext"/>, and
        /// optionally returns a query argument to be added to the field. <paramref name="usesServices"/> can be set to
        /// <see langword="true"/> to indicate that the parameter uses the service provider and may need to run within
        /// a scoped service provider for concurrent use.
        /// </summary>
        public static QueryArgument? ProcessParameter<TSource>(
            MethodInfo method,
            ParameterInfo param,
            ParameterExpression resolveFieldContextParameter,
            Func<ParameterExpression, Expression> getServiceProviderExpressionFunc,
            Func<ParameterExpression, Type, Expression> getServiceExpressionFunc,
            Func<ParameterInfo, Type> getGraphTypeFunc,
            out bool usesServices,
            out Expression expr)
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
            if (param.ParameterType == typeof(TSource) && typeof(TSource) != typeof(object) && param.Name == "source") {
                //retrieve the value and cast it to the specified type
                //e.g. Func<IResolveFieldContext, TSource> = (context) => (TSource)context.Source;
                expr = Expression.Convert(Expression.Property(resolveFieldContextParameter, _sourceProperty), param.ParameterType);
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
                expr = getServiceProviderExpressionFunc(resolveFieldContextParameter);
                //note that we have a parameter that pulls from the service provider
                usesServices = true;
                //and do not add it as a QueryArgument
                return null;
            }
            if (param.GetCustomAttribute<FromServicesAttribute>() != null) {
                //if they are pulling from a service context, pull that in
                //e.g. Func<IResolveFieldContext, IMyService> = (context) => (IMyService)AsyncServiceProvider.GetRequiredService(typeof(IMyService));
                expr = getServiceExpressionFunc(resolveFieldContextParameter, param.ParameterType);
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
            //if no specific graphtype set, check for the Id attribute, or pull from registered graph type list
            if (graphType == null) {
                graphType = getGraphTypeFunc(param);
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

        private static readonly ConcurrentDictionary<Type, ConstructorInfo> _funcFieldResolverConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        /// <summary>
        /// Returns a field resolver for a specified delegate expression. Does not create a dedicated service scope for the delegate.
        /// </summary>
        public static IFieldResolver CreateUnscopedResolver(Expression resolveExpression, ParameterExpression resolveFieldContextParameter)
        {
            var constructorInfo = _funcFieldResolverConstructors.GetOrAdd(resolveExpression.Type, t => typeof(FuncFieldResolver<>).MakeGenericType(t).GetConstructors().Single());
            var lambda = Expression.Lambda(resolveExpression, resolveFieldContextParameter).Compile();
            return (IFieldResolver)constructorInfo.Invoke(new[] { lambda });
        }

        private static readonly ConcurrentDictionary<Type, ConstructorInfo> _scopedFieldResolverConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> _scopedAsyncFieldResolverConstructors = new ConcurrentDictionary<Type, ConstructorInfo>();
        /// <summary>
        /// Returns a field resolver for a specified delegate expression. The field resolver creates a dedicated service scope for the delegate.
        /// </summary>
        public static IFieldResolver CreateScopedResolver(Expression resolveExpression, ParameterExpression resolveFieldContextParameter)
        {
            ConstructorInfo constructorInfo;
            if (typeof(Task).IsAssignableFrom(resolveExpression.Type)) {
                constructorInfo = _scopedAsyncFieldResolverConstructors.GetOrAdd(GetTaskType(resolveExpression.Type), t => typeof(ScopedAsyncFieldResolver<>).MakeGenericType(t).GetConstructors().Single());
            } else {
                constructorInfo = _scopedFieldResolverConstructors.GetOrAdd(resolveExpression.Type, t => typeof(ScopedFieldResolver<>).MakeGenericType(t).GetConstructors().Single());
            }
            var lambda = Expression.Lambda(resolveExpression, resolveFieldContextParameter).Compile();
            return (IFieldResolver)constructorInfo.Invoke(new[] { lambda });

            static Type GetTaskType(Type t)
            {
                while (t != null) {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>))
                        return t.GenericTypeArguments[0];
                    t = t.BaseType;
                }
                throw new ArgumentOutOfRangeException(nameof(t), "Type does not inherit from Task<>.");
            }
        }

        private static readonly PropertyInfo _requestServicesProperty = typeof(IResolveFieldContext).GetProperty(nameof(IResolveFieldContext.RequestServices), BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Returns an expression that returns an <see cref="IServiceProvider"/> from a <see cref="IResolveFieldContext"/>.
        /// Defaults to the following:
        /// <code>context =&gt; context.RequestServices</code>.
        /// </summary>
        public static Expression GetServiceProviderExpression(ParameterExpression resolveFieldContextParameter)
        {
            //returns: context.RequestServices
            return Expression.Property(resolveFieldContextParameter, _requestServicesProperty);
        }

        private static readonly MethodInfo _getRequiredServiceMethod = typeof(ServiceProviderServiceExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(x => x.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService) && !x.IsGenericMethod);

        /// <summary>
        /// Returns an expression that gets/creates an instance of the specified type from a <see cref="IResolveFieldContext"/>.
        /// Defaults to the following:
        /// <code>context =&gt; context.RequestServices.GetRequiredService&lt;T&gt;();</code>
        /// </summary>
        public static Expression GetServiceExpression(ParameterExpression resolveFieldContextParameter, Type serviceType)
        {
            //returns: (serviceType)(context.RequestServices.GetRequiredService(serviceType))
            var serviceProvider = GetServiceProviderExpression(resolveFieldContextParameter);
            var type = Expression.Constant(serviceType ?? throw new ArgumentNullException(nameof(serviceType)));
            var getService = Expression.Call(_getRequiredServiceMethod, serviceProvider, type);
            var cast = Expression.Convert(getService, serviceType);
            return cast;
        }

        /// <summary>
        /// Returns an expression that gets the source and casts it to the specified type.
        /// </summary>
        public static Expression GetSourceExpression(ParameterExpression resolveFieldContextParameter, Type sourceType)
        {
            //retrieve the value and cast it to the specified type
            //e.g. Func<IResolveFieldContext, TSourceType> = (context) => (TSourceType)context.Source;
            return Expression.Convert(Expression.Property(resolveFieldContextParameter, _sourceProperty), sourceType);
        }
    }
}
