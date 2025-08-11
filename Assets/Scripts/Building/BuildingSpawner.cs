using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap buildingTilemap;
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private TileBase buildingTile;
    [SerializeField] private Grid grid;

    public Tilemap BuildingTilemap {
        get {
            return buildingTilemap;
        }
    }

    public void SpawnBuildings()
    {
        if (buildingTilemap == null || buildingPrefab == null || buildingTile == null || grid == null) {
            Debug.LogError("BuildingSpawner: Missing references.");
            return;
        }

        foreach (Vector3Int cellPosition in buildingTilemap.cellBounds.allPositionsWithin) {
            if (buildingTilemap.HasTile(cellPosition) && buildingTilemap.GetTile(cellPosition) == buildingTile) {
                Vector3 worldPos = grid.GetCellCenterWorld(cellPosition);
                worldPos += new Vector3(0.5f, 0.5f, 0f);
                Instantiate(buildingPrefab, worldPos, Quaternion.identity);
            }
        }
    }
}
