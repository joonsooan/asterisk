using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Ferrite,
    Aether,
    Biomass,
    CryoCrystal
}

public class ResourceManager : MonoBehaviour
{
    private readonly List<ResourceNode> _allResources = new List<ResourceNode>();
    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    public void AddResourceNode(ResourceNode node)
    {
        if (!_allResources.Contains(node)) {
            _allResources.Add(node);
        }
    }

    public void RemoveResourceNode(ResourceNode node)
    {
        if (_allResources.Contains(node)) {
            _allResources.Remove(node);
        }
    }

    public List<ResourceNode> GetAllResources()
    {
        return _allResources;
    }
}
