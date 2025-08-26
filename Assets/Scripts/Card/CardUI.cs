using UnityEngine;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    public CardData cardData;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.StartDrag(cardData);
    }
}