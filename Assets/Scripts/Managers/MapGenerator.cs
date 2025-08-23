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

    private Bounds _mapWorldBounds;

    public void GenerateMap()
    {
        int totalMapWidth = roomSize.x * mapGridSize.x;
        int totalMapHeight = roomSize.y * mapGridSize.y;

        int mapCenterX = totalMapWidth / 2;
        int mapCenterY = totalMapHeight / 2;

        for (int x = 0; x < mapGridSize.x; x++) {
            for (int y = 0; y < mapGridSize.y; y++) {
                Vector3Int roomOrigin = new Vector3Int(
                    x * roomSize.x, y * roomSize.y, 0);

                for (int i = 0; i < roomSize.x; i++) {
                    for (int j = 0; j < roomSize.y; j++) {
                        TileBase tileToUse = groundTile;
                        if (i == 0 || i == roomSize.x - 1 || j == 0 || j == roomSize.y - 1) {
                            tileToUse = wallTile;
                        }
                        tilemap.SetTile(roomOrigin + new Vector3Int(
                            i - mapCenterX, j - mapCenterY, 0), tileToUse);
                    }
                }
            }
        }

        tilemap.CompressBounds();
        Bounds localBounds = tilemap.localBounds;
        Vector3 worldCenter = tilemap.transform.TransformPoint(localBounds.center);
        Vector3 worldSize = Vector3.Scale(localBounds.size, tilemap.transform.lossyScale);
        _mapWorldBounds = new Bounds(worldCenter, worldSize);

        CameraController cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null) {
            cameraController.SetBounds(_mapWorldBounds);
        }
    }
}
