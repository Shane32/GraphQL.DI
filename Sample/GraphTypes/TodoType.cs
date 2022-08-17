using GraphQL.DataLoader;
using GraphQL.DI;
using Sample.DataLoaders;
using Sample.DbModels;

namespace Sample.GraphTypes;

public class TodoType : DIObjectGraphType<TodoObjType, Todo>
{
    // mix and match if you like
    public TodoType()
    {
        // some properties defined traditionally
        Field(x => x.Id);
        Field(x => x.Title);
        Field(x => x.Notes, true);
    }
}

public class TodoObjType : DIObjectGraphBase<Todo>
{
    private readonly PersonDataLoader _personDataLoader;
    public TodoObjType(PersonDataLoader personDataLoader) => _personDataLoader = personDataLoader;

    // some properties defined here
    // use 'static' and it won't create a dbcontext
    public static bool Completed(Todo source) => source.Completed;
    public static DateTime? CompletedOn(Todo source) => source.CompletionDate;

    // here it is using an instance field "Source"
    public IDataLoaderResult<Person?>? CompletedBy()
    {
        if (!Source.CompletedByPersonId.HasValue)
            return null;

        return _personDataLoader.LoadAsync(Source.CompletedByPersonId.Value);
    }
}
