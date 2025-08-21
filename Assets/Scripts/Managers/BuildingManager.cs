using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap resourceTilemap;
    [SerializeField] private Tilemap buildingTilemap;
    [SerializeField] private TileBase buildingTile;

    public static BuildingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }
    
    public Tilemap GetBuildingTilemap()
    {
        return buildingTilemap;
    }

    public bool CanPlaceBuilding(Vector3Int cellPosition)
    {
        if (grid == null || groundTilemap == null || resourceTilemap == null || buildingTilemap == null) {
            Debug.LogError("BuildingManager: Missing references.");
            return false;
        }

        if (!groundTilemap.HasTile(cellPosition)) return false;
        if (resourceTilemap.HasTile(cellPosition)) return false;
        if (buildingTilemap.HasTile(cellPosition)) return false;

        return true;
    }

    public void PlaceBuilding(GameObject buildingPrefabToPlace, Vector3Int cellPosition)
    {
        if (CanPlaceBuilding(cellPosition)) {
            Vector3 worldPosition = grid.GetCellCenterWorld(cellPosition);
            Instantiate(buildingPrefabToPlace, worldPosition, Quaternion.identity);
            buildingTilemap.SetTile(cellPosition, buildingTile);
            Debug.Log($"Building placed at {cellPosition}.");
        }
        else {
            Debug.Log($"Cannot place building at {cellPosition}.");
        }
    }
}
