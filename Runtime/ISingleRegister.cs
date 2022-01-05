namespace DIReflection.Runtime
{
  internal interface ISingleRegister
  {
    void RegisterSingle<TService>() where TService : class;
    void RegisterSingle<SService, TService>() where TService : class, SService;
    void RegisterSingle<TService>(TService instance) where TService : class;
    void RegisterSingle<SService, TService>(TService instance) where TService : class, SService;
  }
}