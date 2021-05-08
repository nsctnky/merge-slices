using Enums;
using UnityEngine;

public class CircleController : MonoBehaviour
{
    [SerializeField]
    private float angle = 1.5f;

    private bool _isGameStarted;
    private EventManager _eventManager;

    private void Awake()
    {
        GameLoader.AllServicesAddedEvent.AddListener(OnAllServicesAdded);
    }

    private void Update()
    {
        if (!_isGameStarted)
            return;

        transform.Rotate(Vector3.forward, angle);
    }

    private void OnAllServicesAdded()
    {
        _eventManager = ServiceLocator.GetService<EventManager>();
        _eventManager.OnGameStatesChangedEvent.AddListener(OnGameStateChange);
    }

    private void OnGameStateChange(GameStates state)
    {
        if (state.Equals(GameStates.Playing))
        {
            _isGameStarted = true;
        }
    }
}