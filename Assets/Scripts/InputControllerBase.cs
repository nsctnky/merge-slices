using UnityEngine;
using UnityEngine.Events;

public abstract class InputControllerBase : MonoBehaviour
{
    protected UnityEvent OnClickedEvent { get; } = new UnityEvent();

    public void AddOnClickedEvent(UnityAction callback)
    {
        OnClickedEvent.AddListener(callback);
    }

    public void RemoveOnClickedEvent(UnityAction callback)
    {
        OnClickedEvent.RemoveListener(callback);
    }
    
    protected abstract void OnClicked();
}
