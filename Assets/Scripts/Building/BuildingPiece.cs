using UnityEngine;

public class BuildingPiece : MonoBehaviour
{
    [HideInInspector] public GadgetType gadgetType;
    [HideInInspector] public Vector3Int cellPosition;
    
    private void OnDestroy()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.ClearBuildingDataAt(cellPosition);
        }
    }
}