using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// enum wird für Attribute verwendet, die sich nicht mehr ändern.
/// </summary>
public enum TeamColor
{
    Black, White
}
/// <summary>
/// Dieser Code definiert zwei Enums, 'TeamColor' und 'PieceType', um das Team und den Typ der Schachfigur zu
/// spezifizieren. Das Scriptable Object 'BoardLayout' ist ein Asset in Unity, das für das Board-Layout verwendet wird.
/// Es enthält eine interne Klasse 'BoardSquareSetup', die die Position, den Typ und die Farbe der Schachfigur für
/// jedes Feld speichert. Das Scriptable Object hat auch einige Methoden, um auf Informationen in Bezug auf das
/// Board-Layout zuzugreifen, wie die Anzahl der Schachfiguren auf dem Board, die Koordinaten eines Feldes und den
/// Namen und die Farbe der Schachfigur auf einem bestimmten Feld.
/// </summary>
public enum PieceType
{
    PawnBlack, BishopBlack, KnightBlack, RookBlack, QueenBlack, KingBlack,
    PawnWhite, BishopWhite, KnightWhite, RookWhite, QueenWhite, KingWhite
}

/// <summary>
/// Erstellt ein Scriptable Object, welches man in Unity erstellen kann.
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Board/Layout")]
public class BoardLayout : ScriptableObject
{
    [Serializable]
    private class BoardSquareSetup
    {
        public Vector2Int position;
        public PieceType pieceType;
        public TeamColor teamColor;
    }

    [SerializeField] private BoardSquareSetup[] boardSquares;

    public int GetPiecesCount()
    {
        return boardSquares.Length;
    }


    public Vector2Int GetSquareCoordsAtIndex(int index)
    {
        return new Vector2Int(boardSquares[index].position.x - 1, boardSquares[index].position.y - 1);
    }
    public string GetSquarePieceNameAtIndex(int index)
    {
        return boardSquares[index].pieceType.ToString();
    }
    public TeamColor GetSquareTeamColorAtIndex(int index)
    {
        return boardSquares[index].teamColor;
    }

}
