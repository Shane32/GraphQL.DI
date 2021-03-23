using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GraphQL.DI;
using Shouldly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DIObjectGraphTypeTests
{
    [TestClass]
    public class Graph : DIObjectGraphTypeTestBase
    {
        [TestMethod]
        public void GraphName()
        {
            Configure<CGraphName, object>().Name.ShouldBe("TestGraphName");
        }

        [Name("TestGraphName")]
        public class CGraphName : DIObjectGraphBase { }

        [TestMethod]
        public void GraphDescription()
        {
            Configure<CGraphDescription, object>().Description.ShouldBe("TestGraphDescription");
        }

        [System.ComponentModel.Description("TestGraphDescription")]
        public class CGraphDescription : DIObjectGraphBase { }

        [TestMethod]
        public void GraphObsolete()
        {
            Configure<CGraphObsolete, object>().DeprecationReason.ShouldBe("TestDeprecationReason");
        }

        [Obsolete("TestDeprecationReason")]
        public class CGraphObsolete : DIObjectGraphBase { }

        [TestMethod]
        public void GraphMetadata()
        {
            Configure<CGraphMetadata, object>().GetMetadata<string>("test").ShouldBe("value");
        }

        [Metadata("test", "value")]
        public class CGraphMetadata : DIObjectGraphBase { }

        [TestMethod]
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
