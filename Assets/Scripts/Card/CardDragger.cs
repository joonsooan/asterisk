using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private Grid grid;

    private CardDisplay _cardDisplay;
    private GameObject _ghostBuildingInstance;
    private bool _isDragging;

    private void Start()
    {
        _cardDisplay = GetComponent<CardDisplay>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_cardDisplay == null || _cardDisplay.cardData == null) return;
        if (!ResourceManager.Instance.HasEnoughResources(_cardDisplay.cardData.costs)) {
            Debug.Log("Not Enough Resources!");
            return;
        }

        _isDragging = true;
        GameObject buildingPrefab = _cardDisplay.cardData.buildingPrefab;

        if (buildingPrefab != null) {
            _ghostBuildingInstance = Instantiate(buildingPrefab, transform.position, Quaternion.identity);
            
            SpriteRenderer sr = _ghostBuildingInstance.GetComponent<SpriteRenderer>();
            if (sr != null) {
                Color ghostColor = sr.color;
                ghostColor.a = 0.5f;
                sr.color = ghostColor;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || _ghostBuildingInstance == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = 0;
        
        Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPosition);
        _ghostBuildingInstance.transform.position = cellCenterWorld;

        bool canPlace = BuildingManager.Instance.CanPlaceBuilding(cellPosition);
        
        Color color = canPlace ? Color.green : Color.red;
        SpriteRenderer sr = _ghostBuildingInstance.GetComponent<SpriteRenderer>();
        if (sr != null) {
            sr.color = new Color(color.r, color.g, color.b, 0.5f);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging || _ghostBuildingInstance == null) {
            return;
        }

        _isDragging = false;
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector3Int cellPos = grid.WorldToCell(mouseWorldPos);
        
        if (BuildingManager.Instance.CanPlaceBuilding(cellPos)) {
            if (ResourceManager.Instance.HasEnoughResources(_cardDisplay.cardData.costs)) {
                BuildingManager.Instance.PlaceBuilding(_cardDisplay.cardData, cellPos);
                ResourceManager.Instance.SpendResources(_cardDisplay.cardData.costs);
            }
        } else {
            Debug.Log("Can't Place Here");
        }

        Destroy(_ghostBuildingInstance);
    }
}