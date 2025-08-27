using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardData cardData;

    [Header("UI References")]
    [SerializeField] private Image cardIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button buyButton;
    
    private bool _isUIPinned  = false;

    private void Start()
    {
        UpdateCardUI();
        buyButton.onClick.AddListener(OnBuyButtonClick);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onStartDrag.AddListener(OnDragStart);
            GameManager.Instance.onEndDrag.AddListener(OnDragEnd);
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onStartDrag.RemoveListener(OnDragStart);
            GameManager.Instance.onEndDrag.RemoveListener(OnDragEnd);
        }
    }

    private void Update()
    {
        UpdateButtonState();
    }
    
    private void OnDragStart(CardData activeCardData)
    {
        if (activeCardData == cardData)
        {
            _isUIPinned = true;
        }
        else
        {
            _isUIPinned = false;
            GameManager.Instance.cardInfoManager.HideCardInfo();
        }
    }
    
    private void OnDragEnd()
    {
        _isUIPinned = false;
        GameManager.Instance.cardInfoManager.HideCardInfo();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isUIPinned && !GameManager.Instance.IsDragging())
        {
            GameManager.Instance.cardInfoManager.DisplayCardInfo(cardData);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isUIPinned)
        {
            GameManager.Instance.cardInfoManager.HideCardInfo();
        }
    }

    private void OnBuyButtonClick()
    {
        if (GameManager.Instance != null && cardData != null)
        {
            if (GameManager.Instance.IsDragging() && GameManager.Instance.GetActiveCardData() == cardData)
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
        nameText.text = cardData.cardName;
        cardIcon.sprite = cardData.cardIcon;
    }
}