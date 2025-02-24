using System.Reflection;

namespace Lazy.Persistence;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}