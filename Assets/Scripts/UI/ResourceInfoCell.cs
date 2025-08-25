using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceInfoCell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image resourceImage;
    [SerializeField] private TMP_Text resourceAmount;
    
    public void SetInfo(ResourceType type, int amount)
    {
        resourceAmount.text = amount.ToString();
        
        Sprite resourceIcon = ResourceManager.Instance.GetResourceIcon(type);
        if (resourceIcon != null)
        {
            resourceImage.sprite = resourceIcon;
        }
        else
        {
            resourceImage.sprite = null; 
        }
    }
}
