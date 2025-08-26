using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragger : MonoBehaviour
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
    
    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (_isDragging)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
            Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPosition);
            _ghostBuildingInstance.transform.position = cellCenterWorld;

            bool canPlace = BuildingManager.Instance.CanPlaceBuilding(cellPosition);
            
            SpriteRenderer sr = _ghostBuildingInstance.GetComponent<SpriteRenderer>();
            if (sr != null) {
                Color color = canPlace ? Color.green : Color.red;
                sr.color = new Color(color.r, color.g, color.b, 0.5f);
            }

            if (Input.GetMouseButtonDown(0))
            {
                AttemptPlacement(cellPosition);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }

    public void TryStartDrag()
    {
        if (_cardDisplay == null || _cardDisplay.cardData == null) return;
        if (!ResourceManager.Instance.HasEnoughResources(_cardDisplay.cardData.costs))
        {
            Debug.Log("Not Enough Resources!");
            return;
        }

        if (GameManager.Instance.GetActiveDragger() != null)
        {
            GameManager.Instance.GetActiveDragger().EndDrag();
        }
        GameManager.Instance.SetActiveDragger(this);
    
        _isDragging = true;
        
        GameObject buildingPrefab = _cardDisplay.cardData.buildingPrefab;

        if (buildingPrefab != null) {
            _ghostBuildingInstance = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);

            SpriteRenderer sr = _ghostBuildingInstance.GetComponent<SpriteRenderer>();
            if (sr != null) {
                Color ghostColor = sr.color;
                ghostColor.a = 0.5f;
                sr.color = ghostColor;
            }
        }
    }
    
    private void AttemptPlacement(Vector3Int cellPos)
    {
        if (BuildingManager.Instance.CanPlaceBuilding(cellPos)  && IsRoomUnlockedForPlacement(cellPos)) {
            if (ResourceManager.Instance.HasEnoughResources(_cardDisplay.cardData.costs)) {
                BuildingManager.Instance.PlaceBuilding(_cardDisplay.cardData, cellPos);
                ResourceManager.Instance.SpendResources(_cardDisplay.cardData.costs);
                EndDrag();
            }
        } else {
            Debug.Log("Can't Place Here");
        }
    }

    private void CancelPlacement()
    {
        EndDrag();
        Debug.Log("Placement Canceled");
    }

    public void EndDrag()
    {
        if (_ghostBuildingInstance != null)
        {
            Destroy(_ghostBuildingInstance);
        }
        
        _isDragging = false;
        
        if (GameManager.Instance.GetActiveDragger() == this)
        {
            GameManager.Instance.SetActiveDragger(null);
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