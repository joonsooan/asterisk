using TMPro;
using UnityEngine;

public class CardInfoManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resourceInfoCellPrefab;
    public GameObject parent;
    
    [Header("References")]
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;
    [SerializeField] private GameObject cardInfoPanel;
    [SerializeField] private GameObject resourceInfoPanel;

    public void UpdateCardUI(CardData data)
    {
        cardName.text = data.cardName;
        cardDescription.text = data.cardDescription;
        
        if (parent != null)
        {
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }
        }

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

    public void ToggleCardInfoPanel(bool isActive)
    {
        cardInfoPanel.SetActive(isActive);
        if (isActive)
        {
            cardName.text = "";
            cardDescription.text = "";
            
            if (resourceInfoPanel != null)
            {
                foreach (Transform child in resourceInfoPanel.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}