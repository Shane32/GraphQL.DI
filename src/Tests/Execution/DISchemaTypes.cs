using System;
using System.Collections.Generic;
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
        [InlineData(typeof(Class1), false, null)]
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
    }
}
