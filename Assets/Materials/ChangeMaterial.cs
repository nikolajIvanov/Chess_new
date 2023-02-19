using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterial : MonoBehaviour
{
    public Material newMaterial; // das neue Material, das in Unity zugewiesen wird

    private MeshRenderer meshRenderer;
    private Material standardMaterial;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        standardMaterial = meshRenderer.materials[1]; // Material mit dem Namen "standard_material" ist das zweite Material in der Liste
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("Key O pressed.");
            if (meshRenderer.material.name.Contains("standard_material"))
            {
                Debug.Log("Switching to new material.");
                meshRenderer.material = newMaterial;
            }
            else
            {
                Debug.Log("Switching to standard material.");
                meshRenderer.material = standardMaterial;
            }
        }
    }

    public void ResetMaterial()
    {
        meshRenderer.materials[1] = standardMaterial; // Material mit dem Namen "standard_material" ist das zweite Material in der Liste
    }
}
