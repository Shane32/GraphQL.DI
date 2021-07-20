using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using GraphQL;
using GraphQL.DI;
using GraphQL.Execution;
using GraphQL.Instrumentation;
using GraphQL.Language.AST;
using GraphQL.Types;
using Moq;
using Shouldly;
using Xunit;

namespace Tests
{
    public class DIObjectGraphBaseTests
    {
        private readonly DIObjectGraphBase _graph = new CTest();
        private IResolveFieldContext _graphContext => (IResolveFieldContext)_graph;
        private readonly Mock<IResolveFieldContext> _mockContext = new Mock<IResolveFieldContext>(MockBehavior.Strict);
        private IResolveFieldContext _context => _mockContext.Object;

        public DIObjectGraphBaseTests()
        {
            ((IDIObjectGraphBase)_graph).Context = _context;
            ((IDIObjectGraphBase)_graph).Context.ShouldBe(_context);
        }

        [Fact]
        public void Context()
        {
            _graph.Context.ShouldBe(_context);
        }

        [Fact]
        public void RequestAborted()
        {
            var token = new CancellationTokenSource().Token;
            _mockContext.Setup(x => x.CancellationToken).Returns(token);
            _graph.RequestAborted.ShouldBe(token);
        }

        [Fact]
        public void Metrics()
        {
            var metrics = new Metrics();
            _mockContext.Setup(x => x.Metrics).Returns(metrics);
            _graph.Metrics.ShouldBe(metrics);
        }

        [Fact]
        public void UserContext()
        {
            var userContext = new Dictionary<string, object>();
            _mockContext.Setup(x => x.UserContext).Returns(userContext);
            _graph.UserContext.ShouldBe(userContext);
        }

        [Fact]
        public void Source()
        {
            var source = new object();
            _mockContext.Setup(x => x.Source).Returns(source);
            _graph.Source.ShouldBe(source);
        }

        [Fact]
        public void SourceTyped()
        {
            _mockContext.Setup(x => x.Source).Returns(3);
            var graph = new CTest<int>();
            ((IDIObjectGraphBase)graph).Context = _context;
            graph.Source.ShouldBe(3);
        }

        [Fact]
        public void RFC_FieldAst()
        {
            var fieldAst = new Field(default, default);
            _mockContext.Setup(x => x.FieldAst).Returns(fieldAst);
            _graphContext.FieldAst.ShouldBe(fieldAst);
        }

        [Fact]
        public void RFC_FieldDefinition()
        {
            var fieldDef = new FieldType();
            _mockContext.Setup(x => x.FieldDefinition).Returns(fieldDef);
            _graphContext.FieldDefinition.ShouldBe(fieldDef);
        }

        [Fact]
        public void RFC_ParentType()
        {
            var obj = Mock.Of<IObjectGraphType>();
            _mockContext.Setup(x => x.ParentType).Returns(obj);
            _graphContext.ParentType.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Parent()
        {
            var obj = Mock.Of<IResolveFieldContext>();
            _mockContext.Setup(x => x.Parent).Returns(obj);
            _graphContext.Parent.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Arguments()
        {
            var obj = Mock.Of<IDictionary<string, ArgumentValue>>();
            _mockContext.Setup(x => x.Arguments).Returns(obj);
            _graphContext.Arguments.ShouldBe(obj);
        }

        [Fact]
        public void RFC_RootValue()
        {
            var obj = new object();
            _mockContext.Setup(x => x.RootValue).Returns(obj);
            _graphContext.RootValue.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Source()
        {
            var obj = new object();
            _mockContext.Setup(x => x.Source).Returns(obj);
            _graphContext.Source.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Schema()
        {
            var obj = Mock.Of<ISchema>();
            _mockContext.Setup(x => x.Schema).Returns(obj);
            _graphContext.Schema.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Document()
        {
            var obj = new Document();
            _mockContext.Setup(x => x.Document).Returns(obj);
            _graphContext.Document.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Operation()
        {
            var obj = new Operation(default);
            _mockContext.Setup(x => x.Operation).Returns(obj);
            _graphContext.Operation.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Variables()
        {
            var obj = new Variables();
            _mockContext.Setup(x => x.Variables).Returns(obj);
            _graphContext.Variables.ShouldBe(obj);
        }

        [Fact]
        public void RFC_CancellationToken()
        {
            var obj = new CancellationTokenSource().Token;
            _mockContext.Setup(x => x.CancellationToken).Returns(obj);
            _graphContext.CancellationToken.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Errors()
        {
            var obj = new ExecutionErrors();
            _mockContext.Setup(x => x.Errors).Returns(obj);
            _graphContext.Errors.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Path()
        {
            var obj = Mock.Of<IEnumerable<object>>();
            _mockContext.Setup(x => x.Path).Returns(obj);
            Assert.StrictEqual(_graphContext.Path, obj);
        }

        [Fact]
        public void RFC_ResponsePath()
        {
            var obj = Mock.Of<IEnumerable<object>>();
            _mockContext.Setup(x => x.ResponsePath).Returns(obj);
            Assert.StrictEqual(_graphContext.ResponsePath, obj);
        }

        [Fact]
        public void RFC_SubFields()
        {
            var obj = new Dictionary<string, Field>();
            _mockContext.Setup(x => x.SubFields).Returns(obj);
            _graphContext.SubFields.ShouldBe(obj);
        }

        [Fact]
        public void RFC_Extensions()
        {
            var obj = Mock.Of<IDictionary<string, object>>();
            _mockContext.Setup(x => x.Extensions).Returns(obj);
            _graphContext.Extensions.ShouldBe(obj);
        }

        [Fact]
        public void RFC_RequestServices()
        {
            var obj = Mock.Of<IServiceProvider>();
            _mockContext.Setup(x => x.RequestServices).Returns(obj);
            _graphContext.RequestServices.ShouldBe(obj);
        }

        [Fact]
        public void RFC_ArrayPool()
        {
            var obj = Mock.Of<IExecutionArrayPool>();
            _mockContext.Setup(x => x.ArrayPool).Returns(obj);
            _graphContext.ArrayPool.ShouldBe(obj);
        }

        private class CTest : DIObjectGraphBase
        {
        }

        private class CTest<T> : DIObjectGraphBase<T>
        {
        }
    }
}
