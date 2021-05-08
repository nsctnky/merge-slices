using Enums;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameStates GameState { get; private set; }

    private EventManager _eventManager;

    private void Awake()
    {
        GameLoader.CreateLoader();
        GameLoader.GameLoadEvent.AddListener(OnGameLoad);
    }

    private void OnGameLoad()
    {
        GameState = GameStates.None;

        _eventManager = ServiceLocator.GetService<EventManager>();
        _eventManager.OnGameStatesChangedEvent.AddListener(OnGameStateChanged);
        ServiceLocator.GetService<InputControllerBase>().AddOnClickedEvent(OnClicked);
    }

    private void OnClicked()
    {
        if (!GameState.Equals(GameStates.Paused))
            return;

        GameState = GameStates.Playing;
        _eventManager.OnGameStatesChangedEvent.Invoke(GameState);
        Debug.Log("GAME STARTING...");
    }

    private void OnGameStateChanged(GameStates state)
    {
        Debug.Log("Current game state: " + state);
        GameState = state;
    }
}