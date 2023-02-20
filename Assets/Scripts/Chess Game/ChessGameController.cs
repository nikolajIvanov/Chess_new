using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Stellt die Kontrolle des Schachspiels in Unity sicher. Die Klasse ist zuständig für das Erstellen von Spielern,
/// Figuren und dem Spielfeld. Die Methode 'CreatePieceAndInitialize' erstellt eine neue Figur und ordnet sie dem
/// jeweiligen Spieler zu. Die Methode 'StartNewGame' initialisiert das Spiel und setzt den Status auf "Init". Die
/// Methode 'EndTurn' beendet die Runde und prüft, ob das Spiel beendet wurde, indem es überprüft, ob der König noch
/// auf dem Feld ist und ob der Gegner ihn bedroht. Wenn das Spiel beendet ist, wird der Status auf "Finished" gesetzt
/// und die Methode 'OnGameFinished' des UIManager aufgerufen. Außerdem ist es möglich, das Spiel neu zu starten, indem
/// 'RestartGame' aufgerufen wird.
/// </summary>
[RequireComponent(typeof(PiecesCreator))]
public class ChessGameController : MonoBehaviour
{
    private enum GameState
    {
        Init, Play, Finished
    }

    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;
    [SerializeField] private ChessUIManager UIManager;

    private PiecesCreator pieceCreator;
    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer activePlayer;

    private GameState state;
    /// <summary>
    /// Es werden alle Awake() Methoden vor dem Start des Spiels ausgeführt.
    /// Das Spiel startet in der Klasse ChessGameController mit der Methode Start()
    /// Klassen die eine Awake Methode haben:
    /// ChessGameController
    /// Pieces Creator
    /// Board
    /// </summary>
    private void Awake()
    {
        SetDependencies();
        CreatePlayers();
    }

    private void SetDependencies()
    {
        pieceCreator = GetComponent<PiecesCreator>();
    }

    private void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }

    /// <summary>
    /// Das Spiel wird hier gestartet.
    /// </summary>
    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        SetGameState(GameState.Init);
        UIManager.HideUI();
        board.SetDependencies(this);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
        SetGameState(GameState.Play);
    }
    private void SetGameState(GameState state)
    {
        this.state = state;
    }

    internal bool IsGameInProgress()
    {
        return state == GameState.Play;
    }


    /// <summary>
    /// Hier werden die Informationen vom BoardLayout geholt (BoardLayout.cs)
    /// position
    /// pieceType
    /// teamColor
    /// </summary>
    /// <param name="layout"></param>
    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        // Die Schleife durchläuft 32 Durchgänge (layout.GetPiecesCount())
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColor team = layout.GetSquareTeamColorAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);

            Type type = Type.GetType(typeName);
            // Nachdem alle Infos geholt wurden, wird die Figur in der Methode erstellt
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }



    public void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type)
    {
        Piece newPiece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        newPiece.SetData(squareCoords, team, board);
        
        if (newPiece.team == TeamColor.Black)
        {
            newPiece.transform.rotation = Quaternion.Euler(0, 180, 0);
            //newPiece.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        /// Die Methode konnte man für die Unity Asset Figuren nutzen. Das Problem bei uns war, dass jeder die
        /// Materialien anders bezeichnet hat und somit der Aufwand deutlich Größer war.
        /// Learning draus: Organisier dich besser als Team
        /*
        Material teamMaterial = pieceCreator.GetTeamMaterial(team);
        newPiece.SetMaterial(teamMaterial);
        */
        board.SetPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }

    private void GenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.GenerateAllPossibleMoves();
    }

    public bool IsTeamTurnActive(TeamColor team)
    {
        return activePlayer.team == team;
    }
    /// <summary>
    /// Diese Methode wird am Ende jeder Runde aufgerufen und es wird überprüft, ob der König sich noch auf dem Spielfeld
    /// befindet. Falls nicht wird das Spiel beendet.
    /// </summary>
    public void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        if (CheckIfGameIsFinished())
        {
            EndGame();
        }
        else
        {
            ChangeActiveTeam();
        }
    }

    private bool CheckIfGameIsFinished()
    {
        Piece[] kingAttackingPieces = activePlayer.GetPieceAtackingOppositePiceOfType<KingBlack>();
        if (kingAttackingPieces.Length > 0)
        {
            ChessPlayer oppositePlayer = GetOpponentToPlayer(activePlayer);
            Piece attackedKing = oppositePlayer.GetPiecesOfType<KingBlack>().FirstOrDefault();
            oppositePlayer.RemoveMovesEnablingAttakOnPieceOfType<KingBlack>(activePlayer, attackedKing);

            int avaliableKingMoves = attackedKing.avaliableMoves.Count;
            if (avaliableKingMoves == 0)
            {
                bool canCoverKing = oppositePlayer.CanHidePieceFromAttack<KingBlack>(activePlayer);
                if (!canCoverKing)
                    return true;
            }
        }
        return false;
    }

    private void EndGame()
    {
        SetGameState(GameState.Finished);
        UIManager.OnGameFinished(activePlayer.team.ToString());
    }

    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestarted();
        whitePlayer.OnGameRestarted();
        blackPlayer.OnGameRestarted();
        StartNewGame();
    }

    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(p => Destroy(p.gameObject));
        blackPlayer.activePieces.ForEach(p => Destroy(p.gameObject));
    }

    private void ChangeActiveTeam()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    internal void OnPieceRemoved(Piece piece)
    {
        ChessPlayer pieceOwner = (piece.team == TeamColor.White) ? whitePlayer : blackPlayer;
        pieceOwner.RemovePiece(piece);
    }

    internal void RemoveMovesEnablingAttakOnPieceOfType<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveMovesEnablingAttakOnPieceOfType<T>(GetOpponentToPlayer(activePlayer), piece);
    }
}

