using UnityEngine;

public abstract class Damageable : MonoBehaviour, ICombo
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 100;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    
    private int currentHealth;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
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