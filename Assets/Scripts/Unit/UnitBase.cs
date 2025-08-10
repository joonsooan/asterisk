using System.Collections.Generic;
using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    public enum UnitState
    {
        Idle,
        Moving,
        Attacking,
        Gathering,
        Retreating
    }

    [Header("Unit Stats")] 
    public float maxHealth;
    public float currentHealth;
    public float moveSpeed;

    public UnitState currentState;

    public abstract void PerformAction();

    public abstract void SetActionPriority(Dictionary<string, int> priorities);

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