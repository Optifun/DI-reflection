using System;

namespace DIReflection.Runtime.Attributes
{
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
  public class InjectAttribute : Attribute
  {
  }
}