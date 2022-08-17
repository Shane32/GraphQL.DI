using GraphQL.DataLoader;
using Microsoft.EntityFrameworkCore;
using Sample.DbModels;

namespace Sample.DataLoaders;

public class PersonDataLoader : DataLoaderBase<int, Person?>
{
    private readonly TodoDbContext _db;

    public PersonDataLoader(TodoDbContext db)
    {
        _db = db;
    }

    protected override async Task FetchAsync(IEnumerable<DataLoaderPair<int, Person?>> list, CancellationToken cancellationToken)
    {
        var ids = list.Select(x => x.Key);
        var people = await _db.Set<Person>().Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var value in list) {
            people.TryGetValue(value.Key, out var person);
            value.SetResult(person);
        }
    }
}
