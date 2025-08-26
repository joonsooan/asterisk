using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeInfo : MonoBehaviour
{
    public TMP_Text recipeNameText;
    public TMP_Text recipeDescriptionText;
    
    public GridLayoutGroup recipeGrid;
    public GadgetPrefabMapping[] gadgetPrefabs;
    public GameObject emptyPrefab;
    
    private Dictionary<GadgetType, GameObject> gadgetPrefabMap = new Dictionary<GadgetType, GameObject>();

    [System.Serializable]
    public class GadgetPrefabMapping
    {
        public GadgetType type;
        public GameObject prefab;
    }
    
    private void Awake()
    {
        foreach (var mapping in gadgetPrefabs)
        {
            if (!gadgetPrefabMap.ContainsKey(mapping.type))
            {
                gadgetPrefabMap.Add(mapping.type, mapping.prefab);
            }
        }
    }
    
    public void UpdateRecipeInfo(ComboCardData data)
    {
        recipeNameText.text = data.comboName;
        recipeDescriptionText.text = data.comboDescription;

        ClearRecipeGrid();

        Dictionary<Vector2, GadgetType> recipeDict = new Dictionary<Vector2, GadgetType>();
        foreach (var piece in data.recipe)
        {
            recipeDict.Add(new Vector2(piece.relativePosition.x, piece.relativePosition.y), piece.gadgetType);
        }

        for (int y = 1; y >= -1; y--)
        {
            for (int x = -1; x <= 1; x++)
            {
                GameObject prefabToInstantiate;
                Vector2 currentPos = new Vector2(x, y);
                
                if (recipeDict.TryGetValue(currentPos, out var value))
                {
                    prefabToInstantiate = GetGadgetPrefab(value);
                }
                else
                {
                    prefabToInstantiate = emptyPrefab;
                }

                if (prefabToInstantiate != null)
                {
                    Instantiate(prefabToInstantiate, recipeGrid.transform);
                }
            }
        }
    }

    private GameObject GetGadgetPrefab(GadgetType type)
    {
        if (gadgetPrefabMap.TryGetValue(type, out GameObject prefab))
        {
            return prefab;
        }
        return null;
    }

    private void ClearRecipeGrid()
    {
        foreach (Transform child in recipeGrid.transform)
        {
            Destroy(child.gameObject);
        }
    }
}