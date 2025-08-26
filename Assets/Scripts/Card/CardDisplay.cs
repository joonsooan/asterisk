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
        buyButton.onClick.AddListener(OnCardClick);
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void OnCardClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectCard(cardData);
        }
    }

    private void UpdateButtonState()
    {
        if (cardData != null && ResourceManager.Instance != null)
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

        nameText.text = cardData.cardName;
    }
}