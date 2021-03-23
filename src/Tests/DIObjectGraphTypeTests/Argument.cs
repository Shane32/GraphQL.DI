using System.ComponentModel;
using GraphQL.DI;
using GraphQL.Types;
using Shouldly;
using Xunit;

namespace DIObjectGraphTypeTests
{
    public class Argument : DIObjectGraphTypeTestBase
    {
        [Fact]
        public void NonNullValue()
        {
            Configure<CNonNullValue, object>();
            VerifyFieldArgument("Field1", "arg", false, 2);
            VerifyField("Field1", false, false, 2);
            Verify(false);
        }

        public class CNonNullValue : DIObjectGraphBase
        {
            public static int Field1(int arg) => arg;
        }

        [Fact]
        public void NullableValue()
        {
            Configure<CNullableValue, object>();
            VerifyFieldArgument("Field1", "arg", true, (int?)2);
            VerifyField("Field1", true, false, 2);
            Verify(false);
        }

        public class CNullableValue : DIObjectGraphBase
        {
            public static int? Field1(int? arg) => arg;
        }

        [Fact]
        public void NullableValueExplicit()
        {
            Configure<CNullableValueExplicit, object>();
            VerifyFieldArgument("Field1", "arg", true, (int?)2);
            VerifyField("Field1", true, false, 2);
            Verify(false);
        }

        public class CNullableValueExplicit : DIObjectGraphBase
        {
            public static int? Field1([Optional] int arg) => arg;
        }

        [Fact]
        public void NullableObject()
        {
            Configure<CNullableObject, object>();
            VerifyFieldArgument("Field1", "arg", true, "hello");
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CNullableObject : DIObjectGraphBase
        {
            public static string Field1([Optional] string arg) => arg;
        }

        [Fact]
        public void NonNullableObject()
        {
            Configure<CNonNullableObject, object>();
            VerifyFieldArgument("Field1", "arg", false, "hello");
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CNonNullableObject : DIObjectGraphBase
        {
            public static string Field1([Required] string arg) => arg;
        }

        [Fact]
        public void Name()
        {
            Configure<CName, object>();
            VerifyFieldArgument("Field1", "AltName", true, "hello");
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CName : DIObjectGraphBase
        {
            public static string Field1([Name("AltName")] string arg) => arg;
        }

        [Fact]
        public void Description()
        {
            Configure<CDescription, object>();
            var arg = VerifyFieldArgument("Field1", "arg", true, "hello");
            arg.Description.ShouldBe("TestDescription");
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CDescription : DIObjectGraphBase
        {
            public static string Field1([Description("TestDescription")] string arg) => arg;
        }

        [Fact]
        public void GraphType()
        {
            Configure<CGraphType, object>();
            VerifyFieldArgument("Field1", "arg", typeof(StringGraphType), "hello");
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CGraphType : DIObjectGraphBase
        {
            public static string Field1([GraphType(typeof(StringGraphType))] string arg) => arg;
        }

    }
}
