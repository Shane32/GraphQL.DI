using GraphQL.Types;

namespace GraphQL.DI
{
    /// <inheritdoc/>
    public class DIFieldType : FieldType
    {
        /// <summary>
        /// Indicates if the field resolver can run concurrently with other field resolvers.
        /// </summary>
        public bool Concurrent { get; set; } = false;
    }
}
