using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using GraphQL.DI;
using GraphQL.Types;
using Shouldly;
using Xunit;

namespace NullableTests
{
    public class PropertyTests
    {
        [Theory]
        [InlineData(typeof(NullableClass20), "Field1", typeof(int), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass20), "Field2", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass20), "Field3", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass20), "Field4", typeof(List<string>), Nullability.NonNullable, typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass20), "Field5", typeof(int), Nullability.Nullable)]
        [InlineData(typeof(NullableClass20), "Field6", typeof(string), Nullability.NonNullable)]
        public void GetNullability(Type type, string propertyName, Type expectedType, Nullability expectedNullability, Type expectedType2 = null, Nullability? expectedNullability2 = null)
        {
            var property = type.GetProperty(propertyName);
            var actual = property.GetNullabilityInformation().ToList();
            actual.Count.ShouldBe(expectedType2 == null ? 1 : 2);
            actual[0].Type.ShouldBe(expectedType);
            actual[0].Nullable.ShouldBe(expectedNullability);
            if (expectedType2 != null) {
                actual[1].Type.ShouldBe(expectedType2);
                actual[1].Nullable.ShouldBe(expectedNullability2.Value);
            }
        }

        [Theory]
        [InlineData(typeof(NullableClass20), "Field1", typeof(int), false)]
        [InlineData(typeof(NullableClass20), "Field2", typeof(string), false)]
        [InlineData(typeof(NullableClass20), "Field3", typeof(string), true)]
        [InlineData(typeof(NullableClass20), "Field4", typeof(string), true, true, false)]
        [InlineData(typeof(NullableClass20), "Field5", typeof(int), true)]
        [InlineData(typeof(NullableClass20), "Field6", typeof(string), false)]
        public void GetTypeInformation(Type type, string propertyName, Type expectedType, bool isNullable = false, bool isList = false, bool isNullableList = false, bool? isNullableAfterAttributes = null, bool? isNullableListAfterAttributes = null, Type expectedGraphType = null)
        {
            var property = type.GetProperty(propertyName);
            var actual = TestInputGraphType.Instance.GetTypeInformation(property);
            actual.ParameterInfo.ShouldBeNull();
            actual.MemberInfo.ShouldBe(property);
            actual.IsInputType.ShouldBeTrue();
            actual.GraphType.ShouldBeNull();
            actual.Type.ShouldBe(expectedType);
            actual.IsNullable.ShouldBe(isNullable);
            actual.IsList.ShouldBe(isList);
            actual.ListIsNullable.ShouldBe(isNullableList);
            actual = TestInputGraphType.Instance.ApplyAttributes(actual);
            actual.IsNullable.ShouldBe(isNullableAfterAttributes ?? isNullable);
            actual.ListIsNullable.ShouldBe(isNullableListAfterAttributes ?? isNullableList);
            actual.GraphType.ShouldBe(expectedGraphType);
        }
    }
}
