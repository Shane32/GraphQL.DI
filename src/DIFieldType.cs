using GraphQL.Types;

namespace GraphQL.DI
{
    public class DIFieldType : FieldType
    {
        public bool Concurrent { get; set; } = false;
    }
}
