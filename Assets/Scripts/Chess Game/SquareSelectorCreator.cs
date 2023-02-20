using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Regelt alles rund um den Selector (zu finden in dem Order Prefabs > Selector
/// </summary>
public class SquareSelectorCreator : MonoBehaviour
{
	[SerializeField] private Material freeSquareMaterial;
	[SerializeField] private Material enemySquareMaterial;
	[SerializeField] private GameObject selectorPrefab;
	private List<GameObject> instantiatedSelectors = new List<GameObject>();
	/// <summary>
	/// Beim Klicken auf eine Figur wird die Methode aufgerufen, um die Möglichen Züge visuell darzustellen.
	/// </summary>
	/// <param name="squareData"></param>
	public void ShowSelection(Dictionary<Vector3, bool> squareData)
	{
		ClearSelection();
		foreach (var data in squareData)
		{
			GameObject selector = Instantiate(selectorPrefab, data.Key, Quaternion.identity);
			instantiatedSelectors.Add(selector);
			foreach (var setter in selector.GetComponentsInChildren<MaterialSetter>())
			{
				setter.SetSingleMaterial(data.Value ? freeSquareMaterial : enemySquareMaterial);
			}
		}
	}
	/// <summary>
	/// Nach beendindigung des Zugs wird über diese Methode die einzelnen Selektoren entfernt.
	/// </summary>
	public void ClearSelection()
	{
		for (int i = 0; i < instantiatedSelectors.Count; i++)
		{
			Destroy(instantiatedSelectors[i]);
		}
	}
}