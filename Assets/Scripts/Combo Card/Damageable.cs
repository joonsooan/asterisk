using UnityEngine;

public abstract class Damageable : MonoBehaviour, ICombo
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 100;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    
    protected int currentHealth;

    protected virtual void OnEnable()
    {
        currentHealth = maxHealth;
        TargetManager.Instance?.RegisterTarget(this);
    }
    
    protected virtual void OnDisable()
    {
        TargetManager.Instance?.UnregisterTarget(this);
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}