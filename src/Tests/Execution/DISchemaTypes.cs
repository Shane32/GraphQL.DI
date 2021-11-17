using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.DI;
using GraphQL.Types;
using Moq;
using Shouldly;
using Xunit;

namespace Execution
{
    public class DISchemaTypesTests
    {
        [Theory]
        [InlineData(typeof(int), false, typeof(IntGraphType))]
        [InlineData(typeof(int), true, typeof(IntGraphType))]
        [InlineData(typeof(Class1), false, typeof(AutoObjectGraphType<Class1>))]
        [InlineData(typeof(Class1), true, typeof(AutoInputObjectGraphType<Class1>))]
        public void GetGraphTypeFromClrType(Type clrType, bool isInputType, Type graphType)
        {
            var mySchemaTypes = new MySchemaTypes();
            var mappedTypes = new List<(Type, Type)>();
            mySchemaTypes.GetGraphTypeFromClrType(clrType, isInputType, mappedTypes).ShouldBe(graphType);
        }

        private class MySchemaTypes : DISchemaTypes
        {
            public MySchemaTypes() : base(new Schema(), Mock.Of<IServiceProvider>(), true, true) { }

            public new Type GetGraphTypeFromClrType(Type clrType, bool isInputType, List<(Type ClrType, Type GraphType)> typeMappings)
                => base.GetGraphTypeFromClrType(clrType, isInputType, typeMappings);
        }

        private class Class1 { }

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
