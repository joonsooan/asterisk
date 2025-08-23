using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap resourceTilemap;
    [SerializeField] private TileBase[] resourceTiles;
    [SerializeField] private GameObject[] resourcePrefabs;
    [SerializeField] private Grid grid;
    [SerializeField] private Transform parentTransform;

    private readonly Dictionary<Vector2Int, List<GameObject>> _roomResources = new Dictionary<Vector2Int, List<GameObject>>();

    public Tilemap ResourceTilemap {
        get {
            return resourceTilemap;
        }
    }

    public void SpawnResources()
    {
        if (parentTransform == null) {
            parentTransform = transform;
        }

        _roomResources.Clear();

        foreach (Vector3Int pos in resourceTilemap.cellBounds.allPositionsWithin) {
            TileBase currentTile = resourceTilemap.GetTile(pos);

            if (currentTile == null) continue;

            Vector2Int roomCoords = GetRoomCoordinates(pos);

            if (!_roomResources.ContainsKey(roomCoords)) {
                _roomResources.Add(roomCoords, new List<GameObject>());
            }

            for (int i = 0; i < resourceTiles.Length; i++) {
                TileBase t = resourceTiles[i];
                if (currentTile != t) continue;

                Vector3 worldPos = grid.GetCellCenterWorld(pos);
                GameObject resourceNodeObj = Instantiate(resourcePrefabs[i], worldPos, Quaternion.identity, parentTransform);
                ResourceNode nodeComponent = resourceNodeObj.GetComponent<ResourceNode>();

                if (nodeComponent != null) {
                    nodeComponent.cellPosition = pos;
                }

                _roomResources[roomCoords].Add(resourceNodeObj);

                if (!GameManager.Instance.mapGenerator.IsRoomUnlocked(roomCoords.x, roomCoords.y)) {
                    resourceNodeObj.SetActive(false);
                }

                break;
            }
        }
    }

    public void ActivateResourcesInRoom(Vector2Int roomCoords)
    {
        if (_roomResources.TryGetValue(roomCoords, out List<GameObject> roomResource)) {
            roomResource.RemoveAll(go => go == null);
            foreach (GameObject resource in roomResource) {
                if (resource != null) {
                    resource.SetActive(true);
                }
            }
        }
    }

    public void NotifyResourceDestroyed(ResourceNode node)
    {
        if (node == null) return;
        GameObject target = node.gameObject;
        foreach (KeyValuePair<Vector2Int, List<GameObject>> kv in _roomResources) {
            if (kv.Value != null) {
                kv.Value.RemoveAll(go => go == null || go == target);
            }
        }
    }

    private Vector2Int GetRoomCoordinates(Vector3Int pos)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(pos);
        return GameManager.Instance.mapGenerator.GetRoomCoordinates(worldPos);
    }
}
