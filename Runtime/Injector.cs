using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DIReflection.Runtime.Attributes;
using DIReflection.Runtime.Dependencies;
using UnityEngine;

namespace DIReflection.Runtime
{
  internal static class Injector
  {
    internal static List<IImplemented> ResolveDependencies(IEnumerable<IDependency> dependencies)
    {
      DependencyGraph graph = new DependencyGraph();
      graph.AddDependencies(dependencies);

      StringBuilder sb = new StringBuilder();
      List<IDependency> sorted = graph.SortedDependencies();
      foreach (IDependency dependency in sorted)
        sb.AppendLine(dependency.ToString());

      Debug.Log(sb.ToString());
      List<IImplemented> resolved = ApplyInjection(sorted);

      return resolved;
    }

    internal static MethodBase GetInjectableMethod(Type injectionTarget)
    {
      MethodBase[] methods = injectionTarget.GetMethods();
      MethodBase[] constructors = injectionTarget.GetConstructors();
      IEnumerable<MethodBase> members = methods.Union(constructors);

      var injectableMembers = members
        .Where(member => member.HasAttribute(typeof(InjectAttribute)))
        .ToArray();

      if (injectableMembers.Length > 1)
        throw new Exception($"Type {injectionTarget.FullName} has multiple Injection Attributes");

      MethodBase injectableMethod = injectableMembers.First();
      return injectableMethod;
    }

    private static List<IImplemented> ApplyInjection(List<IDependency> sortedDependencies)
    {
      List<IImplemented> instantiated = new List<IImplemented>();
      foreach (IDependency node in sortedDependencies)
      {
        object instance = ConstructInstance(node, instantiated);
        instantiated.Add(new InstanceDependency(node.Source, node.Target, instance, node.Persistence));
      }

      return instantiated;
    }

    internal static object InjectDependencies(IReflected targetNode, List<IImplemented> sortedDependencies)
    {
      var instances = targetNode.InjectedTypes
        .Select(type => GetSuppliedImplementation(sortedDependencies, type).Implementation)
        .ToArray();

      if (instances.Length != targetNode.InjectedTypes.Length)
        throw new Exception($"Injection corrupted: required dependency for {targetNode.Target.Name} not found");

      MethodBase injectionMethod = targetNode.InjectionMethod;
      if (injectionMethod.IsConstructor)
        return InjectConstructor(targetNode, instances);

      if (targetNode is MonoDependency monoDependency)
        return InjectMethod(monoDependency, monoDependency.Implementation, instances);

      return InjectMethod(targetNode, instances);
    }

    private static object ConstructInstance(IDependency node, List<IImplemented> constructedDependencies)
    {
      var reflectionDependency = node as IReflected;
      var implementedDependency = node as IImplemented;

      if (implementedDependency != null && node.IsImplemented ||
          reflectionDependency != null && IsSimpleMonobehaviour(reflectionDependency))
        return implementedDependency.Implementation;

      if (reflectionDependency == null)
        throw new Exception("Can't inject null value");

      if (reflectionDependency.CanInject)
        return InjectDependencies(reflectionDependency, constructedDependencies);

      return SimpleConstruct(node);
    }

    private static object InjectConstructor(IReflected target, object[] dependencies) =>
      Activator.CreateInstance(target.Target, dependencies);

    private static object InjectMethod(IReflected target, object[] dependencies)
    {
      object instance = SimpleConstruct(target);
      instance = InjectMethod(target, instance, dependencies);
      return instance;
    }

    private static object InjectMethod(IReflected target, object instance, object[] dependencies)
    {
      MethodBase injectionMethod = target.InjectionMethod;
      injectionMethod.Invoke(instance, dependencies);
      return instance;
    }

    private static object SimpleConstruct(IDependency target) =>
      Activator.CreateInstance(target.Target);

    private static IImplemented GetSuppliedImplementation(List<IImplemented> sortedDependencies, Type type) =>
      sortedDependencies.Find(node => type == node.Source || type.IsAssignableFrom(node.Source));

    private static bool IsSimpleMonobehaviour(IReflected reflectionDependency) =>
      reflectionDependency.IsMonoBehaviour &&
      reflectionDependency.Dependencies.Count() == 0;
  }
}