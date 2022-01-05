using DIReflection.Runtime.Contexts;

namespace DIReflection.Runtime
{
  public interface IContextProvider
  {
    GlobalContext GlobalContext { get; }
    // SceneContext GetRequiredContext<TContextReceiver>();
  }
}