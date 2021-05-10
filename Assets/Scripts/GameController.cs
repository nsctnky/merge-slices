using System.Collections;
using Enums;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private CircleController circleController;
    [SerializeField]
    private Transform pieceBulletPosition;

    private readonly PieceController[] _pieces = new PieceController[8];
    private PieceController _pieceController;

    private void Awake()
    {
        GameLoader.GameInitialized.AddListener(OnGameInitialized);
        circleController.AddTriggerListener(OnTriggerInvoke);
    }

    private void OnDestroy()
    {
        ServiceLocator.GetService<InputControllerBase>().RemoveOnClickedEvent(OnClicked);
    }

    private void OnGameInitialized()
    {
        ServiceLocator.GetService<InputControllerBase>().AddOnClickedEvent(OnClicked);
        ServiceLocator.GetService<EventManager>().OnGameStatesChangedEvent.AddListener(OnGameStateChanged);
    }

    private void OnClicked()
    {
        if (_pieceController == null)
            return;

        _pieceController.Move();
    }

    private void OnGameStateChanged(GameStates state)
    {
        if (!state.Equals(GameStates.Playing))
            return;

        PrepareNewPiece();
        _pieceController.SetPieceStateActive();
    }

    private void OnTriggerInvoke(PieceTriggerData triggerData)
    {
        StartCoroutine(WaitForSnap(triggerData));
    }

    private IEnumerator WaitForSnap(PieceTriggerData triggerData)
    {
        _pieceController.transform.SetParent(circleController.transform);
        yield return _pieceController.StartSnapping(triggerData.Position, triggerData.Rotation);
        yield return StartMerge(triggerData.Index);
        OnSnapCompleted();
    }

    private Coroutine StartMerge(int index)
    {
        return StartCoroutine(IterateMerge(index));
    }

    private IEnumerator IterateMerge(int index)
    {
        if (_pieces[index] == null)
        {
            _pieces[index] = _pieceController;
        }
        else
        {
            GivePieceToPool(_pieceController);
        }

        AdjacentPiece adj = GetAdjacentPiece(index);
        if (adj.Next != null && adj.Prev != null)
        {
            Debug.Log("Merging both");
            yield return adj.Prev.StartRotate(_pieceController.RotationZ);
            yield return adj.Next.StartRotate(_pieceController.RotationZ);

            GivePieceToPool(_pieces[GetPrevIndex(index)]);
            GivePieceToPool(_pieces[GetNextIndex(index)]);
            _pieces[GetPrevIndex(index)] = null;
            _pieces[GetNextIndex(index)] = null;
        }
        else if (adj.Prev != null)
        {
            Debug.Log("Merging prev");
            yield return _pieceController.StartRotate(adj.Prev.RotationZ);
            _pieces[index] = null;
            GivePieceToPool(_pieceController);
        }
        else if (adj.Next != null)
        {
            Debug.Log("Merging next");
            yield return _pieceController.StartRotate(adj.Next.RotationZ);
            _pieces[index] = null;
            GivePieceToPool(_pieceController);
        }

        // do merges
        yield return new WaitForEndOfFrame();
    }

    private void OnSnapCompleted()
    {
        // after snapped process..
        PrepareNewPiece();
        _pieceController.SetPieceStateActive();
    }

    private void PrepareNewPiece()
    {
        _pieceController = null;
        _pieceController = GetPiece();
        _pieceController.Init(transform, pieceBulletPosition);
    }

    private PieceController GetPiece()
    {
        return ServiceLocator.GetService<PoolManager>().GetPooledObjectByTag<PieceController>("Piece");
    }

    private void GivePieceToPool(PieceController go)
    {
        go.StopAllCoroutines();
        ServiceLocator.GetService<PoolManager>().GiveObjectToPool(go.gameObject);
    }

    private void ClearArray()
    {
        for (int i = 0; i < _pieces.Length; i++)
            _pieces[i] = null;
    }

    private AdjacentPiece GetAdjacentPiece(int index)
    {
        int prev = GetPrevIndex(index);
        int next = GetNextIndex(index);

        return new AdjacentPiece(_pieces[prev], _pieces[next]);
    }

    private int GetPrevIndex(int index)
    {
        int prev = index - 1;

        if (prev < 0)
            prev = _pieces.Length - 1;

        return prev;
    }

    private int GetNextIndex(int index)
    {
        int next = index + 1;

        if (next >= _pieces.Length)
            next = 0;

        return next;
    }
}

public readonly struct AdjacentPiece
{
    public readonly PieceController Prev;
    public readonly PieceController Next;

    public AdjacentPiece(PieceController prev, PieceController next)
    {
        Prev = prev;
        Next = next;
    }
}