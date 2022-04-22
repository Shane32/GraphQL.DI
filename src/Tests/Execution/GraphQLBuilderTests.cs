using System;
using GraphQL.DI;
using GraphQL.Types;
using Moq;
using Shouldly;
using Xunit;

namespace Execution
{
    public class GraphQLBuilderTests
    {
        private readonly Mock<IServiceRegister> _mockServiceRegister = new Mock<IServiceRegister>(MockBehavior.Strict);
        private readonly Mock<IGraphQLBuilder> _mockGraphQLBuilder = new Mock<IGraphQLBuilder>(MockBehavior.Strict);
        private IGraphQLBuilder _graphQLBuilder => _mockGraphQLBuilder.Object;

        public GraphQLBuilderTests()
        {
            _mockGraphQLBuilder.Setup(x => x.Services).Returns(_mockServiceRegister.Object);
        }

        [Fact]
        public void AddDIGraphTypes()
        {
            _mockServiceRegister.Setup(x => x.TryRegister(typeof(DIObjectGraphType<>), typeof(DIObjectGraphType<>), ServiceLifetime.Transient, RegistrationCompareMode.ServiceType)).Returns(_mockServiceRegister.Object).Verifiable();
            _mockServiceRegister.Setup(x => x.TryRegister(typeof(DIObjectGraphType<,>), typeof(DIObjectGraphType<,>), ServiceLifetime.Transient, RegistrationCompareMode.ServiceType)).Returns(_mockServiceRegister.Object).Verifiable();
            _graphQLBuilder.AddDIGraphTypes().ShouldBe(_graphQLBuilder);
            _mockGraphQLBuilder.Verify();
        }

        [Fact]
        public void AddDIClrTypeMappings()
        {
            IGraphTypeMappingProvider mapper = null;
            _mockServiceRegister.Setup(x => x.Register(typeof(IGraphTypeMappingProvider), It.IsAny<IGraphTypeMappingProvider>(), false))
                .Returns<Type, IGraphTypeMappingProvider, bool>((_, m, _) => {
                    mapper = m;
                    return _mockServiceRegister.Object;
                });
            _graphQLBuilder.AddDIClrTypeMappings();
            mapper.ShouldNotBeNull();

            mapper.GetGraphTypeFromClrType(typeof(Class1), false, null).ShouldBe(typeof(DIObjectGraphType<Base1, Class1>));
            mapper.GetGraphTypeFromClrType(typeof(Class2), false, null).ShouldBe(typeof(DIObjectGraphType<Base2, Class2>));
            mapper.GetGraphTypeFromClrType(typeof(Class3), false, null).ShouldBeNull();

            mapper.GetGraphTypeFromClrType(typeof(Class1), false, typeof(Class4)).ShouldBe(typeof(DIObjectGraphType<Base1, Class1>));
            mapper.GetGraphTypeFromClrType(typeof(Class2), false, typeof(Class4)).ShouldBe(typeof(DIObjectGraphType<Base2, Class2>));
            mapper.GetGraphTypeFromClrType(typeof(Class3), false, typeof(Class4)).ShouldBe(typeof(Class4));
        }

        private class Class1 { }
        private class Class2 { }
        private class Class3 { }
        private class Class4 { }
        private class Base1 : DIObjectGraphBase<Class1> { } //don't register because graph1
        private class Base2 : DIObjectGraphBase<Class2> { } //register because graph2 is input
    }
}
