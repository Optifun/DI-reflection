using System;

namespace DIReflection.Runtime.Dependencies
{
  internal class InstanceDependency : Dependency, IImplemented
  {
    public override bool IsImplemented => Implementation != null;
    public object Implementation { get; }

    protected internal InstanceDependency(Type source,
      Type target,
      object implementation,
      DependencyPersistence persistence) : base(source, target, persistence)
    {
      Implementation = implementation;
    }
  }
}