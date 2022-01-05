using System;
using System.Collections.Generic;
using System.Linq;
using DIReflection.Runtime.Dependencies;
using UnityEngine;
using UnityEngine.Assertions;

namespace DIReflection.Runtime
{
  internal class DependencyGraph
  {
    private readonly List<IDependency> _nodes = new List<IDependency>();

    internal void AddDependencies(IEnumerable<IDependency> dependencies)
    {
      foreach (IDependency dependency in dependencies)
        AddSingleDependency(dependency);

      SetupDependentNodes();
    }

    internal void AddDependency(IDependency dependency)
    {
      AddSingleDependency(dependency);
      SetupDependentNodes();
    }

    internal List<IDependency> Dependencies() =>
      _nodes;

    internal List<IDependency> SortedDependencies()
    {
      CheckDependencyTree();

      return _nodes
        .TopologicalSort(node => node.Dependencies.Values)
        .ToList();
    }

    private void CheckDependencyTree()
    {
      IEnumerable<Dependency> unresolved = _nodes
        .Where(node => !node.CheckResolved())
        .Select(node => node as Dependency)
        .ToList();

      foreach (Dependency dependency in unresolved)
        LogMissingDependencies(dependency);

      if (unresolved.Any())
        throw new Exception($"Can not resolve dependencies: dependency tree is not complete");
    }

    private void AddSingleDependency(IDependency dependency)
    {
      if (_nodes.Exists(n => n.Source == dependency.Source))
        throw new Exception($"Conflict dependencies provided for {dependency.Source.FullName}");

      _nodes.Add(dependency);
    }

    private void SetupDependentNodes()
    {
      foreach (IDependency node in _nodes)
      {
        if (node.IsImplemented || node.CheckResolved())
          continue;

        var dependency = node as IReflected;
        Assert.IsNotNull(dependency, $"Corrupted dependency state. {node}");

        SetDependencies(dependency);
      }
    }

    private void SetDependencies(IReflected target)
    {
      foreach (Type injectedType in target.InjectedTypes)
      {
        IDependency dependency = _nodes.Find(dep => dep.Source.IsAssignableFrom(injectedType));
        target.DependOn(dependency);
      }
    }

    private static void LogMissingDependencies(Dependency dependency)
    {
      foreach (var missing in dependency.Dependencies.Where(pair => pair.Value == null))
        Debug.LogError($"Can not resolve dependencies: type {dependency.Target} has missing dependency {missing.Key.Name}");
    }
  }
}