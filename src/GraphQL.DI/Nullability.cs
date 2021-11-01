using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.DI
{
    public enum Nullability : byte
    {
        Unknown = 0,
        NonNullable = 1,
        Nullable = 2,
    }
}
