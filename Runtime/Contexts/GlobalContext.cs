using DIReflection.Runtime.Dependencies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DIReflection.Runtime.Contexts
{
  public sealed class GlobalContext : Context, IServiceLocator
  {
    public static GlobalContext Instance
    {
      get
      {
        if (_instance != null)
          return _instance;

        _instance = FindObjectOfType<GlobalContext>();
        if (_instance != null)
          return _instance;

        _instance = new GameObject("Container").AddComponent<GlobalContext>();
        return _instance;
      }
    }

    private static GlobalContext _instance;

    private DiContainer container;

    public TObject Resolve<TObject>() where TObject : class =>
      container.Resolve<TObject>();

    internal void ActivateSceneContext(SceneContext context)
    {
      foreach (var provider in context.Providers)
        container.UnpackProvider(provider);
    }

    protected override void Install()
    {
      if (Instance != this)
      {
        Debug.LogWarning("Found and destroyed GlobalContext duplicate");
        Destroy(gameObject);
      }

      SceneManager.sceneLoaded += OnSceneLoaded;
      SceneManager.sceneUnloaded += OnSceneUnloaded;

      container = new DiContainer();
      ProvideGlobalDependencies();

      DontDestroyOnLoad(gameObject);
    }

    protected override void Uninstall()
    {
      SceneManager.sceneLoaded -= OnSceneLoaded;
      SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene _, LoadSceneMode sceneMode) =>
      container.Inject();

    private void OnSceneUnloaded(Scene _) =>
      container.ClearTransient();

    private void ProvideGlobalDependencies()
    {
      RegisterSelf();

      foreach (var provider in Providers)
        container.UnpackProvider(provider);
    }

    private void RegisterSelf()
    {
      var keyType = typeof(GlobalContext);
      var containerDependency = Dependency.Construct(keyType, keyType, this, DependencyPersistence.Single);
      containerDependency.AddDependency(container);
    }
  }
}