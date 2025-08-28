using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    public static event Action<ResourceType[]> OnMineableTypesChanged;
    
    public IReadOnlyList<ResourceType> CurrentMineableTypes => _currentMineableTypes;
    private List<ResourceType> _currentMineableTypes = new();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        _currentMineableTypes = ((ResourceType[])Enum.GetValues(typeof(ResourceType))).ToList();
    }

    public void UpdateAllLifterMineableTypes(List<ResourceType> newTypes)
    {
        _currentMineableTypes = new List<ResourceType>(newTypes);

        OnMineableTypesChanged?.Invoke(newTypes.ToArray());
    }
}