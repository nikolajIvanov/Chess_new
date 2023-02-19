﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(SquareSelectorCreator))]
public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;

    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;

    private Piece[,] grid;
    private Piece selectedPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;
    // public Animator mAnimator;


    private void Awake()
    {
        squareSelector = GetComponent<SquareSelectorCreator>();
        CreateGrid();
    }

    public void SetDependencies(ChessGameController chessController)
    {
        this.chessController = chessController;
    }



    private void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

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
        chessController.RemoveMovesEnablingAttakOnPieceOfType<King>(piece);
        selectedPiece = piece;
        List<Vector2Int> selection = selectedPiece.avaliableMoves;
        ShowSelectionSquares(selection);
    }

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
            /*
            switch (piece.tag)
            {
                case "Pawn":
                    if (piece.mAnimator != null)
                    {
                        piece.mAnimator.SetTrigger("Pawn_Death");
                        StartCoroutine(WaitAndTakePiece(piece, 52f));
                    }
                    break;
                case "Rook":
                    if (piece.mAnimator != null)
                    {
                        piece.mAnimator.SetTrigger("Rook_destroy");
                        StartCoroutine(WaitAndTakePiece(piece, 2f));
                    }
                    break;
                case "King":
                    if (piece.mAnimator != null)
                    {
                        piece.mAnimator.SetTrigger("King_death");
                        StartCoroutine(WaitAndTakePiece(piece, 2f));
                    }
                    break;
                default:
                    TakePiece(piece);
                    break;
            }
            */
            if (selectedPiece.tag == "Queen" && piece.CompareTag("Rook"))
            {
                if (selectedPiece.mAnimator != null)
                {
                    selectedPiece.mAnimator.SetTrigger("Queen_kill");
                    piece.mAnimator.SetTrigger("Rook_destroy");
                    StartCoroutine(WaitAndTakePiece(piece, 2f));
                    
                }
            }
            else
            {
                TakePiece(piece);
            }
        }
    }
    
    
    private IEnumerator WaitAndTakePiece(Piece piece, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        TakePiece(piece);
    }

    private void TakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            chessController.OnPieceRemoved(piece);
            Destroy(piece.gameObject);
        }
    }


    public void PromotePiece(Piece piece)
    {
        if (piece.tag == "Pawn")
        {
            if (piece.mAnimator != null)
            {
                piece.mAnimator.SetTrigger("Pawn_Promotion");
                StartCoroutine(WaitAndTakePiece(piece, 2f));
            }
        }
        TakePiece(piece);
        if (piece.team ==TeamColor.Black)
        {
            chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(QueenBlack));
        }
        else
        {
            chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(QueenWhite));
        }
        
    }

    internal void OnGameRestarted()
    {
        selectedPiece = null;
        CreateGrid();
    }

}
