using System.Collections.Generic;
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
    [SerializeField] private Transform parentTransform;
    
    [Header("Combo Buildings")]
    private List<ComboCardData> comboCardDataList;
    private readonly Dictionary<GadgetType, TileBase> _gadgetTypeToTileCache = new Dictionary<GadgetType, TileBase>();
    private readonly Dictionary<Vector3Int, BuildingPiece> _placedPieces = new Dictionary<Vector3Int, BuildingPiece>();
    
    public static BuildingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }

        LoadAllComboCards();
        CacheAllGadgetTiles();
    }
    
    private void LoadAllComboCards()
    {
        comboCardDataList = new List<ComboCardData>(
            Resources.LoadAll<ComboCardData>("Combo Cards"));
    }
    
    private void CacheAllGadgetTiles()
    {
        CardData[] allCards = Resources.LoadAll<CardData>("Cards"); 
        
        foreach (var card in allCards)
        {
            if (card.gadgetType != GadgetType.None && card.gadgetTile != null)
            {
                if (!_gadgetTypeToTileCache.ContainsKey(card.gadgetType))
                {
                    _gadgetTypeToTileCache[card.gadgetType] = card.gadgetTile;
                }
            }
        }
    }

    public bool CanPlaceBuilding(Vector3Int cellPosition)
    {
        if (grid == null || groundTilemap == null || 
            resourceTilemap == null || buildingTilemap == null) {
            Debug.LogError("BuildingManager: Missing references.");
            return false;
        }

        if (!groundTilemap.HasTile(cellPosition)) return false;
        if (resourceTilemap.HasTile(cellPosition)) return false;
        if (buildingTilemap.HasTile(cellPosition)) return false;

        return true;
    }

    public void PlaceBuilding(CardData cardData, Vector3Int cellPosition)
    {
        if (parentTransform == null) {
            parentTransform = this.transform;
        }
        
        if (CanPlaceBuilding(cellPosition)) {
            Vector3 worldPosition = grid.GetCellCenterWorld(cellPosition);
            GameObject newPieceObject = 
                Instantiate(cardData.buildingPrefab, worldPosition, Quaternion.identity, parentTransform);

            BuildingPiece pieceComponent = newPieceObject.GetComponent<BuildingPiece>();
            if (pieceComponent != null)
            {
                pieceComponent.gadgetType = cardData.gadgetType;
                pieceComponent.cellPosition = cellPosition;
                _placedPieces[cellPosition] = pieceComponent;
            }

            if (cardData.gadgetTile != null) {
                buildingTilemap.SetTile(cellPosition, cardData.gadgetTile);
                Debug.Log($"Placed building at {cellPosition}.");
            }

            CheckForComboBuildings(cellPosition);
        } else {
            Debug.Log($"Cannot place building at {cellPosition}.");
        }
    }
    
    private void CheckForComboBuildings(Vector3Int placedPos)
    {
        if (comboCardDataList == null || comboCardDataList.Count == 0) return;

        foreach (var comboData in comboCardDataList) {
            if (CheckPatternAround(placedPos, comboData)) {
                return;
            }
        }
    }

    private bool CheckPatternAround(Vector3Int placedPos, ComboCardData comboData)
    {
        foreach (var piece in comboData.recipe) {
            Vector3Int originPosCandidate = placedPos - piece.relativePosition;

            if (CheckPattern(originPosCandidate, comboData)) {
                CreateComboBuilding(originPosCandidate, comboData);
                return true;
            }
        }
        return false;
    }
    
    private bool CheckPattern(Vector3Int originPos, ComboCardData comboData)
    {
        foreach (var piece in comboData.recipe) {
            Vector3Int targetPos = originPos + piece.relativePosition;
            TileBase targetTile = buildingTilemap.GetTile(targetPos);
            
            if (!_gadgetTypeToTileCache.TryGetValue(piece.gadgetType, out TileBase requiredTile) || 
                targetTile != requiredTile) {
                return false;
            }
        }
        return true;
    }
    
    private void CreateComboBuilding(Vector3Int originPos, ComboCardData comboData)
    {
        foreach (var piece in comboData.recipe) {
            Vector3Int targetPos = originPos + piece.relativePosition;
            RemoveBuildingPieceAtPosition(targetPos);
            buildingTilemap.SetTile(targetPos, null);
        }
        
        Vector3 worldPos = grid.GetCellCenterWorld(originPos);
        foreach (var piece in comboData.recipe) {
            Vector3 targetPos = worldPos + piece.relativePosition;
            Instantiate(comboData.comboPrefab, targetPos, Quaternion.identity, parentTransform);
        }

        if (comboData.comboTile != null) {
            foreach (var piece in comboData.recipe) {
                Vector3Int targetPos = originPos + piece.relativePosition;
                buildingTilemap.SetTile(targetPos, comboData.comboTile);
            }
        }

        Debug.Log($"Combo Building '{comboData.comboName}' Created");
    }
    
    private void RemoveBuildingPieceAtPosition(Vector3Int cellPos)
    {
        if (_placedPieces.Remove(cellPos, out BuildingPiece piece))
        {
            if (piece != null)
            {
                Destroy(piece.gameObject);
                Debug.Log($"Removed BuildingPiece at {cellPos}.");
            }
        }
    }
    
    public void RemoveResourceTile(Vector3Int cellPosition)
    {
        if (resourceTilemap != null)
        {
            resourceTilemap.SetTile(cellPosition, null);
        }
    }
}
