using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.DI;
using Shouldly;
using Xunit;

namespace NullableTests
{
    public class ArgumentTests
    {
        [Theory]
        [InlineData(typeof(NullableClass9), "Field1", "arg1", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass9), "Field1", "arg2", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass10), "Field1", "arg1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass10), "Field1", "arg2", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass11), "Field2", "arg1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass11), "Field2", "arg2", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass13), "Field2", "arg1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass13), "Field2", "arg2", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass13), "Field2", "arg3", typeof(int), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass13), "Field2", "arg4", typeof(int), Nullability.Nullable)]
        [InlineData(typeof(NullableClass13), "Field2", "arg5", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass13), "Field2", "arg6", typeof(IEnumerable<string>), Nullability.NonNullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass13), "Field2", "arg7", typeof(IEnumerable<string>), Nullability.NonNullable, typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass13), "Field2", "arg8", typeof(IEnumerable<string>), Nullability.Nullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass13), "Field2", "arg9", typeof(IEnumerable<string>), Nullability.Nullable, typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass14), "Field2", "arg1", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass14), "Field2", "arg2", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass14), "Field2", "arg3", typeof(int), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass14), "Field2", "arg4", typeof(int), Nullability.Nullable)]
        [InlineData(typeof(NullableClass14), "Field2", "arg5", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass14), "Field2", "arg6", typeof(IEnumerable<string>), Nullability.NonNullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass14), "Field2", "arg7", typeof(IEnumerable<string>), Nullability.NonNullable, typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass14), "Field2", "arg8", typeof(IEnumerable<string>), Nullability.Nullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass14), "Field2", "arg9", typeof(IEnumerable<string>), Nullability.Nullable, typeof(string), Nullability.Nullable)]
        public void Argument(Type type, string methodName, string argumentName, Type expectedType, Nullability expectedNullability, Type expectedType2 = null, Nullability? expectedNullability2 = null)
        {
            var method = type.GetMethod(methodName);
            var argument = method.GetParameters().Single(x => x.Name == argumentName);
            var actual = argument.GetNullabilityInformation().ToList();
            actual.Count.ShouldBe(expectedType2 == null ? 1 : 2);
            actual[0].Type.ShouldBe(expectedType);
            actual[0].Nullable.ShouldBe(expectedNullability);
            if (expectedType2 != null) {
                actual[1].Type.ShouldBe(expectedType2);
                actual[1].Nullable.ShouldBe(expectedNullability2.Value);
            }
        }

        [Theory]
        [InlineData(typeof(NullableClass9), "Field1", "arg1", typeof(string), true)]
        [InlineData(typeof(NullableClass9), "Field1", "arg2", typeof(string), true)]
        [InlineData(typeof(NullableClass10), "Field1", "arg1", typeof(string), false)]
        [InlineData(typeof(NullableClass10), "Field1", "arg2", typeof(string), false)]
        [InlineData(typeof(NullableClass11), "Field2", "arg1", typeof(string), false)]
        [InlineData(typeof(NullableClass11), "Field2", "arg2", typeof(string), false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg1", typeof(string), false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg2", typeof(string), true)]
        [InlineData(typeof(NullableClass13), "Field2", "arg3", typeof(int), false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg4", typeof(int), true)]
        [InlineData(typeof(NullableClass13), "Field2", "arg5", typeof(string), false, false, false, true)]
        [InlineData(typeof(NullableClass13), "Field2", "arg6", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg7", typeof(string), true, true, false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg8", typeof(string), false, true, true)]
        [InlineData(typeof(NullableClass13), "Field2", "arg9", typeof(string), true, true, true)]
        [InlineData(typeof(NullableClass14), "Field2", "arg1", typeof(string), true)]
        [InlineData(typeof(NullableClass14), "Field2", "arg2", typeof(string), false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg3", typeof(int), false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg4", typeof(int), true)]
        [InlineData(typeof(NullableClass14), "Field2", "arg5", typeof(string), true, false, false, false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg6", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg7", typeof(string), true, true, false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg8", typeof(string), false, true, true)]
        [InlineData(typeof(NullableClass14), "Field2", "arg9", typeof(string), true, true, true)]
        [InlineData(typeof(NullableClass19), "Field18", "arg1", typeof(string), true)]
        [InlineData(typeof(NullableClass19), "Field18", "arg2", typeof(string), false, true, true)]
        [InlineData(typeof(NullableClass19), "Field18", "arg3", typeof(object), true)]
        [InlineData(typeof(NullableClass19), "Field18", "arg4", typeof(object), false, true, true)]
        public void Argument_GetTypeInformation(Type type, string methodName, string argumentName, Type expectedType, bool isNullable = false, bool isList = false, bool isNullableList = false, bool? isNullableAfterAttributes = null, bool? isNullableListAfterAttributes = null)
        {
            var method = type.GetMethod(methodName);
            var argument = method.GetParameters().Single(x => x.Name == argumentName);
            var actual = TestGraphType.Instance.GetTypeInformation(argument, true);
            actual.ParameterInfo.ShouldBe(argument);
            actual.MemberInfo.ShouldBe(argument.Member);
            actual.IsInputType.ShouldBeTrue();
            actual.GraphType.ShouldBeNull();
            actual.Type.ShouldBe(expectedType);
            actual.IsNullable.ShouldBe(isNullable);
            actual.IsList.ShouldBe(isList);
            actual.ListIsNullable.ShouldBe(isNullableList);
            actual = TestGraphType.Instance.ApplyAttributes(actual);
            actual.IsNullable.ShouldBe(isNullableAfterAttributes ?? isNullable);
            actual.ListIsNullable.ShouldBe(isNullableListAfterAttributes ?? isNullableList);
        }
    }
}
