using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Lifter : UnitBase
{
    [Header("References")]
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private UnitMining unitMining;
    [SerializeField] private UnitSpriteController unitSpriteController;

    [SerializeField] private float searchInterval = 2f;
    private ResourceNode _targetResourceNode;

    private void Start()
    {
        currentHealth = maxHealth;
        currentState = UnitState.Idle;
        StartCoroutine(FindNearestResourceCoroutine());
    }

    private void Update()
    {
        DecideNextAction();
    }

    private void FixedUpdate()
    {
        if (currentState == UnitState.Moving) {
            unitMovement.MoveToTarget();
        }
    }

    private void DecideNextAction()
    {
        if (_targetResourceNode == null || _targetResourceNode.IsDepleted) {
            _targetResourceNode = null;
            if (currentState == UnitState.Mining) {
                currentState = UnitState.Idle;
            }
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, _targetResourceNode.transform.position);

        if (distanceToTarget <= unitMining.miningRange) {
            currentState = UnitState.Mining;
            unitMovement.StopMovement();
        }
        else {
            currentState = UnitState.Moving;
        }

        if (currentState == UnitState.Mining && unitMining.IsMining == false) {
            unitMining.StartMining(_targetResourceNode);
        }
        else if (currentState != UnitState.Mining && unitMining.IsMining) {
            unitMining.StopMining();
        }
    }

    private IEnumerator FindNearestResourceCoroutine()
    {
        while (true) {
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

            yield return new WaitForSeconds(searchInterval);
        }
    }

    public override void PerformAction()
    {
    }

    public override void SetActionPriority(Dictionary<string, int> priorities)
    {
    }
}
