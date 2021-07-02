using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using GraphQL.DI;
using Sample.DbModels;
using Microsoft.EntityFrameworkCore;
using GraphQL;
using System.Threading;

namespace Sample.GraphTypes
{
    public class MutationType : DIObjectGraphType<Mutation> { }
    public class Mutation : DIObjectGraphBase<object>
    {
        public readonly TodoDbContext _db;
        public Mutation(TodoDbContext db) => _db = db;

        public async Task<Todo> AddTodoAsync([Required] string title, string notes, CancellationToken cancellationToken)
        {
            var todo = new Todo {
                Title = title,
                Notes = notes,
            };
            _db.Add(todo);
            await _db.SaveChangesAsync(cancellationToken);
            return todo;
        }

        public async Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken)
        {
            var todo = await _db.Set<Todo>().Where(x => x.Id == id).SingleOrDefaultAsync();
            if (todo == null)
                return false;
            _db.Set<Todo>().Remove(todo);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Todo> SetCompleteAsync(int id, int completedByPersonId, CancellationToken cancellationToken)
        {
            var todo = await _db.Set<Todo>().Where(x => x.Id == id).SingleOrDefaultAsync(cancellationToken);
            if (todo == null)
                return null;
            if (todo.Completed)
                throw new ExecutionError($"Task id {id} has already been completed");
            todo.Completed = true;
            todo.CompletedByPersonId = completedByPersonId;
            todo.CompletionDate = DateTime.Now;
            await _db.SaveChangesAsync(cancellationToken);
            return todo;
        }
    }
}
