namespace DIReflection.Runtime.Dependencies
{
  internal interface IImplemented : IDependency
  {
    object Implementation { get; }
  }
}