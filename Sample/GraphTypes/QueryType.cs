using GraphQL.DI;
using Microsoft.EntityFrameworkCore;
using Sample.DbModels;

namespace Sample.GraphTypes;

public class QueryType : DIObjectGraphType<Query> { }
public class Query : DIObjectGraphBase<object>
{
    private readonly TodoDbContext _db;
    public Query(TodoDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Todo>> TodosAsync(int? id, IEnumerable<int> ids, int? completedByPersonId, CancellationToken cancellationToken)
    {
        IQueryable<Todo> query = _db.Set<Todo>();
        if (id.HasValue)
            query = query.Where(x => x.Id == id);
        if (ids != null)
            query = query.Where(x => ids.Contains(x.Id));
        if (completedByPersonId != null)
            query = query.Where(x => x.CompletedByPersonId == completedByPersonId);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Todo> TodoAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Set<Todo>().Where(x => x.Id == id).SingleOrDefaultAsync(cancellationToken);
    }
}
