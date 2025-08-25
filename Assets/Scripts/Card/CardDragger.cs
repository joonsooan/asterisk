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

    public void StartDrag()
    {
        if (_cardDisplay == null || _cardDisplay.cardData == null) return;
        if (!ResourceManager.Instance.HasEnoughResources(_cardDisplay.cardData.costs))
        {
            Debug.Log("Not Enough Resources!");
            return;
        }

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
        if (BuildingManager.Instance.CanPlaceBuilding(cellPos)) {
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

    private void EndDrag()
    {
        if (_ghostBuildingInstance != null)
        {
            Destroy(_ghostBuildingInstance);
        }
        _isDragging = false;
    }
}