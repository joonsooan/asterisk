using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ResourceType
{
    Ferrite,
    Aether,
    Biomass,
    CryoCrystal,
    Solana
}

[System.Serializable]
public class ResourceStats
{
    public ResourceType resourceType;
    public int amountToMine = 100;
    public float timeToMinePerUnit = 0.1f;
}

public class ResourceManager : MonoBehaviour
{
    [Header("Resource Stats")]
    [SerializeField] private List<ResourceStats> resourceStatsList;
    
    [Header("Resource UI")]
    [SerializeField] private TMP_Text ferriteNumber;
    [SerializeField] private TMP_Text aetherNumber;
    [SerializeField] private TMP_Text biomassNumber;
    [SerializeField] private TMP_Text cryoCrystalNumber;
    [SerializeField] private TMP_Text solanaNumber;
    
    private readonly List<ResourceNode> _allResources = new List<ResourceNode>();
    private readonly Dictionary<ResourceType, ResourceStats> _resourceStats = new Dictionary<ResourceType, ResourceStats>();
    private readonly Dictionary<ResourceType, int> _resourceCounts = new Dictionary<ResourceType, int>();

    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeResourceStats();
            ResetResourceCount();
            UpdateAllResourceUI();
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            _resourceCounts[ResourceType.Ferrite] += 100000;
            _resourceCounts[ResourceType.Aether] += 100000;
            _resourceCounts[ResourceType.Biomass] += 100000;
            _resourceCounts[ResourceType.CryoCrystal] += 100000;
            UpdateAllResourceUI();
        }
    }

    private void InitializeResourceStats()
    {
        _resourceStats.Clear();
        foreach (var stats in resourceStatsList) {
            _resourceStats[stats.resourceType] = stats;
        }
    }
    
    public ResourceStats GetResourceStats(ResourceType type)
    {
        if (_resourceStats.TryGetValue(type, out var stats)) {
            return stats;
        }
        return null;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") {
            FindAndConnectUI();
            ResetResourceCount();
            UpdateAllResourceUI();
        }
    }

    private void ResetResourceCount()
    {
        _resourceCounts[ResourceType.Ferrite] = 0;
        _resourceCounts[ResourceType.Aether] = 0;
        _resourceCounts[ResourceType.Biomass] = 0;
        _resourceCounts[ResourceType.CryoCrystal] = 0;
        _resourceCounts[ResourceType.Solana] = 0;
    }

    private void FindAndConnectUI()
    {
        ferriteNumber = GameObject.Find("Resource0_txt")?.GetComponent<TMP_Text>();
        aetherNumber = GameObject.Find("Resource1_txt")?.GetComponent<TMP_Text>();
        biomassNumber = GameObject.Find("Resource2_txt")?.GetComponent<TMP_Text>();
        cryoCrystalNumber = GameObject.Find("Resource3_txt")?.GetComponent<TMP_Text>();
        solanaNumber = GameObject.Find("Resource4_txt")?.GetComponent<TMP_Text>();
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (_resourceCounts.ContainsKey(type)) {
            _resourceCounts[type] += amount;
            UpdateResourceUI(type);
        }
    }

    private bool HasEnoughResources(ResourceType type, int amount)
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
        if (GameManager.Instance == null || ferriteNumber == null || aetherNumber == null || biomassNumber == null || cryoCrystalNumber == null || solanaNumber == null) {
            return;
        }
        
        if (type == ResourceType.Solana)
        {
            int requiredAmount = GameManager.Instance.GetRequiredAmountForCurrentQuota();
            string displayText = $"{_resourceCounts[type]} / {requiredAmount}";
            solanaNumber.text = displayText;
        }
        else
        {
            switch (type)
            {
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
    }

    public void UpdateAllResourceUI()
    {
        if (GameManager.Instance == null || ferriteNumber == null || aetherNumber == null || biomassNumber == null || cryoCrystalNumber == null || solanaNumber == null) {
            return;
        }
        
        ferriteNumber.text = _resourceCounts[ResourceType.Ferrite].ToString();
        aetherNumber.text = _resourceCounts[ResourceType.Aether].ToString();
        biomassNumber.text = _resourceCounts[ResourceType.Biomass].ToString();
        cryoCrystalNumber.text = _resourceCounts[ResourceType.CryoCrystal].ToString();
        
        int requiredAmount = GameManager.Instance.GetRequiredAmountForCurrentQuota();
        string displayText = $"{_resourceCounts[ResourceType.Solana]} / {requiredAmount}";
        solanaNumber.text = displayText;
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

        ResourceSpawner[] spawners = FindObjectsByType<ResourceSpawner>(FindObjectsSortMode.None);
        foreach (ResourceSpawner spawner in spawners) {
            if (spawner != null) {
                spawner.NotifyResourceDestroyed(node);
            }
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

    public int GetResource(ResourceType type)
    {
        if (_resourceCounts.TryGetValue(type, out int value)) {
            return value;
        }
        return 0;
    }
}
