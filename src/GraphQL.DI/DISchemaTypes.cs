using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <inheritdoc/>
    public class DISchemaTypes : SchemaTypes
    {
        private readonly bool _autoMapInputTypes = true;
        private readonly bool _autoMapOutputTypes = true;

        private readonly Type _autoInputObjectGraphType = typeof(AutoInputObjectGraphType<>);
        /*
        /// <summary>
        /// The generic type definition which is used to construct auto-mapped input graph types.
        /// Defaults to <see cref="AutoInputObjectGraphType{TSourceType}"/>.
        /// </summary>
        protected Type AutoInputObjectGraphType {
            get => _autoInputObjectGraphType;
            set {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (!value.IsGenericTypeDefinition || value.GenericTypeArguments.Length != 1)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"The type '{value}' is not a generic type definition with an arity of 1.");
                if (!typeof(IInputObjectGraphType).IsAssignableFrom(value))
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"The type '{value}' does not implement {nameof(IInputObjectGraphType)}");
                _autoInputObjectGraphType = value;
            }
        }
        */

        private readonly Type _autoObjectGraphType = typeof(AutoObjectGraphType<>);
        /*
        /// <summary>
        /// The generic type definition which is used to construct auto-mapped Output graph types.
        /// Defaults to <see cref="AutoObjectGraphType{TSourceType}"/>.
        /// </summary>
        protected Type AutoObjectGraphType {
            get => _autoObjectGraphType;
            set {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (!value.IsGenericTypeDefinition || value.GenericTypeArguments.Length != 1)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"The type '{value}' is not a generic type definition with an arity of 1.");
                if (!typeof(IObjectGraphType).IsAssignableFrom(value))
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"The type '{value}' does not implement {nameof(IObjectGraphType)}");
                _autoObjectGraphType = value;
            }
        }
        */

        /// <summary>
        /// Initializes a new instance for the specified schema, and with the specified type resolver.
        /// Input and output clr types not mapped to a graph type utilize <see cref="AutoInputObjectGraphType{TSourceType}"/>
        /// and <see cref="AutoObjectGraphType{TSourceType}"/> respectively.
        /// </summary>
        public DISchemaTypes(ISchema schema, IServiceProvider serviceProvider) : base(schema, serviceProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance for the specified schema, and with the specified type resolver.
        /// Input and output clr types not mapped to a graph type utilize <see cref="AutoInputObjectGraphType{TSourceType}"/>
        /// and <see cref="AutoObjectGraphType{TSourceType}"/> respectively, if specified via
        /// <paramref name="autoMapInputTypes"/> and <paramref name="autoMapOutputTypes"/>.
        /// </summary>
        public DISchemaTypes(ISchema schema, IServiceProvider serviceProvider, bool autoMapInputTypes, bool autoMapOutputTypes) : base(schema, serviceProvider)
        {
            if (autoMapInputTypes == false || autoMapOutputTypes == false)
                throw new NotSupportedException(
                    "This functionality is not yet supported due to a design constraint within GraphQL.NET. " +
                    $"Please override {nameof(AutoMapInputType)} and/or {nameof(AutoMapOutputType)} to disable auto mapping.");
            _autoMapInputTypes = autoMapInputTypes;
            _autoMapOutputTypes = autoMapOutputTypes;
        }

        /// <inheritdoc/>
        protected override Type? GetGraphTypeFromClrType(Type clrType, bool isInputType, List<(Type ClrType, Type GraphType)> typeMappings)
        {
            var type = base.GetGraphTypeFromClrType(clrType, isInputType, typeMappings);
            if (type == null && isInputType && clrType.IsClass && AutoMapInputType(clrType)) {
                return typeof(AutoInputObjectGraphType<>).MakeGenericType(clrType);
            }
            if (type == null && !isInputType && clrType.IsClass && AutoMapOutputType(clrType)) {
                return typeof(AutoObjectGraphType<>).MakeGenericType(clrType);
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
