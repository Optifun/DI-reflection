using DIReflection.Runtime.Attributes;
using DIReflection.Runtime.Contexts;

namespace DIReflection.Runtime
{
  public class ContextProvider : IContextProvider
  {
    public GlobalContext GlobalContext { get; protected set; }

    [Inject]
    public ContextProvider(GlobalContext globalContext)
    {
      GlobalContext = globalContext;
    }

    // public SceneContext GetRequiredContext<TContextReceiver>()
    // {
    //   var receiverType = typeof(TContextReceiver);
    //   var attribute = receiverType.GetCustomAttribute<RequireContextAttribute>();
    //   if (attribute == null)
    //     return null;
    //
    //   var gameObject = GameObject.FindObjectOfType(attribute._contextType) as GameObject;
    //   if (gameObject == null)
    //     return null;
    //
    //   return gameObject.GetComponent(attribute._contextType) as SceneContext;
    // }
  }
}