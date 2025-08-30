using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Unit_Lifter : UnitBase
{
    private readonly Dictionary<ResourceType, int> _currentCarryAmounts = new();
    private WaitForSeconds _searchWait;
    
    private Coroutine _findResourceCoroutine;
    private IStorage _targetStorage;
    private ResourceNode _targetResourceNode;

    [Header("VFX")]
    [SerializeField] private string canvasName = "FloatingText Canvas";
    [SerializeField] private GameObject floatingNumTextPrefab;
    [SerializeField] private bool showFloatingText;
    private Canvas _canvas;

    [Header("References")]
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private UnitMining unitMining;
    [SerializeField] private UnitSpriteController unitSpriteController;
    
    [Header("Cargo")]
    public int maxCarryAmount = 50;
    [SerializeField] private float unloadDistance = 1.5f;
    [SerializeField] private float resourceSearchInterval = 2.0f;
    public ResourceType[] mineableResourceTypes;

    private void Awake()
    {
        _canvas = GameObject.Find(canvasName)?.GetComponent<Canvas>();

        SubscribeEvents();
        InitializeCarryAmounts();
        
        _searchWait = new WaitForSeconds(resourceSearchInterval);
    }
    
    private void Start()
    {
        currentHealth = maxHealth;
        
        if (UnitManager.Instance != null)
        {
            mineableResourceTypes = UnitManager.Instance.CurrentMineableTypes.ToArray();
        }
        else
        {
            mineableResourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
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

    private void OnDestroy()
    {
        UnsubscribeEvents();
        if (_targetResourceNode != null && _targetResourceNode.IsReserved && _targetResourceNode.GetReservedUnit() == this) {
            _targetResourceNode.Unreserve();
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        UnitManager.OnMineableTypesChanged += HandleMineableTypesChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        UnitManager.OnMineableTypesChanged -= HandleMineableTypesChanged;
    }
    
    private void HandleMineableTypesChanged(ResourceType[] newTypes)
    {
        mineableResourceTypes = newTypes;

        if (currentState == UnitState.Mining || currentState == UnitState.Moving)
        {
            if (_targetResourceNode != null && !mineableResourceTypes.Contains(_targetResourceNode.resourceType))
            {
                HandleTargetLoss();
            }
        }
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        if (_targetStorage is MainStructure)
        {
            _targetStorage = null;
        }
    }

    private void SubscribeEvents()
    {
        if (unitMining != null) {
            unitMining.OnResourceMined += HandleResourceMined;
        }
        ResourceManager.OnNewStorageAdded += HandleNewStorageAdded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void UnsubscribeEvents()
    {
        if (unitMining != null) {
            unitMining.OnResourceMined -= HandleResourceMined;
        }
        ResourceManager.OnNewStorageAdded -= HandleNewStorageAdded;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void InitializeCarryAmounts()
    {
        
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType))) {
            _currentCarryAmounts[type] = 0;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _targetStorage = null;
    }

    public void TryStartActions()
    {
        if (_findResourceCoroutine != null)
        {
            StopCoroutine(_findResourceCoroutine);
        }
        _findResourceCoroutine = StartCoroutine(FindNearestResourceCoroutine());
    }

    private void DecideNextAction()
    {
        switch (currentState)
        {
            case UnitState.Idle:
                if (_findResourceCoroutine == null) {
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
            
            case UnitState.Unloading:
                break;
        }
    }

    private void HandleNewStorageAdded()
    {
        if (currentState is UnitState.Idle or UnitState.ReturningToStorage)
        {
            FindAndSetStorage();
            if (_targetStorage != null)
            {
                unitMovement.SetNewTarget(_targetStorage.GetPosition());
                currentState = UnitState.ReturningToStorage;
            }
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
        if (_targetStorage == null || _targetStorage.StorageIsFull())
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

    private void HandleResourceMined(ResourceType type, int amount)
    {
        if (currentState != UnitState.Mining) return;
        
        _currentCarryAmounts[type] = _currentCarryAmounts.GetValueOrDefault(type) + amount;
        ShowFloatingText(amount);

        int totalCarriedAmount = _currentCarryAmounts.Values.Sum();
        
        if (totalCarriedAmount >= maxCarryAmount)
        {
            unitMining.StopMining();
            _targetResourceNode?.Unreserve();
            
            FindAndSetStorage();
            if (_targetStorage != null)
            {
                unitMovement.SetNewTarget(_targetStorage.GetPosition());
                currentState = UnitState.ReturningToStorage;
            }
            else
            {
                unitMovement.StopMovement();
                Debug.Log("Cargo is full, no storage available. Waiting.");
            }
        }
    }
    
    private void StartUnloadingAction()
    {
        currentState = UnitState.Unloading;
        StartCoroutine(UnloadResourceCoroutine());
    }

    private IEnumerator UnloadResourceCoroutine()
    {
        unitMovement.StopMovement();
        yield return new WaitForSeconds(1f);
        
        if (_targetStorage != null)
        {
            foreach (var pair in _currentCarryAmounts.Where(p => p.Value > 0))
            {
                _targetStorage.AddResource(pair.Key, pair.Value);
            }
        }

        InitializeCarryAmounts();
        _targetResourceNode = null;
        _targetStorage = null;
        
        currentState = UnitState.Idle;
        TryStartActions();
    }
    
    private void HandleTargetLoss()
    {
        unitMining?.StopMining(); 
        
        _targetResourceNode?.Unreserve();
        currentState = UnitState.Idle;
        unitMovement.StopMovement();
        _targetResourceNode = null;
    }

    private void HandleStorageLoss()
    {
        if (_targetResourceNode == null)
        {
            FindAndSetStorage();
            if (_targetStorage == null)
            {
                currentState = UnitState.Idle;
                unitMovement.StopMovement();
                Debug.Log("모든 저장소가 가득 찼습니다. 대기합니다.");
                return;
            }
        }
        unitMovement.SetNewTarget(_targetStorage.GetPosition());
    }

    private void StartMiningAction()
    {
        currentState = UnitState.Mining;
        unitMovement.StopMovement();
        unitMining.StartMining(_targetResourceNode);
    }
    
    private void FindAndSetStorage()
    {
        var storages = ResourceManager.Instance.GetAllStorages();
        
        var availableStorages = storages.Where(s => s != null && !s.StorageIsFull());

        _targetStorage = availableStorages
            .OrderBy(s => Vector2.Distance(transform.position, s.GetPosition()))
            .FirstOrDefault();
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
    
    private void FindAndSetTarget()
    {
        int totalCarriedAmount = _currentCarryAmounts.Values.Sum();
    
        if (totalCarriedAmount > 0)
        {
            HandleTargetNotFound();
            return;
        }
        
        var allResources = ResourceManager.Instance.GetAllResources();
        
        var availableResources = allResources.Where(r => 
            r != null && !r.IsDepleted && r.gameObject.activeInHierarchy && 
            mineableResourceTypes.Contains(r.resourceType) &&
            (!r.IsReserved || r.GetReservedUnit() == this)
        );

        var newTarget = availableResources
            .OrderBy(r => Vector2.Distance(transform.position, r.transform.position))
            .FirstOrDefault();

        if (newTarget != null)
        {
            if (newTarget.Reserve(this))
            {
                if (_targetResourceNode != null && _targetResourceNode != newTarget)
                {
                    _targetResourceNode.Unreserve();
                }
                
                _targetResourceNode = newTarget;
                unitMovement.SetNewTarget(_targetResourceNode.transform.position);
                currentState = UnitState.Moving;
            }
        }
        else
        {
            _targetResourceNode = null;
            currentState = UnitState.Idle;
        }
    }

    private void HandleTargetNotFound()
    {
        int totalCarriedAmount = _currentCarryAmounts.Values.Sum();
        if (totalCarriedAmount > 0)
        {
            Debug.Log("[자원 고갈] 남은 자원을 저장고에 저장합니다.");
            FindAndSetStorage();
            if (_targetStorage != null)
            {
                currentState = UnitState.ReturningToStorage;
                unitMovement.SetNewTarget(_targetStorage.GetPosition());
            }
            else
            {
                currentState = UnitState.Idle;
                Debug.Log("모든 저장소가 가득 찼습니다. 대기합니다.");
            }
        }
    }

    private void ShowFloatingText(int amount)
    {
        if (!showFloatingText || floatingNumTextPrefab == null || _canvas == null) return;

        GameObject textInstance = Instantiate(floatingNumTextPrefab, transform.position, Quaternion.identity, _canvas.transform);

        FloatingNumText floatingText = textInstance.GetComponent<FloatingNumText>();
        if (floatingText != null) {
            floatingText.SetText($"+{amount}");
        }
    }
}