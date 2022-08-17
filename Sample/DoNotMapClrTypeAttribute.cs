namespace Sample;

/// <summary>
/// Indicates that GetClrTypeMappings should
/// skip this class when scanning an assembly for CLR type mappings.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class DoNotMapClrTypeAttribute : Attribute
{
}
