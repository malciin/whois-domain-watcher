using System.Reflection;

namespace DomainWatcher.Core.Extensions;

public static class AssemblyExtensions
{
    public static IEnumerable<Type> GetInstantiableTypesAssignableTo<T>(this Assembly assembly)
    {
        return GetInstantiableTypesAssignableTo(assembly, typeof(T));
    }

    public static IEnumerable<Type> GetInstantiableTypesAssignableTo(this Assembly assembly, Type type)
    {
        return assembly
            .GetTypes()
            .Where(x => !x.IsAbstract && x.IsAssignableTo(type));
    }
}
