using EfLocalDb;
using GraphQL.DI;
using GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.DataLoaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sample
{
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

            services.AddGraphQL(opts => {
                opts.UnhandledExceptionDelegate = e => {
                    Debug.WriteLine($"Unhandled exception:\n{e.Exception}\n");
                };
            })
                .AddSystemTextJson()
                .AddGraphTypes();
            services.AddSingleton<DIDocumentExecuter>();
            services.AddSingleton<TodoSchema>();
            foreach (var type in typeof(TodoSchema).Assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition)) {
                var baseType = type.BaseType;
                while (baseType != null) {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(DIObjectGraphBase<>)) {
                        services.AddScoped(type);
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }

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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            //GET calls to /graphql
            var graphQlPath = new Microsoft.AspNetCore.Http.PathString("/graphql");
            app.MapWhen(context => context.Request.Method == "GET" && context.Request.Path.Equals(graphQlPath, StringComparison.OrdinalIgnoreCase), app2 => app2.UseGraphQLGraphiQL("/graphql"));
            //POST calls to /graphql
            app.UseGraphQL<TodoSchema>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
