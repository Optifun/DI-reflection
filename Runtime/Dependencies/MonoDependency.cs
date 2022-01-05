using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DIReflection.Runtime.Dependencies
{
  internal class MonoDependency : Dependency, IImplemented, IReflected
  {
    public MethodBase InjectionMethod { get; }
    public Type[] InjectedTypes { get; } = Array.Empty<Type>();
    public bool CanInject => _monoInstance != null && _monoInstance.enabled && InjectionMethod != null;

    public object Implementation => _monoInstance;
    public override bool IsImplemented => Implementation != null;

    private MonoBehaviour _monoInstance;

    public MonoDependency(Type source,
      Type target,
      MonoBehaviour instance,
      DependencyPersistence persistence)
      : base(source, target, persistence)
    {
      _monoInstance = instance;
      InjectedTypes = Array.Empty<Type>();
    }

    public MonoDependency(Type source,
      Type target,
      MonoBehaviour instance,
      MethodBase injectionMethod,
      Type[] injectedTypes,
      DependencyPersistence persistence)
      : base(source, target, persistence)
    {
      InjectionMethod = injectionMethod;
      InjectedTypes = injectedTypes;
      _monoInstance = instance;
      
      foreach (Type type in InjectedTypes)
        Dependencies.Add(type, null);
    }

    public MonoDependency(Type source,
      Type target,
      DependencyPersistence persistence)
      : this(source, target, null, persistence)
    {
    }

    public MonoDependency(Type source,
      Type target,
      MethodBase injectionMethod,
      Type[] injectedTypes,
      DependencyPersistence persistence)
      : this(source, target, null, persistence)
    {
      InjectionMethod = injectionMethod;
      InjectedTypes = injectedTypes;
    }

    public MonoDependency With(MonoBehaviour instance) =>
      new MonoDependency(
        Source,
        Target,
        InjectionMethod,
        InjectedTypes,
        Persistence)
      {
        _monoInstance = instance,
      };

    public override string ToString()
    {
      string dependencies = String.Join(", ", InjectedTypes.Select(type => type.Name));
      return $"MonoDependency - {Target.Name} {{ {dependencies} }}";
    }
  }
}