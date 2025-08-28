using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour 
{
    [Header("Card Info Panel")]
    [SerializeField] private GameObject cardInfoPanel;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescriptionText;
    [SerializeField] private GameObject cardResourcePanel;
    [SerializeField] private GameObject resourceInfoCellPrefab;
    
    [Header("Recipe Info Panel")]
    [SerializeField] private GameObject recipeInfoPanel;
    [SerializeField] private RecipeInfo recipeInfoComponent;
    
    private ComboCardData _pinnedRecipeData = null;

    private void Start()
    {
        if (recipeInfoPanel.activeSelf)
        {
            recipeInfoComponent.ClearInfo();
        }
    }

    public void DisplayCardInfo(CardData data)
    {
        if (data == null) return;
        cardInfoPanel.SetActive(true);
        cardNameText.text = data.displayName;
        cardDescriptionText.text = data.description;
        
        foreach (Transform child in cardResourcePanel.transform) Destroy(child.gameObject);
        
        foreach (var cost in data.costs)
        {
            var cell = Instantiate(resourceInfoCellPrefab, cardResourcePanel.transform);
            cell.GetComponent<ResourceInfoCell>().SetInfo(cost.resourceType, cost.amount);
        }
    }
    
    public void HideCardInfo()
    {
        cardInfoPanel.SetActive(false);
    }
    
    public void DisplayRecipeInfo(ComboCardData data)
    {
        if (data == null || recipeInfoPanel == null) return;
        
        recipeInfoPanel.SetActive(true); 
        recipeInfoComponent.UpdateRecipeInfo(data);
    }

    public void HideRecipeInfo()
    {
        if (recipeInfoPanel == null || !recipeInfoPanel.activeSelf) return;

        if (_pinnedRecipeData != null)
        {
            recipeInfoComponent.UpdateRecipeInfo(_pinnedRecipeData);
        }
        else
        {
            recipeInfoComponent.ClearInfo();
        }
    }
    
    public void PinRecipeInfo(ComboCardData data)
    {
        if (_pinnedRecipeData == data)
        {
            _pinnedRecipeData = null;
            recipeInfoComponent.ClearInfo();
        }
        else
        {
            _pinnedRecipeData = data;
            DisplayRecipeInfo(data);
        }
    }

    
    public void ToggleRecipePanel()
    {
        if (recipeInfoPanel == null) return;

        bool shouldBeActive = !recipeInfoPanel.activeSelf;
        recipeInfoPanel.SetActive(shouldBeActive);

        if (!shouldBeActive)
        {
            _pinnedRecipeData = null;
        }
        else
        {
            recipeInfoComponent.ClearInfo();
        }
    }
}