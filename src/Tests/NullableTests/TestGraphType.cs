using System.Reflection;
using GraphQL.DI;

namespace NullableTests
{
    public class TestGraphType : DIObjectGraphType<TestClass>
    {
        public static TestGraphType Instance = new();
        public new TypeInformation GetTypeInformation(ParameterInfo parameterInfo, bool isInputArgument) => base.GetTypeInformation(parameterInfo, isInputArgument);
        public new TypeInformation ApplyAttributes(TypeInformation typeInformation) => base.ApplyAttributes(typeInformation);
    }

    public class TestInputGraphType : AutoInputObjectGraphType<TestClass>
    {
        public static TestInputGraphType Instance = new();
        public new TypeInformation GetTypeInformation(PropertyInfo propertyInfo) => base.GetTypeInformation(propertyInfo);
        public new TypeInformation ApplyAttributes(TypeInformation typeInformation) => base.ApplyAttributes(typeInformation);
    }

    public class TestClass : DIObjectGraphBase { }
}
