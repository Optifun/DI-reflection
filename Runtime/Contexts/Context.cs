using UnityEngine;

namespace DIReflection.Runtime.Contexts
{
  public abstract class Context : MonoBehaviour
  {
    public DependencyProvider[] Providers;

    private void Awake() => 
      Install();

    private void OnDestroy() => 
      Uninstall();

    protected abstract void Install();

    protected virtual void Uninstall()
    {
    }
  }
}