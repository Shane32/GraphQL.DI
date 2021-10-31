using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.DI
{
    /// <summary>
    /// Contains type and nullability information for a method return type or argument type.
    /// </summary>
    public class TypeInformation
    {
        /// <summary>
        /// The underlying type represented. This might be the underlying type of a <see cref="Nullable{T}"/>
        /// or the underlying type of a <see cref="IEnumerable{T}"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Indicates if the underlying type is nullable.
        /// </summary>
        public bool IsNullable { get; }

        /// <summary>
        /// Indicates that this represents a list of elements.
        /// </summary>
        public bool IsList { get; }

        /// <summary>
        /// Indicates if the list is nullable.
        /// </summary>
        public bool ListIsNullable { get; }

        /// <summary>
        /// Initializes an instance with the specified properties.
        /// </summary>
        /// <param name="type">The underlying type.</param>
        /// <param name="isNullable">Indicates that the underlying type is nullable.</param>
        /// <param name="isList">Indicates that this represents a list of elements.</param>
        /// <param name="listIsNullable">Indicates that the list is nullable.</param>
        public TypeInformation(Type type, bool isNullable, bool isList, bool listIsNullable)
        {
            Type = type;
            IsNullable = isNullable;
            IsList = isList;
            ListIsNullable = listIsNullable;
        }
    }
}
