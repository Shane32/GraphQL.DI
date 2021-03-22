using System;
using Xunit;
using Shouldly;
using GraphQL.DI;
using GraphQL.Types;
using System.ComponentModel;
using Moq;
using GraphQL;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Tests
{
    public class DIObjectGraphType
    {
        private object _source;
        private ObjectGraphType<object> _graphType;
        private readonly Mock<IServiceProvider> _scopedServiceProviderMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IResolveFieldContext> _contextMock;

        public DIObjectGraphType() : base()
        {
            _scopedServiceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            _scopeMock = new Mock<IServiceScope>(MockBehavior.Strict);
            _scopeMock.Setup(x => x.Dispose());
            _scopeMock.SetupGet(x => x.ServiceProvider).Returns(_scopedServiceProviderMock.Object);
            _scopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
            _serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_scopeFactoryMock.Object);
            _contextMock = new Mock<IResolveFieldContext>(MockBehavior.Strict);
            _contextMock.SetupGet(x => x.RequestServices).Returns(_serviceProviderMock.Object);
            _contextMock.SetupGet(x => x.Source).Returns(() => _source);
        }

        private ObjectGraphType<object> Configure<T>(bool instance = false, bool scoped = false) where T : DIObjectGraphBase, new()
        {
            if (instance) {
                if (scoped) {
                    _scopedServiceProviderMock.Setup(x => x.GetService(typeof(T))).Returns(() => new T()).Verifiable();
                } else {
                    _serviceProviderMock.Setup(x => x.GetService(typeof(T))).Returns(() => new T()).Verifiable();
                }
            }
            _graphType = new DIObjectGraphType<T>();
            return _graphType;
        }

        private FieldType VerifyField<T>(string fieldName, bool nullable, bool concurrent, T returnValue)
        {
            var context = _contextMock.Object;
            _graphType.ShouldNotBeNull();
            _graphType.Fields.ShouldNotBeNull();
            var field = _graphType.Fields.Find(fieldName);
            field.ShouldNotBeNull();
            field.Type.ShouldBe(nullable ? typeof(GraphQLClrOutputTypeReference<T>) : typeof(NonNullGraphType<GraphQLClrOutputTypeReference<T>>));
            field.ShouldBeOfType<DIFieldType>().Concurrent.ShouldBe(concurrent);
            field.Resolver.ShouldNotBeNull();
            field.Resolver.Resolve(context).ShouldBe(returnValue);
            return field;
        }

        private async Task<FieldType> VerifyFieldAsync<T>(string fieldName, bool nullable, bool concurrent, T returnValue)
        {
            var context = _contextMock.Object;
            _graphType.ShouldNotBeNull();
            _graphType.Fields.ShouldNotBeNull();
            var field = _graphType.Fields.Find(fieldName);
            field.ShouldNotBeNull();
            field.Type.ShouldBe(nullable ? typeof(GraphQLClrOutputTypeReference<T>) : typeof(NonNullGraphType<GraphQLClrOutputTypeReference<T>>));
            field.ShouldBeOfType<DIFieldType>().Concurrent.ShouldBe(concurrent);
            field.Resolver.ShouldNotBeNull();
            var ret = field.Resolver.Resolve(context);
            var taskRet = ret.ShouldBeOfType<Task<T>>();
            var final = await taskRet;
            final.ShouldBe(returnValue);
            return field;
        }

        private void Verify(bool scoped)
        {
            if (scoped) {
                _contextMock.Verify(x => x.RequestServices, Times.Once);
                _serviceProviderMock.Verify(x => x.GetService(typeof(IServiceScopeFactory)), Times.Once);
                _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
                _scopeMock.Verify(x => x.ServiceProvider, Times.Once);
            }
            else {
                _contextMock.Verify(x => x.RequestServices, Times.Between(0, 100, Moq.Range.Inclusive));
            }
            _contextMock.Verify();
            _serviceProviderMock.Verify();
            _scopeMock.Verify();
            _scopeFactoryMock.Verify();
            _scopedServiceProviderMock.Verify();
            _contextMock.VerifyNoOtherCalls();
            _serviceProviderMock.VerifyNoOtherCalls();
            _scopeMock.VerifyNoOtherCalls();
            _scopeFactoryMock.VerifyNoOtherCalls();
            _scopedServiceProviderMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GraphName()
        {
            Configure<CGraphName>().Name.ShouldBe("TestGraphName");
        }

        [Name("TestGraphName")]
        public class CGraphName : DIObjectGraphBase { }

        [Fact]
        public void GraphDescription()
        {
            Configure<CGraphDescription>().Description.ShouldBe("TestGraphDescription");
        }

        [Description("TestGraphDescription")]
        public class CGraphDescription : DIObjectGraphBase { }

        [Fact]
        public void GraphObsolete()
        {
            Configure<CGraphObsolete>().DeprecationReason.ShouldBe("TestDeprecationReason");
        }

        [Obsolete("TestDeprecationReason")]
        public class CGraphObsolete : DIObjectGraphBase { }

        [Fact]
        public void GraphMetadata()
        {
            Configure<CGraphMetadata>().GetMetadata<string>("test").ShouldBe("value");
        }

        [Metadata("test", "value")]
        public class CGraphMetadata : DIObjectGraphBase { }

        [Fact]
        public void FieldStaticMethod()
        {
            Configure<CFieldStaticMethod>();
            VerifyField("Field1", nullable: true, concurrent: false, returnValue: "hello");
            Verify(false);
        }

        public class CFieldStaticMethod : DIObjectGraphBase
        {
            public CFieldStaticMethod() => throw new Exception();

            public static string Field1() => "hello";
        }

        [Fact]
        public void FieldInstanceMethod()
        {
            Configure<CFieldInstanceMethod>(true);
            VerifyField("Field1", nullable: true, concurrent: false, returnValue: "hello");
            Verify(false);
        }

        public class CFieldInstanceMethod : DIObjectGraphBase
        {
            public string Field1() => "hello";
        }

        [Fact]
        public async Task FieldStaticAsyncMethod()
        {
            Configure<CFieldStaticAsyncMethod>();
            await VerifyFieldAsync("Field1", nullable: true, concurrent: true, returnValue: "hello");
            Verify(false);
        }

        public class CFieldStaticAsyncMethod : DIObjectGraphBase
        {
            public CFieldStaticAsyncMethod() => throw new Exception();

            public static Task<string> Field1() => Task.FromResult("hello");
        }

        [Fact]
        public async Task FieldInstanceAsyncMethod()
        {
            Configure<CFieldInstanceAsyncMethod>(true);
            await VerifyFieldAsync("Field1", nullable: true, concurrent: false, returnValue: "hello");
            Verify(false);
        }

        public class CFieldInstanceAsyncMethod : DIObjectGraphBase
        {
            public Task<string> Field1() => Task.FromResult("hello");
        }

        [Fact]
        public void FieldName()
        {
            Configure<CFieldName>();
            VerifyField("FieldTest", true, false, "hello");
            Verify(false);
        }

        public class CFieldName : DIObjectGraphBase
        {
            [Name("FieldTest")]
            public static string Field1() => "hello";
        }

        [Fact]
        public void FieldDescription()
        {
            Configure<CFieldDescription>();
            VerifyField("Field1", true, false, "hello").Description.ShouldBe("DescriptionTest");
            Verify(false);
        }

        public class CFieldDescription : DIObjectGraphBase
        {
            [Description("DescriptionTest")]
            public static string Field1() => "hello";
        }

        [Fact]
        public void FieldObsolete()
        {
            Configure<CFieldObsolete>();
            VerifyField("Field1", true, false, "hello").DeprecationReason.ShouldBe("ObsoleteTest");
            Verify(false);
        }

        public class CFieldObsolete : DIObjectGraphBase
        {
            [Obsolete("ObsoleteTest")]
            public static string Field1() => "hello";
        }

        [Fact]
        public void FieldContext()
        {
            Configure<CFieldContext>();
            _source = "testSource";
            VerifyField("Field1", true, false, "testSource");
            _contextMock.Verify(x => x.Source, Times.Once);
            Verify(false);
        }

        public class CFieldContext : DIObjectGraphBase
        {
            public static string Field1(IResolveFieldContext context) => (string)context.Source;
        }

        [Fact]
        public void FieldContext_Typed()
        {
            Configure<CFieldContext_Typed>();
            _source = "testSource";
            VerifyField("Field1", true, false, "testSource");
            _contextMock.Verify(x => x.Source, Times.AtLeastOnce);
            Verify(false);
        }

        public class CFieldContext_Typed : DIObjectGraphBase
        {
            public static string Field1(IResolveFieldContext<object> context) => (string)context.Source;
        }

        [Fact]
        public void FieldContext_WrongType()
        {
            Should.Throw<InvalidOperationException>(() => Configure<CFieldContext_WrongType>());
        }

        public class CFieldContext_WrongType : DIObjectGraphBase
        {
            public static string Field1(IResolveFieldContext<string> context) => context.Source;
        }

        [Fact]
        public void FieldSource()
        {
            Configure<CFieldSource>();
            _source = "testSource";
            VerifyField("Field1", true, false, "testSource");
            _contextMock.Verify(x => x.Source, Times.Once);
            Verify(false);
        }

        public class CFieldSource : DIObjectGraphBase
        {
            public static string Field1([FromSource] object source) => (string)source;
        }

        [Fact]
        public void FieldSource_WrongType()
        {
            Should.Throw<InvalidOperationException>(() => Configure<CFieldSource_WrongType>());
        }

        public class CFieldSource_WrongType : DIObjectGraphBase
        {
            public static string Field1([FromSource] string source) => source;
        }

    }
}
