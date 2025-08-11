using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    public enum UnitState
    {
        Idle,
        Moving,
        Attacking,
        Retreating,
        Mining,
        ReturningToStorage,
        Unloading
    }

    [Header("Unit Stats")]
    public float maxHealth;
    public float currentHealth;
    public float moveSpeed;

    public UnitState currentState;

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " has been destroyed.");
        Destroy(gameObject);
    }
}
