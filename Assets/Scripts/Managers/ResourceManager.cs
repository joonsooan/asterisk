using System;
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

[Serializable]
public class ResourceStats
{
    public ResourceType resourceType;
    public int amountToMine = 100;
    public float timeToMinePerUnit = 0.1f;
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public static event Action OnNewStorageAdded;
    public static event Action<IStorage> OnStorageRemoved;

    [Header("Resource Start Values")]
    [SerializeField] private int ferriteInitialAmount;
    [SerializeField] private int aetherInitialAmount;
    [SerializeField] private int biomassInitialAmount;
    [SerializeField] private int cryoCrystalInitialAmount;
    [SerializeField] private int solanaInitialAmount;

    [Header("Resource Icons")]
    [SerializeField] private List<Sprite> resourceIcons;

    [Header("Resource Stats")]
    [SerializeField] private List<ResourceStats> resourceStatsList;

    [Header("Resource UI")]
    [SerializeField] private TMP_Text ferriteNumber;
    [SerializeField] private TMP_Text aetherNumber;
    [SerializeField] private TMP_Text biomassNumber;
    [SerializeField] private TMP_Text cryoCrystalNumber;
    [SerializeField] private TMP_Text solanaNumber;

    private readonly List<ResourceNode> _allResources = new();
    private readonly List<IStorage> _allStorages = new();
    private readonly Dictionary<ResourceType, ResourceStats> _resourceStats = new();
    private readonly Dictionary<ResourceType, int> _resourceCounts = new();
    private MainStructure _mainStructure;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.C))
        {
            AddCheatResources();
        }
#endif
    }
    
    private void Initialize()
    {
        InitializeResourceStats();
        ResetResourceCount();
    }

    private void InitializeResourceStats()
    {
        _resourceStats.Clear();
        foreach (var stats in resourceStatsList)
        {
            _resourceStats[stats.resourceType] = stats;
        }
    }

    private void ResetResourceCount()
    {
        _resourceCounts[ResourceType.Ferrite] = ferriteInitialAmount;
        _resourceCounts[ResourceType.Aether] = aetherInitialAmount;
        _resourceCounts[ResourceType.Biomass] = biomassInitialAmount;
        _resourceCounts[ResourceType.CryoCrystal] = cryoCrystalInitialAmount;
        _resourceCounts[ResourceType.Solana] = solanaInitialAmount;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            _mainStructure = null;
            _allStorages.Clear();
            _allResources.Clear();
            FindAndConnectUI();
            ResetResourceCount();
            UpdateAllResourceUI();
        }
    }
    
    public void AddResource(ResourceType type, int amount)
    {
        if (_resourceCounts.ContainsKey(type))
        {
            _resourceCounts[type] += amount;
            UpdateResourceUI(type);
        }
    }

    public bool SpendResources(CardCost[] costs)
    {
        if (!HasEnoughResources(costs))
        {
            Debug.Log("Not Enough Resources");
            return false;
        }

        var requiredResources = new Dictionary<ResourceType, int>();
        foreach (var cost in costs)
        {
            if (requiredResources.ContainsKey(cost.resourceType))
            {
                requiredResources[cost.resourceType] += cost.amount;
            }
            else
            {
                requiredResources.Add(cost.resourceType, cost.amount);
            }
        }

        var availableInStorages = new Dictionary<ResourceType, int>();
        var storages = GetAllStorages();
        foreach (var type in requiredResources.Keys)
        {
            int totalInStorages = 0;
            foreach (var storage in storages)
            {
                totalInStorages += storage.GetCurrentResourceAmount(type);
            }
            availableInStorages[type] = totalInStorages;
        }

        foreach (var req in requiredResources)
        {
            if (GetResource(req.Key) < req.Value)
            {
                Debug.Log($"Not Enough Resources for {req.Key} after final check.");
                return false;
            }
        }

        foreach (var cost in costs)
        {
            _resourceCounts[cost.resourceType] -= cost.amount;

            int remainingToWithdraw = cost.amount;
            foreach (var storage in storages)
            {
                if (remainingToWithdraw <= 0) break;
                if (storage.TryWithdrawResource(cost.resourceType, remainingToWithdraw, out int amountWithdrawn))
                {
                    remainingToWithdraw -= amountWithdrawn;
                }
            }
            
            UpdateResourceUI(cost.resourceType);
        }

        return true;
    }

    public bool SpendResources(ResourceType type, int amount)
    {
        return SpendResources(new[] { new CardCost { resourceType = type, amount = amount } });
    }

    public bool HasEnoughResources(CardCost[] costs)
    {
        foreach (var cost in costs)
        {
            if (_resourceCounts.GetValueOrDefault(cost.resourceType) < cost.amount)
            {
                return false;
            }
        }
        return true;
    }

    public int GetResource(ResourceType type)
    {
        _resourceCounts.TryGetValue(type, out int value);
        return value;
    }
    
    public ResourceStats GetResourceStats(ResourceType type)
    {
        _resourceStats.TryGetValue(type, out var stats);
        return stats;
    }
    
    public void AddStorage(IStorage storage)
    {
        if (!_allStorages.Contains(storage))
        {
            _allStorages.Add(storage);
            OnNewStorageAdded?.Invoke();
        }
    }

    public void RemoveStorage(IStorage storage)
    {
        if (_allStorages.Remove(storage))
        {
            OnStorageRemoved?.Invoke(storage);
        }
    }

    public List<IStorage> GetAllStorages() => _allStorages;

    public void RegisterMainStructure(MainStructure mainStructure)
    {
        _mainStructure = mainStructure;
        InitializeMainStructureStorage();
    }
    
    private void InitializeMainStructureStorage()
    {
        if (_mainStructure == null) return;
        
        _mainStructure.InitializeStorage(ResourceType.Ferrite, ferriteInitialAmount);
        _mainStructure.InitializeStorage(ResourceType.Aether, aetherInitialAmount);
        _mainStructure.InitializeStorage(ResourceType.Biomass, biomassInitialAmount);
        _mainStructure.InitializeStorage(ResourceType.CryoCrystal, cryoCrystalInitialAmount);
        _mainStructure.InitializeStorage(ResourceType.Solana, solanaInitialAmount);

        _mainStructure.UpdateStorageUI();
    }
    
    public void AddResourceNode(ResourceNode node)
    {
        if (!_allResources.Contains(node))
        {
            _allResources.Add(node);
        }
    }

    public void RemoveResourceNode(ResourceNode node)
    {
        _allResources.Remove(node);
    }

    public List<ResourceNode> GetAllResources() => _allResources;
    
    private void FindAndConnectUI()
    {
        ferriteNumber = GameObject.Find("Resource0_txt")?.GetComponent<TMP_Text>();
        aetherNumber = GameObject.Find("Resource1_txt")?.GetComponent<TMP_Text>();
        biomassNumber = GameObject.Find("Resource2_txt")?.GetComponent<TMP_Text>();
        cryoCrystalNumber = GameObject.Find("Resource3_txt")?.GetComponent<TMP_Text>();
        solanaNumber = GameObject.Find("Resource4_txt")?.GetComponent<TMP_Text>();
    }

    private void UpdateResourceUI(ResourceType type)
    {
        if (!IsUIConnected()) return;
        
        if (type == ResourceType.Solana)
        {
            UpdateSolanaUI();
        }
        else
        {
            GetResourceText(type).text = _resourceCounts[type].ToString();
        }
    }

    public void UpdateAllResourceUI()
    {
        if (!IsUIConnected()) return;
        
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.Solana)
            {
                GetResourceText(type).text = _resourceCounts[type].ToString();
            }
        }
        UpdateSolanaUI();
    }
    
    private void UpdateSolanaUI()
    {
        int requiredAmount = GameManager.Instance != null ? GameManager.Instance.GetRequiredAmountForCurrentQuota() : 0;
        solanaNumber.text = $"{_resourceCounts[ResourceType.Solana]} / {requiredAmount}";
    }
    
    private bool IsUIConnected() => ferriteNumber != null && aetherNumber != null && biomassNumber != null && cryoCrystalNumber != null && solanaNumber != null;

    private TMP_Text GetResourceText(ResourceType type)
    {
        return type switch
        {
            ResourceType.Ferrite => ferriteNumber,
            ResourceType.Aether => aetherNumber,
            ResourceType.Biomass => biomassNumber,
            ResourceType.CryoCrystal => cryoCrystalNumber,
            _ => null
        };
    }
    
    public Sprite GetResourceIcon(ResourceType type)
    {
        int index = (int)type;
        return index >= 0 && index < resourceIcons.Count ? resourceIcons[index] : null;
    }
    
    private void AddCheatResources()
    {
        const int cheatAmount = 999999;
        Debug.Log($"<color=orange>CHEAT ACTIVATED:</color> All resources set to {cheatAmount}.");

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            _resourceCounts[type] = cheatAmount;

            _mainStructure?.InitializeStorage(type, cheatAmount);
        }

        UpdateAllResourceUI();
    }
}