using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GraphQL.DI;
using Shouldly;
using Xunit;

namespace AutoObjectGraphTypeTests
{
    public class Graph : AutoObjectGraphTypeTestBase
    {
        [Fact]
        public void GraphName()
        {
            Configure<CGraphName>().Name.ShouldBe("TestGraphName");
        }

        [Name("TestGraphName")]
        public class CGraphName { }

        [Fact]
        public void GraphDefaultName()
        {
            Configure<CGraphDefaultName>().Name.ShouldBe("CGraphDefaultName");
        }

        public class CGraphDefaultName { }

        [Fact]
        public void GraphDescription()
        {
            Configure<CGraphDescription>().Description.ShouldBe("TestGraphDescription");
        }

        [Description("TestGraphDescription")]
        public class CGraphDescription { }

        [Fact]
        public void GraphObsolete()
        {
#pragma warning disable CS0618 // Member is obsolete
            Configure<CGraphObsolete>().DeprecationReason.ShouldBe("TestDeprecationReason");
#pragma warning restore CS0618 // Member is obsolete
        }

        [Obsolete("TestDeprecationReason")]
        public class CGraphObsolete { }

        [Fact]
        public void GraphMetadata()
        {
            Configure<CGraphMetadata>().GetMetadata<string>("test").ShouldBe("value");
        }

        [Metadata("test", "value")]
        public class CGraphMetadata { }

        [Fact]
        public void CanOverrideMembers()
        {
            var test = new CCanOverrideMembersGraphType();
            test.Fields.Count.ShouldBe(0);
        }

        public class CCanOverrideMembersGraphType : AutoObjectGraphType<CCanOverrideMembers>
        {
            protected override IEnumerable<MethodInfo> GetMethodsToProcess() => Array.Empty<MethodInfo>();
        }

        public class CCanOverrideMembers
        {
            public static string Field1() => "hello";
        }
    }
}
