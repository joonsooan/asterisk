using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Lifter : UnitBase
{
    [Header("Gathering Stats")]
    public int miningRate = 10;
    public float miningRange = 1.5f;

    [Header("Sprites")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteRight;

    private readonly float _searchInterval = 2f;
    private Coroutine _mineCoroutine;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private ResourceNode _targetResourceNode;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

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
            MoveToTarget();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, miningRange);
    }

    public override void PerformAction()
    {
        //
    }

    public override void SetActionPriority(Dictionary<string, int> priorities)
    {
        //
    }

    private IEnumerator FindNearestResourceCoroutine()
    {
        while (true) {
            ResourceNode[] allResources = FindObjectsByType<ResourceNode>(FindObjectsSortMode.None);
            float minDistance = float.MaxValue;
            ResourceNode nearest = null;

            foreach (ResourceNode resource in allResources) {
                float distance = Vector2.Distance(transform.position, resource.transform.position);
                if (distance < minDistance) {
                    minDistance = distance;
                    nearest = resource;
                }
            }

            _targetResourceNode = nearest;
            yield return new WaitForSeconds(_searchInterval);
        }
    }

    private void MoveToTarget()
    {
        if (_targetResourceNode != null) {
            Vector2 direction = (_targetResourceNode.transform.position - transform.position).normalized;
            _rb.linearVelocity = direction * moveSpeed;
            UpdateSpriteDirection(direction);
        }
        else {
            _rb.linearVelocity = Vector2.zero;
            currentState = UnitState.Idle;
        }
    }

    private void DecideNextAction()
    {
        // 목표가 없는 경우 State = Idle
        if (_targetResourceNode == null || _targetResourceNode.IsDepleted) {
            _targetResourceNode = null;
            if (currentState == UnitState.Mining) {
                currentState = UnitState.Idle;
            }
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, _targetResourceNode.transform.position);

        if (distanceToTarget <= miningRange) {
            // 채굴 범위에 들어오면 State = Mining
            currentState = UnitState.Mining;
            _rb.linearVelocity = Vector2.zero;
        }
        else {
            // 채굴 범위 밖이면 State = Moving
            currentState = UnitState.Moving;
        }

        if (currentState == UnitState.Mining && _mineCoroutine == null) {
            _mineCoroutine = StartCoroutine(MineResourceCoroutine());
        }
        else if (currentState != UnitState.Mining && _mineCoroutine != null) {
            StopCoroutine(_mineCoroutine);
            _mineCoroutine = null;
        }
    }

    private IEnumerator MineResourceCoroutine()
    {
        yield return new WaitForSeconds(1.0f);

        while (true) {
            if (_targetResourceNode != null && !_targetResourceNode.IsDepleted) {
                _targetResourceNode.Mine(miningRate);
                Debug.Log($"자원 채굴 중... 남은 양: {_targetResourceNode.amountToMine}");
            }
            else {
                _mineCoroutine = null;
                yield break;
            }
            yield return new WaitForSeconds(1.0f); // 1초마다 반복
        }
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (direction != Vector2.zero) {
            if (Mathf.Abs(direction.x) > 0) {
                _sr.flipX = direction.x < 0;
                _sr.sprite = spriteRight;
            }
            else if (direction.y > 0) {
                _sr.sprite = spriteUp;
            }
            else if (direction.y < 0) {
                _sr.sprite = spriteDown;
            }
        }
    }
}
