using UnityEngine;

public class MainStructure : MonoBehaviour, IStorage
{
    [SerializeField] private int maxStorageAmount = 1000;
    private int _currentStorageAmount = 0;

    public bool StorageIsFull()
    {
        return _currentStorageAmount >= maxStorageAmount;
    }

    public void AddResource(ResourceType type, int amount)
    {
        _currentStorageAmount += amount;
        if (_currentStorageAmount > maxStorageAmount)
        {
            _currentStorageAmount = maxStorageAmount;
        }
        ResourceManager.Instance.AddResource(type, amount);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.RemoveStorage(this);
        }
    }
}
