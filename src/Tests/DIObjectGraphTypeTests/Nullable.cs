using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using GraphQL.DI;
using GraphQL.Types;
using Shouldly;
using Tests.NullabilityTestClasses;
using Xunit;

namespace DIObjectGraphTypeTests
{
    public class Nullable
    {
        //Verify... methods verify that .NET is building the classes
        //  with the expected attributes on them. Failure is not an
        //  error, but simply indicates that the Method and Argument
        //  tests may not be testing the anticipated scenarios
        //Method and Argument should always pass
        [Theory]
        [InlineData(typeof(NullableClass1), 1, 0)] //default not nullable
        [InlineData(typeof(NullableClass2), 2, 0)] //default nullable
        [InlineData(typeof(NullableClass5), null, null)]
        [InlineData(typeof(NullableClass6), null, null)]
        [InlineData(typeof(NullableClass7), 1, 0)] //default not nullable
        [InlineData(typeof(NullableClass8), 2, 0)] //default nullable
        [InlineData(typeof(NullableClass9), null, null)]
        [InlineData(typeof(NullableClass10), null, null)]
        [InlineData(typeof(NullableClass11), 1, 0)] //default not nullable
        [InlineData(typeof(NullableClass12), null, null)]
        [InlineData(typeof(NullableClass13), 1, 0)] //default not nullable
        [InlineData(typeof(NullableClass14), 2, 0)] //default nullable
        [InlineData(typeof(NullableClass15), null, null)]
        [InlineData(typeof(NullableClass16), 1, 0)]
        [InlineData(typeof(NullableClass16.NestedClass1), null, 0)]
        [InlineData(typeof(NullableClass17), 1, 0)]
        [InlineData(typeof(NullableClass18<>), null, null)]
        public void VerifyTestClass(Type type, int? nullableContext, int? nullable)
        {
            var actualHasNullableContext = type.CustomAttributes.FirstOrDefault(
                x => x.AttributeType.Name == "NullableContextAttribute");
            if (nullableContext == null) {
                actualHasNullableContext.ShouldBeNull();
            } else {
                actualHasNullableContext.ShouldNotBeNull();
                actualHasNullableContext.ConstructorArguments[0].Value.ShouldBe(nullableContext);
            }

            var actualHasNullable = type.CustomAttributes.FirstOrDefault(
                x => x.AttributeType.Name == "NullableAttribute");
            if (nullable == null) {
                actualHasNullable.ShouldBeNull();
            } else {
                actualHasNullable.ShouldNotBeNull();
                actualHasNullable.ConstructorArguments[0].ArgumentType.ShouldBe(typeof(byte));
                actualHasNullable.ConstructorArguments[0].Value.ShouldBe(nullable);
            }
        }

        [Theory]
        [InlineData(typeof(NullableClass1), "Field1", 2, null)]
        [InlineData(typeof(NullableClass1), "Field2", 2, null)]
        [InlineData(typeof(NullableClass1), "Field3", null, null)]
        [InlineData(typeof(NullableClass1), "Field4", null, null)]
        [InlineData(typeof(NullableClass1), "Field5", null, null)]
        [InlineData(typeof(NullableClass1), "Field6", null, null)]
        [InlineData(typeof(NullableClass1), "Field7", null, null)]
        [InlineData(typeof(NullableClass1), "Field8", null, null)]
        [InlineData(typeof(NullableClass1), "Field9", null, null)]
        [InlineData(typeof(NullableClass1), "Field10", null, null)]
        [InlineData(typeof(NullableClass2), "Field1", null, null)]
        [InlineData(typeof(NullableClass2), "Field2", null, null)]
        [InlineData(typeof(NullableClass2), "Field3", null, null)]
        [InlineData(typeof(NullableClass2), "Field4", null, null)]
        [InlineData(typeof(NullableClass2), "Field5", null, null)]
        [InlineData(typeof(NullableClass2), "Field6", null, null)]
        [InlineData(typeof(NullableClass2), "Field7", null, null)]
        [InlineData(typeof(NullableClass2), "Field8", null, null)]
        [InlineData(typeof(NullableClass2), "Field9", 1, null)]
        [InlineData(typeof(NullableClass2), "Field10", 1, null)]
        [InlineData(typeof(NullableClass2), "Field11", null, null)]
        [InlineData(typeof(NullableClass5), "Test", 1, null)]
        [InlineData(typeof(NullableClass6), "Field1", 1, null)]
        [InlineData(typeof(NullableClass6), "Field2", 2, null)]
        [InlineData(typeof(NullableClass7), "Field1", null, null)]
        [InlineData(typeof(NullableClass7), "Field2", null, null)]
        [InlineData(typeof(NullableClass7), "Field3", 2, null)]
        [InlineData(typeof(NullableClass8), "Field1", null, null)]
        [InlineData(typeof(NullableClass8), "Field2", null, null)]
        [InlineData(typeof(NullableClass8), "Field3", 1, null)]
        [InlineData(typeof(NullableClass8), "Field4", null, null)]
        [InlineData(typeof(NullableClass9), "Field1", 2, "1")]
        [InlineData(typeof(NullableClass10), "Field1", 1, "2")]
        [InlineData(typeof(NullableClass11), "Field1", null, null)]
        [InlineData(typeof(NullableClass11), "Field2", null, "2")]
        [InlineData(typeof(NullableClass12), "Field1", 1, null)]
        [InlineData(typeof(NullableClass12), "Field2", null, "12")]
        [InlineData(typeof(NullableClass12), "Field3", null, "21")]
        [InlineData(typeof(NullableClass12), "Field4", 2, null)]
        [InlineData(typeof(NullableClass13), "Field1", null, null)]
        [InlineData(typeof(NullableClass13), "Field2", null, null)]
        [InlineData(typeof(NullableClass14), "Field1", null, null)]
        [InlineData(typeof(NullableClass14), "Field2", null, null)]
        [InlineData(typeof(NullableClass15), "Field1", 1, null)]
        [InlineData(typeof(NullableClass15), "Field2", null, "12")]
        [InlineData(typeof(NullableClass15), "Field3", null, "21")]
        [InlineData(typeof(NullableClass15), "Field4", 2, null)]
        [InlineData(typeof(NullableClass16), "Field1", null, null)]
        [InlineData(typeof(NullableClass16), "Field2", null, null)]
        [InlineData(typeof(NullableClass16), "Field3", 2, null)]
        [InlineData(typeof(NullableClass16.NestedClass1), "Field1", null, null)]
        [InlineData(typeof(NullableClass16.NestedClass1), "Field2", null, null)]
        [InlineData(typeof(NullableClass16.NestedClass1), "Field3", 2, null)]
        [InlineData(typeof(NullableClass17), "Field1", null, null)]
        [InlineData(typeof(NullableClass17), "Field2", null, null)]
        [InlineData(typeof(NullableClass17), "Field3", null, "12")]
        [InlineData(typeof(NullableClass18<>), "Field1", null, "112")]
        [InlineData(typeof(NullableClass18<>), "Field2", null, "11221")]
        [InlineData(typeof(NullableClass18<>), "Field3", null, "12")]
        [InlineData(typeof(NullableClass18<>), "Field4", null, "12")]
        [InlineData(typeof(NullableClass18<>), "Field5", null, "1112")]
        [InlineData(typeof(NullableClass18<>), "Field6", null, "112")]
        [InlineData(typeof(NullableClass18<>), "Field7", null, "1012")]
        [InlineData(typeof(NullableClass18<>), "Field8", null, "1012")]
        [InlineData(typeof(NullableClass18<>), "Field9", null, "102")]
        [InlineData(typeof(NullableClass18<>), "Field10", null, "112")]
        public void VerifyTestMethod(Type type, string methodName, int? nullableContext, string nullableValues)
        {
            var method = type.GetMethod(methodName);
            var methodNullableAttribute = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "NullableAttribute");
            methodNullableAttribute.ShouldBeNull(); //should not be possible to apply the attribute here

            var methodNullableContextAttribute = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "NullableContextAttribute");
            if (nullableContext.HasValue) {
                methodNullableContextAttribute.ShouldNotBeNull();
                methodNullableContextAttribute.ConstructorArguments[0].ArgumentType.ShouldBe(typeof(byte));
                methodNullableContextAttribute.ConstructorArguments[0].Value.ShouldBeOfType<byte>().ShouldBe((byte)nullableContext.Value);
            } else {
                methodNullableContextAttribute.ShouldBeNull();
            }

            var parameterNullableAttribute = method.ReturnParameter.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "NullableAttribute");
            if (nullableValues != null) {
                parameterNullableAttribute.ShouldNotBeNull();
                var expectedValues = nullableValues.Select(x => (byte)int.Parse(x.ToString())).ToArray();
                if (expectedValues.Length == 1) {
                    parameterNullableAttribute.ConstructorArguments[0].ArgumentType.ShouldBe(typeof(byte));
                    var actualValue = parameterNullableAttribute.ConstructorArguments[0].Value.ShouldBeOfType<byte>().ToString();
                    actualValue.ShouldBe(nullableValues);
                } else {
                    parameterNullableAttribute.ConstructorArguments[0].ArgumentType.ShouldBe(typeof(byte[]));
                    var actualValues = string.Join("", parameterNullableAttribute.ConstructorArguments[0].Value.ShouldBeOfType<ReadOnlyCollection<CustomAttributeTypedArgument>>().Select(x => x.Value.ToString()));
                    actualValues.ShouldBe(nullableValues);
                }
            } else {
                parameterNullableAttribute.ShouldBeNull();
            }
        }

        [Theory]
        [InlineData(typeof(NullableClass9), "Field1", "arg1", null)]
        [InlineData(typeof(NullableClass9), "Field1", "arg2", null)]
        [InlineData(typeof(NullableClass10), "Field1", "arg1", null)]
        [InlineData(typeof(NullableClass10), "Field1", "arg2", null)]
        [InlineData(typeof(NullableClass11), "Field2", "arg1", null)]
        [InlineData(typeof(NullableClass11), "Field2", "arg2", null)]
        [InlineData(typeof(NullableClass13), "Field2", "arg1", null)]
        [InlineData(typeof(NullableClass13), "Field2", "arg2", "2")]
        [InlineData(typeof(NullableClass13), "Field2", "arg3", null)]
        [InlineData(typeof(NullableClass13), "Field2", "arg4", null)]
        [InlineData(typeof(NullableClass13), "Field2", "arg5", null)]
        [InlineData(typeof(NullableClass13), "Field2", "arg6", null)]
        [InlineData(typeof(NullableClass13), "Field2", "arg7", "12")]
        [InlineData(typeof(NullableClass13), "Field2", "arg8", "21")]
        [InlineData(typeof(NullableClass13), "Field2", "arg9", "2")]
        [InlineData(typeof(NullableClass14), "Field2", "arg1", null)]
        [InlineData(typeof(NullableClass14), "Field2", "arg2", "1")]
        [InlineData(typeof(NullableClass14), "Field2", "arg3", null)]
        [InlineData(typeof(NullableClass14), "Field2", "arg4", null)]
        [InlineData(typeof(NullableClass14), "Field2", "arg5", null)]
        [InlineData(typeof(NullableClass14), "Field2", "arg6", "1")]
        [InlineData(typeof(NullableClass14), "Field2", "arg7", "12")]
        [InlineData(typeof(NullableClass14), "Field2", "arg8", "21")]
        [InlineData(typeof(NullableClass14), "Field2", "arg9", null)]
        public void VerifyTestArgument(Type type, string methodName, string argumentName, string nullableValues)
        {
            var method = type.GetMethod(methodName);
            var argument = method.GetParameters().Single(x => x.Name == argumentName);
            var parameterNullableAttribute = argument.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "NullableAttribute");
            if (nullableValues != null) {
                parameterNullableAttribute.ShouldNotBeNull();
                var expectedValues = nullableValues.Select(x => (byte)int.Parse(x.ToString())).ToArray();
                if (expectedValues.Length == 1) {
                    parameterNullableAttribute.ConstructorArguments[0].ArgumentType.ShouldBe(typeof(byte));
                    var actualValue = parameterNullableAttribute.ConstructorArguments[0].Value.ShouldBeOfType<byte>().ToString();
                    actualValue.ShouldBe(nullableValues);
                } else {
                    parameterNullableAttribute.ConstructorArguments[0].ArgumentType.ShouldBe(typeof(byte[]));
                    var actualValues = string.Join("", parameterNullableAttribute.ConstructorArguments[0].Value.ShouldBeOfType<ReadOnlyCollection<CustomAttributeTypedArgument>>().Select(x => x.Value.ToString()));
                    actualValues.ShouldBe(nullableValues);
                }
            } else {
                parameterNullableAttribute.ShouldBeNull();
            }
        }

        [Theory]
        [InlineData(typeof(NullableClass1), "Field1", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass1), "Field2", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass1), "Field3", typeof(int), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass1), "Field4", typeof(int), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass1), "Field5", typeof(int), Nullability.Nullable)]
        [InlineData(typeof(NullableClass1), "Field6", typeof(int), Nullability.Nullable)]
        [InlineData(typeof(NullableClass1), "Field7", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass1), "Field8", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass1), "Field9", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass1), "Field10", typeof(List<string>), Nullability.NonNullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass2), "Field1", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass2), "Field2", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass2), "Field3", typeof(int), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass2), "Field4", typeof(int), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass2), "Field5", typeof(int), Nullability.Nullable)]
        [InlineData(typeof(NullableClass2), "Field6", typeof(int), Nullability.Nullable)]
        [InlineData(typeof(NullableClass2), "Field7", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass2), "Field8", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass2), "Field9", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass2), "Field10", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass2), "Field11", typeof(List<string>), Nullability.Nullable, typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass5), "Test", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass6), "Field1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass6), "Field2", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass7), "Field1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass7), "Field2", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass7), "Field3", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass8), "Field1", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass8), "Field2", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass8), "Field3", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass8), "Field4", typeof(int), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass9), "Field1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass10), "Field1", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass11), "Field1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass11), "Field2", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass12), "Field1", typeof(IDataLoaderResult<string>), Nullability.NonNullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass12), "Field2", typeof(IDataLoaderResult<string>), Nullability.NonNullable, typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass12), "Field3", typeof(IDataLoaderResult<string>), Nullability.Nullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass12), "Field4", typeof(IDataLoaderResult<string>), Nullability.Nullable, typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass13), "Field1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass13), "Field2", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass14), "Field1", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass14), "Field2", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass15), "Field1", typeof(Task<string>), Nullability.NonNullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass15), "Field2", typeof(Task<string>), Nullability.NonNullable, typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass15), "Field3", typeof(Task<string>), Nullability.Nullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass15), "Field4", typeof(Task<string>), Nullability.Nullable, typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass16), "Field1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass16), "Field2", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass16), "Field3", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass16.NestedClass1), "Field1", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass16.NestedClass1), "Field2", typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass16.NestedClass1), "Field3", typeof(string), Nullability.Nullable)]
        [InlineData(typeof(NullableClass17), "Field1", typeof(Task<string>), Nullability.NonNullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass17), "Field2", typeof(Task<string>), Nullability.NonNullable, typeof(string), Nullability.NonNullable)]
        [InlineData(typeof(NullableClass17), "Field3", typeof(Task<string>), Nullability.NonNullable, typeof(string), Nullability.Nullable)]
        public void Method_GetNullability(Type type, string methodName, Type expectedType, Nullability expectedNullability, Type expectedType2 = null, Nullability? expectedNullability2 = null)
        {
            var method = type.GetMethod(methodName);
            var actual = TestGraphType.Instance.GetNullability(method.ReturnParameter).ToList();
            actual.Count.ShouldBe(expectedType2 == null ? 1 : 2);
            actual[0].Item1.ShouldBe(expectedType);
            actual[0].Item2.ShouldBe(expectedNullability);
            if (expectedType2 != null) {
                actual[1].Item1.ShouldBe(expectedType2);
                actual[1].Item2.ShouldBe(expectedNullability2.Value);
            }
        }

        public static IEnumerable<object[]> Class18_GetNullability_TestCases()
        {
            return new object[][] {
                new object[] { "Field1", new List<(Type, Nullability)>() {
                    (typeof(Tuple<string, string>), Nullability.NonNullable),
                    (typeof(string), Nullability.NonNullable),
                    (typeof(string), Nullability.Nullable),
                } },
                new object[] { "Field2", new List<(Type, Nullability)>() {
                    (typeof(Tuple<Tuple<string, string>, string>), Nullability.NonNullable),
                    (typeof(Tuple<string, string>), Nullability.NonNullable),
                    (typeof(string), Nullability.Nullable),
                    (typeof(string), Nullability.Nullable),
                    (typeof(string), Nullability.NonNullable),
                } },
                new object[] { "Field3", new List<(Type, Nullability)>() {
                    (typeof(Tuple<int?, string>), Nullability.NonNullable),
                    (typeof(int), Nullability.Nullable),
                    (typeof(string), Nullability.Nullable),
                } },
                new object[] { "Field4", new List<(Type, Nullability)>() {
                    (typeof(Tuple<Guid, string>), Nullability.NonNullable),
                    (typeof(Guid), Nullability.NonNullable),
                    (typeof(string), Nullability.Nullable),
                } },
                new object[] { "Field5", new List<(Type, Nullability)>() {
                    (typeof(Tuple<string[], string>), Nullability.NonNullable),
                    (typeof(string[]), Nullability.NonNullable),
                    (typeof(string), Nullability.NonNullable),
                    (typeof(string), Nullability.Nullable),
                } },
                new object[] { "Field6", new List<(Type, Nullability)>() {
                    (typeof(Tuple<int[], string>), Nullability.NonNullable),
                    (typeof(int[]), Nullability.NonNullable),
                    (typeof(int), Nullability.NonNullable),
                    (typeof(string), Nullability.Nullable),
                } },
                new object[] { "Field7", new List<(Type, Nullability)>() {
                    (typeof(Tuple<(int, string), string>), Nullability.NonNullable),
                    (typeof((int, string)), Nullability.NonNullable),
                    (typeof(int), Nullability.NonNullable),
                    (typeof(string), Nullability.NonNullable),
                    (typeof(string), Nullability.Nullable),
                } },
                new object[] { "Field8", new List<(Type, Nullability)>() {
                    (typeof(Tuple<(int, string), string>), Nullability.NonNullable),
                    (typeof((int, string)), Nullability.NonNullable),
                    (typeof(int), Nullability.NonNullable),
                    (typeof(string), Nullability.NonNullable),
                    (typeof(string), Nullability.Nullable),
                } },
                new object[] { "Field9", new List<(Type, Nullability)>() {
                    (typeof(Tuple<TestStruct<Guid>, string>), Nullability.NonNullable),
                    (typeof(TestStruct<Guid>), Nullability.NonNullable),
                    (typeof(Guid), Nullability.NonNullable),
                    (typeof(string), Nullability.Nullable),
                } },
                new object[] { "Field10", new List<(Type, Nullability)>() {
                    (typeof(Tuple<,>).MakeGenericType(typeof(NullableClass18<>).GetGenericArguments()[0], typeof(string)), Nullability.NonNullable),
                    (typeof(NullableClass18<>).GetGenericArguments()[0], Nullability.NonNullable),
                    (typeof(string), Nullability.Nullable),
                } },
            };
        }

        [Theory]
        [MemberData(nameof(Class18_GetNullability_TestCases))]
        public void Method_GetNullability_Class18(string methodName, List<(Type, Nullability)> expected)
        {
            var method = typeof(NullableClass18<>).GetMethod(methodName).ShouldNotBeNull();
            var actual = TestGraphType.Instance.GetNullability(method.ReturnParameter).ToList();
            expected.ShouldBe(actual);
        }

        [Theory]
        [InlineData(typeof(NullableClass1), "Field1", typeof(string), true)]
        [InlineData(typeof(NullableClass1), "Field2", typeof(string), true, false, false, false)]
        [InlineData(typeof(NullableClass1), "Field3", typeof(int), false)]
        [InlineData(typeof(NullableClass1), "Field4", typeof(int), false, false, false, true)]
        [InlineData(typeof(NullableClass1), "Field5", typeof(int), true)]
        [InlineData(typeof(NullableClass1), "Field6", typeof(int), true, false, false, false)]
        [InlineData(typeof(NullableClass1), "Field7", typeof(string), false)]
        [InlineData(typeof(NullableClass1), "Field8", typeof(string), false, false, false, true)]
        [InlineData(typeof(NullableClass1), "Field9", typeof(string), false, false, false, false, false, typeof(IdGraphType))]
        [InlineData(typeof(NullableClass1), "Field10", typeof(string), false, true, false, false, true)]
        [InlineData(typeof(NullableClass2), "Field1", typeof(string), true)]
        [InlineData(typeof(NullableClass2), "Field2", typeof(string), true, false, false, false)]
        [InlineData(typeof(NullableClass2), "Field3", typeof(int), false)]
        [InlineData(typeof(NullableClass2), "Field4", typeof(int), false, false, false, true)]
        [InlineData(typeof(NullableClass2), "Field5", typeof(int), true)]
        [InlineData(typeof(NullableClass2), "Field6", typeof(int), true, false, false, false)]
        [InlineData(typeof(NullableClass2), "Field7", typeof(string), true)]
        [InlineData(typeof(NullableClass2), "Field8", typeof(string), true)]
        [InlineData(typeof(NullableClass2), "Field9", typeof(string), false)]
        [InlineData(typeof(NullableClass2), "Field10", typeof(string), false, false, false, true)]
        [InlineData(typeof(NullableClass2), "Field11", typeof(string), true, true, true, true, false)]
        [InlineData(typeof(NullableClass5), "Test", typeof(string), false)]
        [InlineData(typeof(NullableClass6), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass6), "Field2", typeof(string), true)]
        [InlineData(typeof(NullableClass7), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass7), "Field2", typeof(string), false)]
        [InlineData(typeof(NullableClass7), "Field3", typeof(string), true)]
        [InlineData(typeof(NullableClass8), "Field1", typeof(string), true)]
        [InlineData(typeof(NullableClass8), "Field2", typeof(string), true)]
        [InlineData(typeof(NullableClass8), "Field3", typeof(string), false)]
        [InlineData(typeof(NullableClass8), "Field4", typeof(int), false)]
        [InlineData(typeof(NullableClass9), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass10), "Field1", typeof(string), true)]
        [InlineData(typeof(NullableClass11), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass11), "Field2", typeof(string), true)]
        [InlineData(typeof(NullableClass12), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass12), "Field2", typeof(string), true)]
        [InlineData(typeof(NullableClass12), "Field3", typeof(string), false)]
        [InlineData(typeof(NullableClass12), "Field4", typeof(string), true)]
        [InlineData(typeof(NullableClass13), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass13), "Field2", typeof(string), false)]
        [InlineData(typeof(NullableClass14), "Field1", typeof(string), true)]
        [InlineData(typeof(NullableClass14), "Field2", typeof(string), true)]
        [InlineData(typeof(NullableClass15), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass15), "Field2", typeof(string), true)]
        [InlineData(typeof(NullableClass15), "Field3", typeof(string), false)]
        [InlineData(typeof(NullableClass15), "Field4", typeof(string), true)]
        [InlineData(typeof(NullableClass16), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass16), "Field2", typeof(string), false)]
        [InlineData(typeof(NullableClass16), "Field3", typeof(string), true)]
        [InlineData(typeof(NullableClass16.NestedClass1), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass16.NestedClass1), "Field2", typeof(string), false)]
        [InlineData(typeof(NullableClass16.NestedClass1), "Field3", typeof(string), true)]
        [InlineData(typeof(NullableClass17), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass17), "Field2", typeof(string), false)]
        [InlineData(typeof(NullableClass17), "Field3", typeof(string), true)]
        [InlineData(typeof(NullableClass19), "Field1", typeof(string), false)]
        [InlineData(typeof(NullableClass19), "Field2", typeof(string), false)]
        [InlineData(typeof(NullableClass19), "Field3", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field4", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field5", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field6", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field7", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field8", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field9", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field10", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field11", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field12", typeof(object), true, true, false)]
        [InlineData(typeof(NullableClass19), "Field13", typeof(object), true, true, false)]
        [InlineData(typeof(NullableClass19), "Field14", typeof(string), false)]
        [InlineData(typeof(NullableClass19), "Field15", typeof(object), true)]
        [InlineData(typeof(NullableClass19), "Field16", typeof(string), false, true, false)]
        [InlineData(typeof(NullableClass19), "Field17", typeof(object), false)]
        public void Method_GetTypeInformation(Type type, string methodName, Type expectedType, bool isNullable = false, bool isList = false, bool isNullableList = false, bool? isNullableAfterAttributes = null, bool? isNullableListAfterAttributes = null, Type expectedGraphType = null)
        {
            var method = type.GetMethod(methodName);
            var actual = TestGraphType.Instance.GetTypeInformation(method.ReturnParameter, false);
            actual.ParameterInfo.ShouldBe(method.ReturnParameter);
            actual.IsInputType.ShouldBeFalse();
            actual.GraphType.ShouldBeNull();
            actual.Type.ShouldBe(expectedType);
            actual.IsNullable.ShouldBe(isNullable);
            actual.IsList.ShouldBe(isList);
            actual.ListIsNullable.ShouldBe(isNullableList);
            actual = TestGraphType.Instance.ApplyAttributes(actual);
            actual.IsNullable.ShouldBe(isNullableAfterAttributes ?? isNullable);
            actual.ListIsNullable.ShouldBe(isNullableListAfterAttributes ?? isNullableList);
            actual.GraphType.ShouldBe(expectedGraphType);
        }

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
            var actual = TestGraphType.Instance.GetNullability(argument).ToList();
            actual.Count.ShouldBe(expectedType2 == null ? 1 : 2);
            actual[0].Item1.ShouldBe(expectedType);
            actual[0].Item2.ShouldBe(expectedNullability);
            if (expectedType2 != null) {
                actual[1].Item1.ShouldBe(expectedType2);
                actual[1].Item2.ShouldBe(expectedNullability2.Value);
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
        private class TestGraphType : DIObjectGraphType<TestClass>
        {
            public static TestGraphType Instance = new();
            public new IEnumerable<(Type, Nullability)> GetNullability(ParameterInfo parameterInfo) => base.GetNullability(parameterInfo);
            public new TypeInformation GetTypeInformation(ParameterInfo parameterInfo, bool isInputArgument) => base.GetTypeInformation(parameterInfo, isInputArgument);
            public new TypeInformation ApplyAttributes(TypeInformation typeInformation) => base.ApplyAttributes(typeInformation);
        }

        private class TestClass : DIObjectGraphBase { }
    }
}
