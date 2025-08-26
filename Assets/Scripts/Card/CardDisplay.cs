using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    
    [Header("Shortcut Key")]
    [SerializeField] private KeyCode shortcutKey;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button buyButton;

    private CardDragger _cardDragger;
    
    private void Start()
    {
        _cardDragger = GetComponent<CardDragger>();
        UpdateCardUI();
        buyButton.onClick.AddListener(OnCardClick);
    }
    
    private void Update()
    {
        UpdateButtonState();
        // CheckForShortcutKey();
    }

    private void OnCardClick()
    {
        CardDragger activeDragger = GameManager.Instance.GetActiveDragger();

        if (activeDragger == _cardDragger)
        {
            _cardDragger.EndDrag();
        }
        else
        {
            if (GameManager.Instance.cardInfoManager != null)
            {
                GameManager.Instance.cardInfoManager.UpdateCardUI(cardData);
            }

            _cardDragger.TryStartDrag();
        }
    }

    // private void CheckForShortcutKey()
    // {
    //     if (!GameManager.Instance.isCameraActive && Input.GetKeyDown(shortcutKey) && buyButton.interactable)
    //     {
    //         OnCardClick();
    //     }
    // }

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
    }
}