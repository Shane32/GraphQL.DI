#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using GraphQL;
using GraphQL.DI;
using GraphQL.Types;
using Moq;
using Shouldly;
using Xunit;

namespace AutoObjectGraphTypeTests
{
    public class Property
    {
        private readonly IObjectGraphType _graphType = new AutoObjectGraphType<CDefault>();

        public class CDefault
        {
            public string? Field1 { get; set; } = "1";
            public string Field2 { get; set; } = "2";
            public int? Field3 { get; set; } = 3;
            public int Field4 { get; set; } = 4;
            public List<string?> Field5 { get; set; } = null!;
            public List<string>? Field6 { get; set; }
            public string[] Field7 { get; set; } = null!;
            [Required]
            public string? Field8 { get; set; } = "8";
            [Optional]
            public string Field9 { get; set; } = "9";
            [OptionalList]
            public List<string> Field10 { get; set; } = null!;
            [RequiredList]
            public List<string?>? Field11 { get; set; }
            [GraphType(typeof(GuidGraphType))]
            public object? Field12 { get; set; } = 12f;
            [Name("Field14")]
            public string? Field13 { get; set; } = "13";
            [Ignore]
            public string? Field15 { get; set; } = "15";
            [DefaultValue(5)]
            public int Field16 { get; set; } = 16;
            [Description("Example")]
            public int Field17 { get; set; } = 17;
            [Obsolete("Example")]
            public int Field18 { get; set; } = 18;
            [Id]
            public int Field19 { get; set; } = 19;
            public int Field20 => 3;
            [Name(null!)]
            public int Field21 { get; set; }
            public int Field22() => 3;
            [Metadata("test", 3)]
            [Metadata("test2", 5)]
            public int Field23 { get; set; }
            public IEnumerable? Field24 { get; set; }
            public ICollection Field25 { get; set; } = null!;
            public Class1<int> Field26 { get; set; } = null!;
            public int?[]? Field27 { get; set; }
            public int Field28 { set { } }
        }

        public class Class1<T> { }

        [Theory]
        [InlineData("Field1", typeof(string), "1", true)]
        [InlineData("Field2", typeof(string), "2", false)]
        [InlineData("Field3", typeof(int), 3, true)]
        [InlineData("Field4", typeof(int), 4, false)]
        [InlineData("Field5", typeof(string), null, true, true, false)]
        [InlineData("Field6", typeof(string), null, false, true, true)]
        [InlineData("Field7", typeof(string), null, false, true, false)]
        [InlineData("Field8", typeof(string), "8", false)]
        [InlineData("Field9", typeof(string), "9", true)]
        [InlineData("Field10", typeof(string), null, false, true, true)]
        [InlineData("Field11", typeof(string), null, true, true, false)]
        [InlineData("Field12", null, 12f, true, false, false, typeof(GuidGraphType))]
        [InlineData("Field13", null, null, true)]
        [InlineData("Field14", typeof(string), "13", true)]
        [InlineData("Field15", null, "15", true)]
        [InlineData("Field16", typeof(int), 16, false)]
        [InlineData("Field17", typeof(int), 17, false)]
        [InlineData("Field18", typeof(int), 18, false)]
        [InlineData("Field19", null, 19, true, false, false, typeof(NonNullGraphType<IdGraphType>))]
        [InlineData("Field20", typeof(int), 3, false)]
        [InlineData("Field21", null, 0, true)]
        [InlineData("Field22", typeof(int), 3, false)]
        [InlineData("Field23", typeof(int), 0, false)]
        [InlineData("Field24", typeof(object), null, true, true, true)]
        [InlineData("Field25", typeof(object), null, true, true, false)]
        [InlineData("Field26", typeof(Class1<int>), null, false)]
        [InlineData("Field27", typeof(int), null, true, true, true)]
        [InlineData("Field28", null, null, true)]
        public void Types(string name, Type? fieldType, object expectedResult, bool isNullable, bool isList = false, bool listIsNullable = false, Type? graphType = null)
        {
            if (graphType == null && fieldType != null) {
                graphType = fieldType.GetGraphTypeFromType(isNullable, TypeMappingMode.OutputType);
                if (isList) {
                    graphType = typeof(ListGraphType<>).MakeGenericType(graphType);
                    if (!listIsNullable) {
                        graphType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
                    }
                }
            }
            var field = _graphType.Fields.Find(name);
            if (graphType == null) {
                field.ShouldBeNull();
                return;
            }
            field.ShouldNotBeNull();
            field.Type.ShouldBe(graphType);
            var obj = new CDefault();
            var mockResolveFieldContext = new Mock<IResolveFieldContext>(MockBehavior.Loose);
            mockResolveFieldContext.Setup(x => x.Source).Returns(obj);
            field.Resolver.ShouldNotBeNull();
            var ret = field.Resolver.Resolve(mockResolveFieldContext.Object);
            ret.ShouldBe(expectedResult);
        }

        [Fact]
        public void Description()
        {
            var field = _graphType.Fields.Find("Field17");
            field.ShouldNotBeNull();
            field.Description.ShouldBe("Example");
        }

        [Fact]
        public void Obsolete()
        {
            var field = _graphType.Fields.Find("Field18");
            field.ShouldNotBeNull();
            field.DeprecationReason.ShouldBe("Example");
        }

        [Fact]
        public void Metadata()
        {
            var field = _graphType.Fields.Find("Field23");
            field.ShouldNotBeNull();
            field.Metadata.ShouldNotBeNull();
            field.Metadata.ShouldContainKeyAndValue("test", 3);
            field.Metadata.ShouldContainKeyAndValue("test2", 5);
        }

        [Theory]
        [InlineData("Field1", "Field1")]
        [InlineData("Field14", "Field13")]
        public void Expression(string graphFieldName, string classPropertyName)
        {
            var field = _graphType.Fields.Find(graphFieldName);
            field.ShouldNotBeNull();
            field.Metadata.ShouldNotBeNull();
            field.Metadata.ShouldContainKey("ORIGINAL_EXPRESSION_PROPERTY_NAME");
            var actual = field.Metadata["ORIGINAL_EXPRESSION_PROPERTY_NAME"]
                .ShouldBeOfType<string>();
            actual.ShouldBe(classPropertyName);
        }

        [Fact]
        public void ExpressionCorrectlyResolves()
        {
            var graph = new AutoInputObjectGraphType<Class2>();
            var dic = new Dictionary<string, object?> {
                { "Field2", "value" }
            };
            var actual = (Class2)graph.ParseDictionary(dic);
            actual.Field1.ShouldBe("value");
        }

        public class Class2
        {
            [Name("Field2")]
            public string? Field1 { get; set; }
        }
    }
}
