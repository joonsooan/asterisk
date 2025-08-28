using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ComboType
{
    Storage,
    Generator,
    ChargeStation,
    Harvester,
}

[CreateAssetMenu(fileName = "New Combo Card Data", menuName = "Card System/Combo Card Data")]
public class ComboCardData : DisplayableData
{
    [Header("Combo-Specific Info")]
    public GameObject comboPrefab;
    public TileBase comboTile;
    public ComboType comboType;
    public List<ComboPiece> recipe;

    [System.Serializable]
    public class ComboPiece
    {
        public GadgetType gadgetType;
        public Vector3Int relativePosition;
    }
}