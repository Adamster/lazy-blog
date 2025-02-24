using System.Reflection;

namespace Lazy.Presentation;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}