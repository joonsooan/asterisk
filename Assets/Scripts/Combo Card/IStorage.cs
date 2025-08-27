using UnityEngine;

public interface IStorage
{
    bool StorageIsFull();
    void AddResource(ResourceType type, int amount);
    Vector3 GetPosition();
}