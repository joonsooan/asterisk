using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Combo Card Data", menuName = "Card System/Combo Card Data")]
public class ComboCardData : ScriptableObject
{
    public Sprite comboIcon;
    public string comboName;
    public GameObject comboPrefab;
    public TileBase comboTile;
    [TextArea] public string comboDescription;

    [System.Serializable]
    public class ComboPiece
    {
        public GadgetType gadgetType;
        public Vector3Int relativePosition;
    }

    public List<ComboPiece> recipe;
}