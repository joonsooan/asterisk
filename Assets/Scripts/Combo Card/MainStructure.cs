using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainStructure : Damageable, IStorage
{
    public event System.Action<int, int> OnStorageChanged;
    
    [Header("UI Settings")]
    [SerializeField] private GameObject storageSliderPrefab;
    [SerializeField] private string canvasName = "ObjectUI_Canvas";
    [SerializeField] private Vector3 sliderOffset = new Vector3(0, 1.5f, 0);
    private GameObject _sliderInstance;
    
    [Header("Storage Settings")]
    [SerializeField] private int maxStorageAmount = 1000;
    private int _currentStorageAmount = 0;

    [Header("Production Settings")]
    [SerializeField] private List<UnitData> producibleUnits;

    private readonly Queue<UnitData> _productionQueue = new();
    
    private bool _isProducing;

    private void Awake()
    {
        currentHealth = maxHealth;
    }
    
    protected new void OnEnable()
    {
        base.OnEnable();
    }

    private void Start()
    {
        GameObject unitMakeButtonObj = GameObject.Find("Unit Make Btn");

        if (unitMakeButtonObj != null)
        {
            Button unitMakeButton = unitMakeButtonObj.GetComponent<Button>();

            if (unitMakeButton != null)
            {
                unitMakeButton.onClick.AddListener(() => AddUnitToQueue(0));
            }
        }
        
        if (storageSliderPrefab != null)
        {
            Canvas canvas = GameObject.Find(canvasName)?.GetComponent<Canvas>();
            if (canvas != null)
            {
                _sliderInstance = Instantiate(storageSliderPrefab, canvas.transform);
                var controller = _sliderInstance.GetComponent<StorageSlider>();
                controller?.Initialize(this, sliderOffset);
            }
        }
        
        OnStorageChanged?.Invoke(_currentStorageAmount, maxStorageAmount);
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.RemoveStorage(this);
            GameManager.Instance.GameOver();
        }
        
        if (_sliderInstance != null)
        {
            Destroy(_sliderInstance);
        }
    }

    public bool StorageIsFull()
    {
        return _currentStorageAmount >= maxStorageAmount;
    }

    public void AddResource(ResourceType type, int amount)
    {
        _currentStorageAmount += amount;
        if (_currentStorageAmount > maxStorageAmount)
        {
            _currentStorageAmount = maxStorageAmount;
        }
        
        OnStorageChanged?.Invoke(_currentStorageAmount, maxStorageAmount);
        ResourceManager.Instance.AddResource(type, amount);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void AddUnitToQueue(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= producibleUnits.Count)
        {
            return;
        }

        UnitData unitData = producibleUnits[unitIndex];

        if (!CanProduceUnit(unitData))
        {
            Debug.Log("Can't produce unit");
            return;
        }

        ResourceManager.Instance.SpendResources(unitData.productionCosts);
        _productionQueue.Enqueue(unitData);

        if (!_isProducing)
        {
            StartCoroutine(ProcessProductionQueue());
        }
    }

    private IEnumerator ProcessProductionQueue()
    {
        _isProducing = true;

        while (_productionQueue.Count > 0)
        {
            UnitData unitToProduce = _productionQueue.Dequeue();

            yield return new WaitForSeconds(unitToProduce.productionTime);

            Instantiate(unitToProduce.unitPrefab, transform.position, Quaternion.identity, BuildingManager.Instance.grid.transform);
        }
        
        _isProducing = false;
    }
    
    private bool CanProduceUnit(UnitData unitData)
    {
        if (unitData.productionCosts == null || unitData.productionCosts.Length == 0) return true;
        
        return ResourceManager.Instance.HasEnoughResources(unitData.productionCosts);
    }
}