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
            GivePieceToPool(_pieceController.gameObject);
        }

        AdjacentPiece adj = GetAdjacentPiece(index);
        if (adj.Next != null && adj.Prev != null)
        {
        }
        else if (adj.Prev != null)
        {
            Debug.Log("Merging prev");
            yield return _pieceController.StartRotate(adj.Prev.RotationZ);
            _pieces[index] = null;
            GivePieceToPool(_pieceController.gameObject);
        }
        else if (adj.Next != null)
        {
            Debug.Log("Merging next");
            yield return _pieceController.StartRotate(adj.Next.RotationZ);
            _pieces[index] = null;
            GivePieceToPool(_pieceController.gameObject);
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

    private void GivePieceToPool(GameObject go)
    {
        ServiceLocator.GetService<PoolManager>().GiveObjectToPool(go);
    }
    
    private void ClearArray()
    {
        for (int i = 0; i < _pieces.Length; i++)
            _pieces[i] = null;
    }

    private AdjacentPiece GetAdjacentPiece(int index)
    {
        int prev = index - 1;
        int next = index + 1;

        if (prev < 0)
            prev = _pieces.Length - 1;

        if (next >= _pieces.Length)
            next = 0;

        return new AdjacentPiece(_pieces[prev], _pieces[next]);
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