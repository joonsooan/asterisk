using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Generation Settings")]
    public Tilemap tilemap;
    public TileBase groundTile;
    public TileBase wallTile;
    public Vector2Int roomSize = new Vector2Int(50, 30);
    public Vector2Int mapGridSize = new Vector2Int(5, 5);

    public void GenerateMap()
    {
        for (int x = 0; x < mapGridSize.x; x++) {
            for (int y = 0; y < mapGridSize.y; y++) {
                Vector3Int roomOrigin = new Vector3Int(
                    x * roomSize.x, y * roomSize.y, 0);

                int moveX = roomSize.x * mapGridSize.x / 2;
                int moveY = roomSize.y * mapGridSize.y / 2;

                for (int i = 0; i < roomSize.x; i++) {
                    for (int j = 0; j < roomSize.y; j++) {
                        TileBase tileToUse = groundTile;
                        if (i == 0 || i == roomSize.x - 1 || j == 0 || j == roomSize.y - 1) {
                            tileToUse = wallTile;
                        }
                        tilemap.SetTile(roomOrigin + new Vector3Int(
                            i - moveX, j - moveY, 0), tileToUse);
                    }
                }
            }
        }
    }
}
