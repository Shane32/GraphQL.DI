using System;

namespace GraphQL.DI
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ConcurrentAttribute : Attribute
    {
        public ConcurrentAttribute() { }
        public ConcurrentAttribute(bool createNewScope) { CreateNewScope = createNewScope; }
        public bool Concurrent { get; set; } = true;
        public bool CreateNewScope { get; set; } = false;
    }
}
