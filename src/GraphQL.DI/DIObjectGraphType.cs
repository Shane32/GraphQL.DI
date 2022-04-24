using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.DI
{
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
            if (name.EndsWith("Graph") && name.Length > 5)
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
            => (Expression<Func<IResolveFieldContext, TDIGraph>>)((IResolveFieldContext context) => MemberInstanceFunc(context));

        /// <inheritdoc/>
        private TDIGraph MemberInstanceFunc(IResolveFieldContext context)
        {
            // create a new instance of DIObject, filling in any constructor arguments from DI
            var graph = ActivatorUtilities.GetServiceOrCreateInstance<TDIGraph>(context.RequestServices ?? throw new MissingRequestServicesException());
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
            if (argumentInfo.ParameterInfo.ParameterType == typeof(IServiceProvider))
            {
                argumentInfo.SetDelegate(context => context.RequestServices ?? throw new MissingRequestServicesException());
            }
            if (argumentInfo.ParameterInfo.Name == "source" && argumentInfo.ParameterInfo.ParameterType == typeof(TSource))
            {
                argumentInfo.SetDelegate(context => (TSource)context.Source);
            }
            argumentInfo.ApplyAttributes();
            return argumentInfo;
        }

        #region Patch for IEnumerable
        /// <inheritdoc/>
        protected override TypeInformation GetTypeInformation(MemberInfo memberInfo)
        {
            var typeInformation = memberInfo switch
            {
                PropertyInfo propertyInfo => new MyTypeInformation(propertyInfo, false),
                MethodInfo methodInfo => new MyTypeInformation(methodInfo),
                FieldInfo fieldInfo => new MyTypeInformation(fieldInfo, false),
                _ => throw new ArgumentOutOfRangeException(nameof(memberInfo), "Only properties, methods and fields are supported."),
            };
            typeInformation.ApplyAttributes();
            return typeInformation;
        }

        private class MyTypeInformation : TypeInformation
        {
            public MyTypeInformation(PropertyInfo propertyInfo, bool isInput) : base(propertyInfo, isInput) { }
            public MyTypeInformation(MethodInfo methodInfo) : base(methodInfo) { }
            public MyTypeInformation(FieldInfo fieldInfo, bool isInput) : base(fieldInfo, isInput) { }

            public override Type ConstructGraphType()
            {
                var type = GraphType;
                if (type != null)
                {
                    if (!IsNullable)
                        type = typeof(NonNullGraphType<>).MakeGenericType(type);
                }
                else
                {
                    type = GetGraphTypeFromType(Type, IsNullable, IsInputType ? TypeMappingMode.InputType : TypeMappingMode.OutputType);
                }
                if (IsList)
                {
                    type = typeof(ListGraphType<>).MakeGenericType(type);
                    if (!ListIsNullable)
                        type = typeof(NonNullGraphType<>).MakeGenericType(type);
                }
                return type;
            }

            /// <summary>
            /// Gets the graph type for the indicated type.
            /// </summary>
            /// <param name="type">The type for which a graph type is desired.</param>
            /// <param name="isNullable">if set to <c>false</c> if the type explicitly non-nullable.</param>
            /// <param name="mode">Mode to use when mapping CLR type to GraphType.</param>
            /// <returns>A Type object representing a GraphType that matches the indicated type.</returns>
            /// <remarks>This can handle arrays, lists and other collections implementing IEnumerable.</remarks>
            private static Type GetGraphTypeFromType(Type type, bool isNullable = false, TypeMappingMode mode = TypeMappingMode.UseBuiltInScalarMappings)
            {
                while (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDataLoaderResult<>))
                {
                    type = type.GetGenericArguments()[0];
                }

                if (type == typeof(IDataLoaderResult))
                {
                    type = typeof(object);
                }

                if (typeof(Task).IsAssignableFrom(type))
                    throw new ArgumentOutOfRangeException(nameof(type), "Task types cannot be coerced to a graph type; please unwrap the task type before calling this method.");

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = type.GetGenericArguments()[0];
                    if (!isNullable)
                    {
                        throw new ArgumentOutOfRangeException(nameof(isNullable),
                            $"Explicitly nullable type: Nullable<{type.Name}> cannot be coerced to a non nullable GraphQL type.");
                    }
                }

                Type? graphType = null;

                if (type.IsArray)
                {
                    var clrElementType = type.GetElementType()!;
                    var elementType = GetGraphTypeFromType(clrElementType, IsNullableType(clrElementType), mode); // isNullable from elementType, not from parent array
                    graphType = typeof(ListGraphType<>).MakeGenericType(elementType);
                }
                else if (TryGetEnumerableElementType(type, out var clrElementType))
                {
                    var elementType = GetGraphTypeFromType(clrElementType!, IsNullableType(clrElementType!), mode); // isNullable from elementType, not from parent container
                    graphType = typeof(ListGraphType<>).MakeGenericType(elementType);
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var attr = type.GetCustomAttribute<GraphQLMetadataAttribute>();
                    if (attr != null)
                    {
                        if (mode == TypeMappingMode.InputType)
                            graphType = attr.InputType;
                        else if (mode == TypeMappingMode.OutputType)
                            graphType = attr.OutputType;
                        else if (attr.InputType == attr.OutputType) // scalar
                            graphType = attr.InputType;
                    }
#pragma warning restore CS0618 // Type or member is obsolete

                    if (mode == TypeMappingMode.InputType)
                    {
                        var inputAttr = type.GetCustomAttribute<InputTypeAttribute>();
                        if (inputAttr != null)
                            graphType = inputAttr.InputType;
                    }
                    else if (mode == TypeMappingMode.OutputType)
                    {
                        var outputAttr = type.GetCustomAttribute<OutputTypeAttribute>();
                        if (outputAttr != null)
                            graphType = outputAttr.OutputType;
                    }

                    if (graphType == null)
                    {
                        if (mode == TypeMappingMode.UseBuiltInScalarMappings)
                        {
                            if (!SchemaTypes.BuiltInScalarMappings.TryGetValue(type, out graphType))
                            {
                                if (type.IsEnum)
                                {
                                    graphType = typeof(EnumerationGraphType<>).MakeGenericType(type);
                                }
                                else
                                {
                                    throw new ArgumentOutOfRangeException(nameof(type), $"The CLR type '{type.FullName}' cannot be coerced effectively to a GraphQL type.");
                                }
                            }
                        }
                        else
                        {
                            graphType = (mode == TypeMappingMode.OutputType ? typeof(GraphQLClrOutputTypeReference<>) : typeof(GraphQLClrInputTypeReference<>)).MakeGenericType(type);
                        }
                    }
                }

                if (!isNullable)
                {
                    graphType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
                }

                return graphType;

                //TODO: rewrite nullability condition in v5
                static bool IsNullableType(Type type) => !type.IsValueType || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            private static readonly Type[] _enumerableTypes = new[] {
                typeof(IEnumerable<>),
                typeof(IList<>),
                typeof(List<>),
                typeof(ICollection<>),
                typeof(IReadOnlyCollection<>),
                typeof(IReadOnlyList<>),
                typeof(HashSet<>),
                typeof(ISet<>),
            };

            private static bool TryGetEnumerableElementType(Type type, out Type? elementType)
            {
                if (type == typeof(IEnumerable))
                {
                    elementType = typeof(object);
                    return true;
                }

                if (!type.IsGenericType || !_enumerableTypes.Contains(type.GetGenericTypeDefinition()))
                {
                    elementType = null;
                    return false;
                }

                elementType = type.GetGenericArguments()[0];
                return true;
            }
        }
        #endregion
    }
}
