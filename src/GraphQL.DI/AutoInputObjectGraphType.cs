using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <summary>
    /// An automatically-generated graph type for public properties on the specified input model.
    /// </summary>
    public class AutoInputObjectGraphType<TSourceType> : InputObjectGraphType<TSourceType>
    {
        private const string ORIGINAL_EXPRESSION_PROPERTY_NAME = nameof(ORIGINAL_EXPRESSION_PROPERTY_NAME);

        /// <summary>
        /// Creates a GraphQL type from <typeparamref name="TSourceType"/>.
        /// </summary>
        public AutoInputObjectGraphType()
        {
            var classType = typeof(TSourceType);
            //allow default name / description / obsolete tags to remain if not overridden
            var nameAttribute = classType.GetCustomAttribute<NameAttribute>();
            if (nameAttribute != null)
                Name = nameAttribute.Name;
            //note: should probably take the default name from TSourceType's name, rather than this type's name
            var descriptionAttribute = classType.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
                Description = descriptionAttribute.Description;
            var obsoleteAttribute = classType.GetCustomAttribute<ObsoleteAttribute>();
            if (obsoleteAttribute != null)
                DeprecationReason = obsoleteAttribute.Message;
            //pull metadata
            foreach (var metadataAttribute in classType.GetCustomAttributes<MetadataAttribute>())
                Metadata.Add(metadataAttribute.Key, metadataAttribute.Value);

            foreach (var property in GetRegisteredProperties()) {
                if (property != null) {
                    var fieldType = ProcessProperty(property);
                    if (fieldType != null)
                        AddField(fieldType);

                }
            }
        }

        /// <summary>
        /// Returns a list of properties that should have fields created for them.
        /// </summary>
        protected virtual IEnumerable<PropertyInfo> GetRegisteredProperties()
            => typeof(TSourceType).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanWrite);

        /// <summary>
        /// Processes the specified property and returns a <see cref="FieldType"/>
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual FieldType? ProcessProperty(PropertyInfo property)
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
                graphType = InferGraphType(ApplyAttributes(GetTypeInformation(property)));
            }
            //load the description
            string? description = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
            //load the deprecation reason
            string? obsoleteDescription = property.GetCustomAttribute<ObsoleteAttribute>()?.Message;
            //load the default value
            object? defaultValue = property.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            //create the field
            var fieldType = new FieldType() {
                Type = graphType,
                Name = fieldName,
                Description = description,
                DeprecationReason = obsoleteDescription,
                DefaultValue = defaultValue,
            };
            //set name of property
            fieldType.WithMetadata(ORIGINAL_EXPRESSION_PROPERTY_NAME, property.Name);
            //load the metadata
            foreach (var metaAttribute in property.GetCustomAttributes<MetadataAttribute>())
                fieldType.WithMetadata(metaAttribute.Key, metaAttribute.Value);
            //return the field
            return fieldType;
        }

        /// <inheritdoc cref="ReflectionExtensions.GetNullabilityInformation(ParameterInfo)"/>
        protected virtual IEnumerable<(Type Type, Nullability Nullable)> GetNullabilityInformation(PropertyInfo parameter)
        {
            return parameter.GetNullabilityInformation();
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
        protected virtual TypeInformation GetTypeInformation(PropertyInfo propertyInfo)
        {
            var isList = false;
            var isNullableList = false;
            var typeTree = GetNullabilityInformation(propertyInfo);
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
