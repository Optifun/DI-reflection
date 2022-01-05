using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DIReflection.Runtime.Dependencies;
using UnityEngine;

namespace DIReflection.Runtime
{
  public abstract class DependencyProvider : MonoBehaviour, ISingleRegister, ITransientRegister
  {
    private OrderedDictionary Implementations { get; set; } = new OrderedDictionary();

    public abstract void ProvideDependencies();

    public void RegisterSingle<TService>() where TService : class =>
      RegisterSingle<TService, TService>();

    public void RegisterSingle<SService, TService>() where TService : class, SService
    {
      Type serviceType = typeof(SService);
      Type instanceType = typeof(TService);

      CreateDependency(serviceType, instanceType, null, DependencyPersistence.Single);
    }

    public void RegisterSingle<TService>(TService instance) where TService : class =>
      RegisterSingle<TService, TService>(instance);

    public void RegisterSingle<SService, TService>(TService instance) where TService : class, SService
    {
      Type serviceType = typeof(SService);
      Type instanceType = instance.GetType();

      CreateDependency(serviceType, instanceType, instance, DependencyPersistence.Single);
    }

    public void RegisterTransient<TService>() where TService : class =>
      RegisterTransient<TService, TService>();

    public void RegisterTransient<SService, TService>() where TService : class, SService
    {
      Type serviceType = typeof(SService);
      Type instanceType = typeof(TService);

      CreateDependency(serviceType, instanceType, null, DependencyPersistence.Transient);
    }

    public void RegisterTransient<TService>(TService instance) where TService : class =>
      RegisterTransient<TService, TService>();

    public void RegisterTransient<SService, TService>(TService instance) where TService : class, SService
    {
      Type serviceType = typeof(SService);
      Type instanceType = instance.GetType();

      CreateDependency(serviceType, instanceType, instance, DependencyPersistence.Transient);
    }

    internal List<Dependency> GetDependencies() =>
      Implementations.Values.ToListImplicit<Dependency>();

    private void CreateDependency(Type serviceType, Type instanceType, object instance, DependencyPersistence persistence)
    {
      // if (typeof(MonoBehaviour).IsAssignableFrom(instanceType))
      //   throw new ArgumentException($"Can not provide dependency of type '{serviceType.FullName}', because it is MonoBehaviour");

      if (Implementations.Contains(serviceType))
        throw new ArgumentException($"Implementation of type '{serviceType.FullName}' was already registered");

      var dependency = Dependency.Construct(serviceType, instanceType, instance, persistence);
      Implementations.Add(serviceType, dependency);
    }
  }
}