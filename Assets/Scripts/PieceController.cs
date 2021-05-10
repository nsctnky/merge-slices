using System;
using System.Collections;
using Enums;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    [SerializeField]
    private PolygonCollider2D collider;
    [SerializeField]
    private Rigidbody2D rigidbody;
    [SerializeField]
    private float speed = 1f;

    private PieceStates _pieceState;
    private Coroutine _moveCoroutine;

    public int Value { get; private set; }

    public float RotationZ
    {
        get { return transform.localRotation.eulerAngles.z; }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PieceTrigger"))
            return;

        StopCoroutine(_moveCoroutine);
        collider.enabled = false;
        rigidbody.simulated = false;
    }

    public void Init(Transform parent, Transform position)
    {
        Debug.Log("New piece get from pool.");
        collider.enabled = true;
        rigidbody.simulated = true;
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        transform.SetParent(parent);
        transform.localPosition = position.localPosition;
        gameObject.SetActive(true);
        _pieceState = PieceStates.Waiting;
    }

    public void Move()
    {
        if (!_pieceState.Equals(PieceStates.Active))
            return;

        Debug.Log("Piece Moving....");
        _moveCoroutine = StartCoroutine(IterateMove());
        _pieceState = PieceStates.Moving;
    }
    
    public Coroutine StartRotate(float target)
    {
        return StartCoroutine(IterateRotate(target));
    }

    public Coroutine StartSnapping(Vector2 position, float rotation)
    {
        return StartCoroutine(IterateSnap(position, rotation));
    }

    public void SetPieceStateActive()
    {
        _pieceState = PieceStates.Active;
    }

    private void OnSnapCompleted()
    {
        // after snap..
        _pieceState = PieceStates.Locked;
    }

    private IEnumerator IterateRotate(float target)
    {
        Quaternion targetQua = Quaternion.Euler(0, 0, target);
        while (Quaternion.Angle(transform.localRotation, targetQua) > 10f)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetQua, 5f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        
        transform.localRotation = Quaternion.Euler(0, 0, target);
    }

    private IEnumerator IterateMove()
    {
        while (true)
        {
            transform.Translate(Vector2.up * (speed * Time.deltaTime));
            yield return new WaitForEndOfFrame();

            if (_pieceState.Equals(PieceStates.Snapping))
                yield break;
        }
    }

    private IEnumerator IterateSnap(Vector2 position, float rotation)
    {
        _pieceState = PieceStates.Snapping;
        transform.localPosition = position;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
        yield return new WaitForEndOfFrame();
        OnSnapCompleted();
    }
}