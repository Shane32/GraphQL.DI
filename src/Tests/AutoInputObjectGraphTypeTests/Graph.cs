using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GraphQL.DI;
using GraphQL.Types;
using Shouldly;
using Xunit;

namespace AutoInputObjectGraphTypeTests
{
    public class Graph
    {
        private IInputObjectGraphType _graphType;

        private IInputObjectGraphType Configure<TSourceType>()
        {
            _graphType = new AutoInputObjectGraphType<TSourceType>();
            return _graphType;
        }

        [Fact]
        public void GraphName()
        {
            Configure<CGraphName>().Name.ShouldBe("TestGraphName");
        }

        [Name("TestGraphName")]
        public class CGraphName { }

        [Fact]
        public void DefaultGraphName()
        {
            Configure<CDefault>().Name.ShouldBe("CDefault");
        }

        public class CDefault { }

        [Fact]
        public void DefaultGraphNameInputModel()
        {
            Configure<CDefaultInputModel>().Name.ShouldBe("CDefaultInput");
        }

        public class CDefaultInputModel { }

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

        public class CCanOverrideMembersGraphType : AutoInputObjectGraphType<CCanOverrideMembers>
        {
            protected override IEnumerable<PropertyInfo> GetPropertiesToProcess() => Array.Empty<PropertyInfo>();
        }

        public class CCanOverrideMembers
        {
            public static string Field1() => "hello";
        }
    }
}
