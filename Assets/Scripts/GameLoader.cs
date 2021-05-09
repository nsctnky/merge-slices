using UnityEngine;
using UnityEngine.Events;

public class GameLoader : MonoBehaviour
{
    public static UnityEvent GameInitialized { get; } = new UnityEvent();
    public static UnityEvent ServiceLocatorInitialized { get; } = new UnityEvent();
    
    private static GameLoader _instance;
    
    public static GameLoader CreateLoader()
    {
        if (_instance != null)
            return _instance;
        
        return new GameObject("_GameLoader_").AddComponent<GameLoader>();
    }

    private void Awake()
    {
        Debug.Log("Game loader instantiated.");
        GameInitialized.AddListener(OnGameInitialized);
        ServiceLocatorInitialized.AddListener(OnServiceLocatorInitialized);
    
        ServiceLocator.InitializeServiceLocator();
    }

    private void Start()
    {
        GameInitialized.Invoke();
    }

    private void OnGameInitialized()
    {
        Debug.Log("Game initialized.");
        DestroyLoader();
    }

    private void OnServiceLocatorInitialized()
    {
        ServiceLocator.AddService<EventManager>(new EventManager());
        ServiceLocator.AddService<InputControllerBase>(InputControllerBase.CreateInputController());
    }

    private void DestroyLoader()
    {
        GameInitialized.RemoveAllListeners();
        ServiceLocatorInitialized.RemoveAllListeners();
        Debug.Log("GameLoader was destroyed.");
        Destroy(gameObject);
    }
}