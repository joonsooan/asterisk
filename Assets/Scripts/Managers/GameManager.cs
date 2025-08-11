using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        BuildingSpawner[] buildingSpawners = FindObjectsByType<BuildingSpawner>(FindObjectsSortMode.None);
        foreach (BuildingSpawner spawner in buildingSpawners) {
            spawner.SpawnBuildings();
            if (spawner.BuildingTilemap != null) {
                spawner.BuildingTilemap.gameObject.SetActive(false);
            }
        }

        ResourceSpawner[] resourceSpawners = FindObjectsByType<ResourceSpawner>(FindObjectsSortMode.None);
        foreach (ResourceSpawner spawner in resourceSpawners) {
            spawner.SpawnResources();
            if (spawner.ResourceTilemap != null) {
                spawner.ResourceTilemap.gameObject.SetActive(false);
            }
        }

        Unit_Lifter[] allUnits = FindObjectsByType<Unit_Lifter>(FindObjectsSortMode.None);
        foreach (Unit_Lifter unit in allUnits) {
            unit.StartUnitActions();
        }
    }

    private void Update()
    {
        SetTimeScale();
    }

    private void SetTimeScale()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Time.timeScale = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Time.timeScale = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            Time.timeScale = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            Time.timeScale = 4;
        }
    }
}
