using ServiceLifetime = GraphQL.DI.ServiceLifetime;

namespace Execution;

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
        _mockServiceRegister.Verify();
    }

    [Fact]
    public void AddDIGraphBases()
    {
        var registeredTypes = new List<Type>();
        _mockServiceRegister.Setup(x => x.TryRegister(It.IsAny<Type>(), It.IsAny<Type>(), ServiceLifetime.Transient, RegistrationCompareMode.ServiceType))
            .Returns<Type, Type, ServiceLifetime, RegistrationCompareMode>((serviceType, implementationType, _, _) => {
                // Verify the type meets the criteria (class, not abstract, implements IDIObjectGraphBase)
                if (!implementationType.IsClass || implementationType.IsAbstract || !typeof(IDIObjectGraphBase).IsAssignableFrom(implementationType)) {
                    throw new InvalidOperationException($"Invalid type registration: {implementationType}");
                }
                // Service type should match implementation type
                if (serviceType != implementationType) {
                    throw new InvalidOperationException($"Service type {serviceType} does not match implementation type {implementationType}");
                }
                registeredTypes.Add(implementationType);
                return _mockServiceRegister.Object;
            })
            .Verifiable();

        // Call with specific assembly to test the overload and avoid assembly scanning issues
        _graphQLBuilder.AddDIGraphBases().ShouldBe(_graphQLBuilder);
        _mockGraphQLBuilder.Verify();
        _mockServiceRegister.Verify();

        // Verify Base1 and Base2 were registered
        registeredTypes.ShouldContain(typeof(Base1));
        registeredTypes.ShouldContain(typeof(Base2));
    }

    [Fact]
    public void AddDIClrTypeMappings()
    {
        IGraphTypeMappingProvider? mapper = null;
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

    [Fact]
    public void AddDI()
    {
        // Setup expectations for AddDIGraphBases
        var requiredTypes = new List<Type> {
            typeof(DIObjectGraphType<>),
            typeof(DIObjectGraphType<,>),
            typeof(Base1),
            typeof(Base2)
        };
        var registeredTypes = new List<Type>();
        _mockServiceRegister.Setup(x => x.TryRegister(It.IsAny<Type>(), It.IsAny<Type>(), ServiceLifetime.Transient, RegistrationCompareMode.ServiceType))
            .Returns<Type, Type, ServiceLifetime, RegistrationCompareMode>((serviceType, implementationType, _, _) => {
                if (serviceType == implementationType && requiredTypes.Contains(serviceType)) {
                    registeredTypes.Add(serviceType);
                    return _mockServiceRegister.Object;
                }
                if (!implementationType.IsClass || implementationType.IsAbstract || !typeof(IDIObjectGraphBase).IsAssignableFrom(implementationType)) {
                    throw new InvalidOperationException($"Invalid type registration: {implementationType}");
                }
                if (serviceType != implementationType) {
                    throw new InvalidOperationException($"Service type {serviceType} does not match implementation type {implementationType}");
                }
                registeredTypes.Add(implementationType);
                return _mockServiceRegister.Object;
            })
            .Verifiable();

        // Setup expectations for AddDIClrTypeMappings
        IGraphTypeMappingProvider? mapper = null;
        _mockServiceRegister.Setup(x => x.Register(typeof(IGraphTypeMappingProvider), It.IsAny<IGraphTypeMappingProvider>(), false))
            .Returns<Type, IGraphTypeMappingProvider, bool>((_, m, _) => {
                mapper = m;
                return _mockServiceRegister.Object;
            });

        // Call AddDI with specific assembly
        _graphQLBuilder.AddDI().ShouldBe(_graphQLBuilder);

        // Verify all expectations were met
        _mockGraphQLBuilder.Verify();
        _mockServiceRegister.Verify();

        // Verify required types were registered by AddDI
        foreach (var type in requiredTypes) {
            registeredTypes.ShouldContain(type);
        }

        // Verify mapper was created and works correctly
        mapper.ShouldNotBeNull();
        mapper.GetGraphTypeFromClrType(typeof(Class1), false, null).ShouldBe(typeof(DIObjectGraphType<Base1, Class1>));
        mapper.GetGraphTypeFromClrType(typeof(Class2), false, null).ShouldBe(typeof(DIObjectGraphType<Base2, Class2>));
        mapper.GetGraphTypeFromClrType(typeof(Class3), false, null).ShouldBeNull();
    }

    private class Class1 { }
    private class Class2 { }
    private class Class3 { }
    private class Class4 { }
    private class Base1 : DIObjectGraphBase<Class1> { } //don't register because graph1
    private class Base2 : DIObjectGraphBase<Class2> { } //register because graph2 is input
}
