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
    
    public IReadOnlyList<UnitBase> EnemyUnits => _enemyUnits;
    public IReadOnlyList<UnitBase> AllyUnits => _allyUnits;

    private readonly List<UnitBase> _enemyUnits = new();
    private readonly List<UnitBase> _allyUnits = new();
    
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
    
    public void AddUnit(UnitBase unit)
    {
        if (unit.unitType == UnitBase.UnitType.Enemy && !_enemyUnits.Contains(unit))
        {
            _enemyUnits.Add(unit);
        }
        else if (unit.unitType == UnitBase.UnitType.Ally && !_allyUnits.Contains(unit))
        {
            _allyUnits.Add(unit);
        }
    }

    public void RemoveUnit(UnitBase unit)
    {
        if (unit.unitType == UnitBase.UnitType.Enemy)
        {
            _enemyUnits.Remove(unit);
        }
        else if (unit.unitType == UnitBase.UnitType.Ally)
        {
            _allyUnits.Remove(unit);
        }
    }
}