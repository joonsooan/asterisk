using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    
    [Header("Shortcut Key")]
    [SerializeField] private KeyCode shortcutKey;

    [Header("UI References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;

    private CardDragger _cardDragger;
    
    private void Start()
    {
        _cardDragger = GetComponent<CardDragger>();
        UpdateCardUI();
        buyButton.onClick.AddListener(() => _cardDragger.StartDrag());
    }
    
    private void Update()
    {
        UpdateButtonState();
        CheckForShortcutKey();
    }
    
    private void CheckForShortcutKey()
    {
        if (Input.GetKeyDown(shortcutKey) && buyButton.interactable)
        {
            _cardDragger.StartDrag();
        }
    }

    private void UpdateButtonState()
    {
        if (cardData != null && ResourceManager.Instance != null) {
            bool canAfford = ResourceManager.Instance.HasEnoughResources(cardData.costs);
            buyButton.interactable = canAfford;
        }
    }

    private void UpdateCardUI()
    {
        if (cardData == null) {
            return;
        }

        nameText.text = cardData.cardName;
        // cardImage.sprite = cardData.cardImage;
        
        StringBuilder costString = new StringBuilder();
        for (int i = 0; i < cardData.costs.Length; i++) {
            CardCost cost = cardData.costs[i];
            costString.Append($"{cost.amount} {cost.resourceType}");
            if (i < cardData.costs.Length - 1) {
                costString.Append("\n");
            }
        }
        costText.text = costString.ToString();
    }
}