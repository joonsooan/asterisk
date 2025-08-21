using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Combo Card Data", menuName = "Card System/Combo Card Data")]
public class ComboCardData : ScriptableObject
{
    public string comboName;
    public GameObject comboPrefab;

    [System.Serializable]
    public class ComboPiece
    {
        public GadgetType gadgetType;
        public Vector3Int relativePosition;
    }

    public List<ComboPiece> recipe;
}