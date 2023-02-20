using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PiecesCreator : MonoBehaviour
{
    /// <summary>
    /// [SerializeField] wird verwendet, um die die Informationen in den Variablen über das Spiel aufrechtzuerhalten
    /// bzw. anzupassen.
    /// </summary>
    [SerializeField] private GameObject[] piecesPrefabs;
    [SerializeField] private Material blackMaterial;
    [SerializeField] private Material whiteMaterial;
    
    /// <summary>
    /// Dict in dem die Namen der Figuren mit dem jeweiligen GameObject als Value hinterlegt werden
    /// </summary>
    private Dictionary<string, GameObject> nameToPieceDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        foreach (var piece in piecesPrefabs)
        {
            nameToPieceDict.Add(piece.GetComponent<Piece>().GetType().ToString(), piece);
        }
    }

    public GameObject CreatePiece(Type type)
    {
        GameObject prefab = nameToPieceDict[type.ToString()];
        // Es wird überprüft ob es die Figur gibt
        if (prefab)
        {
            // Hier wird die Figur erstellt
            GameObject newPiece = Instantiate(prefab);
            return newPiece;
        }
        return null;
    }
    /// <summary>
    /// Übergibt den Spielfiguren die zugewiesenen Farben. Es folgt eine Abfrage in der geprüft wird, ob das Übergebene
    /// Team die Farbe "White" hat, wenn ja wird das ihm das "whiteMaterial" zurückgegeben,wenn nein das "blackMaterial"
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public Material GetTeamMaterial(TeamColor team)
    {
        return team == TeamColor.White ? whiteMaterial : blackMaterial;
    }
}
