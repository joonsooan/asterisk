using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ResourceType
{
    Ferrite,
    Aether,
    Biomass,
    CryoCrystal
}

public class ResourceManager : MonoBehaviour
{
    [Header("Resource UI")]
    [SerializeField] private TMP_Text ferriteNumber;
    [SerializeField] private TMP_Text aetherNumber;
    [SerializeField] private TMP_Text biomassNumber;
    [SerializeField] private TMP_Text cryoCrystalNumber;
    
    private readonly Dictionary<ResourceType, int> _resourceCounts = new Dictionary<ResourceType, int>();
    private readonly List<ResourceNode> _allResources = new List<ResourceNode>();
    
    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _resourceCounts[ResourceType.Ferrite] = 0;
            _resourceCounts[ResourceType.Aether] = 0;
            _resourceCounts[ResourceType.Biomass] = 0;
            _resourceCounts[ResourceType.CryoCrystal] = 0;
            
            UpdateAllResourceUI();
        }
        else {
            Destroy(gameObject);
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (_resourceCounts.ContainsKey(type)) {
            _resourceCounts[type] += amount;
            UpdateResourceUI(type);
        }
    }

    public bool HasEnoughResources(ResourceType type, int amount)
    {
        if (_resourceCounts.ContainsKey(type)) {
            return _resourceCounts[type] >= amount;
        }
        return false;
    }

    public void SpendResources(ResourceType type, int amount)
    {
        if (HasEnoughResources(type, amount)) {
            _resourceCounts[type] -= amount;
            UpdateResourceUI(type);
        }
    }
    
    public bool HasEnoughResources(CardCost[] costs)
    {
        foreach (CardCost cost in costs) {
            if (!HasEnoughResources(cost.resourceType, cost.amount)) {
                return false;
            }
        }
        return true;
    }

    public void SpendResources(CardCost[] costs)
    {
        if (HasEnoughResources(costs)) {
            foreach (CardCost cost in costs) {
                SpendResources(cost.resourceType, cost.amount);
            }
        }
    }

    private void UpdateResourceUI(ResourceType type)
    {
        switch (type) {
            case ResourceType.Ferrite:
                ferriteNumber.text = _resourceCounts[type].ToString();
                break;
            case ResourceType.Aether:
                aetherNumber.text = _resourceCounts[type].ToString();
                break;
            case ResourceType.Biomass:
                biomassNumber.text = _resourceCounts[type].ToString();
                break;
            case ResourceType.CryoCrystal:
                cryoCrystalNumber.text = _resourceCounts[type].ToString();
                break;
        }
    }
    
    private void UpdateAllResourceUI()
    {
        ferriteNumber.text = _resourceCounts[ResourceType.Ferrite].ToString();
        aetherNumber.text = _resourceCounts[ResourceType.Aether].ToString();
        biomassNumber.text = _resourceCounts[ResourceType.Biomass].ToString();
        cryoCrystalNumber.text = _resourceCounts[ResourceType.CryoCrystal].ToString();
    }

    public void AddResourceNode(ResourceNode node)
    {
        if (!_allResources.Contains(node)) {
            _allResources.Add(node);
        }
    }

    public void RemoveResourceNode(ResourceNode node)
    {
        if (_allResources.Contains(node)) {
            _allResources.Remove(node);
        }
    }
    
    public void RemoveResourceNodeTile(Vector3Int cellPosition)
    {
        BuildingManager.Instance.RemoveResourceTile(cellPosition);
    }

    public List<ResourceNode> GetAllResources()
    {
        return _allResources;
    }
}