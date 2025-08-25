using TMPro;
using UnityEngine;

public class CardInformation : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resourceInfoCellPrefab;
    public GameObject parent;
    
    [Header("References")]
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;
    
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
                ResourceInfoCell cellScript = newCell.GetComponent<ResourceInfoCell>();
                
                if (cellScript != null)
                {
                    cellScript.SetInfo(cost.resourceType, cost.amount);
                }
            }
        }
    }
}
