using UnityEngine;
using UnityEngine.Events;

public abstract class InputControllerBase : MonoBehaviour
{
    protected UnityEvent OnClickedEvent { get; } = new UnityEvent();

    public static InputControllerBase CreateInputController()
    {
        RuntimePlatform platform = Application.platform;
        GameObject go = new GameObject($"InputController_{platform}");

        switch (platform)
        {
            case RuntimePlatform.Android:
                go.AddComponent<InputControllers.Mobile>();
                return go.GetComponent<InputControllerBase>();
            default:
                go.AddComponent<InputControllers.Standalone>();
                return go.GetComponent<InputControllerBase>();
        }
    }

    protected virtual void OnDestroy()
    {
        OnClickedEvent.RemoveAllListeners();
    }

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
