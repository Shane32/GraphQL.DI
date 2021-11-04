using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using GraphQL.Types;

namespace GraphQL.DI
{
    /// <inheritdoc/>
    public class DISchemaTypes : SchemaTypes
    {
        /// <inheritdoc/>
        public DISchemaTypes(ISchema schema, IServiceProvider serviceProvider) : base(schema, serviceProvider)
        {
        }

        /// <inheritdoc/>
        protected override Type? GetGraphTypeFromClrType(Type clrType, bool isInputType, List<(Type ClrType, Type GraphType)> typeMappings)
        {
            var type = base.GetGraphTypeFromClrType(clrType, isInputType, typeMappings);
            if (type == null && clrType.IsClass && clrType.IsConstructedGenericType && isInputType) {
                return typeof(AutoInputObjectGraphType<>).MakeGenericType(clrType);
            }
            return type;
        }
    }
}
