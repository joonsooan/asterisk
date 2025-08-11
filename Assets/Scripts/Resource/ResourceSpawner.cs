using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap resourceTilemap;
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private TileBase resourceTile;
    [SerializeField] private Grid grid;

    public Tilemap ResourceTilemap {
        get {
            return resourceTilemap;
        }
    }

    public void SpawnResources()
    {
        if (resourceTilemap == null || resourcePrefab == null || resourceTile == null || grid == null) {
            Debug.LogError("ResourceSpawner: Missing references.");
            return;
        }

        foreach (Vector3Int pos in resourceTilemap.cellBounds.allPositionsWithin) {
            if (resourceTilemap.HasTile(pos) && resourceTilemap.GetTile(pos) == resourceTile) {
                Vector3 worldPos = grid.GetCellCenterWorld(pos);
                worldPos += new Vector3(0.5f, 0.5f, 0f);
                Instantiate(resourcePrefab, worldPos, Quaternion.identity);
            }
        }
    }
}
