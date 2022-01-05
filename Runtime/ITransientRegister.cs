namespace DIReflection.Runtime
{
  internal interface ITransientRegister
  {
    void RegisterTransient<TService>() where TService : class;
    void RegisterTransient<SService, TService>() where TService : class, SService;
    void RegisterTransient<TService>(TService instance) where TService : class;
    void RegisterTransient<SService, TService>(TService instance) where TService : class, SService;
  }
}