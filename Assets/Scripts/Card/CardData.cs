using UnityEngine;

[System.Serializable]
public class CardCost
{
    public ResourceType resourceType;
    public int amount;
}

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    public Sprite cardImage;
    public GameObject buildingPrefab;
    public string cardName;
    public CardCost[] costs;
}