using System.Collections;
using Enums;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private CircleController circleController;
    [SerializeField]
    private Transform pieceBulletPosition;
    [SerializeField]
    private int nextValue;
    [SerializeField]
    private PieceController[] _pieces = new PieceController[8];
    private PieceController _pieceController;
    private int _lastIndex;
    private Coroutine _mergeCoroutine;

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
        _lastIndex = triggerData.Index;
        if (_pieces[_lastIndex] == null)
        {
            _pieces[_lastIndex] = _pieceController;
        }
        else
        {
            GivePieceToPool(_pieceController);
        }

        while (IsMergeExist(_lastIndex))
        {
            yield return StartMerge(_lastIndex);
            yield return new WaitForEndOfFrame();

            if (_mergeCoroutine != null)
                StopCoroutine(_mergeCoroutine);
        }

        OnSnapCompleted();
    }

    private Coroutine StartMerge(int index)
    {
        _mergeCoroutine = StartCoroutine(IterateMerge(index));
        return _mergeCoroutine;
    }

    private IEnumerator IterateMerge(int index)
    {
        Debug.Log("Merge starting");
        AdjacentPiece adj = GetAdjacentPiece(index);
        bool isPrevEquals = adj.Prev != null && adj.Prev.Value.Equals(_pieceController.Value);
        bool isNextEquals = adj.Next != null && adj.Next.Value.Equals(_pieceController.Value);
        
        if (isPrevEquals && isNextEquals)
        {
            int rnd = Randomize();
            PieceController randomAdj = rnd == 0 ? adj.Next : adj.Prev;
            if (Randomize() == 1)
            {
                yield return _pieceController.StartRotate(randomAdj.RotationZ);
                GivePieceToPool(_pieceController);
                _pieces[index] = null;
                _lastIndex = rnd == 0 ? GetNextIndex(index) : GetPrevIndex(index);
            }
            else
            {
                yield return randomAdj.StartRotate(_pieceController.RotationZ);
                int i = rnd == 0 ? GetNextIndex(index) : GetPrevIndex(index);
                GivePieceToPool(randomAdj);
                _pieces[i] = null;
                _lastIndex = index;
            }

            Debug.Log("Merge Finished");
            yield break;
        }

        if (isPrevEquals)
        {
            Debug.Log("Merge Prev");
            int prev = GetPrevIndex(index);
            if (Randomize() == 1)
            {
                yield return _pieceController.StartRotate(adj.Prev.RotationZ);
                GivePieceToPool(_pieceController);
                _pieces[index] = null;
                _lastIndex = prev;
            }
            else
            {
                yield return adj.Prev.StartRotate(_pieceController.RotationZ);
                GivePieceToPool(adj.Prev);
                _pieces[prev] = null;
                _lastIndex = index;
            }
        }
        else if (isNextEquals)
        {
            Debug.Log("Merge Next");
            int next = GetNextIndex(index);
            if (Randomize() == 1)
            {
                yield return _pieceController.StartRotate(adj.Next.RotationZ);
                GivePieceToPool(_pieceController);
                _pieces[index] = null;
                _lastIndex = next;
            }
            else
            {
                yield return adj.Next.StartRotate(_pieceController.RotationZ);
                GivePieceToPool(adj.Next);
                _pieces[next] = null;
                _lastIndex = index;
            }
        }

        Debug.Log("Merge Finished");
        yield break;
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
        PieceController piece = ServiceLocator.GetService<PoolManager>().GetPooledObjectByTag<PieceController>("Piece");
        piece.SetValue(nextValue);
        return piece;
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

    private bool IsMergeExist(int index)
    {
        AdjacentPiece adj = GetAdjacentPiece(index);
        bool isPrevEquals = adj.Prev != null && adj.Prev.Value.Equals(_pieceController.Value);
        bool isNextEquals = adj.Next != null && adj.Next.Value.Equals(_pieceController.Value);
        return isNextEquals || isPrevEquals;
    }

    private int Randomize()
    {
        return Random.Range(0, 2);
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