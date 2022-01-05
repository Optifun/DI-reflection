using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DIReflection.Runtime.Attributes;
using UnityEngine;

namespace DIReflection.Runtime.Dependencies
{
  internal abstract class Dependency : IDependency, IEquatable<Dependency>
  {
    private static readonly Type MonoBehaviourType = typeof(MonoBehaviour);

    public Type Source { get; }
    public Type Target { get; }
    public DependencyPersistence Persistence { get; }

    public abstract bool IsImplemented { get; }
    public bool IsMonoBehaviour { get; }
    public Dictionary<Type, IDependency> Dependencies { get; protected set; }

    private bool? _resolved;

    protected Dependency(Type source, Type target, DependencyPersistence persistence)
    {
      Source = source;
      Target = target;
      Persistence = persistence;
      Dependencies = new Dictionary<Type, IDependency>();
      IsMonoBehaviour = MonoBehaviourType.IsAssignableFrom(target);
    }

    public static Dependency Construct(Type source, Type target, object instance, DependencyPersistence persistence)
    {
      bool isMonobehaviour = MonoBehaviourType.IsAssignableFrom(target);
      if (instance != null && !isMonobehaviour)
        return new InstanceDependency(source, target, instance, persistence);

      if (!target.HasAttributeInMethod(typeof(InjectAttribute)) &&
          !target.HasAttributeInConstructor(typeof(InjectAttribute)))
      {
        if (isMonobehaviour)
        {
          if (instance != null && instance is MonoBehaviour behaviour)
            return new MonoDependency(source, target, behaviour, persistence);

          return new MonoDependency(source, target, persistence);
        }

        return new ReflectionDependency(source, target, persistence);
      }

      MethodBase methodType = Injector.GetInjectableMethod(target);
      Type[] dependencies = GetMethodParameters(methodType).ToArray();
      if (isMonobehaviour)
      {
        if (instance != null && instance is MonoBehaviour behaviour)
          return new MonoDependency(source, target, behaviour, methodType, dependencies, persistence);

        return new MonoDependency(source, target, methodType, dependencies, persistence);
      }

      return new ReflectionDependency(source, target, methodType, dependencies, persistence);
    }

    public bool CheckResolved()
    {
      if (_resolved != null)
        return (bool) _resolved;

      _resolved =
        IsImplemented ||
        Dependencies.Values.All(dependency => dependency != null && dependency.CheckResolved());

      return (bool) _resolved;
    }

    public void DependOn(IDependency dependency)
    {
      Type keyType = Dependencies.Keys.First(key => dependency.Source.IsAssignableFrom(key));
      if (keyType == null)
        throw new ArgumentException($"Injection failure: {Target.Name} can not depend on - {dependency.Target.Name}");

      _resolved = null;
      Dependencies.Remove(keyType);
      Dependencies.Add(keyType, dependency);
    }

    public bool IsDependsOn(IDependency dependency) =>
      Dependencies.ContainsValue(dependency);

    internal void AddDependency(DiContainer container)
    {
      switch (Persistence)
      {
        case DependencyPersistence.Single:
          AddSingle(container);
          break;
        case DependencyPersistence.Transient:
          AddTransient(container);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(Persistence));
      }
    }

    private void AddTransient(DiContainer container)
    {
      if (container.Dependencies.ContainsKey(Source))
        throw new Exception($"Can't register two instances of {Source} Transient dependency");

      if (DiContainer.SingleDependencies.ContainsKey(Source))
        throw new Exception($"Can't register Transient dependency for {Source} already provided as Single");

      container.Dependencies.Add(Source, this);
    }

    private void AddSingle(DiContainer container)
    {
      if (DiContainer.SingleDependencies.ContainsKey(Source))
        throw new Exception($"Can't register two instances of {Source} as Single dependency");

      if (container.Dependencies.ContainsKey(Source))
        throw new Exception($"Can't register Single dependency for {Source} already provided as Transient");

      DiContainer.SingleDependencies.Add(Source, this);
    }

    public override string ToString() =>
      $"Dependency - {Target.Name}";

    private static IEnumerable<Type> GetMethodParameters(MethodBase method) =>
      method.GetParameters()
        .Select(param => param.ParameterType);

    public static bool operator ==(Dependency left, Dependency right) =>
      Equals(left, right);

    public static bool operator !=(Dependency left, Dependency right) =>
      !Equals(left, right);

    public bool Equals(Dependency other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Dependencies.Equals(other.Dependencies)
             && Source.Equals(other.Source)
             && Target.Equals(other.Target);
    }

    public override bool Equals(object obj) =>
      ReferenceEquals(this, obj)
      || obj is Dependency other
      && Equals(other);

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = Dependencies.GetHashCode();
        hashCode = (hashCode * 397) ^ Source.GetHashCode();
        hashCode = (hashCode * 397) ^ Target.GetHashCode();
        return hashCode;
      }
    }
  }

  internal class DependencyComparer : IComparer<IDependency>
  {
    public int Compare(IDependency x, IDependency y)
    {
      bool depends = x.IsDependsOn(y);
      bool serves = y.IsDependsOn(x);

      if (depends && serves)
        throw new ArgumentException($"Can't resolve circular dependency for {x.Target.Name} and {y.Target.Name}");

      if (depends != serves)
        return depends ? 1 : -1;

      return 0;
    }
  }
}