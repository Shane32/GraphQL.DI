using System;
using GraphQL;
using GraphQL.DI;
using GraphQL.Types;
using Shouldly;

namespace AutoInputObjectGraphTypeTests
{
    public class AutoInputObjectGraphTypeTestBase
    {
        protected IComplexGraphType _graphType;

        protected IComplexGraphType Configure<TSource>()
        {
            _graphType = new AutoInputObjectGraphType<TSource>();
            return _graphType;
        }

        protected FieldType VerifyField<T>(string fieldName, bool nullable, bool isList = false, bool isNullableList = false)
        {
            var type = typeof(T).GetGraphTypeFromType(nullable, TypeMappingMode.InputType);
            if (isList) {
                type = typeof(ListGraphType<>).MakeGenericType(type);
                if (!isNullableList) {
                    type = typeof(NonNullGraphType<>).MakeGenericType(type);
                }
            }
            return VerifyField(fieldName, type);
        }

        protected FieldType VerifyField(string fieldName, Type fieldGraphType)
        {
            _graphType.ShouldNotBeNull();
            _graphType.Fields.ShouldNotBeNull();
            var field = _graphType.Fields.Find(fieldName);
            field.ShouldNotBeNull();
            field.Type.ShouldBe(fieldGraphType);
            field.Resolver.ShouldBeNull();
            return field;
        }
    }
}
