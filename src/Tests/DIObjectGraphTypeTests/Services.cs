using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DI;
using Shouldly;
using Xunit;

namespace DIObjectGraphTypeTests
{
    public class Services : DIObjectGraphTypeTestBase
    {
        [Fact]
        public void Service()
        {
            Configure<CService, object>();
            _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CService : DIObjectGraphBase
        {
            public static string Field1([FromServices] string arg) => arg;
        }

        [Fact]
        public void ServiceMissingThrows()
        {
            Configure<CService, object>();
            _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns((string)null!).Verifiable();
            Should.Throw<InvalidOperationException>(() => VerifyField("Field1", true, false, "hello"));
            Verify(false);
        }

        [Fact]
        public async Task ScopedService()
        {
            Configure<CScopedService, object>();
            _scopedServiceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(true);
        }

        public class CScopedService : DIObjectGraphBase
        {
            [Scoped]
            public static Task<string> Field1([FromServices] string arg) => Task.FromResult(arg);
        }
    }
}
