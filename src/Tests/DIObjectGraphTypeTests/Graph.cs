using System.ComponentModel;

namespace DIObjectGraphTypeTests;

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
    public void GraphDefaultName()
    {
        Configure<CGraphDefaultName, object>().Name.ShouldBe("CGraphDefaultName");
        new DIObjectGraphType<CGraphDefaultName>().Name.ShouldBe("CGraphDefaultName");
    }

    public class CGraphDefaultName : DIObjectGraphBase { }

    [Fact]
    public void GraphDefaultName2()
    {
        Configure<CGraphDefaultName2, Class1>().Name.ShouldBe("CGraphDefaultName2");
    }

    [Fact]
    public void GraphDefaultName3()
    {
        Configure<CGraphDefaultNameGraph, object>().Name.ShouldBe("CGraphDefaultName");
        new DIObjectGraphType<CGraphDefaultNameGraph>().Name.ShouldBe("CGraphDefaultName");
    }

    public class CGraphDefaultNameGraph : DIObjectGraphBase { }

    public class CGraphDefaultName2 : DIObjectGraphBase<Class1> { }
    public class Class1 { }

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

    public class CCanOverrideMembers : DIObjectGraphBase
    {
        public static string Field1() => "hello";
    }
}
