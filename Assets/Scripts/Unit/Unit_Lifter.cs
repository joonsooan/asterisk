using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Unit_Lifter : UnitBase
{
    [Header("References")]
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private UnitMining unitMining;

    [Header("Cargo")]
    public int maxCarryAmount = 50;
    [SerializeField] private float unloadDistance = 1.5f;
    [SerializeField] private float resourceSearchInterval = 2.0f;
    public ResourceType[] mineableResourceTypes;

    [Header("VFX")]
    [SerializeField] private string canvasName = "ObejectUI Canvas";
    [SerializeField] private GameObject floatingNumTextPrefab;
    [SerializeField] private bool showFloatingText;
    
    private readonly Dictionary<ResourceType, int> _currentCarryAmounts = new();
    private WaitForSeconds _searchWait;
    private Coroutine _findResourceCoroutine;
    private IStorage _targetStorage;
    private ResourceNode _targetResourceNode;
    private Canvas _canvas;
    
    protected override void Awake()
    {
        base.Awake();
        _canvas = GameObject.Find(canvasName)?.GetComponent<Canvas>();
        _searchWait = new WaitForSeconds(resourceSearchInterval);
        InitializeCarryAmounts();
    }

    private void Start()
    {
        mineableResourceTypes = UnitManager.Instance != null 
            ? UnitManager.Instance.CurrentMineableTypes.ToArray() 
            : (ResourceType[])Enum.GetValues(typeof(ResourceType));
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        SubscribeEvents();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribeEvents();
    }
    
    private void OnDestroy()
    {
        if (_targetResourceNode != null && _targetResourceNode.IsReserved && _targetResourceNode.GetReservedUnit() == this)
        {
            _targetResourceNode.Unreserve();
        }
    }

    private void Update() => DecideNextAction();

    private void FixedUpdate()
    {
        if (currentState is UnitState.Moving or UnitState.ReturningToStorage)
        {
            unitMovement.MoveToTarget();
        }
    }
    
    private void DecideNextAction()
    {
        switch (currentState)
        {
            case UnitState.Idle:
                if (_findResourceCoroutine == null)
                {
                    TryStartActions();
                }
                break;
            
            case UnitState.Moving:
                OnMoving();
                break;
            
            case UnitState.Mining:
                OnMining();
                break;
            
            case UnitState.ReturningToStorage:
                OnReturnToStorage();
                break;
        }
    }

    private void OnMoving()
    {
        if (_targetResourceNode == null || _targetResourceNode.IsDepleted)
        {
            HandleTargetLoss();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, _targetResourceNode.transform.position);
        if (distanceToTarget <= unitMining.miningRange)
        {
            StartMiningAction();
        }
    }

    private void OnMining()
    {
        if (_targetResourceNode == null || _targetResourceNode.IsDepleted)
        {
            HandleTargetLoss();
        }
    }

    private void OnReturnToStorage()
    {
        if (_targetStorage == null || (_targetStorage.GetTotalCurrentAmount() >= _targetStorage.GetMaxCapacity()))
        {
            HandleStorageLoss();
            return;
        }

        float distanceToStorage = Vector2.Distance(transform.position, _targetStorage.GetPosition());
        if (distanceToStorage <= unloadDistance)
        {
            StartUnloadingAction();
        }
    }
    
    private void StartMiningAction()
    {
        currentState = UnitState.Mining;
        unitMovement.StopMovement();
        unitMining.StartMining(_targetResourceNode);
    }
    
    private void StartUnloadingAction()
    {
        currentState = UnitState.Unloading;
        StartCoroutine(UnloadResourceCoroutine());
    }
    
    public void TryStartActions()
    {
        if (_findResourceCoroutine != null)
        {
            StopCoroutine(_findResourceCoroutine);
        }
        _findResourceCoroutine = StartCoroutine(FindNearestResourceCoroutine());
    }
    
    private IEnumerator FindNearestResourceCoroutine()
    {
        while (true)
        {
            if (currentState == UnitState.Idle)
            {
                FindAndSetTarget();
            }
            yield return _searchWait;
        }
    }
    
    private IEnumerator UnloadResourceCoroutine()
    {
        unitMovement.StopMovement();
        yield return new WaitForSeconds(1f);

        if (_targetStorage != null)
        {
            foreach (var pair in _currentCarryAmounts.Where(p => p.Value > 0))
            {
                _targetStorage.TryAddResource(pair.Key, pair.Value);
            }
        }

        InitializeCarryAmounts();
        _targetResourceNode = null;
        _targetStorage = null;

        currentState = UnitState.Idle;
    }
    
    private void FindAndSetTarget()
    {
        if (_currentCarryAmounts.Values.Sum() > 0)
        {
            GoToStorage();
            return;
        }
        
        ResourceNode closestNode = null;
        float minDistance = float.MaxValue;
        
        var availableResources = ResourceManager.Instance.GetAllResources()
            .Where(r => r != null && !r.IsDepleted && r.gameObject.activeInHierarchy && 
                        mineableResourceTypes.Contains(r.resourceType) &&
                        (!r.IsReserved || r.GetReservedUnit() == this));

        foreach (var resource in availableResources)
        {
            float distance = Vector2.Distance(transform.position, resource.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNode = resource;
            }
        }

        if (closestNode != null)
        {
            if (closestNode.Reserve(this))
            {
                if (_targetResourceNode != null && _targetResourceNode != closestNode)
                {
                    _targetResourceNode.Unreserve();
                }

                _targetResourceNode = closestNode;

                bool pathFound = unitMovement.SetNewTarget(_targetResourceNode.transform.position);
                
                if (pathFound)
                {
                    currentState = UnitState.Moving;
                }
                else
                {
                    _targetResourceNode.Unreserve();
                    _targetResourceNode = null;
                    currentState = UnitState.Idle;
                }
            }
        }
        else
        {
            currentState = UnitState.Idle;
        }
    }

    private void FindAndSetStorage()
    {
        IStorage closestStorage = null;
        float minDistance = float.MaxValue;

        var availableStorages = ResourceManager.Instance.GetAllStorages()
            .Where(s => s != null && s.GetTotalCurrentAmount() < s.GetMaxCapacity());

        foreach (var storage in availableStorages)
        {
            float distance = Vector2.Distance(transform.position, storage.GetPosition());
            if (distance < minDistance)
            {
                minDistance = distance;
                closestStorage = storage;
            }
        }
        
        _targetStorage = closestStorage;
    }
    
    private void GoToStorage()
    {
        FindAndSetStorage();
        if (_targetStorage != null)
        {
            currentState = UnitState.ReturningToStorage;
            unitMovement.SetNewTarget(_targetStorage.GetPosition(), unloadDistance * 0.9f);
        }
        else
        {
            currentState = UnitState.Idle;
            Debug.Log("모든 저장소가 가득 찼습니다. 대기합니다.");
        }
    }

    private void HandleTargetLoss()
    {
        unitMining?.StopMining();
        _targetResourceNode?.Unreserve();
        _targetResourceNode = null;
        currentState = UnitState.Idle;
        unitMovement.StopMovement();
    }

    private void HandleStorageLoss()
    {
        FindAndSetStorage();
        if (_targetStorage != null)
        {
            unitMovement.SetNewTarget(_targetStorage.GetPosition(), unloadDistance * 0.9f);
        }
        else
        {
            currentState = UnitState.Idle;
            unitMovement.StopMovement();
            Debug.Log("모든 저장소가 가득 찼습니다. 대기합니다.");
        }
    }
    
    private void SubscribeEvents()
    {
        if (unitMining != null)
        {
            unitMining.OnResourceMined += HandleResourceMined;
        }
        ResourceManager.OnNewStorageAdded += HandleNewStorageAdded;
        ResourceManager.OnStorageRemoved += HandleStorageRemoved;
        UnitManager.OnMineableTypesChanged += HandleMineableTypesChanged;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void UnsubscribeEvents()
    {
        if (unitMining != null)
        {
            unitMining.OnResourceMined -= HandleResourceMined;
        }
        ResourceManager.OnNewStorageAdded -= HandleNewStorageAdded;
        ResourceManager.OnStorageRemoved -= HandleStorageRemoved;
        UnitManager.OnMineableTypesChanged -= HandleMineableTypesChanged;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void HandleResourceMined(ResourceType type, int amount)
    {
        if (currentState != UnitState.Mining) return;

        _currentCarryAmounts[type] += amount;
        ShowFloatingText(amount);

        if (_currentCarryAmounts.Values.Sum() >= maxCarryAmount)
        {
            unitMining.StopMining();
            _targetResourceNode?.Unreserve();
            _targetResourceNode = null;
            GoToStorage();
        }
    }
    
    private void HandleNewStorageAdded()
    {
        if (_currentCarryAmounts.Values.Sum() > 0 && currentState is UnitState.Idle or UnitState.ReturningToStorage)
        {
            GoToStorage();
        }
    }

    private void HandleStorageRemoved(IStorage storage)
    {
        if (_targetStorage == storage)
        {
            _targetStorage = null;
            if (currentState is UnitState.ReturningToStorage or UnitState.Unloading)
            {
                GoToStorage();
            }
        }
    }

    private void HandleMineableTypesChanged(ResourceType[] newTypes)
    {
        mineableResourceTypes = newTypes;

        if ((currentState == UnitState.Mining || currentState == UnitState.Moving) && 
            _targetResourceNode != null && !mineableResourceTypes.Contains(_targetResourceNode.resourceType))
        {
            HandleTargetLoss();
        }
        else if (currentState == UnitState.Idle)
        {
            FindAndSetTarget();
        }
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        if (_targetStorage != null && (_targetStorage as Component)?.gameObject.scene == scene)
        {
            _targetStorage = null;
        }
    }
    
    private void InitializeCarryAmounts()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            _currentCarryAmounts[type] = 0;
        }
    }

    private void ShowFloatingText(int amount)
    {
        if (!showFloatingText || floatingNumTextPrefab == null || _canvas == null) return;

        GameObject textInstance = Instantiate(floatingNumTextPrefab, transform.position, Quaternion.identity, _canvas.transform);
        if (textInstance.TryGetComponent<FloatingNumText>(out var floatingText))
        {
            floatingText.SetText($"+{amount}");
        }
    }
}