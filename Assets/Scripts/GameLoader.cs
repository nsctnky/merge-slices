using UnityEngine;
using UnityEngine.Events;

public class GameLoader : MonoBehaviour
{
    public static UnityEvent AllServicesAddedEvent { get; } = new UnityEvent();
    private static GameLoader _instance;
    
    public static GameLoader CreateLoader()
    {
        if (_instance != null)
            return _instance;
        
        return new GameObject("_GameLoader_").AddComponent<GameLoader>();
    }

    private void Awake()
    {
        Debug.Log("Game loader instantiated...");
        AllServicesAddedEvent.AddListener(OnAllServicesAdded);
    }

    private void Start()
    {
        ServiceLocator.AddService<EventManager>(new EventManager());
        ServiceLocator.AddService<InputControllerBase>(InputControllerBase.CreateInputController());

        AllServicesAddedEvent.Invoke();
    }

    private void OnAllServicesAdded()
    {
        Debug.Log("OnGameLoaded and gameLoader will be destroyed.");
        AllServicesAddedEvent.RemoveAllListeners();
        Destroy(gameObject);
    }
}