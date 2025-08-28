using UnityEngine;

public class Combo_2 : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHealth = 100;
    // [SerializeField] private float constructionTime = 3f;

    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }
    
    private void OnDestroy()
    {
        
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Destroy();
        }
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}