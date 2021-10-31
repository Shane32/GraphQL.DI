using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.DI
{
    internal struct NullabilityInformation
    {
        public Nullability DefaultNullability;
        public Nullability[] Nullability;
    }
}
