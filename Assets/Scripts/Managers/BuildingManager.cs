using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
    [Header("References")]
    public Grid grid;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap resourceTilemap;
    [SerializeField] private Tilemap buildingTilemap;
    [SerializeField] private TileBase mainStructureTile;
    [SerializeField] private Transform parentTransform;
    
    [Header("Combo Buildings")]
    private List<ComboCardData> _comboCardDataList;
    private readonly Dictionary<GadgetType, TileBase> _gadgetTypeToTileCache = new Dictionary<GadgetType, TileBase>();
    private readonly Dictionary<Vector3Int, BuildingPiece> _placedPieces = new Dictionary<Vector3Int, BuildingPiece>();
    
    public static event Action<Vector3Int> OnTilemapChanged;
    
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
        _comboCardDataList = new List<ComboCardData>(
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
    
    public bool IsResourceTile(Vector3Int cellPosition)
    {
        if (resourceTilemap == null) return false;
        return resourceTilemap.HasTile(cellPosition);
    }
    
    public bool IsBuildingTile(Vector3Int cellPosition)
    {
        if (buildingTilemap == null) return false;
        return buildingTilemap.HasTile(cellPosition);
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
            parentTransform = transform;
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
                OnTilemapChanged?.Invoke(cellPosition);
            }

            CheckForComboBuildings(cellPosition);
        }
    }
    
    private void CheckForComboBuildings(Vector3Int placedPos)
    {
        if (_comboCardDataList == null || _comboCardDataList.Count == 0) return;

        foreach (var comboData in _comboCardDataList) {
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
        List<Vector3Int> recipePositions = new List<Vector3Int>();
        foreach (var piece in comboData.recipe)
        {
            recipePositions.Add(originPos + piece.relativePosition);
        }

        foreach (var targetPos in recipePositions)
        {
            RemoveBuildingPieceAtPosition(targetPos);
        }
        
        foreach (var targetPos in recipePositions)
        {
            Vector3 worldPos = grid.GetCellCenterWorld(targetPos);

            GameObject newComboPieceObject = Instantiate(comboData.comboPrefab, worldPos, Quaternion.identity, parentTransform);
            
            BuildingPiece markerPiece = newComboPieceObject.GetComponent<BuildingPiece>();
            if (markerPiece == null)
            {
                markerPiece = newComboPieceObject.AddComponent<BuildingPiece>();
            }

            markerPiece.cellPosition = targetPos;
            _placedPieces[targetPos] = markerPiece;
            
            if (comboData.comboTile != null)
            {
                buildingTilemap.SetTile(targetPos, comboData.comboTile);
            }

            HandleComboBuildingLogic(newComboPieceObject, comboData);
        }

        foreach (var targetPos in recipePositions)
        {
            OnTilemapChanged?.Invoke(targetPos);
        }

        Debug.Log($"Combo Building '{comboData.displayName}' Created");
    }

    private void HandleComboBuildingLogic(GameObject comboObject, ComboCardData comboData)
    {
        if (comboObject == null || ResourceManager.Instance == null)
        {
            return;
        }

        switch (comboData.comboType)
        {
            case ComboType.Storage:
                IStorage storage = comboObject.GetComponent<IStorage>();
                if (storage != null)
                {
                    ResourceManager.Instance.AddStorage(storage);
                }
                break;
        }
    }
    
    public void ClearBuildingDataAt(Vector3Int cellPosition)
    {
        if (buildingTilemap == null) return;
        
        if (buildingTilemap.HasTile(cellPosition))
        {
            buildingTilemap.SetTile(cellPosition, null);
            OnTilemapChanged?.Invoke(cellPosition);
        }
        _placedPieces.Remove(cellPosition);
    }

    private void RemoveBuildingPieceAtPosition(Vector3Int cellPos)
    {
        if (_placedPieces.Remove(cellPos, out BuildingPiece piece))
        {
            if (piece != null)
            {
                Destroy(piece.gameObject);
            }
        }
        buildingTilemap.SetTile(cellPos, null);
    }
    
    public void RemoveResourceTile(Vector3Int cellPosition)
    {
        if (resourceTilemap != null)
        {
            resourceTilemap.SetTile(cellPosition, null);
        }
    }
}
