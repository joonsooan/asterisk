using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap resourceTilemap;
    [SerializeField] private TileBase[] resourceTiles;
    [SerializeField] private GameObject[] resourcePrefabs;
    [SerializeField] private Grid grid;

    public Tilemap ResourceTilemap {
        get {
            return resourceTilemap;
        }
    }

    public void SpawnResources()
    {
        foreach (Vector3Int pos in resourceTilemap.cellBounds.allPositionsWithin) {
            TileBase currentTile = resourceTilemap.GetTile(pos);

            if (currentTile == null) continue;
            for (var i = 0; i < resourceTiles.Length; i++)
            {
                var t = resourceTiles[i];
                if (currentTile != t) continue;

                Vector3 worldPos = grid.GetCellCenterWorld(pos);
                Instantiate(resourcePrefabs[i], worldPos, Quaternion.identity);
                break;
            }
        }
    }
}
