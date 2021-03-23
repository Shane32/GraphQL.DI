using System;
using System.Linq;
using System.Reflection;
using GraphQL.DI;
using Shouldly;
using Tests.NullabilityTestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DIObjectGraphTypeTests
{
    [TestClass]
    public class Nullable
    {
        //Verify... methods verify that .NET is building the classes
        //  with the expected attributes on them. Failure is not an
        //  error, but simply indicates that the Method and Argument
        //  tests may not be testing the anticipated scenarios
        //Method and Argument should always pass
        [DataTestMethod]
        [DataRow(typeof(NullableClass1), 1)] //default not nullable
        [DataRow(typeof(NullableClass2), 2)] //default nullable
        [DataRow(typeof(NullableClass5), null)]
        [DataRow(typeof(NullableClass6), null)]
        [DataRow(typeof(NullableClass7), 1)] //default not nullable
        [DataRow(typeof(NullableClass8), 2)] //default nullable
        [DataRow(typeof(NullableClass9), null)]
        [DataRow(typeof(NullableClass10), null)]
        [DataRow(typeof(NullableClass11), 1)] //default not nullable
        [DataRow(typeof(NullableClass12), null)]
        [DataRow(typeof(NullableClass13), 1)] //default not nullable
        [DataRow(typeof(NullableClass14), 2)] //default nullable
        [DataRow(typeof(NullableClass15), null)]
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

        [DataTestMethod]
        [DataRow(typeof(NullableClass1), "Field1", false, true)]
        [DataRow(typeof(NullableClass1), "Field2", false, true)]
        [DataRow(typeof(NullableClass1), "Field3", false, false)]
        [DataRow(typeof(NullableClass1), "Field4", false, false)]
        [DataRow(typeof(NullableClass1), "Field5", false, false)]
        [DataRow(typeof(NullableClass1), "Field6", false, false)]
        [DataRow(typeof(NullableClass1), "Field7", false, false)]
        [DataRow(typeof(NullableClass1), "Field8", false, false)]
        [DataRow(typeof(NullableClass2), "Field1", false, false)]
        [DataRow(typeof(NullableClass2), "Field2", false, false)]
        [DataRow(typeof(NullableClass2), "Field3", false, false)]
        [DataRow(typeof(NullableClass2), "Field4", false, false)]
        [DataRow(typeof(NullableClass2), "Field5", false, false)]
        [DataRow(typeof(NullableClass2), "Field6", false, false)]
        [DataRow(typeof(NullableClass2), "Field7", false, false)]
        [DataRow(typeof(NullableClass2), "Field8", false, false)]
        [DataRow(typeof(NullableClass2), "Field9", false, true)]
        [DataRow(typeof(NullableClass2), "Field10", false, true)]
        [DataRow(typeof(NullableClass5), "Test", false, true)]
        [DataRow(typeof(NullableClass6), "Field1", false, true)]
        [DataRow(typeof(NullableClass6), "Field2", false, true)]
        [DataRow(typeof(NullableClass7), "Field1", false, false)]
        [DataRow(typeof(NullableClass7), "Field2", false, false)]
        [DataRow(typeof(NullableClass7), "Field3", false, true)]
        [DataRow(typeof(NullableClass8), "Field1", false, false)]
        [DataRow(typeof(NullableClass8), "Field2", false, false)]
        [DataRow(typeof(NullableClass8), "Field3", false, true)]
        [DataRow(typeof(NullableClass8), "Field4", false, false)]
        [DataRow(typeof(NullableClass9), "Field1", true, true)]
        [DataRow(typeof(NullableClass10), "Field1", true, true)]
        [DataRow(typeof(NullableClass11), "Field1", false, false)]
        [DataRow(typeof(NullableClass11), "Field2", true, false)]
        [DataRow(typeof(NullableClass12), "Field1", false, true)]
        [DataRow(typeof(NullableClass12), "Field2", true, false)]
        [DataRow(typeof(NullableClass12), "Field3", true, false)]
        [DataRow(typeof(NullableClass12), "Field4", false, true)]
        [DataRow(typeof(NullableClass13), "Field1", false, false)]
        [DataRow(typeof(NullableClass13), "Field2", false, false)]
        [DataRow(typeof(NullableClass14), "Field1", false, false)]
        [DataRow(typeof(NullableClass14), "Field2", false, false)]
        [DataRow(typeof(NullableClass15), "Field1", false, true)]
        [DataRow(typeof(NullableClass15), "Field2", true, false)]
        [DataRow(typeof(NullableClass15), "Field3", true, false)]
        [DataRow(typeof(NullableClass15), "Field4", false, true)]
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

        [DataTestMethod]
        [DataRow(typeof(NullableClass9), "Field1", "arg1", false)]
        [DataRow(typeof(NullableClass9), "Field1", "arg2", false)]
        [DataRow(typeof(NullableClass10), "Field1", "arg1", false)]
        [DataRow(typeof(NullableClass10), "Field1", "arg2", false)]
        [DataRow(typeof(NullableClass11), "Field2", "arg1", false)]
        [DataRow(typeof(NullableClass11), "Field2", "arg2", false)]
        [DataRow(typeof(NullableClass13), "Field2", "arg1", false)]
        [DataRow(typeof(NullableClass13), "Field2", "arg2", true)]
        [DataRow(typeof(NullableClass13), "Field2", "arg3", false)]
        [DataRow(typeof(NullableClass13), "Field2", "arg4", false)]
        [DataRow(typeof(NullableClass13), "Field2", "arg5", false)]
        [DataRow(typeof(NullableClass14), "Field2", "arg1", false)]
        [DataRow(typeof(NullableClass14), "Field2", "arg2", true)]
        [DataRow(typeof(NullableClass14), "Field2", "arg3", false)]
        [DataRow(typeof(NullableClass14), "Field2", "arg4", false)]
        [DataRow(typeof(NullableClass14), "Field2", "arg5", false)]
        public void VerifyTestArgument(Type type, string methodName, string argumentName, bool hasNullable)
        {
            var method = type.GetMethod(methodName);
            var argument = method.GetParameters().Single(x => x.Name == argumentName);
            var actualHasNullable = argument.CustomAttributes.Any(
                x => x.AttributeType.Name == "NullableAttribute");
            actualHasNullable.ShouldBe(hasNullable);
        }

        [DataTestMethod]
        [DataRow(typeof(NullableClass1), "Field1", true)]
        [DataRow(typeof(NullableClass1), "Field2", false)]
        [DataRow(typeof(NullableClass1), "Field3", false)]
        [DataRow(typeof(NullableClass1), "Field4", true)]
        [DataRow(typeof(NullableClass1), "Field5", true)]
        [DataRow(typeof(NullableClass1), "Field6", false)]
        [DataRow(typeof(NullableClass1), "Field7", false)]
        [DataRow(typeof(NullableClass1), "Field8", true)]
        [DataRow(typeof(NullableClass2), "Field1", true)]
        [DataRow(typeof(NullableClass2), "Field2", false)]
        [DataRow(typeof(NullableClass2), "Field3", false)]
        [DataRow(typeof(NullableClass2), "Field4", true)]
        [DataRow(typeof(NullableClass2), "Field5", true)]
        [DataRow(typeof(NullableClass2), "Field6", false)]
        [DataRow(typeof(NullableClass2), "Field7", true)]
        [DataRow(typeof(NullableClass2), "Field8", true)]
        [DataRow(typeof(NullableClass2), "Field9", false)]
        [DataRow(typeof(NullableClass2), "Field10", true)]
        [DataRow(typeof(NullableClass5), "Test", false)]
        [DataRow(typeof(NullableClass6), "Field1", false)]
        [DataRow(typeof(NullableClass6), "Field2", true)]
        [DataRow(typeof(NullableClass7), "Field1", false)]
        [DataRow(typeof(NullableClass7), "Field2", false)]
        [DataRow(typeof(NullableClass7), "Field3", true)]
        [DataRow(typeof(NullableClass8), "Field1", true)]
        [DataRow(typeof(NullableClass8), "Field2", true)]
        [DataRow(typeof(NullableClass8), "Field3", false)]
        [DataRow(typeof(NullableClass8), "Field4", false)]
        [DataRow(typeof(NullableClass9), "Field1", false)]
        [DataRow(typeof(NullableClass10), "Field1", true)]
        [DataRow(typeof(NullableClass11), "Field1", false)]
        [DataRow(typeof(NullableClass11), "Field2", true)]
        [DataRow(typeof(NullableClass12), "Field1", false)]
        [DataRow(typeof(NullableClass12), "Field2", true)]
        [DataRow(typeof(NullableClass12), "Field3", true)]
        [DataRow(typeof(NullableClass12), "Field4", true)]
        [DataRow(typeof(NullableClass13), "Field1", false)]
        [DataRow(typeof(NullableClass13), "Field2", false)]
        [DataRow(typeof(NullableClass14), "Field1", true)]
        [DataRow(typeof(NullableClass14), "Field2", true)]
        [DataRow(typeof(NullableClass15), "Field1", false)]
        [DataRow(typeof(NullableClass15), "Field2", true)]
        [DataRow(typeof(NullableClass15), "Field3", true)]
        [DataRow(typeof(NullableClass15), "Field4", true)]
        public void Method(Type type, string methodName, bool expected)
        {
            var method = type.GetMethod(methodName);
            var nullable = TestGraphType.Instance.CheckNullability(method);
            nullable.ShouldBe(expected);
        }

        [DataTestMethod]
        [DataRow(typeof(NullableClass9), "Field1", "arg1", true)]
        [DataRow(typeof(NullableClass9), "Field1", "arg2", true)]
        [DataRow(typeof(NullableClass10), "Field1", "arg1", false)]
        [DataRow(typeof(NullableClass10), "Field1", "arg2", false)]
        [DataRow(typeof(NullableClass11), "Field2", "arg1", false)]
        [DataRow(typeof(NullableClass11), "Field2", "arg2", false)]
        [DataRow(typeof(NullableClass13), "Field2", "arg1", false)]
        [DataRow(typeof(NullableClass13), "Field2", "arg2", true)]
        [DataRow(typeof(NullableClass13), "Field2", "arg3", false)]
        [DataRow(typeof(NullableClass13), "Field2", "arg4", true)]
        [DataRow(typeof(NullableClass13), "Field2", "arg5", true)]
        [DataRow(typeof(NullableClass14), "Field2", "arg1", true)]
        [DataRow(typeof(NullableClass14), "Field2", "arg2", false)]
        [DataRow(typeof(NullableClass14), "Field2", "arg3", false)]
        [DataRow(typeof(NullableClass14), "Field2", "arg4", true)]
        [DataRow(typeof(NullableClass14), "Field2", "arg5", false)]
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
