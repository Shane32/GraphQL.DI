using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DI;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace DIObjectGraphTypeTests
{
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
            public CStaticMethod() => throw new Exception();

            public static string Field1() => "hello";
        }

        [Fact]
        public void InstanceMethod()
        {
            Configure<CInstanceMethod, object>(true);
            VerifyField("Field1", nullable: true, concurrent: false, returnValue: "hello");
            Verify(false);
        }

        public class CInstanceMethod : DIObjectGraphBase
        {
            public string Field1() => "hello";
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
            public CStaticAsyncMethod() => throw new Exception();

            public static Task<string> Field1() => Task.FromResult("hello");
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
            public Task<string> Field1() => Task.FromResult("hello");
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
            public static string Field1() => "hello";
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
            public static string Field1() => "hello";
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
            public static string Field1() => "hello";
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
            public static string Field1(IResolveFieldContext context) => (string)context.Source;
        }

        [Fact]
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
            public static string Field1(IResolveFieldContext<object> context) => (string)context.Source;
        }

        [Fact]
        public void Context_WrongType()
        {
            Should.Throw<InvalidOperationException>(() => Configure<CContext_WrongType, object>());
        }

        public class CContext_WrongType : DIObjectGraphBase
        {
            public static string Field1(IResolveFieldContext<string> context) => context.Source;
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
            public static string Field1([FromSource] object source) => (string)source;
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
            public static string Field1(IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<string>();
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
            [Concurrent(true)]
            public static Task<string> Field1(IServiceProvider serviceProvider) => Task.FromResult(serviceProvider.GetRequiredService<string>());
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
        public void Required()
        {
            Configure<CRequired, object>();
            VerifyField("Field1", false, false, "hello");
            Verify(false);
        }

        public class CRequired : DIObjectGraphBase<object>
        {
            [Required]
            public static string Field1() => "hello";
        }

        [Fact]
        public void CustomType()
        {
            Configure<CCustomType, object>();
            VerifyField("Field1", typeof(StringGraphType), false, "hello");
            Verify(false);
        }

        public class CCustomType : DIObjectGraphBase<object>
        {
            [GraphType(typeof(StringGraphType))]
            public static string Field1() => "hello";
        }

        [Fact]
        public async Task Concurrent()
        {
            Configure<CConcurrent, object>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(false);
        }

        public class CConcurrent : DIObjectGraphBase<object>
        {
            [Concurrent]
            public static Task<string> Field1() => Task.FromResult("hello");
        }

        [Fact]
        public void ConcurrentIgnoredForSynchronousMethods()
        {
            Configure<CConcurrentIgnoredForSynchronousMethods, object>();
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CConcurrentIgnoredForSynchronousMethods : DIObjectGraphBase<object>
        {
            [Concurrent]
            public static string Field1() => "hello";
        }

        [Fact]
        public async Task AlwaysConcurrentForStatic()
        {
            Configure<CAlwaysConcurrentForStatic, object>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(false);
        }

        public class CAlwaysConcurrentForStatic : DIObjectGraphBase<object>
        {
            public static Task<string> Field1() => Task.FromResult("hello");
        }

        [Fact]
        public async Task NotScopedWhenNoServices()
        {
            Configure<CNotScopedWhenNoServices, object>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(false);
        }

        public class CNotScopedWhenNoServices : DIObjectGraphBase<object>
        {
            [Concurrent(true)]
            public static Task<string> Field1() => Task.FromResult("hello");
        }

        [Fact]
        public async Task AlwaysConcurrentForStaticUnlessService()
        {
            Configure<CAlwaysConcurrentForStaticUnlessService, object>();
            _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
            await VerifyFieldAsync("Field1", true, false, "hello");
            Verify(false);
        }

        public class CAlwaysConcurrentForStaticUnlessService : DIObjectGraphBase<object>
        {
            public static Task<string> Field1([FromServices] string value) => Task.FromResult(value);
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
            [Concurrent(true)]
            public static Task<string> Field1(IServiceProvider services) => Task.FromResult("hello");
        }

        [Fact]
        public async Task InheritsConcurrent()
        {
            Configure<CInheritsConcurrent, object>();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(true);
        }

        [Concurrent(true)]
        public class CInheritsConcurrent : DIObjectGraphBase<object>
        {
            public static Task<string> Field1(IServiceProvider services) => Task.FromResult("hello");
        }

        [Fact]
        public async Task InheritedConcurrentOverridable()
        {
            Configure<CInheritedConcurrentOverridable, object>();
            await VerifyFieldAsync("Field1", true, false, "hello");
            Verify(false);
        }

        [Concurrent(true)]
        public class CInheritedConcurrentOverridable : DIObjectGraphBase<object>
        {
            [Concurrent(Concurrent = false)]
            public static Task<string> Field1(IServiceProvider services) => Task.FromResult("hello");
        }
    }
}
