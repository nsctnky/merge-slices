using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private CircleController circleController;

    private PieceController _pieceController;

    void Awake()
    {
        GameLoader.GameInitialized.AddListener(OnGameInitialized);
    }

    private void OnDestroy()
    {
        ServiceLocator.GetService<InputControllerBase>().RemoveOnClickedEvent(OnClicked);
    }

    private void OnGameInitialized()
    {
        ServiceLocator.GetService<InputControllerBase>().AddOnClickedEvent(OnClicked);
    }

    private void OnClicked()
    {
        if (_pieceController == null)
            return;
        
        _pieceController.Move();
    }

    private void OnSnapCompleted()
    {
        // after snapped process..
        var x = circleController.SnappedIndex;
        
        PrepareNewPiece();
    }

    private void PrepareNewPiece()
    {
        _pieceController = null;
        _pieceController = GetPiece();
        _pieceController.Init(OnSnapCompleted);
        _pieceController.SetPieceStateActive();
    }

    private PieceController GetPiece()
    {
        return ServiceLocator.GetService<PoolManager>().GetPooledObjectByTag<PieceController>("Piece");
    }

    private bool IsMergeExist()
    {
        return false;
    }
}