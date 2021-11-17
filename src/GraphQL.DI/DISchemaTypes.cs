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

        /// <summary>
        /// Initializes a new instance for the specified schema, and with the specified type resolver.
        /// Input and output clr types not mapped to a graph type utilize <see cref="AutoInputObjectGraphType{TSourceType}"/>
        /// and <see cref="AutoObjectGraphType{TSourceType}"/> respectively.
        /// </summary>
        public DISchemaTypes(ISchema schema, IServiceProvider serviceProvider) : base(schema, serviceProvider)
        {
            _autoMapInputTypes = true;
            _autoMapOutputTypes = true;
        }

        /// <summary>
        /// Initializes a new instance for the specified schema, and with the specified type resolver.
        /// Input and output clr types not mapped to a graph type utilize <see cref="AutoInputObjectGraphType{TSourceType}"/>
        /// and <see cref="AutoObjectGraphType{TSourceType}"/> respectively, if specified via
        /// <paramref name="autoMapInputTypes"/> and <paramref name="autoMapOutputTypes"/>.
        /// </summary>
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
        /// Called when no input graph type is registered for the specified CLR type.
        /// Return true if the specified CLR type should be wrapped with <see cref="AutoInputObjectGraphType{TSourceType}"/>.
        /// Defaults value depends on the constructor arguments.
        /// </summary>
        protected virtual bool AutoMapInputType(Type clrType) => _autoMapInputTypes;

        /// <summary>
        /// Called when no output graph type is registered for the specified CLR type.
        /// Return true if the specified CLR type should be wrapped with <see cref="AutoObjectGraphType{TSourceType}"/>.
        /// Defaults value depends on the constructor arguments.
        /// </summary>
        protected virtual bool AutoMapOutputType(Type clrType) => _autoMapOutputTypes;
    }
}
