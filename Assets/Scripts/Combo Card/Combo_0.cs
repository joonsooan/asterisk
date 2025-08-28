using UnityEngine;

public class Combo_0 : MonoBehaviour, ICombo, IStorage
{
    [Header("Settings")]
    [SerializeField] private int maxHealth = 100;
    // [SerializeField] private float constructionTime = 3f;

    [Header("Structure Values")]
    [SerializeField] private int maxStorageAmount;
    
    private int _currentHealth;
    private int _currentStorageAmount;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _currentStorageAmount = 0;
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.RemoveStorage(this);
        }
    }

    public bool StorageIsFull()
    {
        return  _currentStorageAmount >= maxStorageAmount;
    }
    
    public void AddResource(ResourceType type, int amount)
    {
        _currentStorageAmount += amount;
        if (_currentStorageAmount > maxStorageAmount)
        {
            _currentStorageAmount = maxStorageAmount;
        }
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