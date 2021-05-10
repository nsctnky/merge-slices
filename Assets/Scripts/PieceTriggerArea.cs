using System;
using UnityEngine;

public class PieceTriggerArea : MonoBehaviour
{
    [SerializeField]
    private int index;

    private Action<PieceTriggerData> _triggerCallback;
    
    public void Init(Action<PieceTriggerData> triggerCallback)
    {
        _triggerCallback = triggerCallback;
    }

    private Vector2 GetPosition()
    {
        return transform.localPosition;
    }

    private float GetRotation()
    {
        return transform.rotation.eulerAngles.z;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.CompareTag("Piece"))
            return;
        
        _triggerCallback?.Invoke(new PieceTriggerData(index, GetPosition(), GetRotation()));
    }
}

public readonly struct PieceTriggerData
{
    public readonly int Index;
    public readonly Vector2 Position;
    public readonly float Rotation;

    public PieceTriggerData(int index, Vector2 position, float rotation)
    {
        Index = index;
        Position = position;
        Rotation = rotation;
    }

    public override string ToString()
    {
        return $"Index: {Index}, Position: {Position}, Rotation: {Rotation}";
    }
}