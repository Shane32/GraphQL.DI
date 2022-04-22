using System.ComponentModel;
using System.Threading;
using GraphQL;
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
        public void NonNullableObject2()
        {
            Configure<CNonNullableObject2, object>();
            VerifyFieldArgument("Field1", "arg", false, "hello");
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CNonNullableObject2 : DIObjectGraphBase
        {
            public static string Field1([System.ComponentModel.DataAnnotations.Required] string arg) => arg;
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
            VerifyFieldArgument("Field1", "arg", typeof(IdGraphType), "hello");
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CGraphType : DIObjectGraphBase
        {
            public static string Field1([InputType(typeof(IdGraphType))] string arg) => arg;
        }

        [Fact]
        public void GraphTypeInherited()
        {
            Configure<CGraphTypeInherited, object>();
            VerifyFieldArgument("Field1", "arg", typeof(IdGraphType), "hello");
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CGraphTypeInherited : DIObjectGraphBase
        {
            public static string Field1([MyIdGraphType] string arg) => arg;
        }

        public class MyIdGraphTypeAttribute : InputTypeAttribute
        {
            public MyIdGraphTypeAttribute() : base(typeof(IdGraphType)) { }
        }

        [Fact]
        public void Id()
        {
            Configure<CIdGraphType, object>();
            VerifyFieldArgument("Field1", "arg", typeof(IdGraphType), "hello");
            VerifyField("Field1", true, false, "hello");
            Verify(false);
        }

        public class CIdGraphType : DIObjectGraphBase
        {
            public static string Field1([Id] string arg) => arg;
        }

        [Fact]
        public void IdNonNull()
        {
            Configure<CIdGraphType2, object>();
            VerifyFieldArgument("Field1", "arg", typeof(NonNullGraphType<IdGraphType>), "3");
            VerifyField("Field1", true, false, "3");
            Verify(false);
        }

        public class CIdGraphType2 : DIObjectGraphBase
        {
            public static string Field1([Id] int arg) => arg.ToString();
        }

        [Fact]
        public void CancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            cts.Cancel();
            Configure<CCancellationToken, object>();
            _contextMock.SetupGet(x => x.CancellationToken).Returns(token).Verifiable();
            VerifyField("Field1", true, false, "hello + True");
            Verify(false);
        }

        public class CCancellationToken : DIObjectGraphBase
        {
            public static string Field1(CancellationToken cancellationToken) => "hello + " + cancellationToken.IsCancellationRequested;
        }

        [Fact]
        public void DefaultValue()
        {
            Configure<CDefaultValue, object>();
            VerifyFieldArgument("Field1", "arg1", false, 2);
            VerifyField("Field1", false, false, 2);
            VerifyFieldArgument<int>("Field2", "arg2", false);
            VerifyField("Field2", false, false, 5);
            Verify(false);
        }

        public class CDefaultValue : DIObjectGraphBase
        {
            public static int Field1(int arg1 = 4) => arg1;
            public static int Field2(int arg2 = 5) => arg2;
        }

    }
}
