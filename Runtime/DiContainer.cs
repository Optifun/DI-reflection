using System;
using System.Collections.Generic;
using System.Linq;
using DIReflection.Runtime.Attributes;
using DIReflection.Runtime.Dependencies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DIReflection.Runtime
{
  internal class DiContainer : IServiceLocator, IDisposable
  {
    internal static readonly Dictionary<Type, IDependency> SingleDependencies = new Dictionary<Type, IDependency>();

    internal readonly Dictionary<Type, IDependency> Dependencies = new Dictionary<Type, IDependency>();

    public TObject Resolve<TObject>() where TObject : class
    {
      TObject implementation;

      if (TrySingleImplementation(out implementation))
        return implementation;

      if (TryImplementation(out implementation))
        return implementation;

      return null;
    }

    public DiContainer() =>
      RegisterSelf();

    public void Inject()
    {
      IEnumerable<IDependency> dependencies = Dependencies.Values.Union(SingleDependencies.Values);

      List<MonoDependency> monoDependencies = dependencies
        .Where(dep => dep.IsMonoBehaviour && !dep.IsImplemented)
        .Cast<MonoDependency>()
        .ToList();

      var commonDependencies = dependencies.Except(monoDependencies);
      List<IDependency> sceneDependencies = CollectFromScene(monoDependencies);

      List<IDependency> preparedDependencies = commonDependencies.Union(sceneDependencies).ToList();
      List<IImplemented> implementedDependencies = Injector.ResolveDependencies(preparedDependencies);

      foreach (IImplemented dependency in implementedDependencies)
      {
        var instance = dependency;
        if (SingleDependencies.TryGetValue(instance.Source, out IDependency registered) && !(registered is IImplemented))
          SingleDependencies[instance.Source] = instance;

        if (Dependencies.ContainsKey(instance.Source))
          Dependencies[instance.Source] = instance;
      }
    }

    internal void UnpackProvider(DependencyProvider provider)
    {
      provider.ProvideDependencies();
      List<Dependency> dependencies = provider.GetDependencies();
      foreach (Dependency dependency in dependencies)
        dependency.AddDependency(this);
    }

    internal void ClearTransient()
    {
      var transientDependencies = Dependencies
        .Values
        .Where(dep => dep.Persistence == DependencyPersistence.Transient)
        .ToList();

      foreach (IDependency dependency in transientDependencies)
        Dependencies.Remove(dependency.Source);
    }

    private List<IDependency> CollectFromScene(List<MonoDependency> monoDependencies)
    {
      List<IDependency> registeredBehaviours = new List<IDependency>();
      Type attribute = typeof(InjectAttribute);
      GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

      foreach (GameObject gameObject in rootGameObjects)
      {
        var components = gameObject.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
          Type targetType = component.GetType();
          MonoDependency monoDependency = monoDependencies.FirstOrDefault(dep => dep.Target == targetType);

          if (monoDependency == null)
          {
            if (targetType.HasAttributeInConstructor(attribute) || targetType.HasAttributeInMethod(attribute))
              Debug.LogWarning($"Component {targetType.FullName} has InjectAttribute, but is not registered in any DependencyProvider", component);

            continue;
          }

          if (monoDependency.IsImplemented)
            continue;

          if (monoDependency.Persistence == DependencyPersistence.Single && registeredBehaviours.Any(dep => dep.Source == monoDependency.Source))
            throw new Exception($"Injection failure: found multiple behaviours of {monoDependency.Source.Name} registered as Single!");

          MonoDependency componentDependency = monoDependency.With(component);
          registeredBehaviours.Add(componentDependency);
        }
      }

      return registeredBehaviours;
    }

    private MonoDependency TryGetMonoDependency(MonoBehaviour component)
    {
      Type target = component.GetType();
      IDependency implementation = TrySingleImplementation(target) ?? TryImplementation(target);

      return implementation as MonoDependency;
    }

    private void RegisterSelf()
    {
      var keyType = typeof(DiContainer);
      var containerDependency = Dependency.Construct(keyType, keyType, this, DependencyPersistence.Single);
      containerDependency.AddDependency(this);
    }

    private bool TryImplementation<TObject>(out TObject implementation) where TObject : class =>
      TryGetImplementation(Dependencies, out implementation);

    private IDependency TryImplementation(Type implementationType)
    {
      Dependencies.TryGetValue(implementationType, out IDependency value);
      return value;
    }

    private bool TrySingleImplementation<TObject>(out TObject implementation) where TObject : class =>
      TryGetImplementation(SingleDependencies, out implementation);

    private IDependency TrySingleImplementation(Type implementationType)
    {
      SingleDependencies.TryGetValue(implementationType, out IDependency value);
      return value;
    }

    private static bool TryGetImplementation<TObject>(Dictionary<Type, IDependency> dependencies, out TObject implementation) where TObject : class
    {
      var key = typeof(TObject);
      if (dependencies.TryGetValue(key, out IDependency dependency))
      {
        implementation = CastImplementation(dependency) as TObject;
        return true;
      }

      implementation = null;
      return false;
    }

    private static object CastImplementation(IDependency dependency) =>
      dependency is IImplemented implementation ? implementation.Implementation : null;

    public void Dispose()
    {
      SingleDependencies.Clear();
      Dependencies.Clear();
    }
  }
}