using Enums;
using InputControllers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private readonly RuntimePlatform _currentRuntimePlatform = Application.platform;

    private InputControllerBase _inputController;
    private GameStates _gameStates;
    
    private void Awake()
    {
        _gameStates = GameStates.None;
        _inputController = InstantiateInputController(_currentRuntimePlatform);
        
        _inputController.AddOnClickedEvent(OnClicked);
        
        ServiceLocator.AddService(new EventManager());
    }

    private void OnClicked()
    {
        
    }

    #region INPUT CONTROLLER
    private InputControllerBase InstantiateInputController(RuntimePlatform platform)
    {
        GameObject go = new GameObject($"InputController_{platform}");

        switch (platform)
        {
            case RuntimePlatform.Android:
                go.AddComponent<Mobile>();
                return go.GetComponent<Mobile>();
            default:
                go.AddComponent<Standalone>();
                return go.GetComponent<Standalone>();
        }
    }
    #endregion INPUT CONTROLLER

    #region TEST FUNC
    [ContextMenu("TEST_ServiceLocator")]
    private void TEST_ServiceLocator()
    {
        ServiceLocator.GetService<EventManager>().TEST_Do();
    }
    #endregion TEST FUNC
}
