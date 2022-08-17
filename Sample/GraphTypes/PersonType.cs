using GraphQL.Types;
using Sample.DbModels;

namespace Sample.GraphTypes;

public class PersonType : ObjectGraphType<Person>
{
    public PersonType()
    {
        Field(x => x.Id);
        Field(x => x.Name);
    }
}
