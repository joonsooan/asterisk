using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    public static event Action<ResourceType[]> OnMineableTypesChanged;

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
    }

    public void UpdateAllLifterMineableTypes(List<ResourceType> newTypes)
    {
        OnMineableTypesChanged?.Invoke(newTypes.ToArray());
    }
}