using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.DI;
using GraphQL.Execution;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using Moq;
using Shouldly;
using Xunit;

namespace Execution
{
    public class DIDocumentStrategyTests
    {
        private readonly IDocumentExecuter _executer = new DIDocumentExecuter();
        private readonly ISchema _schema = new MySchema();
        private readonly IDocumentWriter _writer = new DocumentWriter(false);

        [Fact]
        public async Task ItRuns()
        {
            var actual = await RunQuery("{field1,field2,field3,field4,field5,field6,field7,field8,field9,field10,field11,field12,field13,field14}");
            actual.ShouldBe(@"{""data"":{""field1"":""ret1"",""field2"":""ret2"",""field3"":""ret3"",""field4"":""ret4"",""field5"":""ret5"",""field6"":""ret6"",""field7"":""ret7"",""field8"":""ret8"",""field9"":""ret9"",""field10"":""ret10"",""field11"":""ret11"",""field12"":""ret12"",""field13"":""ret13"",""field14"":""ret14""}}");
        }

        [Fact]
        public async Task InvalidMaxTasksThrows()
        {
            await Should.ThrowAsync<InvalidOperationException>(async () => {
                var de = new DIExecutionStrategy();
                var ret = await _executer.ExecuteAsync(new ExecutionOptions {
                    Query = "{field1}",
                    Schema = _schema,
                    MaxParallelExecutionCount = -1,
                    RequestServices = Mock.Of<IServiceProvider>(),
                    ThrowOnUnhandledException = true,
                });
            });
        }

        [Fact]
        public async Task NullRequestServicesThrows()
        {
            var document = GraphQL.Language.CoreToVanillaConverter.Convert(GraphQLParser.Parser.Parse("{field1}"));
            var e = await Should.ThrowAsync<ArgumentNullException>(async () => await new DIExecutionStrategy().ExecuteAsync(new ExecutionContext() {
                Schema = _schema,
                Document = document,
                Operation = document.Operations.First(),
            }));
            e.ParamName.ShouldBe("context.RequestServices");
        }

        private async Task<string> RunQuery(string query)
        {
            var result = await _executer.ExecuteAsync(new ExecutionOptions {
                Query = query,
                Schema = _schema,
                MaxParallelExecutionCount = 2,
                RequestServices = Mock.Of<IServiceProvider>(),
            });
            result.Errors.ShouldBeNull();
            return await _writer.WriteToStringAsync(result);
        }

        private class MySchema : Schema
        {
            public MySchema()
            {
                Query = new DIObjectGraphType<Graph1>();
            }
        }

        private class Graph1 : DIObjectGraphBase
        {
            public static string Field1() => "ret1";
            public static Task<string> Field2() => Task.FromResult("ret2");
            public static Task<string> Field3() => Task.FromResult("ret3");
            public static string Field4() => "ret4";
            public static async Task<string> Field5() { await Task.Yield(); return "ret5"; }
            public static async Task<string> Field6() { await Task.Yield(); return "ret6"; }
            public static async Task<string> Field7() { await Task.Yield(); return "ret7"; }
            public static IDataLoaderResult<string> Field8() => new SimpleDataLoader<string>(c => Task.FromResult("ret8"));
            public string Field9() => "ret9";
            public string Field10() => "ret10";
            public async Task<string> Field11() { await Task.Yield(); return "ret11"; }
            public async Task<string> Field12() { await Task.Yield(); return "ret12"; }
            public async Task<string> Field13() { await Task.Yield(); return "ret13"; }
            public static async Task<IDataLoaderResult<string>> Field14() { await Task.Yield(); return new SimpleDataLoader<string>(c => Task.FromResult("ret14")); }
        }
    }
}
