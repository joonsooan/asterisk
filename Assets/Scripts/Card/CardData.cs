using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class CardCost
{
    public ResourceType resourceType;
    public int amount;
}

public enum GadgetType
{
    None,
    MetalPanel,
    ChargeCoil,
    BioFilter,
    Cooler,
    ModularPanel,
    ElectroMagnet,
    BioCultivator,
    CrystalLink,
    AetherCondenser
}

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public GameObject buildingPrefab;
    public GadgetType gadgetType;
    public TileBase gadgetTile;
    [TextArea] public string cardDescription; 
    
    public CardCost[] costs;
}
