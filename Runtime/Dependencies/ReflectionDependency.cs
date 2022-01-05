using System;
using System.Linq;
using System.Reflection;

namespace DIReflection.Runtime.Dependencies
{
  internal class ReflectionDependency : Dependency, IReflected
  {
    public MethodBase InjectionMethod { get; }
    public Type[] InjectedTypes { get; }

    public override bool IsImplemented => false;
    public bool CanInject => InjectionMethod != null && InjectedTypes.Length > 0 && !IsMonoBehaviour;

    protected internal ReflectionDependency(Type source,
      Type target,
      MethodBase injectionMethod,
      Type[] injectedTypes,
      DependencyPersistence persistence) : base(source, target, persistence)
    {
      InjectionMethod = injectionMethod;
      InjectedTypes = injectedTypes;

      foreach (Type type in InjectedTypes)
        Dependencies.Add(type, null);
    }

    protected internal ReflectionDependency(Type source,
      Type target,
      DependencyPersistence persistence) : base(source, target, persistence)
    {
      InjectedTypes = Array.Empty<Type>();
    }

    public override string ToString()
    {
      string dependencies = String.Join(", ", InjectedTypes.Select(type => type.Name));
      return $"ReflectionDependency - {Target.Name} {{{dependencies}}}";
    }
  }
}