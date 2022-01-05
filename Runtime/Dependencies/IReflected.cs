using System;
using System.Reflection;

namespace DIReflection.Runtime.Dependencies
{
  internal interface IReflected : IDependency
  {
    MethodBase InjectionMethod { get; }
    Type[] InjectedTypes { get; }
    bool CanInject { get; }
  }
}