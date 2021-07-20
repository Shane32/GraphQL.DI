using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.DI
{
    public interface IDIObjectGraphBase
    {
        public IResolveFieldContext Context { get; set; }
    }

    public interface IDIObjectGraphBase<TSource> : IDIObjectGraphBase
    {
    }
}