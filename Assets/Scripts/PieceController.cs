using System.Collections;
using Enums;
using TMPro;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    [SerializeField]
    private int value = 2;
    [SerializeField]
    private TextMeshPro title;
    [SerializeField]
    private PolygonCollider2D collider;
    [SerializeField]
    private Rigidbody2D rigidbody;
    [SerializeField]
    private float speed = 1f;

    private PieceStates _pieceState;
    private Coroutine _moveCoroutine;

    public int Value
    {
        get { return value; }
    }

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
        collider.enabled = true;
        rigidbody.simulated = true;
        title.text = value.ToString();
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

        _moveCoroutine = StartCoroutine(IterateMove());
        _pieceState = PieceStates.Moving;
    }

    public Coroutine StartRotate(float target)
    {
        return StartCoroutine(IterateSmoothRotate(target));
    }

    public Coroutine StartSnapping(Vector2 position, float rotation)
    {
        return StartCoroutine(IterateSnap(position, rotation));
    }

    public void SetPieceStateActive()
    {
        _pieceState = PieceStates.Active;
    }

    public void SetValue(int val)
    {
        value = val;
    }
    
    private void OnSnapCompleted()
    {
        // after snap..
        _pieceState = PieceStates.Locked;
    }

    private IEnumerator IterateSmoothRotate(float target)
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
        Quaternion targetQua = Quaternion.Euler(0, 0, rotation);

        while (Vector2.Distance(transform.localPosition, position) > 0.1f)
        {
            transform.localPosition = Vector2.Lerp(transform.localPosition, position, 10f * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetQua, 10f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        transform.localPosition = position;
        transform.localRotation = targetQua;
        title.text = Value.ToString();
        yield return new WaitForEndOfFrame();
        OnSnapCompleted();
    }
}