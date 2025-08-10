using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [Header("Resource Stats")]
    public int amountToMine = 100;

    public bool IsDepleted {
        get {
            return amountToMine <= 0;
        }
    }

    public void Mine(int workAmount)
    {
        amountToMine -= workAmount;

        if (IsDepleted) {
            Debug.Log(gameObject.name + " 을 모두 캤습니다.");
            Destroy(gameObject);
        }
    }
}
