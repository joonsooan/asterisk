using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : InfoDisplayTrigger
{
    public CardData cardData;

    [Header("UI References")]
    [SerializeField] private Image cardIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button buyButton;

    private void Awake()
    {
        UpdateCardUI();
        buyButton.onClick.AddListener(OnBuyButtonClick);
    }
    
    private void Update()
    {
        UpdateButtonState();
    }

    protected override DisplayableData GetData() => cardData;
    protected override void ShowInfo() => GameManager.Instance?.uiManager.DisplayCardInfo(cardData);
    protected override void HideInfo() => GameManager.Instance?.uiManager.HideCardInfo();

    private void OnBuyButtonClick()
    {
        if (GameManager.Instance != null && cardData != null)
        {
            if (GameManager.Instance.IsDragging() && GameManager.Instance.GetActiveData() == cardData)
            {
                GameManager.Instance.EndDrag();
            }
            else
            {
                GameManager.Instance.StartDrag(cardData);
            }
        }
    }

    private void UpdateButtonState()
    {
        if (cardData != null && ResourceManager.Instance != null && buyButton != null)
        {
            bool canAfford = ResourceManager.Instance.HasEnoughResources(cardData.costs);
            buyButton.interactable = canAfford;
        }
    }

    private void UpdateCardUI()
    {
        nameText.text = cardData.displayName;
        cardIcon.sprite = cardData.icon;
    }
}