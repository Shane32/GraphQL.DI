using System;
using System.ComponentModel;
using GraphQL.DI;
using Shouldly;
using Xunit;

namespace DIObjectGraphTypeTests
{
    public class Graph : DIObjectGraphTypeTestBase
    {
        [Fact]
        public void GraphName()
        {
            Configure<CGraphName, object>().Name.ShouldBe("TestGraphName");
        }

        [Name("TestGraphName")]
        public class CGraphName : DIObjectGraphBase { }

        [Fact]
        public void GraphDescription()
        {
            Configure<CGraphDescription, object>().Description.ShouldBe("TestGraphDescription");
        }

        [Description("TestGraphDescription")]
        public class CGraphDescription : DIObjectGraphBase { }

        [Fact]
        public void GraphObsolete()
        {
            Configure<CGraphObsolete, object>().DeprecationReason.ShouldBe("TestDeprecationReason");
        }

        [Obsolete("TestDeprecationReason")]
        public class CGraphObsolete : DIObjectGraphBase { }

        [Fact]
        public void GraphMetadata()
        {
            Configure<CGraphMetadata, object>().GetMetadata<string>("test").ShouldBe("value");
        }

        [Metadata("test", "value")]
        public class CGraphMetadata : DIObjectGraphBase { }
    }
}
