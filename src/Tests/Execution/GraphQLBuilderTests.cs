using System;
using System.Collections.Generic;
using GraphQL.DI;
using GraphQL.Types;
using Moq;
using Shouldly;
using Xunit;

namespace Execution
{
    /*
    public class GraphQLBuilderTests
    {
        private readonly Mock<IGraphQLBuilder> _mockGraphQLBuilder = new Mock<IGraphQLBuilder>(MockBehavior.Strict);
        private IGraphQLBuilder _graphQLBuilder => _mockGraphQLBuilder.Object;

        [Fact]
        public void AddDIGraphTypes()
        {
            _mockGraphQLBuilder.Setup(x => x.TryRegister(typeof(DIObjectGraphType<>), typeof(DIObjectGraphType<>), ServiceLifetime.Transient)).Returns(_graphQLBuilder).Verifiable();
            _mockGraphQLBuilder.Setup(x => x.TryRegister(typeof(DIObjectGraphType<,>), typeof(DIObjectGraphType<,>), ServiceLifetime.Transient)).Returns(_graphQLBuilder).Verifiable();
            _graphQLBuilder.AddDIGraphTypes().ShouldBe(_graphQLBuilder);
            _mockGraphQLBuilder.Verify();
        }

        [Fact]
        public void AddDIClrTypeMappings()
        {
            var actual = new List<(Type clrType, Type graphType)>();
            var existingMappings = new (Type clrType, Type graphType)[] {
                (typeof(Class7), typeof(Graph7)),
                (typeof(Class8), typeof(Graph8))
            };
            var mockSchema = new Mock<ISchema>(MockBehavior.Strict);
            mockSchema.Setup(x => x.TypeMappings).Returns(existingMappings).Verifiable();
            mockSchema.Setup(x => x.RegisterTypeMapping(It.IsAny<Type>(), It.IsAny<Type>()))
                .Callback<Type, Type>((clrType, graphType) => actual.Add((clrType, graphType)));
            _mockGraphQLBuilder.Setup(x => x.Register(typeof(IConfigureSchema), It.IsAny<IConfigureSchema>(), false))
                .Returns<Type, IConfigureSchema, bool>((_, configure, _) => {
                    configure.Configure(mockSchema.Object, null);
                    return _mockGraphQLBuilder.Object;
                })
                .Verifiable();
            _graphQLBuilder.AddDIClrTypeMappings();
            mockSchema.Verify();
            _mockGraphQLBuilder.Verify();
            var expected = new List<(Type clrType, Type graphType)> {
                (typeof(Class2), typeof(DIObjectGraphType<Base2, Class2>)),
                (typeof(Class4), typeof(DIObjectGraphType<Base4, Class4>)),
                (typeof(Class8), typeof(DIObjectGraphType<Base8, Class8>)),
            };
            actual.ShouldBe(actual);
        }


        private class Class1 { }
        private class Class2 { }
        private class Class3 { }
        private class Class4 { }
        private class Class7 { }
        private class Class8 { }
        private class Base1 : DIObjectGraphBase<Class1> { } //don't register because graph1
        private class Base2 : DIObjectGraphBase<Class2> { } //register because graph2 is input
        private class Base3 : DIObjectGraphBase<Class3> { } //don't register because graph3
        private class Base4 : DIObjectGraphBase<Class4> { } //register because no conflict
        private class Base5 : DIObjectGraphBase { } //don't register because object type
        private class Base6 : DIObjectGraphBase { } //don't register because object type
        private class Base7 : DIObjectGraphBase<Class7> { } //don't register because graph7 was manually registered
        private class Base8 : DIObjectGraphBase<Class8> { } //register because graph8 is input
        private class Graph1 : ObjectGraphType<Class1> { }
        private class Graph2 : InputObjectGraphType<Class2> { }
        private class Graph3 : DIObjectGraphType<Base3, Class3> { }
        private class Graph5 : DIObjectGraphType<Base5> { }
        private class Graph7 : ObjectGraphType { }
        private class Graph8 : InputObjectGraphType { }
    }
    */
}
