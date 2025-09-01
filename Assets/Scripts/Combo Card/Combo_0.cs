using UnityEngine;

public class Combo_0 : Damageable, IStorage
{
    [Header("Values")]
    [SerializeField] private int maxStorageAmount;
    
    private int _currentStorageAmount;

    protected override void OnEnable()
    {
        base.OnEnable();
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
}