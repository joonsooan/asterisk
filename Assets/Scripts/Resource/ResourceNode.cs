using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [Header("Resource Stats")]
    public ResourceType resourceType;
    [HideInInspector] public int amountToMine; // 채굴해 얻을 수 있는 자원의 총량
    [HideInInspector] public float timeToMinePerUnit; // 한 번 채굴을 완료하는 데 걸리는 시간

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
            ResourceStats stats = ResourceManager.Instance.GetResourceStats(resourceType);
            
            if (stats != null)
            {
                amountToMine = stats.amountToMine;
                timeToMinePerUnit = stats.timeToMinePerUnit;
            }
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
