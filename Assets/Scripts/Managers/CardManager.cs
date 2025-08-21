using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void BuyCard(CardData cardToBuy)
    {
        if (ResourceManager.Instance.HasEnoughResources(cardToBuy.costs)) {
            ResourceManager.Instance.SpendResources(cardToBuy.costs);

            Debug.Log($"{cardToBuy.cardName} 카드를 성공적으로 구매했습니다!");
        }
        else {
            Debug.Log("자원이 부족하여 카드를 구매할 수 없습니다.");
        }
    }
}
