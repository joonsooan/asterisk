using UnityEngine;

public class Turret_Bullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 20f;
    
    private int _damage;
    private Transform _target;

    public void Initialize(int damage, Transform target)
    {
        _damage = damage;
        _target = target;
        
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (_target.position - transform.position).normalized;
        transform.position += direction * (bulletSpeed * Time.deltaTime);
        
        transform.right = direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == _target)
        {
            if (_target.TryGetComponent<UnitBase>(out var unitBase))
            {
                unitBase.TakeDamage(_damage);
            }
            Destroy(gameObject);
        }
    }
}