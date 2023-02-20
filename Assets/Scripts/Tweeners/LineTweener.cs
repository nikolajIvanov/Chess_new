using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTweener : MonoBehaviour, IObjectTweener
{
    [SerializeField] private float movementSpeed;
    /// <summary>
    /// Wird verwendet, um die Figuren zu bewegen
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="targetPosition"></param>
    public void MoveTo(Transform transform, Vector3 targetPosition)
    {
        float distance = Vector3.Distance(targetPosition, transform.position);
        transform.DOMove(targetPosition, distance / movementSpeed);
    }
}