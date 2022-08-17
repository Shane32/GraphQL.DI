using GraphQL.Types;
using Sample.GraphTypes;

namespace Sample;

public class TodoSchema : Schema
{
    public TodoSchema(IServiceProvider serviceProvider, QueryType query, MutationType mutation) : base(serviceProvider)
    {
        Query = query;
        Mutation = mutation;
    }
}
