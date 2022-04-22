using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DI;
using GraphQL.Execution;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace DIObjectGraphTypeTests
{
    public class DIObjectGraphTypeTestBase
    {
        protected object _source;
        protected IComplexGraphType _graphType;
        protected readonly Mock<IServiceProvider> _scopedServiceProviderMock;
        protected readonly Mock<IServiceScope> _scopeMock;
        protected readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        protected readonly Mock<IServiceProvider> _serviceProviderMock;
        protected readonly Mock<IResolveFieldContext> _contextMock;
        protected readonly Dictionary<string, ArgumentValue> _arguments = new();

        public DIObjectGraphTypeTestBase() : base()
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
            _contextMock.SetupGet(x => x.Source).Returns(() => _source!);
            _contextMock.SetupGet(x => x.ParentType).Returns(new ObjectGraphType());
            _contextMock.SetupGet(x => x.Arguments).Returns(() => _arguments);
            _contextMock.SetupGet(x => x.Schema).Returns((ISchema)null);
        }

        protected IComplexGraphType Configure<T, TSource>(bool instance = false, bool scoped = false) where T : DIObjectGraphBase<TSource>, new()
        {
            if (instance) {
                if (scoped) {
                    _scopedServiceProviderMock.Setup(x => x.GetService(typeof(T))).Returns(() => new T()).Verifiable();
                } else {
                    _serviceProviderMock.Setup(x => x.GetService(typeof(T))).Returns(() => new T()).Verifiable();
                }
            }
            _graphType = new DIObjectGraphType<T, TSource>();
            return _graphType;
        }

        protected FieldType VerifyField<T>(string fieldName, bool nullable, bool concurrent, T returnValue)
        {
            return VerifyField(fieldName, typeof(T).GetGraphTypeFromType(nullable, TypeMappingMode.OutputType), concurrent, returnValue);
        }

        protected FieldType VerifyField<T>(string fieldName, Type fieldGraphType, bool concurrent, T returnValue)
        {
            var context = _contextMock.Object;
            _graphType.ShouldNotBeNull();
            _graphType.Fields.ShouldNotBeNull();
            var field = _graphType.Fields.Find(fieldName);
            field.ShouldNotBeNull();
            field.Type.ShouldBe(fieldGraphType);
            field.Resolver.ShouldNotBeNull();
            _contextMock.Setup(x => x.FieldDefinition).Returns(field);
            field.Resolver.ResolveAsync(context).Result.ShouldBe(returnValue);
            _arguments.Clear();
            return field;
        }

        protected async Task<FieldType> VerifyFieldAsync<T>(string fieldName, bool nullable, bool concurrent, T returnValue)
        {
            var context = _contextMock.Object;
            _graphType.ShouldNotBeNull();
            _graphType.Fields.ShouldNotBeNull();
            var field = _graphType.Fields.Find(fieldName);
            field.ShouldNotBeNull();
            field.Type.ShouldBe(typeof(T).GetGraphTypeFromType(nullable, TypeMappingMode.OutputType));
            field.Resolver.ShouldNotBeNull();
            _contextMock.Setup(x => x.FieldDefinition).Returns(field);
            var final = await field.Resolver.ResolveAsync(context);
            final.ShouldBe(returnValue);
            _arguments.Clear();
            return field;
        }

        protected QueryArgument VerifyFieldArgument<T>(string fieldName, string argumentName, bool nullable, T returnValue)
        {
            return VerifyFieldArgument(fieldName, argumentName, typeof(T).GetGraphTypeFromType(nullable, TypeMappingMode.InputType), returnValue);
        }

        protected QueryArgument VerifyFieldArgument<T>(string fieldName, string argumentName, bool nullable)
        {
            return VerifyFieldArgument<T>(fieldName, argumentName, typeof(T).GetGraphTypeFromType(nullable, TypeMappingMode.InputType));
        }

        protected QueryArgument VerifyFieldArgument<T>(string fieldName, string argumentName, Type graphType)
        {
            _graphType.ShouldNotBeNull();
            _graphType.Fields.ShouldNotBeNull();
            var field = _graphType.Fields.Find(fieldName);
            field.ShouldNotBeNull();
            field.Arguments.ShouldNotBeNull();
            var argument = field.Arguments.Find(argumentName);
            argument.ShouldNotBeNull();
            argument.Type.ShouldBe(graphType);
            return argument;
        }

        protected QueryArgument VerifyFieldArgument<T>(string fieldName, string argumentName, Type graphType, T returnValue)
        {
            var argument = VerifyFieldArgument<T>(fieldName, argumentName, graphType);
            _arguments.Add(argumentName, new ArgumentValue(returnValue, ArgumentSource.FieldDefault));
            return argument;
        }

        protected void Verify(bool scoped)
        {
            if (scoped) {
                _contextMock.Verify(x => x.RequestServices, Times.Once);
                _serviceProviderMock.Verify(x => x.GetService(typeof(IServiceScopeFactory)), Times.Once);
                _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
                _scopeMock.Verify(x => x.ServiceProvider, Times.Once);
            } else {
                _serviceProviderMock.Verify(x => x.GetService(typeof(IServiceScopeFactory)), Times.Never);
            }
            _contextMock.Verify();
            _serviceProviderMock.Verify();
            _scopeMock.Verify();
            _scopeFactoryMock.Verify();
            _scopedServiceProviderMock.Verify();
        }
    }
}
