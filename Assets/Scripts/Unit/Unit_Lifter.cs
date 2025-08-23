using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Lifter : UnitBase
{
    [Header("Cargo")]
    public int maxCarryAmount = 50;
    [SerializeField] private float unloadDistance = 1.5f;

    [Header("Values")]
    [SerializeField] private float resourceSearchInterval = 2f;

    [Header("VFX")]
    [SerializeField] private string canvasName = "FloatingText Canvas";
    [SerializeField] private GameObject floatingNumTextPrefab;

    [Header("References")]
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private UnitMining unitMining;
    [SerializeField] private UnitSpriteController unitSpriteController;
    private readonly Dictionary<ResourceType, int> _currentCarryAmounts = new Dictionary<ResourceType, int>();

    private Canvas _canvas;
    private Coroutine _findResourceCoroutine;
    private StorageBuilding _storageBuilding;
    private ResourceNode _targetResourceNode;

    private void Awake()
    {
        GameObject canvasObject = GameObject.Find(canvasName);

        if (canvasObject != null) {
            _canvas = canvasObject.GetComponent<Canvas>();
        }

        unitMining.OnResourceMined += HandleResourceMined;

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType))) {
            _currentCarryAmounts[type] = 0;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        DecideNextAction();
    }

    private void FixedUpdate()
    {
        if (currentState == UnitState.Moving || currentState == UnitState.ReturningToStorage) {
            unitMovement.MoveToTarget();
        }
    }

    private void OnDestroy()
    {
        if (unitMining != null) {
            unitMining.OnResourceMined -= HandleResourceMined;
        }
        if (_findResourceCoroutine != null) {
            StopCoroutine(_findResourceCoroutine);
        }
        if (_targetResourceNode != null && _targetResourceNode.IsReserved) {
            _targetResourceNode.Unreserve();
        }
    }

    public void StartUnitActions()
    {
        _storageBuilding = FindAnyObjectByType<StorageBuilding>();
        if (_findResourceCoroutine != null) {
            StopCoroutine(_findResourceCoroutine);
        }
        _findResourceCoroutine = StartCoroutine(FindNearestResourceCoroutine());
    }

    private void DecideNextAction()
    {
        switch (currentState) {
        case UnitState.Idle:
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

    private void OnMoving()
    {
        if (_targetResourceNode == null || _targetResourceNode.IsDepleted) {
            if (_targetResourceNode != null) {
                _targetResourceNode.Unreserve();
            }
            currentState = UnitState.Idle;
            unitMovement.StopMovement();
            _targetResourceNode = null;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, _targetResourceNode.transform.position);
        if (distanceToTarget <= unitMining.miningRange) {
            currentState = UnitState.Mining;
            unitMovement.StopMovement();
            unitMining.StartMining(_targetResourceNode);
        }
    }

    private void OnMining()
    {
        if (_targetResourceNode == null || _targetResourceNode.IsDepleted) {
            unitMining.StopMining();
            currentState = UnitState.Idle;
            _targetResourceNode = null;

            if (_findResourceCoroutine != null) {
                StopCoroutine(_findResourceCoroutine);
            }
            _findResourceCoroutine = StartCoroutine(FindNearestResourceCoroutine());
        }
    }

    private void OnReturnToStorage()
    {
        float distanceToStorage = Vector2.Distance(transform.position, _storageBuilding.transform.position);
        if (distanceToStorage <= unloadDistance) {
            currentState = UnitState.Unloading;
            StartCoroutine(UnloadResourceCoroutine());
        }
    }

    private void HandleResourceMined(ResourceType type, int amount)
    {
        if (_currentCarryAmounts.ContainsKey(type)) {
            _currentCarryAmounts[type] += amount;
        }
        else {
            _currentCarryAmounts[type] = amount;
        }
        ShowFloatingText(amount);

        int totalCarriedAmount = 0;
        foreach (KeyValuePair<ResourceType, int> pair in _currentCarryAmounts) {
            totalCarriedAmount += pair.Value;
        }

        if (totalCarriedAmount >= maxCarryAmount) {
            unitMining.StopMining();
            currentState = UnitState.ReturningToStorage;
            unitMovement.SetNewTarget(_storageBuilding.transform.position);
            if (_targetResourceNode != null) {
                _targetResourceNode.Unreserve();
            }
        }
    }

    private IEnumerator UnloadResourceCoroutine()
    {
        unitMovement.StopMovement();
        yield return new WaitForSeconds(1f);

        if (ResourceManager.Instance != null) {
            foreach (KeyValuePair<ResourceType, int> pair in _currentCarryAmounts) {
                if (pair.Value > 0) {
                    ResourceManager.Instance.AddResource(pair.Key, pair.Value);

                    // ShowFloatingText(pair.Value);
                }
            }
        }

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType))) {
            _currentCarryAmounts[type] = 0;
        }

        currentState = UnitState.Idle;

        StartUnitActions();
    }

    private IEnumerator FindNearestResourceCoroutine()
    {
        FindAndSetTarget();

        while (true) {
            if (currentState == UnitState.Idle) {
                yield return new WaitForSeconds(resourceSearchInterval);
                FindAndSetTarget();
            }
            else {
                yield return null;
            }
        }
    }

    private void FindAndSetTarget()
    {
        List<ResourceNode> allResources = ResourceManager.Instance.GetAllResources();
        float minDistance = float.MaxValue;
        ResourceNode nearest = null;

        foreach (ResourceNode resource in allResources) {
            if (resource.IsDepleted || resource.IsReserved) continue;

            float distance = Vector2.Distance(transform.position, resource.transform.position);
            if (distance < minDistance) {
                minDistance = distance;
                nearest = resource;
            }
        }

        if (nearest == null) {
            int totalCarriedAmount = 0;
            foreach (KeyValuePair<ResourceType, int> pair in _currentCarryAmounts) {
                totalCarriedAmount += pair.Value;
            }

            if (totalCarriedAmount > 0) {
                Debug.Log("[자원 고갈] 남은 자원을 저장고에 저장합니다.");
                currentState = UnitState.ReturningToStorage;
                unitMovement.SetNewTarget(_storageBuilding.transform.position);
                _targetResourceNode = null;
            }
            else {
                Debug.Log("[자원 고갈] 더 이상 채굴할 자원이 없습니다. 대기합니다.");
            }
        }
        else {
            if (_targetResourceNode != null && _targetResourceNode != nearest) {
                _targetResourceNode.Unreserve();
            }

            _targetResourceNode = nearest;
            _targetResourceNode.Reserve();

            unitMovement.SetNewTarget(_targetResourceNode.transform.position);

            currentState = UnitState.Moving;
        }
    }

    private void ShowFloatingText(int amount)
    {
        if (floatingNumTextPrefab == null || _canvas == null) return;

        GameObject textInstance = Instantiate(floatingNumTextPrefab, transform.position, Quaternion.identity, _canvas.transform);

        FloatingNumText floatingText = textInstance.GetComponent<FloatingNumText>();
        if (floatingText != null) {
            floatingText.SetText($"+{amount}");
        }
    }
}
