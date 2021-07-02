using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
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
#pragma warning disable CS0618 // Member is obsolete
            Configure<CGraphObsolete, object>().DeprecationReason.ShouldBe("TestDeprecationReason");
#pragma warning restore CS0618 // Member is obsolete
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

        [Fact]
        public void CanOverrideMembers()
        {
            var test = new CCanOverrideMembersGraphType();
            test.Fields.Count.ShouldBe(0);
        }

        public class CCanOverrideMembersGraphType : DIObjectGraphType<CCanOverrideMembers>
        {
            protected override IEnumerable<MethodInfo> GetMethodsToProcess() => Array.Empty<MethodInfo>();
        }

        public class CCanOverrideMembers : DIObjectGraphBase
        {
            public static string Field1() => "hello";
        }
    }
}
