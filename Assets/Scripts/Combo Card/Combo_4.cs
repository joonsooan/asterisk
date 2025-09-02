using UnityEngine;
using System.Collections;

public class Combo_4 : Damageable
{
    [Header("Turret Stats")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private GameObject bulletPrefab;

    private Transform _target;
    private Coroutine _attackCoroutine;
    private WaitForSeconds _findTargetWait;
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        _findTargetWait = new WaitForSeconds(0.2f);
        StartCoroutine(UpdateTargetCoroutine());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        StopAllCoroutines();
        _attackCoroutine = null;
    }

    private void Update()
    {
        if (!IsTargetValid())
        {
            _target = null;
            StopAttacking();
            return;
        }

        StartAttacking();
    }

    private bool IsTargetValid()
    {
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            return false;
        }

        return Vector2.Distance(transform.position, _target.position) <= attackRange;
    }

    private IEnumerator UpdateTargetCoroutine()
    {
        while (true)
        {
            if (_target == null)
            {
                FindClosestEnemy();
            }
            yield return _findTargetWait;
        }
    }

    private void FindClosestEnemy()
    {
        var enemies = UnitManager.Instance.EnemyUnits;
        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance && distance <= attackRange)
            {
                minDistance = distance;
                closestTarget = enemy.transform;
            }
        }
        _target = closestTarget;
    }

    private void StartAttacking()
    {
        if (_attackCoroutine == null)
        {
            _attackCoroutine = StartCoroutine(AttackCoroutine());
        }
    }

    private void StopAttacking()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
    }

    private IEnumerator AttackCoroutine()
    {
        while (true)
        {
            Shoot();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || _target == null)
        {
            Debug.Log("Target is null or Bullet is null");
            return;
        }
        
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        if (bullet.TryGetComponent<Turret_Bullet>(out var bulletScript))
        {
            bulletScript.Initialize(attackDamage, _target);
            Debug.Log("Bullet shoot");
        }

    }
}