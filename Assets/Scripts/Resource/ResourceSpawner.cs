using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    public Tilemap resourceTilemap;
    public GameObject resourcePrefab;
    public TileBase resourceTile;

    private void Start()
    {
        foreach (Vector3Int pos in resourceTilemap.cellBounds.allPositionsWithin) {
            if (resourceTilemap.HasTile(pos)) {
                Vector3 worldPos = resourceTilemap.GetCellCenterWorld(pos);

                if (resourceTilemap.GetTile(pos) == resourceTile) {
                    Instantiate(resourcePrefab, worldPos, Quaternion.identity);
                }
            }
        }
    }
}
