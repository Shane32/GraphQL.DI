using System;
using GraphQL.Types;
using GraphQL.Execution;
using GraphQL.Language.AST;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.DI
{
    /// <summary>
    /// Processes an entire GraphQL request, given an input GraphQL request string. This
    /// is intended to be called by user code to process a query. Uses a <see cref="DIExecutionStrategy"/>
    /// implementation for <see cref="ISchema.Query"/> and <see cref="ISchema.Mutation"/>. Can be passed
    /// a <see cref="IExecutionStrategy"/> implementation for <see cref="ISchema.Subscription"/>.
    /// </summary>
    public class DIDocumentExecuter : DocumentExecuter
    {
        /// <summary>
        /// The instance of the execution strategy used for <see cref="ISchema.Query"/> and <see cref="ISchema.Mutation"/>.
        /// </summary>
        protected IExecutionStrategy DIExecutionStrategy = DI.DIExecutionStrategy.Instance;
        /// <summary>
        /// The instance of the execution strategy used for <see cref="ISchema.Subscription"/>.
        /// </summary>
        protected IExecutionStrategy? SubscriptionExecutionStrategy = null;

        /// <summary>
        /// Initializes a new instance without support for subscriptions.
        /// </summary>
        public DIDocumentExecuter() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified <see cref="IExecutionStrategy"/> for <see cref="ISchema.Subscription"/>.
        /// </summary>
        public DIDocumentExecuter(IExecutionStrategy? subscriptionExecutionStrategy) : base()
        {
            SubscriptionExecutionStrategy = subscriptionExecutionStrategy;
        }

        /// <summary>
        /// Initializes a new instance using the specified <see cref="IServiceProvider"/> to pull optional
        /// references to <see cref="IDocumentBuilder"/>, <see cref="IDocumentValidator"/> and <see cref="IComplexityAnalyzer"/>.
        /// Does not support subscriptions.
        /// </summary>
        public DIDocumentExecuter(
            IServiceProvider serviceProvider) : base(
                (serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))).GetService<IDocumentBuilder>() ?? new GraphQLDocumentBuilder(),
                serviceProvider.GetService<IDocumentValidator>() ?? new DocumentValidator(),
                serviceProvider.GetService<IComplexityAnalyzer>() ?? new ComplexityAnalyzer())
        {
        }

        /// <summary>
        /// Initializes a new instance using the specified <see cref="IServiceProvider"/> to pull optional
        /// references to <see cref="IDocumentBuilder"/>, <see cref="IDocumentValidator"/> and <see cref="IComplexityAnalyzer"/>.
        /// Uses the specified <see cref="IExecutionStrategy"/> for <see cref="ISchema.Subscription"/>.
        /// </summary>
        public DIDocumentExecuter(IServiceProvider serviceProvider, IExecutionStrategy? subscriptionExecutionStrategy) : this(serviceProvider)
        {
            SubscriptionExecutionStrategy = subscriptionExecutionStrategy;
        }

        /// <inheritdoc/>
        protected override IExecutionStrategy SelectExecutionStrategy(ExecutionContext context)
        {
            switch (context.Operation.OperationType)
            {
                case OperationType.Query:
                    return DIExecutionStrategy;

                case OperationType.Mutation:
                    return DIExecutionStrategy;

                case OperationType.Subscription:
                    return SubscriptionExecutionStrategy ?? base.SelectExecutionStrategy(context);

                default:
                    return base.SelectExecutionStrategy(context);
            }
        }

    }
}
