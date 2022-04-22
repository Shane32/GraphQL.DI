using EfLocalDb;
using GraphQL;
using GraphQL.AspNetCore3;
using GraphQL.DI;
using GraphQL.MicrosoftDI;
using GraphQL.Server;
using GraphQL.SystemTextJson;
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

            services.AddGraphQL(b => b
                .ConfigureExecutionOptions(opts => opts.UnhandledExceptionDelegate = async e => Debug.WriteLine($"Unhandled exception:\n{e.Exception}\n"))
                .AddSystemTextJson()
                .AddDIGraphTypes()
                .AddGraphTypes());
            services.AddSingleton<TodoSchema>();
            //foreach (var type in typeof(TodoSchema).Assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition)) {
            //    var baseType = type.BaseType;
            //    while (baseType != null) {
            //        if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(DIObjectGraphBase<>)) {
            //            services.AddScoped(type);
            //            break;
            //        }
            //        baseType = baseType.BaseType;
            //    }
            //}

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

            app.UseGraphQL<TodoSchema>();
            app.UseGraphQLGraphiQL();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
