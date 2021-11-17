namespace GraphQL.DI
{
    /// <summary>
    /// This is a required base type of all DI-created graph types. <see cref="IDIObjectGraphBase{TSource}"/>
    /// may be used when the <see cref="IResolveFieldContext.Source"/> type is not <see cref="object"/>.
    /// </summary>
    public interface IDIObjectGraphBase
    {
        /// <summary>
        /// The <see cref="IResolveFieldContext"/> for the current field.
        /// </summary>
        public IResolveFieldContext Context { get; set; }
    }

    /// <summary>
    /// This is a required base type of all DI-created graph types. <see cref="IDIObjectGraphBase"/>
    /// may be used if the <see cref="IResolveFieldContext.Source"/> type is <see cref="object"/>.
    /// </summary>
    public interface IDIObjectGraphBase<TSource> : IDIObjectGraphBase
    {
    }
}
