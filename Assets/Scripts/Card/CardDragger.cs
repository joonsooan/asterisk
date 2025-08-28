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

    public void StartDrag(DisplayableData data)
    {
        if (_isDragging) return;
        
        CardData cardData = data as CardData;
        if (cardData == null || cardData.buildingPrefab == null || _isDragging)
        {
            return;
        }
        
        _activeCardData = cardData;
        _isDragging = true;

        CreateGhostBuilding();
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
    
    private void HandleDragVisuals()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
        Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPosition);
        
        if (_ghostBuildingInstance != null)
        {
            _ghostBuildingInstance.transform.position = cellCenterWorld;
        }
        
        bool canPlace = BuildingManager.Instance.CanPlaceBuilding(cellPosition) && IsRoomUnlockedForPlacement(cellPosition);
        UpdateGhostColor(canPlace);
    }
    
    private void UpdateGhostColor(bool canPlace)
    {
        if (_ghostBuildingInstance == null) return;
        
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
            if (EventSystem.current.IsPointerOverGameObject())
            {
                GameManager.Instance.EndDrag();
                return;
            }
            
            Vector3Int cellPosition = grid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            bool canPlace = BuildingManager.Instance.CanPlaceBuilding(cellPosition) && IsRoomUnlockedForPlacement(cellPosition);
            
            if (canPlace)
            {
                AttemptPlacement(cellPosition);
                GameManager.Instance.EndDrag();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            GameManager.Instance.EndDrag();
        }
    }

    private void AttemptPlacement(Vector3Int cellPos)
    {
        if (ResourceManager.Instance.HasEnoughResources(_activeCardData.costs))
        {
            BuildingManager.Instance.PlaceBuilding(_activeCardData, cellPos);
            ResourceManager.Instance.SpendResources(_activeCardData.costs);
            GameManager.Instance.uiManager?.HideCardInfoOnPlacement();
            GameManager.Instance.uiManager?.UnpinAndHideCardPanel();
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