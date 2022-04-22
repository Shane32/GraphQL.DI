using System;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks a method's return graph type to be a specified DI graph type.
    /// Useful when the return type cannot be inferred (often when it is of type <see cref="object"/>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class DIGraphAttribute : GraphQLAttribute
    {
        /// <summary>
        /// Marks a method's return graph type to be a specified DI graph type.
        /// </summary>
        /// <param name="graphBaseType">A type that inherits <see cref="DIObjectGraphBase"/>.</param>
        public DIGraphAttribute(Type graphBaseType)
        {
            GraphBaseType = graphBaseType;
        }

        /// <inheritdoc/>
        public override void Modify(TypeInformation typeInformation)
        {
            if (typeInformation.IsInputType)
                return;
            var iface = GraphBaseType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDIObjectGraphBase<>));
            if (iface != null) {
                typeInformation.GraphType = typeof(DIObjectGraphType<,>).MakeGenericType(GraphBaseType, iface.GetGenericArguments()[0]);
            } else {
                throw new InvalidOperationException($"Type '{GraphBaseType.Name}' does not implement {nameof(IDIObjectGraphBase)}<TSource>; check {nameof(DIGraphAttribute)} attribute marked on '{typeInformation.MemberInfo.DeclaringType.Name}.{typeInformation.MemberInfo.Name}'.");
            }
        }

        /// <summary>
        /// The DI graph type that inherits <see cref="DIObjectGraphBase"/>.
        /// </summary>
        public Type GraphBaseType { get; }
    }
}
