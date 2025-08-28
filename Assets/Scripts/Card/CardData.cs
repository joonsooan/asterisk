using UnityEngine;
using UnityEngine.Tilemaps;

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

[System.Serializable]
public class CardCost
{
    public ResourceType resourceType;
    public int amount;
}

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Card Data")]
public class CardData : DisplayableData
{
    [Header("Card-Specific Info")]
    public GameObject buildingPrefab;
    public GadgetType gadgetType;
    public TileBase gadgetTile;
    public CardCost[] costs;
}