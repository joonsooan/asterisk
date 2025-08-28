using UnityEngine;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    public CardData cardData;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.IsDragging() || GameManager.Instance.GetActiveData() != cardData)
        {
            GameManager.Instance.StartDrag(cardData);
        }
    }
}