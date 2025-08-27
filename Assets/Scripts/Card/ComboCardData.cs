using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Combotype
{
    Storage,
    UnitFactory,
    UnitCharger,
    Turret,
}

[CreateAssetMenu(fileName = "New Combo Card Data", menuName = "Card System/Combo Card Data")]
public class ComboCardData : ScriptableObject
{
    public Sprite comboIcon;
    public string comboName;
    public GameObject comboPrefab;
    public TileBase comboTile;
    public Combotype comboType;
    
    [TextArea] public string comboDescription;

    [System.Serializable]
    public class ComboPiece
    {
        public GadgetType gadgetType;
        public Vector3Int relativePosition;
    }

    public List<ComboPiece> recipe;
}