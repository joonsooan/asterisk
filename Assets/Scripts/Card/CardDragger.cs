using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid grid;

    private GameObject _ghostBuildingInstance;
    private CardData _activeCardData;
    private bool _isDragging;
    
    public bool IsDragging => _isDragging;

    private void Update()
    {
        if (_isDragging)
        {
            HandleDragVisuals();
            HandleMousePlacement();
        }
    }

    public void StartDrag(CardData cardData)
    {
        if (cardData == null || _isDragging) return;

        _activeCardData = cardData;
        _isDragging = true;

        if (_activeCardData.buildingPrefab != null)
        {
            CreateGhostBuilding();
        }
        else
        {
            EndDrag();
        }
    }

    public void EndDrag()
    {
        if (_ghostBuildingInstance != null)
        {
            Destroy(_ghostBuildingInstance);
        }
        _isDragging = false;
        _activeCardData = null;
    }
    
    private void CreateGhostBuilding()
    {
        _ghostBuildingInstance = Instantiate(_activeCardData.buildingPrefab, Vector3.zero, Quaternion.identity);
        
        SpriteRenderer sr = _ghostBuildingInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color ghostColor = sr.color;
            ghostColor.a = 0.5f;
            sr.color = ghostColor;
        }
    }
    
    public CardData GetActiveCardData()
    {
        return _activeCardData;
    }
    
    private void HandleDragVisuals()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPosition);
        _ghostBuildingInstance.transform.position = cellCenterWorld;
        
        bool canPlace = BuildingManager.Instance.CanPlaceBuilding(cellPosition) && IsRoomUnlockedForPlacement(cellPosition);
        UpdateGhostColor(canPlace);
    }
    
    private void UpdateGhostColor(bool canPlace)
    {
        SpriteRenderer sr = _ghostBuildingInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = canPlace ? Color.green : Color.red;
            sr.color = new Color(color.r, color.g, color.b, 0.5f);
        }
    }

    private void HandleMousePlacement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int cellPosition = grid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            bool canPlace = BuildingManager.Instance.CanPlaceBuilding(cellPosition) && IsRoomUnlockedForPlacement(cellPosition);
            
            if (EventSystem.current.IsPointerOverGameObject())
            {
                EndDrag();
                GameManager.Instance.cardInfoManager.HideCardInfo();
                return;
            }
            
            if (canPlace)
            {
                AttemptPlacement(cellPosition);
                EndDrag();
                GameManager.Instance.cardInfoManager.HideCardInfo();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            EndDrag();
            GameManager.Instance.cardInfoManager.HideCardInfo();
        }
    }

    private void AttemptPlacement(Vector3Int cellPos)
    {
        if (ResourceManager.Instance.HasEnoughResources(_activeCardData.costs))
        {
            BuildingManager.Instance.PlaceBuilding(_activeCardData, cellPos);
            ResourceManager.Instance.SpendResources(_activeCardData.costs);
        }
        else
        {
            Debug.Log("Not enough resources.");
        }
    }

    private bool IsRoomUnlockedForPlacement(Vector3Int cellPos)
    {
        if (GameManager.Instance.mapGenerator == null)
        {
            return false;
        }

        Vector2Int roomCoordinates = GameManager.Instance.mapGenerator.GetRoomCoordinates(grid.GetCellCenterWorld(cellPos));
        return GameManager.Instance.mapGenerator.IsRoomUnlocked(roomCoordinates.x, roomCoordinates.y);
    }
}