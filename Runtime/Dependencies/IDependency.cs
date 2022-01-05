using System;
using System.Collections.Generic;

namespace DIReflection.Runtime.Dependencies
{
  internal interface IDependency
  {
    Type Source { get; }
    Type Target { get; }
    // Dictionary<Type, IDependency> Dependencies { get; }
    bool IsImplemented { get; }
    bool IsMonoBehaviour { get; }
    DependencyPersistence Persistence { get; }
    Dictionary<Type, IDependency> Dependencies { get; }
    bool CheckResolved();
    void DependOn(IDependency dependency);
    bool IsDependsOn(IDependency dependency);
  }
}