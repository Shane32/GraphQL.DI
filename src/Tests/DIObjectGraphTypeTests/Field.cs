using System.ComponentModel;

namespace DIObjectGraphTypeTests;

public class Field : DIObjectGraphTypeTestBase
{
    [Fact]
    public void StaticMethod()
    {
        Configure<CStaticMethod, object>();
        VerifyField("Field1", nullable: true, concurrent: false, returnValue: "hello");
        Verify(false);
    }

    public class CStaticMethod : DIObjectGraphBase
    {
        public CStaticMethod() => throw new InvalidOperationException();

        public static string? Field1() => "hello";
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InstanceMethod(bool registered)
    {
        Configure<CInstanceMethod, object>(true, registered: registered);
        VerifyField("Field1", nullable: true, concurrent: false, returnValue: "hello");
        Verify(false);
    }

    public class CInstanceMethod : DIObjectGraphBase
    {
        public string? Field1() => "hello";
    }

    [Fact]
    public void SetsContext()
    {
        Configure<CVerifyContext, object>(true);
        _graphType!.Fields.Find("Field1")!.Resolver!.ResolveAsync(_contextMock.Object).AsTask().GetAwaiter().GetResult().ShouldBe(_contextMock.Object);
    }

    public class CVerifyContext : DIObjectGraphBase
    {
        public object Field1() => Context;
    }

    [Fact]
    public async Task StaticAsyncMethod()
    {
        Configure<CStaticAsyncMethod, object>();
        await VerifyFieldAsync("Field1", nullable: true, concurrent: true, returnValue: "hello");
        Verify(false);
    }

    public class CStaticAsyncMethod : DIObjectGraphBase
    {
        public CStaticAsyncMethod() => throw new InvalidOperationException();

        public static Task<string?> Field1() => Task.FromResult<string?>("hello");
    }

    [Fact]
    public async Task InstanceAsyncMethod()
    {
        Configure<CInstanceAsyncMethod, object>(true);
        await VerifyFieldAsync("Field1", nullable: true, concurrent: false, returnValue: "hello");
        Verify(false);
    }

    public class CInstanceAsyncMethod : DIObjectGraphBase
    {
        public Task<string?> Field1() => Task.FromResult<string?>("hello");
    }

    [Fact]
    public void Name()
    {
        Configure<CName, object>();
        VerifyField("FieldTest", true, false, "hello");
        Verify(false);
    }

    public class CName : DIObjectGraphBase
    {
        [Name("FieldTest")]
        public static string? Field1() => "hello";
    }

    [Fact]
    public void Description()
    {
        Configure<CDescription, object>();
        VerifyField("Field1", true, false, "hello").Description.ShouldBe("DescriptionTest");
        Verify(false);
    }

    public class CDescription : DIObjectGraphBase
    {
        [Description("DescriptionTest")]
        public static string? Field1() => "hello";
    }

    [Fact]
    public void Obsolete()
    {
        Configure<CObsolete, object>();
        VerifyField("Field1", true, false, "hello").DeprecationReason.ShouldBe("ObsoleteTest");
        Verify(false);
    }

    public class CObsolete : DIObjectGraphBase
    {
        [Obsolete("ObsoleteTest")]
        public static string? Field1() => "hello";
    }

    [Fact]
    public void ImplicitSource()
    {
        Configure<CImplicitSource, string>();
        _source = "testSource";
        VerifyField("Field1", true, false, "testSource");
        _contextMock.Verify(x => x.Source, Times.AtLeastOnce);
        Verify(false);
    }

    public class CImplicitSource : DIObjectGraphBase<string>
    {
        public static string? Field1(string source) => source;
    }

    [Fact]
    public void Context()
    {
        Configure<CContext, object>();
        _source = "testSource";
        VerifyField("Field1", true, false, "testSource");
        _contextMock.Verify(x => x.Source, Times.Once);
        Verify(false);
    }

    public class CContext : DIObjectGraphBase
    {
        public static string? Field1(IResolveFieldContext context) => (string?)context.Source;
    }

    [Fact(Skip = "Not supported in this version")]
    public void Context_Typed()
    {
        Configure<CContext_Typed, object>();
        _source = "testSource";
        VerifyField("Field1", true, false, "testSource");
        _contextMock.Verify(x => x.Source, Times.AtLeastOnce);
        Verify(false);
    }

    public class CContext_Typed : DIObjectGraphBase
    {
        public static string? Field1(IResolveFieldContext<object> context) => (string)context.Source;
    }

    [Fact(Skip = "Not supported in this version")]
    public void Context_WrongType()
    {
        Should.Throw<InvalidOperationException>(() => Configure<CContext_WrongType, object>());
    }

    public class CContext_WrongType : DIObjectGraphBase
    {
        public static string? Field1(IResolveFieldContext<string> context) => context.Source;
    }

    [Fact]
    public void Source()
    {
        Configure<CSource, object>();
        _source = "testSource";
        VerifyField("Field1", true, false, "testSource");
        _contextMock.Verify(x => x.Source, Times.Once);
        Verify(false);
    }

    public class CSource : DIObjectGraphBase
    {
        public static string? Field1([FromSource] object source) => (string)source;
    }

    [Fact]
    public void Source_WrongType()
    {
        Should.Throw<InvalidOperationException>(() => Configure<CSource_WrongType, int>());
    }

    public class CSource_WrongType : DIObjectGraphBase<int>
    {
        public static string Field1([FromSource] string source) => source;
    }

    [Fact]
    public void ServiceProvider()
    {
        Configure<CServiceProvider, object>();
        _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
        VerifyField("Field1", true, false, "hello");
        Verify(false);
    }

    public class CServiceProvider : DIObjectGraphBase
    {
        public static string? Field1(IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<string>();
    }

    [Fact]
    public async Task ServiceProviderForScoped()
    {
        Configure<CServiceProviderForScoped, object>();
        _scopedServiceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
        await VerifyFieldAsync("Field1", true, true, "hello");
        Verify(true);
    }

    public class CServiceProviderForScoped : DIObjectGraphBase
    {
        [Scoped]
        public static Task<string?> Field1(IServiceProvider serviceProvider) => Task.FromResult<string?>(serviceProvider.GetRequiredService<string>());
    }

    [Fact]
    public void NullableValue()
    {
        Configure<CNullableValue, object>();
        VerifyField("Field1", true, false, (int?)null);
        Verify(false);
    }

    public class CNullableValue : DIObjectGraphBase<object>
    {
        public static int? Field1() => null;
    }

    [Fact]
    public void NonNullableValue()
    {
        Configure<CNonNullableValue, object>();
        VerifyField("Field1", false, false, 1);
        Verify(false);
    }

    public class CNonNullableValue : DIObjectGraphBase<object>
    {
        public static int Field1() => 1;
    }

    [Fact]
    public void CustomType()
    {
        Configure<CCustomType, object>();
        VerifyField("Field1", typeof(IdGraphType), false, "hello");
        Verify(false);
    }

    public class CCustomType : DIObjectGraphBase<object>
    {
        [OutputType(typeof(IdGraphType))]
        public static string Field1() => "hello";
    }

    [Fact]
    public void InheritedCustomType()
    {
        Configure<CInheritedCustomType, object>();
        VerifyField("Field1", typeof(IdGraphType), false, "hello");
        Verify(false);
    }

    public class CInheritedCustomType : DIObjectGraphBase<object>
    {
        [MyIdGraphType]
        public static string Field1() => "hello";
    }

    public class MyIdGraphTypeAttribute : OutputTypeAttribute
    {
        public MyIdGraphTypeAttribute() : base(typeof(IdGraphType)) { }
    }

    [Fact]
    public void IdType()
    {
        Configure<CIdType, object>();
        VerifyField("Field1", typeof(IdGraphType), false, "hello");
        Verify(false);
    }

    public class CIdType : DIObjectGraphBase<object>
    {
        [Id]
        public static string? Field1() => "hello";
    }

    [Fact]
    public void IdTypeNonNull()
    {
        Configure<CIdTypeNonNull, object>();
        VerifyField("Field1", typeof(NonNullGraphType<IdGraphType>), false, 2);
        Verify(false);
    }

    public class CIdTypeNonNull : DIObjectGraphBase<object>
    {
        [Id]
        public static int Field1() => 2;
    }

    [Fact]
    public void IdListType()
    {
        Configure<CIdListType, object>();
        VerifyField("Field1", typeof(ListGraphType<IdGraphType>), false, new[] { "hello" });
        Verify(false);
    }

    public class CIdListType : DIObjectGraphBase<object>
    {
        [Id]
        public static string?[]? Field1() => ["hello"];
    }

    [Fact]
    public void DIGraphType()
    {
        Configure<CDIGraphType, object>();
        VerifyField("Field1", typeof(DIObjectGraphType<CDIGraphType2, object>), false, "hello");
        Verify(false);
    }

    public class CDIGraphType2 : DIObjectGraphBase<object>
    {
        public static string? Field1() => "hello";
    }

    public class CDIGraphType : DIObjectGraphBase<object>
    {
        [DIGraph(typeof(CDIGraphType2))]
        public static string? Field1() => "hello";
    }

    [Fact]
    public void DIGraphTypeNonNull()
    {
        Configure<CDIGraphTypeNonNull, object>();
        VerifyField("Field1", typeof(NonNullGraphType<DIObjectGraphType<CDIGraphType2, object>>), false, 2);
        Verify(false);
    }

    public class CDIGraphTypeNonNull : DIObjectGraphBase<object>
    {
        [DIGraph(typeof(CDIGraphType2))]
        public static int Field1() => 2;
    }

    [Fact]
    public void DIGraphListType()
    {
        Configure<CDIGraphListType, object>();
        VerifyField("Field1", typeof(ListGraphType<DIObjectGraphType<CDIGraphType2, object>>), false, new[] { "hello" });
        Verify(false);
    }

    public class CDIGraphListType : DIObjectGraphBase<object>
    {
        [DIGraph(typeof(CDIGraphType2))]
        public static string?[]? Field1() => ["hello"];
    }

    [Fact]
    public void DIGraphTypeInvalid()
    {
        Should.Throw<InvalidOperationException>(() => Configure<CDIGraphTypeInvalid, object>());
    }

    public class CDIGraphTypeInvalid : DIObjectGraphBase<object>
    {
        [DIGraph(typeof(string))]
        public static string Field1() => "hello";
    }

    [Fact]
    public async Task ScopedOnlyWhenSpecifiedWithServices()
    {
        Configure<CScopedOnlyWhenSpecified, object>();
        await VerifyFieldAsync("Field1", true, true, "hello");
        Verify(true);
    }

    public class CScopedOnlyWhenSpecified : DIObjectGraphBase<object>
    {
        [Scoped]
        public static Task<string?> Field1(IServiceProvider services) => Task.FromResult<string?>("hello");
    }

    [Fact]
    public async Task RemoveAsyncFromName()
    {
        Configure<CRemoveAsyncFromName, object>();
        await VerifyFieldAsync("Field1", true, true, "hello");
        Verify(false);
    }

    public class CRemoveAsyncFromName : DIObjectGraphBase<object>
    {
        public static Task<string?> Field1Async() => Task.FromResult<string?>("hello");
    }

    [Fact]
    public void SkipVoidMembers()
    {
        Configure<CSkipVoidMembers, object>();
        _graphType!.Fields.Find("Field1").ShouldBeNull();
        _graphType.Fields.Find("Field2").ShouldBeNull();
        _graphType.Fields.Find("Field3").ShouldNotBeNull();
    }

    public class CSkipVoidMembers : DIObjectGraphBase<object>
    {
        public static Task Field1() => Task.CompletedTask;
        public static void Field2() { }
        public static string Field3() => null!;
    }

    [Fact]
    public void Ignore()
    {
        Configure<CIgnore, object>();
        _graphType!.Fields.Find("Field1").ShouldBeNull();
        _graphType.Fields.Find("Field2").ShouldNotBeNull();
    }

    public class CIgnore : DIObjectGraphBase<object>
    {
        [Ignore]
        public static string Field1() => "hello";
        public static string Field2() => "hello";
    }

    [Fact]
    public void DisposableRegistered()
    {
        Configure<CDisposable, object>(true);
        VerifyField("FieldTest", true, false, "hello");
        Verify(false);
    }

    [Fact]
    public async Task DisposableUnRegistered()
    {
        Configure<CDisposable, object>(true, registered: false);
        var err = await Should.ThrowAsync<InvalidOperationException>(() => VerifyFieldAsync("FieldTest", true, false, "hello"));
        err.Message.ShouldBe("Could not resolve an instance of CDisposable from the service provider. DI graph types that implement IDisposable must be registered in the service provider.");
        Verify(false);
    }

    public class CDisposable : DIObjectGraphBase, IDisposable
    {
        [Name("FieldTest")]
        public string? Field1() => "hello";

        void IDisposable.Dispose() => GC.SuppressFinalize(this);
    }

    [Fact]
    public void AddsMetadata()
    {
        Configure<CAddsMetadata, object>();
        var field = VerifyField("Field1", true, false, "hello");
        field.GetMetadata<string>("test").ShouldBe("value");
        Verify(false);
    }

    public class CAddsMetadata : DIObjectGraphBase<object>
    {
        [Metadata("test", "value")]
        public static string? Field1() => "hello";
    }

    [Fact]
    public void AddsInheritedMetadata()
    {
        Configure<CAddsInheritedMetadata, object>();
        var field = VerifyField("Field1", true, false, "hello");
        field.GetMetadata<string>("test").ShouldBe("value2");
        Verify(false);
    }

    public class CAddsInheritedMetadata : DIObjectGraphBase<object>
    {
        [InheritedMetadata("value2")]
        public static string? Field1() => "hello";
    }

    private class InheritedMetadata : MetadataAttribute
    {
        public InheritedMetadata(string value) : base("test", value) { }
    }

    [Theory]
    [InlineData("Field1", typeof(GraphQLClrOutputTypeReference<string>))]
    [InlineData("Field3", typeof(NonNullGraphType<GraphQLClrOutputTypeReference<int>>))]
    [InlineData("Field5", typeof(GraphQLClrOutputTypeReference<int>))]
    [InlineData("Field6", typeof(GraphQLClrOutputTypeReference<object>))] //Need to fix graphql-dotnet bug 2441 first
    [InlineData("Field7", typeof(GraphQLClrOutputTypeReference<string>))]
    [InlineData("Field8", typeof(NonNullGraphType<GraphQLClrOutputTypeReference<int>>))]
    [InlineData("Field9", typeof(GraphQLClrOutputTypeReference<int>))]
    [InlineData("Field10", typeof(GraphQLClrOutputTypeReference<string>))]
    public void SupportsDataLoader(string fieldName, Type graphType)
    {
        Configure<CSupportsDataLoader, object>(true);
        VerifyField<object?>(fieldName, graphType, false, null);
        Verify(false);
    }

    public class CSupportsDataLoader : DIObjectGraphBase<object>
    {
        public IDataLoaderResult<string?> Field1() => null!;
        public IDataLoaderResult<int> Field3() => null!;
        public IDataLoaderResult<int?> Field5() => null!;
        public IDataLoaderResult Field6() => null!;
        public Task<IDataLoaderResult<string?>> Field7() => Task.FromResult<IDataLoaderResult<string?>>(null!);
        public Task<IDataLoaderResult<int>> Field8() => Task.FromResult<IDataLoaderResult<int>>(null!);
        public Task<IDataLoaderResult<int?>> Field9() => Task.FromResult<IDataLoaderResult<int?>>(null!);
        public Task<IDataLoaderResult<IDataLoaderResult<string?>>> Field10() => Task.FromResult<IDataLoaderResult<IDataLoaderResult<string?>>>(null!);
    }
}
