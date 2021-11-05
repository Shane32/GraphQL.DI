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

namespace AutoInputObjectGraphTypeTests
{
    public class Property
    {
        private readonly IInputObjectGraphType _graphType = new AutoInputObjectGraphType<CDefault>();

        public class CDefault
        {
            public string? Field1 { get; set; }
            public string Field2 { get; set; } = null!;
            public int? Field3 { get; set; }
            public int Field4 { get; set; }
            public List<string?> Field5 { get; set; } = null!;
            public List<string>? Field6 { get; set; }
            public string[] Field7 { get; set; } = null!;
            [Required]
            public string? Field8 { get; set; }
            [Optional]
            public string Field9 { get; set; } = null!;
            [OptionalList]
            public List<string> Field10 { get; set; } = null!;
            [RequiredList]
            public List<string?>? Field11 { get; set; }
            [GraphType(typeof(GuidGraphType))]
            public object? Field12 { get; set; }
            [Name("Field14")]
            public string? Field13 { get; set; }
            [Ignore]
            public string? Field15 { get; set; }
            [DefaultValue(5)]
            public int Field16 { get; set; }
            [Description("Example")]
            public int Field17 { get; set; }
            [Obsolete("Example")]
            public int Field18 { get; set; }
            [Id]
            public int Field19 { get; set; }
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
        }

        public class Class1<T> { }

        [Theory]
        [InlineData("Field1", typeof(string), true)]
        [InlineData("Field2", typeof(string), false)]
        [InlineData("Field3", typeof(int), true)]
        [InlineData("Field4", typeof(int), false)]
        [InlineData("Field5", typeof(string), true, true, false)]
        [InlineData("Field6", typeof(string), false, true, true)]
        [InlineData("Field7", typeof(string), false, true, false)]
        [InlineData("Field8", typeof(string), false)]
        [InlineData("Field9", typeof(string), true)]
        [InlineData("Field10", typeof(string), false, true, true)]
        [InlineData("Field11", typeof(string), true, true, false)]
        [InlineData("Field12", null, true, false, false, typeof(GuidGraphType))]
        [InlineData("Field13", null, true)]
        [InlineData("Field14", typeof(string), true)]
        [InlineData("Field15", null, true)]
        [InlineData("Field16", typeof(int), false)]
        [InlineData("Field17", typeof(int), false)]
        [InlineData("Field18", typeof(int), false)]
        [InlineData("Field19", null, true, false, false, typeof(NonNullGraphType<IdGraphType>))]
        [InlineData("Field20", null, true)]
        [InlineData("Field21", null, true)]
        [InlineData("Field22", null, true)]
        [InlineData("Field23", typeof(int), false)]
        [InlineData("Field24", typeof(object), true, true, true)]
        [InlineData("Field25", typeof(object), true, true, false)]
        [InlineData("Field26", typeof(Class1<int>), false)]
        [InlineData("Field27", typeof(int), true, true, true)]
        public void Types(string name, Type? fieldType, bool isNullable, bool isList = false, bool listIsNullable = false, Type? graphType = null)
        {
            if (graphType == null && fieldType != null) {
                graphType = fieldType.GetGraphTypeFromType(isNullable, TypeMappingMode.InputType);
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
            field.Resolver.ShouldBe(null);
        }

        [Fact]
        public void DefaultValue()
        {
            var field = _graphType.Fields.Find("Field16");
            field.ShouldNotBeNull();
            field.DefaultValue.ShouldBeOfType<int>().ShouldBe(5);
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
