namespace DIReflection.Runtime.Contexts
{
  public sealed class SceneContext : Context
  {
    protected override void Install()
    {
      GlobalContext.Instance.ActivateSceneContext(this);
    }
  }
}