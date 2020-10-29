using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.DI
{
    public class DIObjectGraphType<TDIGraph> : DIObjectGraphType<TDIGraph, object> where TDIGraph : DIObjectGraphBase<object> { }
    public class DIObjectGraphType<TDIGraph, TSource> : ObjectGraphType<TSource> where TDIGraph : DIObjectGraphBase<TSource>
    {
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

            //give inherited classes a chance to mutate the field type list before they are added to the graph type list
            var fieldTypes = CreateFieldTypeList();
            if (fieldTypes != null)
                foreach (var fieldType in fieldTypes)
                    AddField(fieldType);
        }

        //grab some methods via reflection for us to use later
        private static readonly MethodInfo getRequiredServiceMethod = typeof(ServiceProviderServiceExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(x => x.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService) && !x.IsGenericMethod);
        private static readonly MethodInfo asMethod = typeof(ResolveFieldContextExtensions).GetMethod(nameof(ResolveFieldContextExtensions.As), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo getArgumentMethod = typeof(GraphQL.ResolveFieldContextExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(x => x.Name == nameof(GraphQL.ResolveFieldContextExtensions.GetArgument) && x.IsGenericMethod);
        private static readonly PropertyInfo sourceProperty = typeof(IResolveFieldContext).GetProperty(nameof(IResolveFieldContext.Source), BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo requestServicesProperty = typeof(IResolveFieldContext).GetProperty(nameof(IResolveFieldContext.RequestServices), BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo cancellationTokenProperty = typeof(IResolveFieldContext).GetProperty(nameof(IResolveFieldContext.CancellationToken), BindingFlags.Public | BindingFlags.Instance);

        protected virtual List<DIFieldType> CreateFieldTypeList()
        {
            //scan for public members
            var methods = GetMethodsToProcess();
            var fieldTypeList = new List<DIFieldType>(methods.Count());
            foreach (var method in methods.Where(x => !x.ContainsGenericParameters)) {
                var fieldType = ProcessMethod(method);
                if (fieldType != null)
                    fieldTypeList.Add(fieldType);
            }
            return fieldTypeList;
        }

        protected virtual IEnumerable<MethodInfo> GetMethodsToProcess()
        {
            return typeof(TDIGraph).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        protected virtual DIFieldType ProcessMethod(MethodInfo method)
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
                //define the resolve function
                Func<IResolveFieldContext, object> resolverFunc;
                if (method.IsStatic) {
                    //for static methods, no need to pull an instance of the class from the service provider
                    //just call the static method with the executeParams as the parameters
                    Expression exprResolve = Expression.Convert(Expression.Call(method, executeParams.ToArray()), typeof(object));
                    //compile the function and save it as our resolve function
                    resolverFunc = Expression.Lambda<Func<IResolveFieldContext, object>>(exprResolve, resolveFieldContextParameter).Compile();
                } else {
                    //for instance methods, pull an instance of the class from the service provider
                    Expression exprGetService = GetInstanceExpression(resolveFieldContextParameter);
                    //then, call the method with the executeParams as the parameters
                    Expression exprResolve = Expression.Convert(Expression.Call(exprGetService, method, executeParams.ToArray()), typeof(object));
                    //compile the function and save it as our resolve function
                    resolverFunc = Expression.Lambda<Func<IResolveFieldContext, object>>(exprResolve, resolveFieldContextParameter).Compile();
                }

                //determine if this should run concurrently with other resolvers
                //if it's an async static method that does not pull from services, then it's safe to run concurrently
                if (typeof(Task).IsAssignableFrom(method.ReturnType) && method.IsStatic && !anyParamsUseServices) {
                    //mark this field as concurrent, so the execution strategy will run it asynchronously
                    concurrent = true;
                    //set the resolver to run the compiled resolve function
                    resolver = CreateUnscopedResolver(resolverFunc);
                }
                //for methods that return a Task and are marked with the Concurrent attribute,
                else if (typeof(Task).IsAssignableFrom(method.ReturnType) && method.GetCustomAttributes<ConcurrentAttribute>().Any()) {
                    //mark this field as concurrent, so the execution strategy will run it asynchronously
                    concurrent = true;
                    //determine if a new DI scope is required
                    if (method.GetCustomAttributes<ConcurrentAttribute>().Any(x => x.CreateNewScope)) {
                        //the resolve function needs to create a scope,
                        //  then run the compiled resolve function (which creates an instance of the class),
                        //  then release the scope once the task has been awaited
                        resolver = CreateScopedResolver(resolverFunc);
                    } else {
                        //just run the compiled resolve function, and count on the method to handle multithreading issues
                        resolver = CreateUnscopedResolver(resolverFunc);
                    }
                }
                //for non-async methods, and instance methods that are not marked with the Concurrent attribute
                else {
                    //just run the compiled resolve function
                    resolver = CreateUnscopedResolver(resolverFunc);
                }
            }

            //process the method's attributes and add the field
            {
                //determine if the field is required
                var isRequired = method.GetCustomAttribute<RequiredAttribute>() != null || method.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null;
                if (method.ReturnType.IsValueType && !(method.ReturnType.IsConstructedGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                    //non-nullable value types are implicitly required, unless they are optional
                    isRequired = true;
                }
                //determine the graphtype of the field
                var graphTypeAttribute = method.GetCustomAttribute<GraphTypeAttribute>();
                Type graphType = graphTypeAttribute?.Type;
                IGraphType graphTypeResolved = graphTypeAttribute?.ResolvedType;
                //infer the graphtype if it is not specified
                if (graphType == null && graphTypeResolved == null) {
                    if (method.ReturnType.IsConstructedGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)) {
                        graphType = InferOutputGraphType(method.ReturnType.GetGenericArguments()[0], isRequired);
                    } else {
                        graphType = InferOutputGraphType(method.ReturnType, isRequired);
                    }
                }
                //load the description
                string description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
                //load the deprecation reason
                string obsoleteDescription = method.GetCustomAttribute<ObsoleteAttribute>()?.Message;
                //load the metadata
                var metadata = new Dictionary<string, object>();
                foreach (var metaAttribute in method.GetCustomAttributes<MetadataAttribute>())
                    metadata.Add(metaAttribute.Key, metaAttribute.Value);

                //create the field
                return new DIFieldType() {
                    Type = graphType,
                    ResolvedType = graphTypeResolved,
                    Name = methodName,
                    Arguments = new QueryArguments(queryArguments.ToArray()),
                    Resolver = resolver,
                    Description = description,
                    Concurrent = concurrent,
                    DeprecationReason = obsoleteDescription,
                    Metadata = metadata,
                };
            }

        }

        protected virtual Type InferInputGraphType(Type type, bool isRequired)
        {
            return type.GetGraphTypeFromType(!isRequired);
        }

        protected virtual Type InferOutputGraphType(Type type, bool isRequired)
        {
            return type.GetGraphTypeFromType(!isRequired);
        }

        protected virtual Expression GetInstanceExpression(ParameterExpression resolveFieldContextParameter)
        {
            return GetServiceExpression(resolveFieldContextParameter, typeof(TDIGraph));
        }

        protected virtual Expression GetServiceProviderExpression(ParameterExpression resolveFieldContextParameter)
        {
            //returns: context.RequestServices
            return Expression.Property(resolveFieldContextParameter, requestServicesProperty);
        }

        protected virtual Expression GetServiceExpression(ParameterExpression resolveFieldContextParameter, Type serviceType)
        {
            //returns: (serviceType)(context.RequestServices.GetRequiredService(serviceType))
            return Expression.Convert(Expression.Call(getRequiredServiceMethod, GetServiceProviderExpression(resolveFieldContextParameter), Expression.Constant(serviceType)), serviceType);
        }

        protected virtual IFieldResolver CreateUnscopedResolver(Func<IResolveFieldContext, object> resolveFunc)
        {
            return new FuncFieldResolver<object>(resolveFunc);
        }

        protected virtual IFieldResolver CreateScopedResolver(Func<IResolveFieldContext, object> resolverFunc)
        {
            return new AsyncFieldResolver<object>(async (context) => {
                var serviceProvider = context.RequestServices ?? throw new InvalidOperationException("No service provider defined in this context");
                using (var newScope = serviceProvider.CreateScope()) {
                    context = new ContextWrapper(context, newScope.ServiceProvider);
                    //run the compiled resolve function, which should return a Task<>
                    var ret = resolverFunc(context);
                    if (ret is Task task) {
                        await task.ConfigureAwait(false);
                        return task.GetResult();
                    }
                    return ret; //cannot occur, since the return type has already been determined to be a Task<>
                }
            });
        }

        protected virtual QueryArgument ProcessParameter(MethodInfo method, ParameterInfo param, ParameterExpression resolveFieldContextParameter, out bool usesServices, out Expression expr)
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
                expr = Expression.MakeMemberAccess(resolveFieldContextParameter, cancellationTokenProperty);
                //and do not add it as a QueryArgument
                return null;
            }
            if (param.ParameterType.IsConstructedGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IResolveFieldContext<>)) {
                //validate that constructed type matches TSource
                var genericType = param.ParameterType.GetGenericArguments()[0];
                if (!genericType.IsAssignableFrom(typeof(TSource)))
                    throw new InvalidOperationException($"Invalid {nameof(IResolveFieldContext)}<> type for method {method.Name}");
                //convert the IResolveFieldContext to the specified ResolveFieldContext<>
                var asMethodTyped = asMethod.MakeGenericMethod(genericType);
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
                expr = Expression.Convert(Expression.Property(resolveFieldContextParameter, sourceProperty), param.ParameterType);
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

            //determine if this query argument is required
            var isRequired = param.GetCustomAttribute<RequiredAttribute>() != null || param.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null;
            if (param.ParameterType.IsValueType && !param.IsOptional && !(param.ParameterType.IsConstructedGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                //non-nullable value types are implicitly required, unless they are optional
                isRequired = true;
            }

            //load the specified graph type
            var graphTypeAttribute = param.GetCustomAttribute<GraphTypeAttribute>();
            Type graphType = graphTypeAttribute?.Type;
            IGraphType graphTypeResolved = graphTypeAttribute?.ResolvedType;
            //if no specific graphtype set, pull from registered graph type list
            if (graphType == null && graphTypeResolved == null) {
                graphType = InferInputGraphType(param.ParameterType, isRequired);
            }

            //construct the query argument
            QueryArgument argument;
            if (graphType != null)
                argument = new QueryArgument(graphType);
            else
                argument = new QueryArgument(graphTypeResolved);

            argument.Name = nameAttribute?.Name ?? param.Name;
            argument.Description = param.GetCustomAttribute<DescriptionAttribute>()?.Description;
            foreach (var metaAttribute in param.GetCustomAttributes<MetadataAttribute>())
                argument.Metadata.Add(metaAttribute.Key, metaAttribute.Value);

            //pull/create the default value
            object defaultValue = null;
            if (param.IsOptional) {
                defaultValue = param.DefaultValue;
            } else if (param.ParameterType.IsValueType) {
                defaultValue = Activator.CreateInstance(param.ParameterType);
            }

            //construct a call to ResolveFieldContextExtensions.GetArgument, passing in the appropriate default value
            var getArgumentMethodTyped = getArgumentMethod.MakeGenericMethod(param.ParameterType);
            //e.g. Func<IResolveFieldContext, int> = (context) => ResolveFieldContextExtensions.GetArgument<int>(context, argument.Name, defaultValue);
            expr = Expression.Call(getArgumentMethodTyped, resolveFieldContextParameter, Expression.Constant(argument.Name), Expression.Constant(defaultValue, param.ParameterType));

            //return the query argument
            return argument;
        }

    }
}
