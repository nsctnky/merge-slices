using Enums;
using UnityEngine;
using UnityEngine.Events;

public class PieceController : MonoBehaviour
{
    private PieceStates _pieceState;
    private readonly UnityEvent _snapCompletedCallback = new UnityEvent();

    public void Init(UnityAction triggerCallback)
    {
        Debug.Log("New piece get from pool.");
        _snapCompletedCallback.AddListener(triggerCallback);
        _snapCompletedCallback.AddListener(OnSnapCompleted);
        transform.position = Vector3.zero;
        gameObject.SetActive(true);
        _pieceState = PieceStates.Waiting;
    }

    public void Move()
    {
        if (!_pieceState.Equals(PieceStates.Active))
            return;

        Debug.Log("Piece Moving....");
        _pieceState = PieceStates.Moving;
    }

    public void SetPieceStateActive()
    {
        _pieceState = PieceStates.Active;
    }

    private void OnTriggerEnter(Collider other)
    {
        DoSnap();
    }

    private void OnSnapCompleted()
    {
        _snapCompletedCallback.RemoveAllListeners();
        _pieceState = PieceStates.Locked;
    }

    private void DoSnap()
    {
        _snapCompletedCallback?.Invoke();
    }
}