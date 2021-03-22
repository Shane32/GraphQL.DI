using System;
using GraphQL.Execution;
using GraphQL.Language.AST;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.DI
{
    //DIDocumentExecuter is designed to be registered as a singleton
    public class DIDocumentExecuter : DocumentExecuter
    {
        protected IExecutionStrategy DIExecutionStrategy = DI.DIExecutionStrategy.Instance;
        protected IExecutionStrategy SubscriptionExecutionStrategy = null;

        public DIDocumentExecuter() : base()
        {
        }

        public DIDocumentExecuter(IExecutionStrategy subscriptionExecutionStrategy) : base()
        {
            SubscriptionExecutionStrategy = subscriptionExecutionStrategy;
        }

        //pull IDocumentBuilder, IDocumentValidator, and IComplexityAnalyzer from DI if they have been registered
        //if any of them have not been registered, use default implementations
        public DIDocumentExecuter(
            IServiceProvider serviceProvider) : base(
                (serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))).GetService<IDocumentBuilder>() ?? new GraphQLDocumentBuilder(),
                serviceProvider.GetService<IDocumentValidator>() ?? new DocumentValidator(),
                serviceProvider.GetService<IComplexityAnalyzer>() ?? new ComplexityAnalyzer())
        {
        }

        //pull IDocumentBuilder, IDocumentValidator, and IComplexityAnalyzer from DI if they have been registered
        //if any of them have not been registered, use default implementations
        public DIDocumentExecuter(IServiceProvider serviceProvider, IExecutionStrategy subscriptionExecutionStrategy) : this(serviceProvider)
        {
            SubscriptionExecutionStrategy = subscriptionExecutionStrategy;
        }

        protected override IExecutionStrategy SelectExecutionStrategy(ExecutionContext context)
        {
            switch (context.Operation.OperationType)
            {
                case OperationType.Query:
                    return DIExecutionStrategy;

                case OperationType.Mutation:
                    return DIExecutionStrategy;

                case OperationType.Subscription:
                    return SubscriptionExecutionStrategy;

                default:
                    throw new InvalidOperationException($"Unexpected OperationType {context.Operation.OperationType}");
            }
        }

    }
}
