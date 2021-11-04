using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <summary>
    /// Contains type and nullability information for a method return type or argument type.
    /// </summary>
    public struct TypeInformation
    {
        /// <summary>
        /// The argument or return parameter of the method being inspected.
        /// </summary>
        public ParameterInfo? ParameterInfo;

        /// <summary>
        /// The member being inspected.
        /// </summary>
        public MemberInfo MemberInfo;

        /// <summary>
        /// Indicates that this is an input type (an argument); false for output types.
        /// </summary>
        public bool IsInputType;

        /// <summary>
        /// The underlying type represented. This might be the underlying type of a <see cref="Nullable{T}"/>
        /// or the underlying type of a <see cref="IEnumerable{T}"/>.
        /// </summary>
        public Type Type;

        /// <summary>
        /// Indicates if the underlying type is nullable.
        /// </summary>
        public bool IsNullable;

        /// <summary>
        /// Indicates that this represents a list of elements.
        /// </summary>
        public bool IsList;

        /// <summary>
        /// Indicates if the list is nullable.
        /// </summary>
        public bool ListIsNullable;

        /// <summary>
        /// The graph type of the underlying CLR type.
        /// </summary>
        public Type? GraphType;

        /// <summary>
        /// Initializes an instance with the specified properties.
        /// </summary>
        /// <param name="parameterInfo">The argument or return parameter of the method being inspected.</param>
        /// <param name="isInputType">Indicates that this is an input type (an argument); false for output types.</param>
        /// <param name="type">The underlying type.</param>
        /// <param name="isNullable">Indicates that the underlying type is nullable.</param>
        /// <param name="isList">Indicates that this represents a list of elements.</param>
        /// <param name="listIsNullable">Indicates that the list is nullable.</param>
        /// <param name="graphType">The graph type of the underlying CLR type; null if not specified.</param>
        public TypeInformation(ParameterInfo parameterInfo, bool isInputType, Type type, bool isNullable, bool isList, bool listIsNullable, Type? graphType)
        {
            ParameterInfo = parameterInfo;
            MemberInfo = parameterInfo.Member;
            IsInputType = isInputType;
            Type = type;
            IsNullable = isNullable;
            IsList = isList;
            ListIsNullable = listIsNullable;
            GraphType = graphType;
        }

        /// <summary>
        /// Initializes an instance with the specified properties.
        /// </summary>
        /// <param name="memberInfo">The member being inspected.</param>
        /// <param name="isInputType">Indicates that this is an input type (an argument); false for output types.</param>
        /// <param name="type">The underlying type.</param>
        /// <param name="isNullable">Indicates that the underlying type is nullable.</param>
        /// <param name="isList">Indicates that this represents a list of elements.</param>
        /// <param name="listIsNullable">Indicates that the list is nullable.</param>
        /// <param name="graphType">The graph type of the underlying CLR type; null if not specified.</param>
        public TypeInformation(MemberInfo memberInfo, bool isInputType, Type type, bool isNullable, bool isList, bool listIsNullable, Type? graphType)
        {
            ParameterInfo = null;
            MemberInfo = memberInfo;
            IsInputType = isInputType;
            Type = type;
            IsNullable = isNullable;
            IsList = isList;
            ListIsNullable = listIsNullable;
            GraphType = graphType;
        }

        internal Type InferGraphType()
        {
            var t = GraphType;
            if (t != null) {
                if (!IsNullable)
                    t = typeof(NonNullGraphType<>).MakeGenericType(t);
            } else {
                t = Type.GetGraphTypeFromType(IsNullable, IsInputType ? TypeMappingMode.InputType : TypeMappingMode.OutputType);
            }
            if (IsList) {
                t = typeof(ListGraphType<>).MakeGenericType(t);
                if (!ListIsNullable)
                    t = typeof(NonNullGraphType<>).MakeGenericType(t);
            }
            return t;
        }

        /// <summary>
        /// Returns a new instance with <see cref="RequiredAttribute"/>, <see cref="OptionalAttribute"/>, <see cref="RequiredListAttribute"/>,
        /// <see cref="OptionalListAttribute"/>, <see cref="IdAttribute"/> and <see cref="DIGraphAttribute"/>
        /// applied as necessary.
        /// </summary>
        internal TypeInformation ApplyAttributes(ICustomAttributeProvider member)
        {
            var typeInformation = this; //copy struct
            //var member = examineParent ? (ICustomAttributeProvider)typeInformation.ParameterInfo.Member : typeInformation.ParameterInfo;
            if (typeInformation.IsNullable) {
                if (member.IsDefined(typeof(RequiredAttribute), false))
                    typeInformation.IsNullable = false;
                if (member.IsDefined(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false))
                    typeInformation.IsNullable = false;
            } else {
                if (member.IsDefined(typeof(OptionalAttribute), false))
                    typeInformation.IsNullable = true;
            }
            if (typeInformation.IsList) {
                if (typeInformation.ListIsNullable) {
                    if (member.IsDefined(typeof(RequiredListAttribute), false))
                        typeInformation.ListIsNullable = false;
                } else {
                    if (member.IsDefined(typeof(OptionalListAttribute), false))
                        typeInformation.ListIsNullable = true;
                }
            }
            if (member.IsDefined(typeof(IdAttribute), false))
                typeInformation.GraphType = typeof(IdGraphType);
            else if (member.GetCustomAttributes(typeof(DIGraphAttribute), false).SingleOrDefault() is DIGraphAttribute diGraphAttribute) {
                var iface = diGraphAttribute.GraphBaseType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDIObjectGraphBase<>));
                if (iface == null) {
                    throw new InvalidOperationException($"Member '{typeInformation.MemberInfo.DeclaringType.Name}.{typeInformation.MemberInfo.Name}' is marked with [DIGraph] specifying type '{diGraphAttribute.GraphBaseType.Name}' which does not inherit {nameof(IDIObjectGraphBase)}<T>.");
                }
                typeInformation.GraphType = typeof(DIObjectGraphType<,>).MakeGenericType(diGraphAttribute.GraphBaseType, iface.GetGenericArguments()[0]);
            }
            return typeInformation;
        }

    }
}
