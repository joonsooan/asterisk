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
        HandleMouseInput();
    }

    public void StartDrag(CardData cardData)
    {
        if (cardData == null || _isDragging) return;

        _activeCardData = cardData;
        _isDragging = true;

        if (_activeCardData.buildingPrefab != null)
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

    private void HandleMouseInput()
    {
        if (_isDragging)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            Vector3Int cellPosition = grid.WorldToCell(mouseWorldPos);
            Vector3 cellCenterWorld = grid.GetCellCenterWorld(cellPosition);
            _ghostBuildingInstance.transform.position = cellCenterWorld;

            bool canPlace = BuildingManager.Instance.CanPlaceBuilding(cellPosition) && IsRoomUnlockedForPlacement(cellPosition);
            SpriteRenderer sr = _ghostBuildingInstance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = canPlace ? Color.green : Color.red;
                sr.color = new Color(color.r, color.g, color.b, 0.5f);
            }

            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                AttemptPlacement(cellPosition);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                GameManager.Instance.CancelDrag();
            }
        }
    }

    private void AttemptPlacement(Vector3Int cellPos)
    {
        if (ResourceManager.Instance.HasEnoughResources(_activeCardData.costs))
        {
            BuildingManager.Instance.PlaceBuilding(_activeCardData, cellPos);
            ResourceManager.Instance.SpendResources(_activeCardData.costs);
            GameManager.Instance.EndDrag();
        }
        else
        {
            Debug.Log("Not enough resources to place this building.");
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