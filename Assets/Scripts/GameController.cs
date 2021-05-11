using System.Collections;
using Enums;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private CircleController circleController;
    [SerializeField]
    private Transform pieceBulletPosition;
    [SerializeField]
    private GameObject infoText;
    private readonly PieceController[] _pieces = new PieceController[8];
    private PieceController _pieceController;
    private int _lastIndex;
    private Coroutine _mergeCoroutine;
    private int _currentLevel = 1;

    private void Awake()
    {
        GameLoader.GameInitialized.AddListener(OnGameInitialized);
        circleController.AddTriggerListener(OnTriggerInvoke);
    }

    private void OnDestroy()
    {
        ServiceLocator.GetService<InputControllerBase>().RemoveOnClickedEvent(OnClicked);
        ServiceLocator.GetService<EventManager>().OnGameStatesChangedEvent.RemoveListener(OnGameStateChanged);
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

        infoText.gameObject.SetActive(false);
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
            UpdatePieceValue(_pieces[_lastIndex]);
            GivePieceToPool(_pieceController);
            _pieceController = _pieces[_lastIndex];
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
        if (_mergeCoroutine != null)
            StopCoroutine(_mergeCoroutine);

        _mergeCoroutine = StartCoroutine(IterateMerge(index));
        return _mergeCoroutine;
    }

    private IEnumerator IterateMerge(int index)
    {
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
                UpdatePieceValue(randomAdj);
                _pieceController = randomAdj;
                _pieces[index] = null;
                _lastIndex = rnd == 0 ? GetNextIndex(index) : GetPrevIndex(index);
            }
            else
            {
                yield return randomAdj.StartRotate(_pieceController.RotationZ);
                int i = rnd == 0 ? GetNextIndex(index) : GetPrevIndex(index);
                GivePieceToPool(randomAdj);
                UpdatePieceValue(_pieceController);
                _pieces[i] = null;
                _lastIndex = index;
            }

            yield break;
        }

        if (isPrevEquals)
        {
            int prev = GetPrevIndex(index);
            if (Randomize() == 1)
            {
                yield return _pieceController.StartRotate(adj.Prev.RotationZ);
                UpdatePieceValue(adj.Prev);
                GivePieceToPool(_pieceController);
                _pieceController = adj.Prev;
                _pieces[index] = null;
                _lastIndex = prev;
            }
            else
            {
                yield return adj.Prev.StartRotate(_pieceController.RotationZ);
                UpdatePieceValue(_pieceController);
                GivePieceToPool(adj.Prev);
                _pieces[prev] = null;
                _lastIndex = index;
            }
        }
        else if (isNextEquals)
        {
            int next = GetNextIndex(index);
            if (Randomize() == 1)
            {
                yield return _pieceController.StartRotate(adj.Next.RotationZ);
                UpdatePieceValue(adj.Next);
                GivePieceToPool(_pieceController);
                _pieceController = adj.Next;
                _pieces[index] = null;
                _lastIndex = next;
            }
            else
            {
                yield return adj.Next.StartRotate(_pieceController.RotationZ);
                UpdatePieceValue(_pieceController);
                GivePieceToPool(adj.Next);
                _pieces[next] = null;
                _lastIndex = index;
            }
        }
    }

    private void OnSnapCompleted()
    {
        PrepareNewPiece();
        _pieceController.SetPieceStateActive();
    }

    private void PrepareNewPiece()
    {
        _pieceController = null;
        _pieceController = GetPiece();
        _pieceController.SetValue(Random.Range(1, _currentLevel + 1));
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

    private void UpdatePieceValue(PieceController piece)
    {
        int pow = piece.Value + 1;

        if (pow > 5)
            pow = 5;

        piece.SetValue(pow);
        _currentLevel = pow;
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