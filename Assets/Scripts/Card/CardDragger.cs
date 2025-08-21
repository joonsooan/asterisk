using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private Grid grid;

    private CardDisplay cardDisplay;
    private GameObject ghostBuildingInstance;
    private bool isDragging = false;

    private void Start()
    {
        cardDisplay = GetComponent<CardDisplay>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (cardDisplay == null || cardDisplay.cardData == null) return;
        if (!ResourceManager.Instance.HasEnoughResources(cardDisplay.cardData.costs)) {
            Debug.Log("Not Enough Resources!");
            return;
        }

        isDragging = true;
        GameObject buildingPrefab = cardDisplay.cardData.buildingPrefab;

        if (buildingPrefab != null) {
            ghostBuildingInstance = Instantiate(buildingPrefab, transform.position, Quaternion.identity);
            
            SpriteRenderer sr = ghostBuildingInstance.GetComponent<SpriteRenderer>();
            if (sr != null) {
                Color ghostColor = sr.color;
                ghostColor.a = 0.5f;
                sr.color = ghostColor;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || ghostBuildingInstance == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = 0;
        
        Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPosition);
        ghostBuildingInstance.transform.position = cellCenterWorld;

        bool canPlace = BuildingManager.Instance.CanPlaceBuilding(cellPosition);
        
        Color color = canPlace ? Color.green : Color.red;
        SpriteRenderer sr = ghostBuildingInstance.GetComponent<SpriteRenderer>();
        if (sr != null) {
            sr.color = new Color(color.r, color.g, color.b, 0.5f);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || ghostBuildingInstance == null) {
            return;
        }

        isDragging = false;
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector3Int cellPos = grid.WorldToCell(mouseWorldPos);
        GameObject buildingPrefab = cardDisplay.cardData.buildingPrefab;
        
        if (BuildingManager.Instance.CanPlaceBuilding(cellPos)) {
            if (ResourceManager.Instance.HasEnoughResources(cardDisplay.cardData.costs)) {
                BuildingManager.Instance.PlaceBuilding(buildingPrefab, cellPos);
                ResourceManager.Instance.SpendResources(cardDisplay.cardData.costs);
                Debug.Log("Building Constructed");
            }
        } else {
            Debug.Log("Can't Place Here");
        }

        Destroy(ghostBuildingInstance);
    }
}