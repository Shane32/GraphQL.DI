using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.DI;
using GraphQL.SystemTextJson;
using GraphQL.Types;
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
            var actual = await RunQuery("{field1,field2,field3,field4,field5,field6,field7,field8}");
            actual.ShouldBe(@"{""data"":{""field1"":""ret1"",""field2"":""ret2"",""field3"":""ret3"",""field4"":""ret4"",""field5"":""ret5"",""field6"":""ret6"",""field7"":""ret7"",""field8"":""ret8""}}");
        }

        private async Task<string> RunQuery(string query)
        {
            var result = await _executer.ExecuteAsync(new GraphQL.ExecutionOptions {
                Query = query,
                Schema = _schema,
                MaxParallelExecutionCount = 2,
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
        }
    }
}
