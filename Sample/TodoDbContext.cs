using Microsoft.EntityFrameworkCore;
using Sample.DbModels;

namespace Sample;

public class TodoDbContext : DbContext
{
    public DbSet<Todo> Todos { get; set; } = null!;
    public DbSet<Person> People { get; set; } = null!;

    private readonly Todo[] _initialTodoData;
    private readonly Person[] _initialPersonData;

    public TodoDbContext(DbContextOptions options) :
        base(options)
    {
        _initialTodoData = new[] {
            new Todo { Id = 1, Title = "Cut the grass" },
            new Todo { Id = 2, Title = "Eat some tacos", Notes = "With lettuce" },
            new Todo { Id = 3, Title = "Write a memo", Completed = true, CompletedByPersonId = 2, CompletionDate = new DateTime(2021, 7, 2, 12, 53, 0) },
        };
        _initialPersonData = new[] {
            new Person { Id = 1, Name = "John" },
            new Person { Id = 2, Name = "Anne" },
        };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todo>().HasData(_initialTodoData);

        modelBuilder.Entity<Person>().HasData(_initialPersonData);
    }
}
