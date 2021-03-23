using System;

namespace GraphQL.DI
{
    /// <summary>
    /// Marks this class or method as being able to run concurrently. May specify if a new service
    /// scope is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ConcurrentAttribute : Attribute
    {
        /// <summary>
        /// Marks this class or method as being able to run concurrently without a dedicated service scope.
        /// </summary>
        public ConcurrentAttribute() { }

        /// <summary>
        /// Marks this class or method as being able to run concurrently, with or without a dedicated service scope.
        /// </summary>
        public ConcurrentAttribute(bool createNewScope) { CreateNewScope = createNewScope; }

        /// <summary>
        /// Indicates if this class or method is able to run concurrently.
        /// </summary>
        public bool Concurrent { get; set; } = true;

        /// <summary>
        /// Indicates if this class or method requires a dedicated service scope to run concurrently. When set
        /// for a class, a new service scope will be created for each method called.
        /// </summary>
        public bool CreateNewScope { get; set; } = false;
    }
}
