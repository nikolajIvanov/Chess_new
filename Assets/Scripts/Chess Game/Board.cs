using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
/// <summary>
/// Wird verwendet, um ein 8x8 Schachbrett zu erstellen und zu verwalten. Verfügt über Funktionen zum Erstellen und
/// Verwalten von Schachfiguren, zur Umrechnung von Koordinaten in Vector3-Positionen und umgekehrt sowie zur
/// Überprüfung, ob sich Schachfiguren auf einem bestimmten Quadrat befinden. Das Skript wird auch verwendet, um die
/// Züge der Schachfiguren zu verwalten und zu überwachen, einschließlich der Anzeige der möglichen Züge und der
/// Behandlung von Kollisionen zwischen Figuren. Sowie den aktuellen Spielzug zu beenden und an den anderen Spieler
/// zu übergeben.
/// </summary>
[RequireComponent(typeof(SquareSelectorCreator))]
public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;
    
    /// <summary>
    /// Es handelt sich hierbei um ein Empty Object, welches aufs Board ganz unten Links gesetzt wird.
    /// Dies dient als Grundlage für das Gridsystem und für die kalkulation der einzelnen Positionen.
    /// </summary>
    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;

    private Piece[,] grid;
    private Piece selectedPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;

    private Vector3 cameraPosition;
    private Quaternion cameraRotation;



    private void Awake()
    {
        squareSelector = GetComponent<SquareSelectorCreator>();
        CreateGrid();
        cameraPosition = Camera.main.transform.position;
        cameraRotation = Camera.main.transform.rotation;
    }

    public void SetDependencies(ChessGameController chessController)
    {
        this.chessController = chessController;
    }


    /// <summary>
    /// Zur Vereinfachung der Bewegungen wurde ein 8x8 Koordinaten System erstellt, die in die jeweiligen Positionen
    /// umgerechnet werden.
    /// </summary>
    private void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    /// <summary>
    /// Hier werden die Koordinaten in Vector3 Positionen umgerechnet. Grundlage hierfür ist die squareSize, die
    /// 
    /// </summary>
    /// <param name="coords"></param>
    /// <returns></returns>
    public Vector3 CalculatePositionFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
    }

    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).x / squareSize) + BOARD_SIZE / 2;
        int y = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).z / squareSize) + BOARD_SIZE / 2;
        return new Vector2Int(x, y);
    }

    public void OnSquareSelected(Vector3 inputPosition)
    {
        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
                DeselectPiece();
            else if (piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team))
                SelectPiece(piece);
            else if (selectedPiece.CanMoveTo(coords))
                OnSelectedPieceMoved(coords, selectedPiece);
        }
        else
        {
            if (piece != null && chessController.IsTeamTurnActive(piece.team))
                SelectPiece(piece);
        }
    }
    
    private void SelectPiece(Piece piece)
    {
        chessController.RemoveMovesEnablingAttakOnPieceOfType<KingBlack>(piece);
        selectedPiece = piece;
        List<Vector2Int> selection = selectedPiece.avaliableMoves;
        ShowSelectionSquares(selection);
    }
    
    /// <summary>
    /// Sorgt dafür, dass die Möglichen Züge visuell angezeigt werden.
    /// </summary>
    /// <param name="selection"></param>
    private void ShowSelectionSquares(List<Vector2Int> selection)
    {
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for (int i = 0; i < selection.Count; i++)
        {
            Vector3 position = CalculatePositionFromCoords(selection[i]);
            bool isSquareFree = GetPieceOnSquare(selection[i]) == null;
            squaresData.Add(position, isSquareFree);
        }
        squareSelector.ShowSelection(squaresData);
    }

    private void DeselectPiece()
    {
        selectedPiece = null;
        squareSelector.ClearSelection();
    }
    private void OnSelectedPieceMoved(Vector2Int coords, Piece piece)
    {
        TryToTakeOppositePiece(coords);
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(coords);
        DeselectPiece();
        EndTurn();
    }

    private void EndTurn()
    {
        chessController.EndTurn();
    }

    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newCoords.x, newCoords.y] = newPiece;
    }

    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        if (CheckIfCoordinatesAreOnBoard(coords))
            return grid[coords.x, coords.y];
        return null;
    }

    public bool CheckIfCoordinatesAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
            return false;
        return true;
    }

    public bool HasPiece(Piece piece)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (grid[i, j] == piece)
                    return true;
            }
        }
        return false;
    }

    public void SetPieceOnBoard(Vector2Int coords, Piece piece)
    {
        if (CheckIfCoordinatesAreOnBoard(coords))
            grid[coords.x, coords.y] = piece;
    }
    /// <summary>
    /// Diese Methode wird aufgerufen, wenn ein Spieler eine Figur des Gegnerischen Teams zerstört
    /// </summary>
    /// <param name="coords"></param>
    private  void TryToTakeOppositePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece && !selectedPiece.IsFromSameTeam(piece))
        {
            if (selectedPiece.CompareTag("Queen") && piece.CompareTag("Rook"))
            {
                if (selectedPiece.animator != null)
                {
                    /// Mit dieser Kameraeinstellung kann man die Animation besser verfolgen.
                    //Camera.main.transform.position = selectedPiece.transform.position + new Vector3(5.5f, 4f, -2f);
                    //Camera.main.transform.rotation = Quaternion.Euler(20f, -90f, 0f);
                    
                    //selectedPiece.MovePiece(piece.occupiedSquare + new Vector2Int(1, 3));
                    selectedPiece.animator.SetTrigger("Queen_kill");
                    piece.animator.SetTrigger("Rook_destroy");
                    StartCoroutine(WaitAndTakePiece(piece, 5f));
                }
            }
            else
            {
                TakePiece(piece);
            }
        }
    }
    
    /// <summary>
    /// IEnumerator Methoden werden genutzt um "Coroutine" starten zu können.
    /// Diese Methode sollte genutzt werden, damit man die Animation genauer sieht. Sie hat am Ende nicht ganz so
    /// funktioniert wie sie sollte.
    /// </summary>
    /// <param name="piece"></param>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    private IEnumerator WaitAndTakePiece(Piece piece, float waitTime)
    {
        Camera.main.transform.position = cameraPosition;
        Camera.main.transform.rotation = cameraRotation;
        yield return new WaitForSeconds(waitTime);
        Camera.main.transform.position = cameraPosition;
        Camera.main.transform.rotation = cameraRotation;
        TakePiece(piece);
    }
    /// <summary>
    /// Wenn eine Figur geschlagen wurde oder ein Bauer auf die andere Seite schafft, wird diese Methode verwendet,
    /// um in Physisch vom Board zu entfernen.
    /// </summary>
    /// <param name="piece"></param>
    private void TakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            chessController.OnPieceRemoved(piece);
            Destroy(piece.gameObject);
        }
    }

    /// <summary>
    /// Methode wird aufgerufen, wenn ein Bauer es bis ans Ende eines Board schafft und zur Königin wird. Dabei wird
    /// die Coroutine WaitAndTakePiece() aufgerufen.
    /// </summary>
    /// <param name="piece"></param>
    public void PromotePiece(Piece piece)
    {
        if (piece.CompareTag("Pawn"))
        {
            if (piece.animator != null)
            {
                piece.animator.SetTrigger("Pawn_Promotion");
                StartCoroutine(WaitAndTakePiece(piece, 2f));
            }
        }
        
        if (piece.team ==TeamColor.Black)
        {
            chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(QueenBlack));
        }
        else
        {
            chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(QueenWhite));
        }
        
    }
    /// <summary>
    /// Wenn das Spiel neu gestartet wird, erstellt diese Methode ein neues Grid für das Board.
    /// </summary>
    internal void OnGameRestarted()
    {
        selectedPiece = null;
        CreateGrid();
    }
}
