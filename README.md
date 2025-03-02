# GraphQL.DI

[![NuGet](https://img.shields.io/nuget/v/Shane32.GraphQL.DI.svg)](https://www.nuget.org/packages/Shane32.GraphQL.DI) [![Coverage Status](https://coveralls.io/repos/github/Shane32/GraphQL.DI/badge.svg?branch=master)](https://coveralls.io/github/Shane32/GraphQL.DI?branch=master)

## Overview

GraphQL.DI enhances GraphQL.NET's code-first approach by providing dependency injection support for
field resolvers through the `DIObjectGraphBase` class. This enables a more maintainable and testable
approach to building GraphQL APIs by allowing services to be injected directly into your field resolver classes.

## Getting Started

1. **Install the NuGet package**  
   ```bash
   dotnet add package Shane32.GraphQL.DI
   ```
2. **Add `.AddDI()` to your `AddGraphQL(...)` registration**  
   Register your schema and resolvers, for example:
   ```csharp
   services.AddGraphQL(b => b
       .AddSystemTextJson()
       .AddSchema<TodoSchema>()
       .AddDI()
       .AddGraphTypes()
       .AddClrTypeMappings()
       .AddExecutionStrategy<SerialExecutionStrategy>(OperationType.Query)
   );
   ```
4. **Create your DI-based graph types**  
   ```csharp
   public class TodoMutation : DIObjectGraphBase
   {
       private readonly IRepository<Todo> _repository;
   
       public TodoMutation(IRepository<Todo> repository)
       {
           _repository = repository;
       }
   
       public async Task<Todo> AddAsync(string title, string? notes)
       {
           var todo = new Todo {
               Title = title,
               Notes = notes,
           };
           return await _repository.InsertAsync(todo, RequestAborted);
       }
   }
   ```
5. **Add the new graph types to your schema**
   ```csharp
   public class MutationType : ObjectGraphType
   {
       public MutationType()
       {
           Field<NonNullGraphType<TodoMutation>>("todo")
               .Resolve(_ => "");
       }
   }
   ```

See the [Setup](#setup) section below for more detailed instructions on configuring GraphQL.DI in
your project, including how to configure root types for use with GraphQL.DI.

## Background: Type-First vs Code-First in GraphQL.NET

### Type-First Approach

The type-first approach in GraphQL.NET automatically infers the GraphQL schema from your C# models:

```csharp
public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public Person CompletedBy { get; set; }
}
```

It also allows you to write methods in your models if need be for extra capability:

```csharp
public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; }
    [Ignore]
    public int CompletedByPersonId { get; set; }
    public Person CompletedBy([FromServices] IRepository repository)
        => repository.GetPersonById(CompletedByPersonId);
}
```

While services can be injected in the pattern shown above, there are two issues with this approach:

1. Injection of services is not within the constructor and can be considered an antipattern
2. Resolver code is mixed together with your data model

### Code-First Approach

The traditional code-first approach in GraphQL.NET uses `ObjectGraphType<T>`:

```csharp
public class TodoType : ObjectGraphType<Todo>
{
    public TodoType(IRepository repository)
    {
        Field(x => x.Id);
        Field(x => x.Title);
        Field<PersonType>("completedBy")
            .Resolve(context => repository.GetPersonById(context.Source.CompletedByPersonId));
    }
}
```

This can solve both issues noted above -- the data model is separate from the GraphQL type definition,
and services can be resolved via dependency injection in the constructor. However, graph types are
effectively singletons (typically) within the dependency injection container, so if your services
(such as `IRepository` above) is a scoped service, then your code will not run properly.

The most common solution is to resolve those scoped services from within the field resolver, which
again is an antipattern.

```csharp
public class TodoType : ObjectGraphType<Todo>
{
    public TodoType()
    {
        Field(x => x.Id);
        Field(x => x.Title);
        Field<PersonType>("completedBy")
            .Resolve(context =>
            {
                var repository = context.RequestServices!.GetRequiredService<IRepository>();
                return repository.GetPersonById(context.Source.CompletedByPersonId);
            });
    }
}
```

Using the code-first pattern is also less intuitive, but does provide the greatest degree of control
over the graph type.

### Comparison of Approaches

| Aspect             | Type-First   | Code-First      | GraphQL.DI |
|--------------------|--------------|-----------------|------------|
| Setup Complexity   | Low          | Medium          | Low        |
| Type Safety        | High         | High            | High       |
| DI Support         | Limited      | Singletons only | Full       |
| Code Organization  | Models contain resolvers | Separate type definitions | Separate type definitions |
| Performance        | Excellent    | Excellent       | Good       |
| Learning Curve     | Shallow      | Steeper         | Moderate   |
| Best For           | GraphQL-specific models | Database models | Mutations, root types |

## GraphQL.DI Features

GraphQL.DI solves all of the problems noted above by allowing scoped services to be injected directly into the
field resolver classes, while also using a 'type-first' coding pattern for better readability. In addition,
it can extend the traditional GraphQL.NET code-first approach, allowing you to gradually adopt the library
where it makes sense.

### Base Classes

The library provides two base classes:
- `DIObjectGraphBase<TSource>`: Use when you need type-safe access to the source object (e.g., `DIObjectGraphBase<Todo>`)
- `DIObjectGraphBase`: Use when the source object type is unimportant (same as `DIObjectGraphBase<object>`)

Both classes implement `IResolveFieldContext<TSource>`, providing access to all standard GraphQL.NET context properties.

### Pattern 1: Separate Type and Resolver Classes

Split the type definition and field resolution logic into separate classes:

```csharp
// Type definition
public class TodoType : DIObjectGraphType<TodoResolver, Todo>
{
    public TodoType()
    {
        // Classic code-first resolvers are defined here
        Field(x => x.Id);
        Field(x => x.Title);
        // Additional field resolvers are automatically mapped from TodoResolver
    }
}

// Field resolver implementation
public class TodoResolver : DIObjectGraphBase<Todo>
{
    private readonly IRepository _repository;
    
    public TodoResolver(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Person?> CompletedBy()
    {
        return await _repository.GetPersonById(Source.CompletedByPersonId);
    }
}

// Can be used alongside traditional GraphQL.NET types
public class PersonType : ObjectGraphType<Person>
{
    public PersonType()
    {
        Field(x => x.Id);
        Field(x => x.Name);
    }
}
```

### Pattern 2: Direct DIObjectGraphBase Usage

Use `DIObjectGraphBase` directly without defining a separate graph type:

```csharp
public class TodoResolver : DIObjectGraphBase<Todo>
{
    private readonly IRepository _repository;
    
    public TodoResolver(IRepository repository)
    {
        _repository = repository;
    }

    // Properties can be resolved using static methods
    public static int Id(Todo source) => source.Id;
    public static string Title(Todo source) => source.Title;

    // Complex resolvers can use injected services
    public async Task<Person?> CompletedBy()
    {
        return await _repository.GetPersonById(Source.CompletedByPersonId);
    }
}
```

Note that you can access the source in a variety of methods:

```csharp
// Access source by an argument named "source" with correct type
public static int Id(Todo source) => source.Id;
// Access source via IResolveFieldContext
public static int Id(IResolveFieldContext context) => ((Todo)context.Source).Id;
// Access source using [FromSource]
public static int Id([FromSource] Todo obj) => obj.Id;
// Access source using Source property
public int Id() => Source.Id;
// Access source using Context property
public int Id() => ((Todo)Context.Source).Id;
```

In either pattern, the `IRepository` service can be registered as a scoped service,
allowing for proper dependency injection and scoped service usage.

For fields where DI is not necessary, using static methods (such as is shown above) will
perform faster, as the class does not need to be initialized before the field is resolved.
The developer can then choose their own desired balance of code readability versus performance.

## Comparison with ASP.NET Core Controllers

`DIObjectGraphBase` serves a similar purpose to controllers in ASP.NET Core:

| ASP.NET Core Controller                | `DIObjectGraphBase`                      |
|----------------------------------------|------------------------------------------|
| Handles HTTP requests                  | Handles GraphQL field resolution         |
| Injected with services via constructor | Injected with services via constructor   |
| Access to HttpContext                  | Access to ResolveFieldContext            |
| Route parameters via method attributes | GraphQL arguments via method parameters  |
| Returns action results                 | Returns field values                     |

## Available Properties

The `DIObjectGraphBase` class provides easy access to the field context and some commonly-used properties from it:

| Property                | Type                           | Description                        |
|-------------------------|--------------------------------|------------------------------------|
| `Context`               | `IResolveFieldContext`         | The raw field resolution context   |
| `Source`                | `TSource`                      | The parent object being resolved   |
| `RequestAborted`        | `CancellationToken`            | Cancellation token for the request |
| `UserContext`           | `IDictionary<string, object?>` | Custom user context data           |
| `User`                  | `ClaimsPrincipal?`             | The authenticated user             |
| `Metrics`               | `Metrics`                      | Performance metrics data           |

It also directly implements `IResolveFieldContext<TSource>`, so extension methods for `IResolveFieldContext`
can be used with the `this` keyword, such as in this example:

```csharp
// resolver method
public static User GetUser(int id) => this.GetById<User>(id);

// extension method, usable both within code-first resolvers or DI resolvers
public static User GetById<T>(this IResolveFieldContext context, int id)
{
    var repository = context.RequestServices!.GetRequiredService<IRepository<T>>();
    return repository.GetById(id);
}
```

## Advanced Usage

Please note that unlike GraphQL.NET type-first resolvers, only public methods are resolved by default.
Properties and fields are ignored, as well as private or protected members.
This more closely mimics the design of controllers within ASP.NET.

### Service Lifetime

While resolving each or any non-static field defined in a DI graph type,
`ActivatorUtilities.GetServiceOrCreateInstance` is used to create the instance, followed by
initialization of the available properties. The `AddDI` method will automatically register
all these types within the dependency injection framework as transients to expedite the
initialization of the class, but regardless, if multiple fields are requested, each one
will create a new instance before executing the resolver. As such, for best speed/memory use,
define the resolvers as static if they are simple property accesses.

Keep in mind that for any DI graph type classes that implement `IDisposable`, the class
must be registered within the DI framework for proper disposal.

You can also choose to register DI graph type classes as scoped services, so they are re-used
during document execution. It is of course important to use a serial execution strategy in
such cases. Singleton lifetimes are not supported.

### Execution Strategy

For any GraphQL library defined in GraphQL.NET, it is important to use a serial execution
strategy if any scoped services are in use. This prevents two different resolvers from accessing
the same scoped service simultaneously, which is not supported by most libraries (hence why
they are scoped and not a singleton).

This does not change with GraphQL.DI; if any scoped services are in use, configure your
execution strategy to use a serial execution strategy.

Alternatively, you can create a service scope within the field resolver whenever scoped services
are needed. GraphQL.NET provides some extension methods for code-first resolvers, and provides
the `[Scoped]` attribute for type-first resolvers. GraphQL.DI requires the use of the same
`[Scoped]` attribute on each resolver that must create a dedicated service scope prior to execution.

For instance:

```csharp
public class TodoMutation : DIObjectGraphBase
{
    // note: within this class, the Source property would be typed as an object and would return ""
    private readonly IRepository _repository;

    public TodoMutation(IRepository repository)
    {
        _repository = repository;
    }

    [Scoped]
    public async Task<Todo> AddAsync(string title, string notes)
    {
        var todo = new Todo {
            Title = title,
            Notes = notes,
        };
        return await _repository.AddTodoAsync(todo, RequestAborted);
    }

    [Scoped]
    public async Task<Todo> DeleteAsync(int id)
    {
        // etc
    }
}
```

When resolving the `add` field in the above example, GraphQL.NET will create a service scope
before creating a `TodoMutation` instance within which to execute the `AddAsync` method.
This service scope is disposed when execution of the method completes. Another service scope
and `TodoMutation` instance would be created if the `delete` field were also executed within
the same request, so both `add` and `delete` could execute simultaneously without interference.

### Using the `[DIGraph]` Attribute

The `[DIGraph]` attribute is useful to create subgraphs easily, often used within mutations:

```csharp
public class Mutation : DIObjectGraphBase
{
    // The DIGraph attribute below sets DIObjectGraphType<TodoMutation> as the graph type
    // and is equivalent to [OutputType(typeof(DIObjectGraphType<TodoMutation>))]
    [DIGraph(typeof(TodoMutation))]
    public static string Todo() => "";  // a non-null object must be returned
}

public class TodoMutation : DIObjectGraphBase
{
    // note: within this class, the Source property would be typed as an object and would return ""
    private readonly IRepository _repository;

    public TodoMutation(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Todo> AddAsync(string title, string notes)
    {
        var todo = new Todo {
            Title = title,
            Notes = notes,
        };
        return await _repository.AddTodoAsync(todo, RequestAborted);
    }
}
```

### Using Other Attributes

All other type-first attributes from GraphQL.NET are supported, such as `[Id]`, `[Name]`, `[Ignore]`,
`[Scoped]` and `[FromServices]`. Please see GraphQL.NET documentation for further information.

## Setup

1. **Install the NuGet package:**

   ```bash
   dotnet add package Shane32.GraphQL.DI
   ```

2. **Register your types with the DI container:**

   ```csharp
   services.AddGraphQL(b => b
       .AddSystemTextJson()
       .AddSchema<TodoSchema>()
       .AddDI()                    // Register and configure GraphQL.DI types defined within the assembly
       .AddGraphTypes()            // Register GraphQL.NET types defined within the assembly
       .AddClrTypeMappings()       // Enable automatic CLR type mappings
       .AddExecutionStrategy<SerialExecutionStrategy>(OperationType.Query)  // Specify serial execution strategy
   );
   ```

3. **Define your schema with root DI graph types (if/as needed):**

   ```csharp
   public class TodoSchema : Schema
   {
       public TodoSchema(
           IServiceProvider serviceProvider,
           QueryType queryType,                         // sample where QueryType inherits DIObjectGraphType<Query>
           DIObjectGraphType<Mutation> mutationType)    // sample where Mutation inherits DIObjectGraphBase
           : base(serviceProvider)
       {
           Query = queryType;
           Mutation = mutationType;
       }
   }
   ```

## Additional Samples

Below are samples of root query and mutation types.

### Root Query Type

Queries can handle multiple parameters and implement filtering:

```csharp
public class QueryType : DIObjectGraphType<Query>
{
    public QueryType()
    {
        // Traditional code-first resolvers can be defined here
        // All resolvers defined in the Query type below are added to these definitions
    }
}

public class Query : DIObjectGraphBase
{
    private readonly IRepository _repository;

    public Query(IRepository repository)
    {
        _repository = repository;
    }

    // Multiple optional parameters for filtering
    public async Task<IEnumerable<Todo>> TodosAsync(
        int? id,
        IEnumerable<int>? ids,
        int? completedByPersonId,
        CancellationToken cancellationToken) // Can use CancellationToken directly (equivalent to the RequestAborted property)
    {
        IQueryable<Todo> query = _repository.Todos;
        
        if (id.HasValue)
            query = query.Where(x => x.Id == id);
        if (ids != null)
            query = query.Where(x => ids.Contains(x.Id));
        if (completedByPersonId != null)
            query = query.Where(x => x.CompletedByPersonId == completedByPersonId);

        return await query.ToListAsync(cancellationToken);
    }

    // Single item query
    public async Task<Todo> TodoAsync(int id, CancellationToken cancellationToken)
    {
        return await _repository.Todos
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
```

### Root Mutation Type

Mutations are easily implemented using `DIObjectGraphBase` (which is the same
as `DIObjectGraphBase<object>`, useful when the object itself is unimportant):

```csharp
public class Mutation : DIObjectGraphBase
{
    private readonly IRepository _repository;

    public Mutation(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Todo> AddTodoAsync(string title, string notes)
    {
        // Use RequestAborted for cancellation support
        var todo = new Todo {
            Title = title,
            Notes = notes,
        };
        return await _repository.AddTodoAsync(todo, RequestAborted);
    }

    public async Task<Todo?> SetCompleteAsync(int id, int completedByPersonId)
    {
        var todo = await _repository.GetTodoAsync(id, RequestAborted);
        if (todo == null)
            return null;
            
        if (todo.Completed)
            throw new ExecutionError($"Task id {id} has already been completed");

        todo.Completed = true;
        todo.CompletedByPersonId = completedByPersonId;
        todo.CompletionDate = DateTime.Now;
        
        await _repository.SaveChangesAsync(RequestAborted);
        return todo;
    }
}
```

## Important Notes

- Classes inheriting from `DIObjectGraphBase` **must** be registered with a Transient lifetime in the DI container if they implement `IDisposable`
- The `Source` property provides type-safe access to the parent object being resolved
- Static methods in resolver classes will not create an instance of the class, useful for simple property resolution
- The library fully supports GraphQL.NET's data loader pattern for efficient batching and caching of data fetching operations
- Use the `RequestAborted` property or `CancellationToken` parameter for cancellation support
- Throw `ExecutionError` to return specific error messages to GraphQL clients
- Can be used alongside traditional GraphQL.NET types, allowing for gradual adoption
- All standard GraphQL.NET context properties are available through the `Context` property
- Does not support GraphQL.NET reflection caching (`GlobalSwitches.EnableReflectionCaching`)
