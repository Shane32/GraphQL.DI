using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.DI;
using GraphQL.Execution;
using GraphQL.Language.AST;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using Moq;
using Shouldly;
using Xunit;

namespace Execution
{
    public class DIDocumentExecuterTests
    {
        [Fact]
        public void CreateWithDefaults()
        {
            var test = new MockDIDocumentExecuter();
            test.TestSelectExecutionStrategy(OperationType.Query).ShouldBeOfType<DIExecutionStrategy>();
            test.TestSelectExecutionStrategy(OperationType.Mutation).ShouldBeOfType<DIExecutionStrategy>();
            Should.Throw<Exception>(() => test.TestSelectExecutionStrategy(OperationType.Subscription));
        }

        [Fact]
        public void CreateWithSubscription()
        {
            var strategy = new ParallelExecutionStrategy();
            var test = new MockDIDocumentExecuter(strategy);
            test.TestSelectExecutionStrategy(OperationType.Query).ShouldBeOfType<DIExecutionStrategy>();
            test.TestSelectExecutionStrategy(OperationType.Mutation).ShouldBeOfType<DIExecutionStrategy>();
            test.TestSelectExecutionStrategy(OperationType.Subscription).ShouldBe(strategy);
        }

        [Fact]
        public void CreateWithServiceProviderDefaults()
        {
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDocumentBuilder))).Returns((IDocumentBuilder)null).Verifiable();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDocumentValidator))).Returns((IDocumentValidator)null).Verifiable();
            serviceProviderMock.Setup(x => x.GetService(typeof(IComplexityAnalyzer))).Returns((IComplexityAnalyzer)null).Verifiable();
            var test = new MockDIDocumentExecuter(serviceProviderMock.Object);
            test.TestSelectExecutionStrategy(OperationType.Query).ShouldBeOfType<DIExecutionStrategy>();
            test.TestSelectExecutionStrategy(OperationType.Mutation).ShouldBeOfType<DIExecutionStrategy>();
            Should.Throw<Exception>(() => test.TestSelectExecutionStrategy(OperationType.Subscription));
        }

        [Fact]
        public void CreateWithServiceProviderInstances()
        {
            var documentBuilder = new Mock<IDocumentBuilder>(MockBehavior.Strict).Object;
            var documentValidator = new Mock<IDocumentValidator>(MockBehavior.Strict).Object;
            var complexityAnalyzer = new Mock<IComplexityAnalyzer>(MockBehavior.Strict).Object;
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDocumentBuilder))).Returns(documentBuilder).Verifiable();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDocumentValidator))).Returns(documentValidator).Verifiable();
            serviceProviderMock.Setup(x => x.GetService(typeof(IComplexityAnalyzer))).Returns(complexityAnalyzer).Verifiable();
            var test = new MockDIDocumentExecuter(serviceProviderMock.Object);
            test.TestSelectExecutionStrategy(OperationType.Query).ShouldBeOfType<DIExecutionStrategy>();
            test.TestSelectExecutionStrategy(OperationType.Mutation).ShouldBeOfType<DIExecutionStrategy>();
            Should.Throw<Exception>(() => test.TestSelectExecutionStrategy(OperationType.Subscription));
        }

        [Fact]
        public void CreateWithServiceProviderDefaultsAndSubscription()
        {
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDocumentBuilder))).Returns((IDocumentBuilder)null).Verifiable();
            serviceProviderMock.Setup(x => x.GetService(typeof(IDocumentValidator))).Returns((IDocumentValidator)null).Verifiable();
            serviceProviderMock.Setup(x => x.GetService(typeof(IComplexityAnalyzer))).Returns((IComplexityAnalyzer)null).Verifiable();
            var strategy = new ParallelExecutionStrategy();
            var test = new MockDIDocumentExecuter(serviceProviderMock.Object, strategy);
            test.TestSelectExecutionStrategy(OperationType.Query).ShouldBeOfType<DIExecutionStrategy>();
            test.TestSelectExecutionStrategy(OperationType.Mutation).ShouldBeOfType<DIExecutionStrategy>();
            test.TestSelectExecutionStrategy(OperationType.Subscription).ShouldBe(strategy);
        }

        [Fact]
        public void UnknownOperationType()
        {
            var obj = new MockDIDocumentExecuter();
            Should.Throw<Exception>(() => obj.TestSelectExecutionStrategy((OperationType)255));
        }

        private class MockDIDocumentExecuter : DIDocumentExecuter
        {
            public MockDIDocumentExecuter() : base() { }
            public MockDIDocumentExecuter(IExecutionStrategy executionStrategy) : base(executionStrategy) { }
            public MockDIDocumentExecuter(IServiceProvider serviceProvider) : base(serviceProvider) { }
            public MockDIDocumentExecuter(IServiceProvider serviceProvider, IExecutionStrategy executionStrategy) : base(serviceProvider, executionStrategy) { }

            public IExecutionStrategy TestSelectExecutionStrategy(OperationType operationType)
            {
                var context = new ExecutionContext();
                context.Operation = new Operation(new NameNode("test"));
                context.Operation.OperationType = operationType;
                return base.SelectExecutionStrategy(context);
            }
        }
    }
}
