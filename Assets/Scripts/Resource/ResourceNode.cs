using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [Header("Resource Stats")]
    public ResourceType resourceType;
    public int amountToMine = 100;

    [Header("Visuals")]
    [SerializeField] private Color highlightColor = Color.red;
    [HideInInspector] public Vector3Int cellPosition;

    private Color _originalColor;
    private SpriteRenderer _sr;

    public bool IsReserved { get; private set; }

    public bool IsDepleted {
        get {
            return amountToMine <= 0;
        }
    }

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _originalColor = _sr.color;

        if (ResourceManager.Instance != null) {
            ResourceManager.Instance.AddResourceNode(this);
        }
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null) {
            ResourceManager.Instance.RemoveResourceNode(this);
        }
        if (BuildingManager.Instance != null) {
            BuildingManager.Instance.RemoveResourceTile(cellPosition);
        }
    }

    public void Reserve()
    {
        IsReserved = true;
        if (_sr != null) {
            _sr.color = highlightColor;
        }
    }

    public void Unreserve()
    {
        IsReserved = false;
        if (_sr != null) {
            _sr.color = _originalColor;
        }
    }

    public int Mine(int workAmount)
    {
        int amountMined = Mathf.Min(amountToMine, workAmount);
        amountToMine -= amountMined;

        if (IsDepleted) {
            Unreserve();
            Destroy(gameObject);
        }

        return amountMined;
    }
}
