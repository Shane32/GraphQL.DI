using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample
{
    /// <summary>
    /// Indicates that GetClrTypeMappings should
    /// skip this class when scanning an assembly for CLR type mappings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DoNotMapClrTypeAttribute : Attribute
    {
    }
}
