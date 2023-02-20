using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputHandler
{
    /// <summary>
    /// Interface dient für alle Interaktionen mit dem Spiel
    /// </summary>
    /// <param name="inputPosition"></param>
    /// <param name="selectedObject"></param>
    /// <param name="onClick"></param>
    void ProcessInput(Vector3 inputPosition, GameObject selectedObject, Action onClick);
}
