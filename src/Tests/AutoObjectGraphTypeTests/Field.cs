using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.DI;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace AutoObjectGraphTypeTests
{
    public class Field : AutoObjectGraphTypeTestBase
    {
        [Fact]
        public void StaticMethod()
        {
            Configure<CStaticMethod>();
            VerifyField("Field1", nullable: true, concurrent: false, returnValue: "hello");
            Verify(false);
        }

        public class CStaticMethod
        {
            public static string Field1() => "hello";
        }

        [Fact]
        public void InstanceMethod()
        {
            Configure<CInstanceMethod>();
            VerifyField("Field1", nullable: true, concurrent: false, returnValue: "hello");
            Verify(false);
        }

        public class CInstanceMethod
        {
            public string Field1() => "hello";
        }

        [Fact]
        public async Task StaticAsyncMethod()
        {
            Configure<CStaticAsyncMethod>();
            await VerifyFieldAsync("Field1", nullable: true, concurrent: true, returnValue: "hello");
            Verify(false);
        }

        public class CStaticAsyncMethod
        {
            public static Task<string> Field1() => Task.FromResult<string>("hello");
        }

        [Fact]
        public async Task InstanceAsyncMethod()
        {
            Configure<CInstanceAsyncMethod>();
            await VerifyFieldAsync("Field1", nullable: true, concurrent: false, returnValue: "hello");
            Verify(false);
        }

        public class CInstanceAsyncMethod
        {
            public Task<string> Field1() => Task.FromResult<string>("hello");
        }

        [Fact]
        public void Name()
        {
            Configure<CName>();
            VerifyField("FieldTest", true, false, "hello");
            Verify(false);
        }

        public class CName
        {
            [Name("FieldTest")]
            public static string Field1() => "hello";
        }

        [Fact]
        public void Description()
        {
            Configure<CDescription>();
            VerifyField("Field1", true, false, "hello").Description.ShouldBe("DescriptionTest");
            Verify(false);
        }

        public class CDescription
        {
            [Description("DescriptionTest")]
            public static string Field1() => "hello";
        }

        [Fact]
        public void Obsolete()
        {
            Configure<CObsolete>();
            VerifyField("Field1", true, false, "hello").DeprecationReason.ShouldBe("ObsoleteTest");
            Verify(false);
        }

        public class CObsolete
        {
            [Obsolete("ObsoleteTest")]
            public static string Field1() => "hello";
        }

        [Fact]
        public void Context()
        {
            Configure<CContext>();
            _source = "testSource";
            VerifyField("Field1", true, false, "testSource");
            _contextMock.Verify(x => x.Source, Times.Once);
            Verify(false);
        }

        public class CContext
        {
            public static string Field1(IResolveFieldContext context) => (string)context.Source;
        }

        [Fact]
        public void Context_Typed()
        {
            Configure<CContext_Typed>();
            _source = "testSource";
            VerifyField("Field1", true, false, "testSource");
            _contextMock.Verify(x => x.Source, Times.AtLeastOnce);
            Verify(false);
        }

        public class CContext_Typed
        {
            public static string Field1(IResolveFieldContext<object> context) => (string)context.Source;
        }

        [Fact]
        public void Context_WrongType()
        {
            Should.Throw<InvalidOperationException>(() => Configure<CContext_WrongType>());
        }

        public class CContext_WrongType
        {
            public static string Field1(IResolveFieldContext<string> context) => context.Source;
        }

        [Fact]
        public void Source()
        {
            Configure<CSource>();
            _source = new CSource() { Value = "testSource" };
            VerifyField("Field1", true, false, "testSource");
            _contextMock.Verify(x => x.Source, Times.Once);
            Verify(false);
        }

        public class CSource
        {
            public string Value;
            public static string Field1([FromSource] CSource source) => source.Value;
        }

        [Fact]
        public void Source2()
        {
            Configure<CSource2>();
            _source = new CSource2() { Value = "testSource" };
            VerifyField("Field1", true, false, "testSource");
            _contextMock.Verify(x => x.Source, Times.Once);
            Verify(false);
        }

        public class CSource2
        {
            public string Value;
            public string Field1() => Value;
        }

        [Fact]
        public void Source_WrongType()
        {
            Should.Throw<InvalidOperationException>(() => Configure<CSource_WrongType>());
        }

        public class CSource_WrongType
        {
            public static string Field1([FromSource] string source) => source;
        }

        [Fact]
        public void ServiceProvider()
        {
            Configure<CServiceProvider>();
            _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CServiceProvider
        {
            public static string Field1(IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<string>();
        }

        [Fact]
        public async Task ServiceProviderForScoped()
        {
            Configure<CServiceProviderForScoped>();
            _scopedServiceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(true);
        }

        public class CServiceProviderForScoped
        {
            [Concurrent(true)]
            public static Task<string> Field1(IServiceProvider serviceProvider) => Task.FromResult<string>(serviceProvider.GetRequiredService<string>());
        }

        [Fact]
        public void NullableValue()
        {
            Configure<CNullableValue>();
            VerifyField("Field1", true, false, (int?)null);
            Verify(false);
        }

        public class CNullableValue
        {
            public static int? Field1() => null;
        }

        [Fact]
        public void NullableValueExplicit()
        {
            Configure<CNullableValueExplicit>();
            VerifyField("Field1", true, false, (int?)1);
            Verify(false);
        }

        public class CNullableValueExplicit
        {
            [Optional]
            public static int Field1() => 1;
        }

        [Fact]
        public void NonNullableValue()
        {
            Configure<CNonNullableValue>();
            VerifyField("Field1", false, false, 1);
            Verify(false);
        }

        public class CNonNullableValue
        {
            public static int Field1() => 1;
        }

        [Fact]
        public void Required()
        {
            Configure<CRequired>();
            VerifyField("Field1", false, false, "hello");
            Verify(false);
        }

        public class CRequired
        {
            [Required]
            public static string Field1() => "hello";
        }

        [Fact]
        public void InheritedRequired()
        {
            Configure<CInheritedRequired>();
            VerifyField("Field1", false, false, "hello");
            Verify(false);
        }

        public class CInheritedRequired
        {
            [MyRequired]
            public static string Field1() => "hello";
        }

        public class MyRequiredAttribute : RequiredAttribute { }

        [Fact]
        public async Task RequiredTask()
        {
            Configure<CRequiredTask>();
            await VerifyFieldAsync("Field1", false, true, "hello");
            Verify(false);
        }

        public class CRequiredTask
        {
            [Required]
            public static Task<string> Field1() => Task.FromResult("hello");
        }

        [Fact]
        public void CustomType()
        {
            Configure<CCustomType>();
            VerifyField("Field1", typeof(IdGraphType), false, "hello");
            Verify(false);
        }

        public class CCustomType
        {
            [GraphType(typeof(IdGraphType))]
            public static string Field1() => "hello";
        }

        [Fact]
        public void InheritedCustomType()
        {
            Configure<CInheritedCustomType>();
            VerifyField("Field1", typeof(IdGraphType), false, "hello");
            Verify(false);
        }

        public class CInheritedCustomType
        {
            [MyIdGraphType]
            public static string Field1() => "hello";
        }

        public class MyIdGraphTypeAttribute : GraphTypeAttribute
        {
            public MyIdGraphTypeAttribute() : base(typeof(IdGraphType)) { }
        }

        [Fact]
        public void IdType()
        {
            Configure<CIdType>();
            VerifyField("Field1", typeof(IdGraphType), false, "hello");
            Verify(false);
        }

        public class CIdType
        {
            [Id]
            public static string Field1() => "hello";
        }

        [Fact]
        public void IdTypeNonNull()
        {
            Configure<CIdTypeNonNull>();
            VerifyField("Field1", typeof(NonNullGraphType<IdGraphType>), false, 2);
            Verify(false);
        }

        public class CIdTypeNonNull
        {
            [Id]
            public static int Field1() => 2;
        }

        [Fact]
        public void IdListType()
        {
            Configure<CIdListType>();
            VerifyField("Field1", typeof(ListGraphType<IdGraphType>), false, new[] { "hello" });
            Verify(false);
        }

        public class CIdListType
        {
            [Id]
            public static string[] Field1() => new[] { "hello" };
        }

        [Fact]
        public void DIGraphType()
        {
            Configure<CDIGraphType>();
            VerifyField("Field1", typeof(DIObjectGraphType<CDIGraphType2, object>), false, "hello");
            Verify(false);
        }

        public class CDIGraphType2 : DIObjectGraphBase<object>
        {
            public static string Field1() => "hello";
        }

        public class CDIGraphType
        {
            [DIGraph(typeof(CDIGraphType2))]
            public static string Field1() => "hello";
        }

        [Fact]
        public void DIGraphTypeNonNull()
        {
            Configure<CDIGraphTypeNonNull>();
            VerifyField("Field1", typeof(NonNullGraphType<DIObjectGraphType<CDIGraphType2, object>>), false, 2);
            Verify(false);
        }

        public class CDIGraphTypeNonNull
        {
            [DIGraph(typeof(CDIGraphType2))]
            public static int Field1() => 2;
        }

        [Fact]
        public void DIGraphListType()
        {
            Configure<CDIGraphListType>();
            VerifyField("Field1", typeof(ListGraphType<DIObjectGraphType<CDIGraphType2, object>>), false, new[] { "hello" });
            Verify(false);
        }

        public class CDIGraphListType
        {
            [DIGraph(typeof(CDIGraphType2))]
            public static string[] Field1() => new[] { "hello" };
        }

        [Fact]
        public void DIGraphTypeInvalid()
        {
            Should.Throw<InvalidOperationException>(() => Configure<CDIGraphTypeInvalid>());
        }

        public class CDIGraphTypeInvalid
        {
            [DIGraph(typeof(string))]
            public static string Field1() => "hello";
        }

        [Fact]
        public async Task Concurrent()
        {
            Configure<CConcurrent>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(false);
        }

        public class CConcurrent
        {
            [Concurrent]
            public static Task<string> Field1() => Task.FromResult<string>("hello");
        }

        [Fact]
        public void ConcurrentIgnoredForSynchronousMethods()
        {
            Configure<CConcurrentIgnoredForSynchronousMethods>();
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CConcurrentIgnoredForSynchronousMethods
        {
            [Concurrent]
            public static string Field1() => "hello";
        }

        [Fact]
        public async Task AlwaysConcurrentForStatic()
        {
            Configure<CAlwaysConcurrentForStatic>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(false);
        }

        public class CAlwaysConcurrentForStatic
        {
            public static Task<string> Field1() => Task.FromResult<string>("hello");
        }

        [Fact]
        public async Task NotScopedWhenNoServices()
        {
            Configure<CNotScopedWhenNoServices>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(false);
        }

        public class CNotScopedWhenNoServices
        {
            [Concurrent(true)]
            public static Task<string> Field1() => Task.FromResult<string>("hello");
        }

        [Fact]
        public async Task AlwaysConcurrentForStaticUnlessService()
        {
            Configure<CAlwaysConcurrentForStaticUnlessService>();
            _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
            await VerifyFieldAsync("Field1", true, false, "hello");
            Verify(false);
        }

        public class CAlwaysConcurrentForStaticUnlessService
        {
            public static Task<string> Field1([FromServices] string value) => Task.FromResult<string>(value);
        }

        [Fact]
        public async Task ScopedOnlyWhenSpecifiedWithServices()
        {
            Configure<CScopedOnlyWhenSpecified>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(true);
        }

        public class CScopedOnlyWhenSpecified
        {
            [Concurrent(true)]
            public static Task<string> Field1(IServiceProvider services) => Task.FromResult<string>("hello");
        }

        [Fact]
        public async Task InheritsConcurrent()
        {
            Configure<CInheritsConcurrent>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(true);
        }

        [Concurrent(true)]
        public class CInheritsConcurrent
        {
            public static Task<string> Field1(IServiceProvider services) => Task.FromResult<string>("hello");
        }

        [Fact]
        public async Task InheritedConcurrentOverridable()
        {
            Configure<CInheritedConcurrentOverridable>();
            await VerifyFieldAsync("Field1", true, false, "hello");
            Verify(false);
        }

        [Concurrent(true)]
        public class CInheritedConcurrentOverridable
        {
            [Concurrent(Concurrent = false)]
            public static Task<string> Field1(IServiceProvider services) => Task.FromResult<string>("hello");
        }

        [Fact]
        public async Task RemoveAsyncFromName()
        {
            Configure<CRemoveAsyncFromName>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(false);
        }

        public class CRemoveAsyncFromName
        {
            public static Task<string> Field1Async() => Task.FromResult("hello");
        }

        [Fact]
        public void SkipVoidMembers()
        {
            Configure<CSkipVoidMembers>();
            _graphType.Fields.Find("Field1").ShouldBeNull();
            _graphType.Fields.Find("Field2").ShouldBeNull();
            _graphType.Fields.Find("Field3").ShouldNotBeNull();
        }

        public class CSkipVoidMembers
        {
            public static Task Field1() => Task.CompletedTask;
            public static void Field2() { }
            public static string Field3() => null!;
        }

        [Fact]
        public void SkipNullName()
        {
            Configure<CSkipNullName>();
            _graphType.Fields.Find("Field1").ShouldBeNull();
            _graphType.Fields.Find("Field2").ShouldNotBeNull();
        }

        public class CSkipNullName
        {
            [Name(null)]
            public static string Field1() => "hello";
            public static string Field2() => "hello";
        }

        [Fact]
        public void Ignore()
        {
            Configure<CIgnore>();
            _graphType.Fields.Find("Field1").ShouldBeNull();
            _graphType.Fields.Find("Field2").ShouldNotBeNull();
        }

        public class CIgnore
        {
            [Ignore]
            public static string Field1() => "hello";
            public static string Field2() => "hello";
        }

        [Fact]
        public void AddsMetadata()
        {
            Configure<CAddsMetadata>();
            var field = VerifyField("Field1", true, false, "hello");
            field.GetMetadata<string>("test").ShouldBe("value");
            Verify(false);
        }

        public class CAddsMetadata
        {
            [Metadata("test", "value")]
            public static string Field1() => "hello";
        }

        [Fact]
        public void AddsInheritedMetadata()
        {
            Configure<CAddsInheritedMetadata>();
            var field = VerifyField("Field1", true, false, "hello");
            field.GetMetadata<string>("test").ShouldBe("value2");
            Verify(false);
        }

        public class CAddsInheritedMetadata
        {
            [InheritedMetadata("value2")]
            public static string Field1() => "hello";
        }

        private class InheritedMetadata : MetadataAttribute
        {
            public InheritedMetadata(string value) : base("test", value) { }
        }

        [Theory]
        [InlineData("Field1", typeof(GraphQLClrOutputTypeReference<string>))]
        [InlineData("Field2", typeof(NonNullGraphType<GraphQLClrOutputTypeReference<string>>))]
        [InlineData("Field3", typeof(NonNullGraphType<GraphQLClrOutputTypeReference<int>>))]
        [InlineData("Field4", typeof(GraphQLClrOutputTypeReference<int>))]
        [InlineData("Field5", typeof(GraphQLClrOutputTypeReference<int>))]
        //[InlineData("Field6", typeof(GraphQLClrOutputTypeReference<object>)] //Need to fix graphql-dotnet bug 2441 first
        [InlineData("Field7", typeof(GraphQLClrOutputTypeReference<string>))]
        [InlineData("Field8", typeof(NonNullGraphType<GraphQLClrOutputTypeReference<int>>))]
        [InlineData("Field9", typeof(GraphQLClrOutputTypeReference<int>))]
        [InlineData("Field10", typeof(GraphQLClrOutputTypeReference<string>))]
        public void SupportsDataLoader(string fieldName, Type graphType)
        {
            Configure<CSupportsDataLoader>();
            VerifyField<object>(fieldName, graphType, false, null);
            Verify(false);
        }

        public class CSupportsDataLoader
        {
            public IDataLoaderResult<string> Field1() => null;
            [Required]
            public IDataLoaderResult<string> Field2() => null;
            public IDataLoaderResult<int> Field3() => null;
            [Optional]
            public IDataLoaderResult<int> Field4() => null;
            public IDataLoaderResult<int?> Field5() => null;
            public IDataLoaderResult Field6() => null;
            public Task<IDataLoaderResult<string>> Field7() => null;
            public Task<IDataLoaderResult<int>> Field8() => null;
            public Task<IDataLoaderResult<int?>> Field9() => null;
            public Task<IDataLoaderResult<IDataLoaderResult<string>>> Field10() => null;
        }
    }
}
