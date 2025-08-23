using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Tiles")]
    public Tilemap tilemap;
    public TileBase groundTile;
    public TileBase wallTile;
    public TileBase fogTile;

    [Header("Map Generation Settings")]
    public Vector2Int roomSize = new Vector2Int(50, 30);
    public Vector2Int mapGridSize = new Vector2Int(5, 5);

    private Bounds _mapWorldBounds;
    private bool[,] _roomUnlocked;
    private int _totalMapHeight;

    private int _totalMapWidth;

    public void GenerateMap()
    {
        _totalMapWidth = roomSize.x * mapGridSize.x;
        _totalMapHeight = roomSize.y * mapGridSize.y;

        _roomUnlocked = new bool[mapGridSize.x, mapGridSize.y];
        for (int x = 0; x < mapGridSize.x; x++) {
            for (int y = 0; y < mapGridSize.y; y++) {
                _roomUnlocked[x, y] = false;
            }
        }
        _roomUnlocked[2, 2] = true;

        for (int x = 0; x < mapGridSize.x; x++) {
            for (int y = 0; y < mapGridSize.y; y++) {
                DrawRoom(x, y, !_roomUnlocked[x, y]);
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

    public void UnlockRoom(int roomX, int roomY)
    {
        if (roomX < 0 || roomX >= mapGridSize.x || roomY < 0 || roomY >= mapGridSize.y) {
            Debug.LogWarning("Invalid room coordinates provided.");
            return;
        }

        if (!_roomUnlocked[roomX, roomY]) {
            _roomUnlocked[roomX, roomY] = true;
            DrawRoom(roomX, roomY, false);
        }
    }

    private void DrawRoom(int roomX, int roomY, bool isFogged)
    {
        Vector3Int roomOrigin = new Vector3Int(
            roomX * roomSize.x, roomY * roomSize.y, 0);

        int mapCenterX = _totalMapWidth / 2;
        int mapCenterY = _totalMapHeight / 2;

        for (int i = 0; i < roomSize.x; i++) {
            for (int j = 0; j < roomSize.y; j++) {
                TileBase tileToUse = GetTileForPosition(i, j);

                if (isFogged) {
                    tileToUse = fogTile;
                }

                tilemap.SetTile(roomOrigin + new Vector3Int(
                    i - mapCenterX, j - mapCenterY, 0), tileToUse);
            }
        }
    }

    private TileBase GetTileForPosition(int x, int y)
    {
        if (x == 0 || x == roomSize.x - 1 || y == 0 || y == roomSize.y - 1) {
            return wallTile;
        }
        return groundTile;
    }
}
