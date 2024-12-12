using System.Diagnostics;
using EfLocalDb;
using GraphQL;
using GraphQL.AspNetCore3;
using GraphQL.Execution;
using GraphQLParser.AST;
using Sample.DataLoaders;

namespace Sample;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();

        services.AddGraphQL(b => b
            .AddSchema<TodoSchema>()
            .ConfigureExecutionOptions(opts => opts.UnhandledExceptionDelegate = e => {
                Debug.WriteLine($"Unhandled exception:\n{e.Exception}\n");
                return Task.CompletedTask;
            })
            .AddSystemTextJson()
            .AddExecutionStrategy<SerialExecutionStrategy>(OperationType.Query)
            .AddDI()
            .AddClrTypeMappings()
            .AddGraphTypes());

        //construct temporary database with scoped dbcontext instances
        services.AddSingleton(_ => new SqlInstance<TodoDbContext>(builder => new TodoDbContext(builder.Options)));
        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<SqlInstance<TodoDbContext>>().Build().GetAwaiter().GetResult());
        services.AddScoped(serviceProvider => serviceProvider.GetRequiredService<SqlDatabase<TodoDbContext>>().NewDbContext());

        //add some scoped data loaders
        services.AddScoped<PersonDataLoader>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        } else {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseGraphQL<TodoSchema>();
        app.UseGraphQLGraphiQL();

        app.UseEndpoints(endpoints => {
            endpoints.MapRazorPages();
        });
    }
}
