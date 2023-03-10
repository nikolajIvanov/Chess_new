using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Abstrakte Klasse, die jede Figur erhält. Hier werden die allgemeinen Methoden & Attribute für die einzelnen Figuren
/// gebündelt. Vorteil einer Abstrakten Klasse ist die, dass die Klasse selbst nie Instanziiert werden kann, aber
/// eine "Verallgemeinung" für eine Bestimmte Gruppe darlegen kann. In unserem Fall für die Figuren.
/// </summary>
[RequireComponent(typeof(MaterialSetter))]
[RequireComponent(typeof(IObjectTweener))]
public abstract class Piece : MonoBehaviour
{
	[SerializeField] private MaterialSetter materialSetter;
	public Board board { protected get; set; }
	public Vector2Int occupiedSquare { get; set; }
	public TeamColor team { get; set; }
	public bool hasMoved { get; private set; }
	public List<Vector2Int> avaliableMoves;

	private IObjectTweener tweener;
	
	[SerializeField] public Animator animator;

	public abstract List<Vector2Int> SelectAvaliableSquares();

	/// <summary>
	/// Es werden alle Awake() Methoden vor dem Start des Spiels ausgeführt.
	/// Das Spiel startet in der Klasse ChessGameController mit der Methode Start()
	/// </summary>
	private void Awake()
	{
		avaliableMoves = new List<Vector2Int>();
		tweener = GetComponent<IObjectTweener>();
		materialSetter = GetComponent<MaterialSetter>();
		hasMoved = false;
		animator = GetComponent<Animator>();
	}

	public void SetMaterial(Material selectedMaterial)
	{
		materialSetter.SetSingleMaterial(selectedMaterial);
	}

	public bool IsFromSameTeam(Piece piece)
	{
		return team == piece.team;
	}

	public bool CanMoveTo(Vector2Int coords)
	{
		return avaliableMoves.Contains(coords);
	}

	public virtual void MovePiece(Vector2Int coords)
	{
		Vector3 targetPosition = board.CalculatePositionFromCoords(coords);
		occupiedSquare = coords;
		hasMoved = true;
		tweener.MoveTo(transform, targetPosition);
	}
	
	public virtual void AnimationMovePiece(Vector2Int coords)
	{
		Vector3 targetPosition = board.CalculatePositionFromCoords(coords);
		occupiedSquare = coords;
		hasMoved = true;
		tweener.MoveTo(transform, targetPosition);
	} 


	protected void TryToAddMove(Vector2Int coords)
	{
		avaliableMoves.Add(coords);
	}

	public void SetData(Vector2Int coords, TeamColor team, Board board)
	{
		this.team = team;
		occupiedSquare = coords;
		this.board = board;
		// Das GameObjekt wird auf folgende Position im Bord gestellt
		transform.position = board.CalculatePositionFromCoords(coords);
	}

	public bool IsAttackingPieceOfType<T>() where T : Piece
	{
		foreach (var square in avaliableMoves)
		{
			if (board.GetPieceOnSquare(square) is T)
				return true;
		}
		return false;
	}

}