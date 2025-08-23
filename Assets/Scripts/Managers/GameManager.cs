using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Mission Quota")]
    [SerializeField] private float resourceCheckInterval = 120f;
    [SerializeField] private List<int> requiredResourceAmounts;
    [SerializeField] private ResourceType requiredResourceType;

    private int _currentQuotaIndex;
    private ExpansionPanel _expansionPanel;
    private MapGenerator _mapGenerator;
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
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (SceneManager.GetActiveScene().name == "GameScene") {
            Initiate();
        }
    }

    private void Update()
    {
        SetTimeScale();
        ToggleExpansionPanel();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void ToggleExpansionPanel()
    {
        if (Input.GetKeyDown(KeyCode.M)) {
            if (_expansionPanel != null) {
                _expansionPanel.TogglePanelVisibility();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") {
            Initiate();
        }
    }

    private void Initiate()
    {
        _mapGenerator = FindFirstObjectByType<MapGenerator>();
        if (_mapGenerator != null) {
            _mapGenerator.GenerateMap();
        }

        _expansionPanel = FindFirstObjectByType<ExpansionPanel>();
        if (_expansionPanel != null) {
            _expansionPanel.InitiateExpansionPanel();
        }

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

        _currentQuotaIndex = 0;
        StartCoroutine(CheckResourceQuota());
    }

    private IEnumerator CheckResourceQuota()
    {
        while (true) {
            yield return new WaitForSeconds(resourceCheckInterval);

            int currentRequiredAmount = requiredResourceAmounts[_currentQuotaIndex];

            if (ResourceManager.Instance.GetResource(requiredResourceType) >= currentRequiredAmount) {
                ResourceManager.Instance.SpendResources(requiredResourceType, currentRequiredAmount);
                _currentQuotaIndex++;
            }
            else {
                GameOver();
                yield break;
            }
        }
    }

    private void GameOver()
    {
        SceneManager.LoadScene("TitleScene");
        Time.timeScale = 1;
    }

    private void SetTimeScale()
    {
        if (SceneManager.GetActiveScene().name != "GameScene") {
            return;
        }

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
