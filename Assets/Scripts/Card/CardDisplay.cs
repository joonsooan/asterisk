using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button buyButton;

    private void Start()
    {
        UpdateCardUI();
        buyButton.onClick.AddListener(OnBuyButtonClick);
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void OnBuyButtonClick()
    {
        if (GameManager.Instance != null && cardData != null)
        {
            GameManager.Instance.StartDrag(cardData);
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
        if (cardData == null)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = cardData.cardName;
        }
    }
}