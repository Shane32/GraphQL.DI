using System;
using System.Linq;
using System.Reflection;
using GraphQL.DI;
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
        [InlineData(typeof(NullableClass1), 1)] //default not nullable
        [InlineData(typeof(NullableClass2), 2)] //default nullable
        [InlineData(typeof(NullableClass5), null)]
        [InlineData(typeof(NullableClass6), null)]
        [InlineData(typeof(NullableClass7), 1)] //default not nullable
        [InlineData(typeof(NullableClass8), 2)] //default nullable
        [InlineData(typeof(NullableClass9), null)]
        [InlineData(typeof(NullableClass10), null)]
        [InlineData(typeof(NullableClass11), 1)] //default not nullable
        [InlineData(typeof(NullableClass12), null)]
        [InlineData(typeof(NullableClass13), 1)] //default not nullable
        [InlineData(typeof(NullableClass14), 2)] //default nullable
        [InlineData(typeof(NullableClass15), null)]
        public void VerifyTestClass(Type type, int? nullableContext)
        {
            var actualHasNullableContext = type.CustomAttributes.FirstOrDefault(
                x => x.AttributeType.Name == "NullableContextAttribute");
            if (nullableContext == null) {
                actualHasNullableContext.ShouldBeNull();
            } else {
                actualHasNullableContext.ShouldNotBeNull();
                actualHasNullableContext.ConstructorArguments[0].Value.ShouldBe(nullableContext);
            }
        }

        [Theory]
        [InlineData(typeof(NullableClass1), "Field1", false, true)]
        [InlineData(typeof(NullableClass1), "Field2", false, true)]
        [InlineData(typeof(NullableClass1), "Field3", false, false)]
        [InlineData(typeof(NullableClass1), "Field4", false, false)]
        [InlineData(typeof(NullableClass1), "Field5", false, false)]
        [InlineData(typeof(NullableClass1), "Field6", false, false)]
        [InlineData(typeof(NullableClass1), "Field7", false, false)]
        [InlineData(typeof(NullableClass1), "Field8", false, false)]
        [InlineData(typeof(NullableClass2), "Field1", false, false)]
        [InlineData(typeof(NullableClass2), "Field2", false, false)]
        [InlineData(typeof(NullableClass2), "Field3", false, false)]
        [InlineData(typeof(NullableClass2), "Field4", false, false)]
        [InlineData(typeof(NullableClass2), "Field5", false, false)]
        [InlineData(typeof(NullableClass2), "Field6", false, false)]
        [InlineData(typeof(NullableClass2), "Field7", false, false)]
        [InlineData(typeof(NullableClass2), "Field8", false, false)]
        [InlineData(typeof(NullableClass2), "Field9", false, true)]
        [InlineData(typeof(NullableClass2), "Field10", false, true)]
        [InlineData(typeof(NullableClass5), "Test", false, true)]
        [InlineData(typeof(NullableClass6), "Field1", false, true)]
        [InlineData(typeof(NullableClass6), "Field2", false, true)]
        [InlineData(typeof(NullableClass7), "Field1", false, false)]
        [InlineData(typeof(NullableClass7), "Field2", false, false)]
        [InlineData(typeof(NullableClass7), "Field3", false, true)]
        [InlineData(typeof(NullableClass8), "Field1", false, false)]
        [InlineData(typeof(NullableClass8), "Field2", false, false)]
        [InlineData(typeof(NullableClass8), "Field3", false, true)]
        [InlineData(typeof(NullableClass8), "Field4", false, false)]
        [InlineData(typeof(NullableClass9), "Field1", true, true)]
        [InlineData(typeof(NullableClass10), "Field1", true, true)]
        [InlineData(typeof(NullableClass11), "Field1", false, false)]
        [InlineData(typeof(NullableClass11), "Field2", true, false)]
        [InlineData(typeof(NullableClass12), "Field1", false, true)]
        [InlineData(typeof(NullableClass12), "Field2", true, false)]
        [InlineData(typeof(NullableClass12), "Field3", true, false)]
        [InlineData(typeof(NullableClass12), "Field4", false, true)]
        [InlineData(typeof(NullableClass13), "Field1", false, false)]
        [InlineData(typeof(NullableClass13), "Field2", false, false)]
        [InlineData(typeof(NullableClass14), "Field1", false, false)]
        [InlineData(typeof(NullableClass14), "Field2", false, false)]
        [InlineData(typeof(NullableClass15), "Field1", false, true)]
        [InlineData(typeof(NullableClass15), "Field2", true, false)]
        [InlineData(typeof(NullableClass15), "Field3", true, false)]
        [InlineData(typeof(NullableClass15), "Field4", false, true)]
        public void VerifyTestMethod(Type type, string methodName, bool hasNullable, bool hasNullableContext)
        {
            var method = type.GetMethod(methodName);
            var actualHasNullableOnMethod = method.CustomAttributes.Any(
                x => x.AttributeType.Name == "NullableAttribute");
            var actualHasNullableContext = method.CustomAttributes.Any(
                x => x.AttributeType.Name == "NullableContextAttribute");
            var actualHasNullable = method.ReturnParameter.CustomAttributes.Any(
                x => x.AttributeType.Name == "NullableAttribute");
            actualHasNullableOnMethod.ShouldBe(false); //should not be possible to apply the attribute here
            actualHasNullable.ShouldBe(hasNullable);
            actualHasNullableContext.ShouldBe(hasNullableContext);
        }

        [Theory]
        [InlineData(typeof(NullableClass9), "Field1", "arg1", false)]
        [InlineData(typeof(NullableClass9), "Field1", "arg2", false)]
        [InlineData(typeof(NullableClass10), "Field1", "arg1", false)]
        [InlineData(typeof(NullableClass10), "Field1", "arg2", false)]
        [InlineData(typeof(NullableClass11), "Field2", "arg1", false)]
        [InlineData(typeof(NullableClass11), "Field2", "arg2", false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg1", false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg2", true)]
        [InlineData(typeof(NullableClass13), "Field2", "arg3", false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg4", false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg5", false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg1", false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg2", true)]
        [InlineData(typeof(NullableClass14), "Field2", "arg3", false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg4", false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg5", false)]
        public void VerifyTestArgument(Type type, string methodName, string argumentName, bool hasNullable)
        {
            var method = type.GetMethod(methodName);
            var argument = method.GetParameters().Single(x => x.Name == argumentName);
            var actualHasNullable = argument.CustomAttributes.Any(
                x => x.AttributeType.Name == "NullableAttribute");
            actualHasNullable.ShouldBe(hasNullable);
        }

        [Theory]
        [InlineData(typeof(NullableClass1), "Field1", true)]
        [InlineData(typeof(NullableClass1), "Field2", false)]
        [InlineData(typeof(NullableClass1), "Field3", false)]
        [InlineData(typeof(NullableClass1), "Field4", true)]
        [InlineData(typeof(NullableClass1), "Field5", true)]
        [InlineData(typeof(NullableClass1), "Field6", false)]
        [InlineData(typeof(NullableClass1), "Field7", false)]
        [InlineData(typeof(NullableClass1), "Field8", true)]
        [InlineData(typeof(NullableClass2), "Field1", true)]
        [InlineData(typeof(NullableClass2), "Field2", false)]
        [InlineData(typeof(NullableClass2), "Field3", false)]
        [InlineData(typeof(NullableClass2), "Field4", true)]
        [InlineData(typeof(NullableClass2), "Field5", true)]
        [InlineData(typeof(NullableClass2), "Field6", false)]
        [InlineData(typeof(NullableClass2), "Field7", true)]
        [InlineData(typeof(NullableClass2), "Field8", true)]
        [InlineData(typeof(NullableClass2), "Field9", false)]
        [InlineData(typeof(NullableClass2), "Field10", true)]
        [InlineData(typeof(NullableClass5), "Test", false)]
        [InlineData(typeof(NullableClass6), "Field1", false)]
        [InlineData(typeof(NullableClass6), "Field2", true)]
        [InlineData(typeof(NullableClass7), "Field1", false)]
        [InlineData(typeof(NullableClass7), "Field2", false)]
        [InlineData(typeof(NullableClass7), "Field3", true)]
        [InlineData(typeof(NullableClass8), "Field1", true)]
        [InlineData(typeof(NullableClass8), "Field2", true)]
        [InlineData(typeof(NullableClass8), "Field3", false)]
        [InlineData(typeof(NullableClass8), "Field4", false)]
        [InlineData(typeof(NullableClass9), "Field1", false)]
        [InlineData(typeof(NullableClass10), "Field1", true)]
        [InlineData(typeof(NullableClass11), "Field1", false)]
        [InlineData(typeof(NullableClass11), "Field2", true)]
        [InlineData(typeof(NullableClass12), "Field1", false)]
        [InlineData(typeof(NullableClass12), "Field2", true)]
        [InlineData(typeof(NullableClass12), "Field3", true)]
        [InlineData(typeof(NullableClass12), "Field4", true)]
        [InlineData(typeof(NullableClass13), "Field1", false)]
        [InlineData(typeof(NullableClass13), "Field2", false)]
        [InlineData(typeof(NullableClass14), "Field1", true)]
        [InlineData(typeof(NullableClass14), "Field2", true)]
        [InlineData(typeof(NullableClass15), "Field1", false)]
        [InlineData(typeof(NullableClass15), "Field2", true)]
        [InlineData(typeof(NullableClass15), "Field3", true)]
        [InlineData(typeof(NullableClass15), "Field4", true)]
        public void Method(Type type, string methodName, bool expected)
        {
            var method = type.GetMethod(methodName);
            var nullable = TestGraphType.Instance.CheckNullability(method);
            nullable.ShouldBe(expected);
        }

        [Theory]
        [InlineData(typeof(NullableClass9), "Field1", "arg1", true)]
        [InlineData(typeof(NullableClass9), "Field1", "arg2", true)]
        [InlineData(typeof(NullableClass10), "Field1", "arg1", false)]
        [InlineData(typeof(NullableClass10), "Field1", "arg2", false)]
        [InlineData(typeof(NullableClass11), "Field2", "arg1", false)]
        [InlineData(typeof(NullableClass11), "Field2", "arg2", false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg1", false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg2", true)]
        [InlineData(typeof(NullableClass13), "Field2", "arg3", false)]
        [InlineData(typeof(NullableClass13), "Field2", "arg4", true)]
        [InlineData(typeof(NullableClass13), "Field2", "arg5", true)]
        [InlineData(typeof(NullableClass14), "Field2", "arg1", true)]
        [InlineData(typeof(NullableClass14), "Field2", "arg2", false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg3", false)]
        [InlineData(typeof(NullableClass14), "Field2", "arg4", true)]
        [InlineData(typeof(NullableClass14), "Field2", "arg5", false)]
        public void Argument(Type type, string methodName, string argumentName, bool expected)
        {
            var method = type.GetMethod(methodName);
            var argument = method.GetParameters().Single(x => x.Name == argumentName);
            var nullable = TestGraphType.Instance.CheckNullability(method, argument);
            nullable.ShouldBe(expected);
        }

        private class TestGraphType : DIObjectGraphType<TestClass>
        {
            public static TestGraphType Instance = new();
            public bool CheckNullability(MethodInfo method) => base.GetNullability(method);
            public bool CheckNullability(MethodInfo method, ParameterInfo parameter) => base.GetNullability(method, parameter);
        }

        private class TestClass : DIObjectGraphBase { }
    }
}
