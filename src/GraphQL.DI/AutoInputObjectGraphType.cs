using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <summary>
    /// An automatically-generated graph type for public properties on the specified input model.
    /// </summary>
    public class AutoInputObjectGraphType<TSourceType> : InputObjectGraphType<TSourceType>
    {
        /// <summary>
        /// Creates a GraphQL type from <typeparamref name="TSourceType"/>.
        /// </summary>
        public AutoInputObjectGraphType()
        {
            GraphTypeHelper.ConfigureGraph<TSourceType>(this, GetDefaultName);

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
            var fieldTypeList = new List<FieldType>(properties.Count());
            foreach (var property in properties) {
                var fieldType = ProcessProperty(property);
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
            if (GetType() != typeof(AutoInputObjectGraphType<TSourceType>))
                return null;

            //without this code, the name would default to AutoInputObjectGraphType_1
            var name = typeof(TSourceType).Name.Replace('`', '_');
            if (name.EndsWith("Model", StringComparison.InvariantCulture))
                name = name.Substring(0, name.Length - "Model".Length);
            return name;
        }

        /// <summary>
        /// Indicates that the fields and arguments should be added to the graph type alphabetically.
        /// </summary>
        public static bool SortMembers { get; set; } = true;

        /// <summary>
        /// Returns a list of properties that should have fields created for them.
        /// </summary>
        protected virtual IEnumerable<PropertyInfo> GetPropertiesToProcess()
        {
            var props = typeof(TSourceType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanWrite);
            return props;
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
            //load the default value
            fieldType.DefaultValue = property.GetCustomAttribute<DefaultValueAttribute>()?.Value;
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
            => GraphTypeHelper.GetTypeInformation(propertyInfo, true, GetNullabilityInformation);

        /// <summary>
        /// Apply <see cref="RequiredAttribute"/>, <see cref="OptionalAttribute"/>, <see cref="RequiredListAttribute"/>,
        /// <see cref="OptionalListAttribute"/>, <see cref="IdAttribute"/> and <see cref="DIGraphAttribute"/> over
        /// the supplied <see cref="TypeInformation"/>.
        /// Override this method to enforce specific graph types for specific CLR types, or to implement custom
        /// attributes to change graph type selection behavior.
        /// </summary>
        protected virtual TypeInformation ApplyAttributes(TypeInformation typeInformation)
            => typeInformation.ApplyAttributes(typeInformation.MemberInfo);

        /// <summary>
        /// Returns a GraphQL input type for a specified CLR type
        /// </summary>
        protected virtual Type InferGraphType(TypeInformation typeInformation)
            => typeInformation.InferGraphType();

    }
}
