using GraphQL;
using GraphQL.DI;
using GraphQL.Types;
using Shouldly;
using Xunit;

namespace DISchemaTypesTests
{
    public class Properties
    {
        [Fact]
        public void InputOutputTypesWork()
        {
            var schema = new Schema();
            schema.Query = new ObjectGraphType();
            var newField = new FieldType { Name = "Test", Type = typeof(GraphQLClrOutputTypeReference<Class1Model>) };
            newField.Arguments = new QueryArguments(new QueryArgument<GraphQLClrInputTypeReference<Class2InputModel>> { Name = "arg" });
            schema.Query.AddField(newField);
            var schemaTypes = new DISchemaTypes(schema, new DefaultServiceProvider());
            var class1Type = schemaTypes["Class1"].ShouldBeAssignableTo<IObjectGraphType>();
            class1Type.Fields.Count.ShouldBe(1);
            class1Type.Fields.Find("value").ShouldNotBeNull();
            var class2Type = schemaTypes["Class2Input"].ShouldBeAssignableTo<IInputObjectGraphType>();
            class2Type.Fields.Count.ShouldBe(1);
            class2Type.Fields.Find("value").ShouldNotBeNull();
        }

        private class Class1Model
        {
            public int Value { get; set; }
        }

        private class Class2InputModel
        {
            public int Value { get; set; }
        }
    }
}
