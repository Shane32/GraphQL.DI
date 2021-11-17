using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GraphQL.DI;
using GraphQL.Types;
using Shouldly;
using Xunit;

namespace AutoObjectGraphTypeTests
{
    public class Services : AutoObjectGraphTypeTestBase
    {
        [Fact]
        public void Service()
        {
            Configure<CService>();
            _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CService
        {
            public static string Field1([FromServices] string arg) => arg;
        }

        [Fact]
        public void ServiceMissingThrows()
        {
            Configure<CService>();
            _serviceProviderMock.Setup(x => x.GetService(typeof(string))).Returns((string)null!).Verifiable();
            Should.Throw<InvalidOperationException>(() => VerifyField("Field1", true, false, "hello"));
            Verify(false);
        }

        [Fact]
        public async Task ScopedService()
        {
            Configure<CScopedService>();
            _scopedServiceProviderMock.Setup(x => x.GetService(typeof(string))).Returns("hello").Verifiable();
            await VerifyFieldAsync("Field1", true, true, "hello");
            Verify(true);
        }

        public class CScopedService
        {
            [Concurrent(true)]
            public static Task<string> Field1([FromServices] string arg) => Task.FromResult(arg);
        }
    }
}
