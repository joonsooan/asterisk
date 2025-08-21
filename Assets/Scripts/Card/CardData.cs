using UnityEngine;

[System.Serializable]
public class CardCost
{
    public ResourceType resourceType;
    public int amount;
}

public enum GadgetType
{
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
    public Sprite cardImage;
    public string cardName;
    public GameObject buildingPrefab;
    public GadgetType gadgetType;
    
    public CardCost[] costs;
}
