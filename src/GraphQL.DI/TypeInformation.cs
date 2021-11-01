using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
        public ParameterInfo ParameterInfo;

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
            IsInputType = isInputType;
            Type = type;
            IsNullable = isNullable;
            IsList = isList;
            ListIsNullable = listIsNullable;
            GraphType = graphType;
        }
    }
}
