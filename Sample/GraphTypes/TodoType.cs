using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using GraphQL.DI;
using Sample.DataLoaders;
using Sample.DbModels;

namespace Sample.GraphTypes
{
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

        public IDataLoaderResult<Person> CompletedBy(Todo source)
        {
            if (!source.CompletedByPersonId.HasValue)
                return null;

            return _personDataLoader.LoadAsync(source.CompletedByPersonId.Value);
        }
    }
}
