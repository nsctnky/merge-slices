using UnityEngine;
using UnityEngine.Events;

public class GameLoader : MonoBehaviour
{
    public static UnityEvent GameLoadEvent { get; } = new UnityEvent();

    public static GameLoader CreateLoader()
    {
        return new GameObject("_GameLoader_").AddComponent<GameLoader>();
    }
    
    private void Awake()
    {
        Debug.Log("Game loader instantiated...");
        GameLoadEvent.AddListener(OnGameLoaded);
        ServiceLocator.AddService<EventManager>(new EventManager());
        ServiceLocator.AddService<InputControllerBase>(InputControllerBase.CreateInputController());
        
        GameLoadEvent.Invoke();
    }

    private void OnGameLoaded()
    {
        Debug.Log("OnGameLoaded and gameLoader will be destroyed.");
        GameLoadEvent.RemoveAllListeners();
        Destroy(gameObject);
    }
}
