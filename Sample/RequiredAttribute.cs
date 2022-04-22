using GraphQL.Types;

namespace Sample
{
    public class RequiredAttribute : GraphQL.GraphQLAttribute
    {
        public override void Modify(TypeInformation typeInformation) => typeInformation.IsNullable = false;
    }
}
