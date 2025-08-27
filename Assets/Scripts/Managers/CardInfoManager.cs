using TMPro;
using UnityEngine;

public class CardInfoManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resourceInfoCellPrefab;
    [SerializeField] private GameObject parent;
    
    [Header("References")]
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;
    [SerializeField] private GameObject cardInfoPanel;
    [SerializeField] private GameObject resourceInfoPanel;
    [SerializeField] private GameObject recipeInfoPanel;

    private bool _isRecipeInfoPanelActive;

    public void DisplayCardInfo(CardData data)
    {
        if (cardInfoPanel == null) return;
        
        cardInfoPanel.SetActive(true);
        
        ClearResourceInfoPanel();
        
        if (cardName != null) cardName.text = data.cardName;
        if (cardDescription != null) cardDescription.text = data.cardDescription;
        
        if (resourceInfoCellPrefab != null && parent != null)
        {
            foreach (CardCost cost in data.costs)
            {
                GameObject newCell = Instantiate(resourceInfoCellPrefab, parent.transform);
                ResourceInfoCell resourceInfoCell = newCell.GetComponent<ResourceInfoCell>();
                
                if (resourceInfoCell != null)
                {
                    resourceInfoCell.SetInfo(cost.resourceType, cost.amount);
                }
            }
        }
    }

    private void ClearResourceInfoPanel()
    {
        if (resourceInfoPanel != null)
        {
            foreach (Transform child in resourceInfoPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void HideCardInfo()
    {
        cardInfoPanel.SetActive(false);
        
        cardName.text = "";
        cardDescription.text = "";

        ClearResourceInfoPanel();
    }

    public void ToggleRecipePanel()
    {
        if (_isRecipeInfoPanelActive)
        {
            HideRecipePanel();
        }
        else
        {
            DisplayRecipePanel();
        }
        _isRecipeInfoPanelActive = !_isRecipeInfoPanelActive;
    }

    private void DisplayRecipePanel()
    {
        recipeInfoPanel.SetActive(true);
    }

    private void HideRecipePanel()
    {
        recipeInfoPanel.SetActive(false);
    }
}