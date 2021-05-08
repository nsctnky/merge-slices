#if UNITY_EDITOR
using Enums;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField]
    private GameStates state;

    [ContextMenu("Change state")]
    private void TEST_ChangeGameState()
    {
        ServiceLocator.GetService<EventManager>().OnGameStatesChangedEvent.Invoke(state);
    }

    [ContextMenu("Game Load")]
    private void TEST_GameLoad()
    {
        GameLoader.AllServicesAddedEvent.Invoke();
    }
}
#endif