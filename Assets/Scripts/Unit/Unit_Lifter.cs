using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Lifter : UnitBase
{
    [Header("Cargo")]
    public int maxCarryAmount = 50;
    [SerializeField] private int currentCarryAmount;
    [SerializeField] private float unloadDistance = 1.5f;

    [Header("Values")]
    [SerializeField] private float resourceSearchInterval = 2f;

    [Header("References")]
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private UnitMining unitMining;
    [SerializeField] private UnitSpriteController unitSpriteController;

    private Coroutine _findResourceCoroutine;
    private StorageBuilding _storageBuilding;
    private ResourceNode _targetResourceNode;

    private void Awake()
    {
        unitMining.OnResourceMined += HandleResourceMined;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentState = UnitState.Idle;
        _storageBuilding = FindAnyObjectByType<StorageBuilding>();
        _findResourceCoroutine = StartCoroutine(FindNearestResourceCoroutine());
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
    }

    private void DecideNextAction()
    {
        switch (currentState) {
        case UnitState.Idle:
            if (_targetResourceNode != null) {
                currentState = UnitState.Moving;
                unitMovement.SetNewTarget(_targetResourceNode.transform.position);
            }
            break;

        case UnitState.Moving:
            if (_targetResourceNode == null || _targetResourceNode.IsDepleted) {
                currentState = UnitState.Idle;
                unitMovement.StopMovement();
                break;
            }

            float distanceToTarget = Vector2.Distance(transform.position, _targetResourceNode.transform.position);
            if (distanceToTarget <= unitMining.miningRange) {
                currentState = UnitState.Mining;
                unitMovement.StopMovement();
                unitMining.StartMining(_targetResourceNode);
            }
            break;

        case UnitState.Mining:
            if (_targetResourceNode == null || _targetResourceNode.IsDepleted) {
                unitMining.StopMining();
                currentState = UnitState.Idle;
                _targetResourceNode = null;

                if (_findResourceCoroutine != null) {
                    StopCoroutine(_findResourceCoroutine);
                }
                _findResourceCoroutine = StartCoroutine(FindNearestResourceCoroutine());
            }
            break;

        case UnitState.ReturningToStorage:
            float distanceToStorage = Vector2.Distance(transform.position, _storageBuilding.transform.position);
            if (distanceToStorage <= unloadDistance) {
                currentState = UnitState.Unloading;
                StartCoroutine(UnloadResourceCoroutine());
            }
            break;

        case UnitState.Unloading:
            break;
        }
    }

    private void HandleResourceMined(int amount)
    {
        currentCarryAmount += amount;
        Debug.Log($"[자원 획득] 현재 적재량: {currentCarryAmount}/{maxCarryAmount}");

        if (currentCarryAmount >= maxCarryAmount) {
            unitMining.StopMining();
            currentState = UnitState.ReturningToStorage;
            unitMovement.SetNewTarget(_storageBuilding.transform.position);
        }
    }

    private IEnumerator UnloadResourceCoroutine()
    {
        Debug.Log($"[작업 완료] 자원 {currentCarryAmount}를 비웁니다.");
        unitMovement.StopMovement();

        yield return new WaitForSeconds(1f);

        currentCarryAmount = 0;
        currentState = UnitState.Idle;
    }

    private IEnumerator FindNearestResourceCoroutine()
    {
        while (true) {
            if (currentState == UnitState.Idle) {
                ResourceNode[] allResources = FindObjectsByType<ResourceNode>(FindObjectsSortMode.None);
                float minDistance = float.MaxValue;
                ResourceNode nearest = null;

                foreach (ResourceNode resource in allResources) {
                    if (resource.IsDepleted) continue;

                    Vector2 gridPos = unitMovement.GetGridPosition(resource.transform.position);
                    float distance = Vector2.Distance(transform.position, gridPos);
                    if (distance < minDistance) {
                        minDistance = distance;
                        nearest = resource;
                    }
                }

                if (nearest != _targetResourceNode && nearest != null) {
                    _targetResourceNode = nearest;
                    unitMovement.SetNewTarget(nearest.transform.position);
                }
            }
            yield return new WaitForSeconds(resourceSearchInterval);
        }
    }

    public override void PerformAction()
    {
    }

    public override void SetActionPriority(Dictionary<string, int> priorities)
    {
    }
}
