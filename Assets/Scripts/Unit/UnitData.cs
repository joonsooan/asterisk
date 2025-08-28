using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Unit/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Base Information")]
    public string unitName;
    public GameObject unitPrefab;

    [Header("Production Costs")]
    public float productionTime = 3f;
    public CardCost[] productionCosts;
}