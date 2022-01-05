namespace DIReflection.Runtime
{
  internal interface IServiceLocator
  {
    TObject Resolve<TObject>() where TObject : class;
  }
}