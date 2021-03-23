#nullable enable
using System.Threading.Tasks;
using GraphQL.DataLoader;
using GraphQL.DI;

namespace Tests.NullabilityTestClasses
{
    public class NullableClass1
    {
        public static string? Field1() => "field1";
        [Required]
        public static string? Field2() => "field2";
        public static int Field3() => 1;
        [Optional]
        public static int Field4() => 1;
        public static int? Field5() => 1;
        [Required]
        public static int? Field6() => 1;
        public static string Field7() => "field1";
        [Optional]
        public static string Field8() => "field2";
    }

    public class NullableClass2
    {
        public static string? Field1() => "field1";
        [Required]
        public static string? Field2() => "field2";
        public static int Field3() => 1;
        [Optional]
        public static int Field4() => 1;
        public static int? Field5() => 1;
        [Required]
        public static int? Field6() => 1;
        public static string? Field7() => "test";
        public static string? Field8() => "test";
        public static string Field9() => "field1";
        [Optional]
        public static string Field10() => "field2";
    }

    public class NullableClass5
    {
        public static string Test() => "test";
    }

    public class NullableClass6
    {
        public static string Field1() => "test";
        public static string? Field2() => "test";
    }

    public class NullableClass7
    {
        public static string Field1() => "test";
        public static string Field2() => "test";
        public static string? Field3() => "test";
    }

    public class NullableClass8
    {
        public static string? Field1() => "test";
        public static string? Field2() => "test";
        public static string Field3() => "test";
        public static int Field4() => 3;
    }

    public class NullableClass9
    {
        public static string Field1(string? arg1, string? arg2) => "test";
    }

    public class NullableClass10
    {
        public static string? Field1(string arg1, string arg2) => "test";
    }

    public class NullableClass11
    {
        public static string Field1() => "test";
        public static string? Field2(string arg1, string arg2) => "test";
    }

    public class NullableClass12
    {
        public static IDataLoaderResult<string> Field1() => null!;
        public static IDataLoaderResult<string?> Field2() => null!;
        public static IDataLoaderResult<string>? Field3() => null!;
        public static IDataLoaderResult<string?>? Field4() => null!;
    }

    public class NullableClass13
    {
        public static string Field1() => "test";
        public static string Field2(string arg1, string? arg2, int arg3, int? arg4, [Optional] string arg5) => "test";
    }

    public class NullableClass14
    {
        public static string? Field1() => "test";
        public static string? Field2(string? arg1, string arg2, int arg3, int? arg4, [Required] string? arg5) => "test";
    }

    public class NullableClass15
    {
        public static Task<string> Field1() => null!;
        public static Task<string?> Field2() => null!;
        public static Task<string>? Field3() => null!;
        public static Task<string?>? Field4() => null!;
    }
}
