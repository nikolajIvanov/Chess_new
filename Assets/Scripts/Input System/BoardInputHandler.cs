using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardInputHandler : MonoBehaviour, IInputHandler
{
    private Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }
    /// <summary>
    /// Alle Interaktionen die auf dem Board passieren werden hier geregelt.
    /// </summary>
    /// <param name="inputPosition"></param>
    /// <param name="selectedObject"></param>
    /// <param name="onClick"></param>
    public void ProcessInput(Vector3 inputPosition, GameObject selectedObject, Action onClick)
    {
        board.OnSquareSelected(inputPosition);
    }
}