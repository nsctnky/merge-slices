using System;
using UnityEngine;

public class PieceTriggerArea : MonoBehaviour
{
    [SerializeField]
    private int index;

    private Action<int> _triggerCallback;
    
    public void Init(Action<int> triggerCallback)
    {
        _triggerCallback = triggerCallback;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Piece"))
            return;
        
        _triggerCallback?.Invoke(index);
    }
}