using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <inheritdoc/>
    public class DISchemaTypes : SchemaTypes
    {
        private readonly bool _autoMapInputTypes;
        private readonly bool _autoMapOutputTypes;

        /// <inheritdoc/>
        public DISchemaTypes(ISchema schema, IServiceProvider serviceProvider, bool autoMapInputTypes, bool autoMapOutputTypes) : base(schema, serviceProvider)
        {
            _autoMapInputTypes = autoMapInputTypes;
            _autoMapOutputTypes = autoMapOutputTypes;
        }

        /// <inheritdoc/>
        protected override Type? GetGraphTypeFromClrType(Type clrType, bool isInputType, List<(Type ClrType, Type GraphType)> typeMappings)
        {
            var type = base.GetGraphTypeFromClrType(clrType, isInputType, typeMappings);
            if (type == null && isInputType && clrType.IsClass) {
                return typeof(AutoInputObjectGraphType<>).MakeGenericType(clrType);
            }
            return type;
        }

        /// <summary>
        /// Return true if the specified CLR type should be wrapped with <see cref="AutoInputObjectGraphType{TSourceType}"/>
        /// to create a graph type if no graph type is mapped for this input type.
        /// </summary>
        protected virtual bool AutoMapInputType(Type clrType) => _autoMapInputTypes;

        /// <summary>
        /// Return true if the specified CLR type should be wrapped with <see cref="AutoOutputObjectGraphType{TSourceType}"/>
        /// to create a graph type if no graph type is mapped for this output type.
        /// </summary>
        protected virtual bool AutoMapOutputType(Type clrType) => _autoMapOutputTypes;
    }
}
