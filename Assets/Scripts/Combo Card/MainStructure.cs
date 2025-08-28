using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainStructure : MonoBehaviour, IStorage
{
    [Header("Basic Settings")]
    [SerializeField] private int maxHealth = 100;
    
    [Header("Storage Settings")]
    [SerializeField] private int maxStorageAmount = 1000;
    private int _currentStorageAmount = 0;

    [Header("Production Settings")]
    [SerializeField] private List<UnitData> producibleUnits;

    private readonly Queue<UnitData> _productionQueue = new();
    
    private int _currentHealth;
    private bool _isProducing;

    private void Awake()
    {
        _currentHealth = maxHealth;
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
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.RemoveStorage(this);
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
        ResourceManager.Instance.AddResource(type, amount);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Destroy();
        }
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
    
    public void AddUnitToQueue(int unitIndex)
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

            ResourceManager.Instance.SpendResources(unitToProduce.productionCosts);

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