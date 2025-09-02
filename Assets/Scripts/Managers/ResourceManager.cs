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
    
    public static event Action OnNewStorageAdded;
    public static event Action<IStorage> OnStorageRemoved;
    
    private readonly List<ResourceNode> _allResources = new List<ResourceNode>();
    private readonly List<IStorage> _allStorages = new List<IStorage>();
    private readonly Dictionary<ResourceType, ResourceStats> _resourceStats = new Dictionary<ResourceType, ResourceStats>();
    private readonly Dictionary<ResourceType, int> _resourceCounts = new Dictionary<ResourceType, int>();
    
    private MainStructure _mainStructure;
    
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
        _allStorages.Remove(storage);
        OnStorageRemoved?.Invoke(storage); 
    }

    public List<IStorage> GetAllStorages()
    {
        _allStorages.RemoveAll(s => s == null || ((Component)s).gameObject == null);
        return _allStorages;
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
        if (scene.name == "GameScene")
        {
            _mainStructure = null;
            FindAndConnectUI();
            ResetResourceCount();
            UpdateAllResourceUI();
        }
    }
    
    public void RegisterMainStructure(MainStructure mainStructure)
    {
        _mainStructure = mainStructure;
        InitializeMainStructureStorage();
    }
    
    private void InitializeMainStructureStorage()
    {
        if (_mainStructure != null)
        {
            _mainStructure.InitializeStorage(ResourceType.Ferrite, ferriteInitialAmount);
            _mainStructure.InitializeStorage(ResourceType.Aether, aetherInitialAmount);
            _mainStructure.InitializeStorage(ResourceType.Biomass, biomassInitialAmount);
            _mainStructure.InitializeStorage(ResourceType.CryoCrystal, cryoCrystalInitialAmount);
            _mainStructure.InitializeStorage(ResourceType.Solana, solanaInitialAmount);

            _mainStructure.UpdateStorageUI();
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

    public bool SpendResources(CardCost[] costs)
    {
        if (!HasEnoughResources(costs)) {
            Debug.Log("Not Enough Resources");
            return false;
        }

        foreach (var cost in costs)
        {
            int remainingToWithdraw = cost.amount;
            
            foreach (var storage in GetAllStorages())
            {
                if (storage.TryWithdrawResource(cost.resourceType, remainingToWithdraw, out int amountWithdrawn))
                {
                    remainingToWithdraw -= amountWithdrawn;
                    if (remainingToWithdraw <= 0)
                    {
                        break;
                    }
                }
            }
        }
        
        foreach (var cost in costs)
        {
            _resourceCounts[cost.resourceType] -= cost.amount;
            UpdateResourceUI(cost.resourceType);
        }

        return true;
    }
    
    public bool SpendResources(ResourceType type, int amount)
    {
        return SpendResources(new CardCost[] { new CardCost { resourceType = type, amount = amount } });
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
    
    public Sprite GetResourceIcon(ResourceType type)
    {
        int index = (int)type;
        if (index >= 0 && index < resourceIcons.Count)
        {
            return resourceIcons[index];
        }
        return null;
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
