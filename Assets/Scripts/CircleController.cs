using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class CircleController : MonoBehaviour
{
    [SerializeField]
    private float angle = 1.5f;
    [SerializeField]
    private List<PieceTriggerArea> triggers;

    private bool _isGameStarted;
    private EventManager _eventManager;

    private void Awake()
    {
        GameLoader.GameInitialized.AddListener(OnGameInitialized);
    }

    private void FixedUpdate()
    {
        if (!_isGameStarted)
            return;
        
        transform.Rotate(Vector3.forward, angle);
    }

    public void AddTriggerListener(Action<PieceTriggerData> callback)
    {
        foreach (var trigger in triggers)
        {
            trigger.Init(callback);
        }
    }

    private void OnGameInitialized()
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