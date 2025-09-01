using System;
using UnityEngine;

public interface IStorage
{
    event Action<int, int> OnStorageChanged; 
    
    bool StorageIsFull();
    void AddResource(ResourceType type, int amount);
    Vector3 GetPosition();
}