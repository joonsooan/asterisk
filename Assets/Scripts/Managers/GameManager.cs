using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Mission Quota")]
    [SerializeField] private float resourceCheckInterval = 120f;
    [SerializeField] private List<int> requiredResourceAmounts;
    [SerializeField] private ResourceType requiredResourceType;

    [Header("References")]
    public Slider slider;
    public ExpansionPanel expansionPanel;
    public MapGenerator mapGenerator;
    public GameObject cameraActiveObject;
    public CardInfoManager cardInfoManager;

    private int _currentQuotaIndex;
    private CardDragger _activeDragger;
    private Coroutine _quotaCoroutine;
    private Image _cameraActiveImg;
    private TMP_Text _cameraActiveText;
    
    private readonly Color _cameraActiveColor = Color.green;
    private readonly Color _cameraInactiveColor = Color.red;

    [HideInInspector] public bool isCameraActive;
    
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeGameScene();
    }

    private void InitializeGameScene()
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            Initiate();
            isCameraActive = true;
            _cameraActiveImg.color = _cameraActiveColor;
            // ToggleCamera(isCameraActive);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        SetTimeScale();
        ToggleExpansionPanel(); // For Debug
        // ToggleShortcut();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // private void ToggleShortcut()
    // {
    //     if (Input.GetKeyDown(KeyCode.LeftShift))
    //     {
    //         isCameraActive = !isCameraActive;
    //         ToggleCamera(isCameraActive);
    //         cardInfoManager.ToggleCardInfoPanel(!isCameraActive);
    //     }
    // }
    //
    // private void ToggleCamera(bool isActive)
    // {
    //     if (_activeDragger != null)
    //     {
    //         _activeDragger.EndDrag();
    //     }
    //     _cameraActiveImg.color = isActive ? _cameraActiveColor : _cameraInactiveColor;
    //     _cameraActiveText.text = isActive ? "Camera" : "Build";
    // }
    
    private void ToggleExpansionPanel()
    {
        if (Input.GetKeyDown(KeyCode.M) && expansionPanel != null)
        {
            expansionPanel.TogglePanelVisibility();
        }
    }

    public void SetActiveDragger(CardDragger dragger)
    {
        _activeDragger = dragger;
    }

    public CardDragger GetActiveDragger()
    {
        return _activeDragger;
    }

    public int GetRequiredAmountForCurrentQuota()
    {
        if (_currentQuotaIndex >= 0 && _currentQuotaIndex < requiredResourceAmounts.Count)
        {
            return requiredResourceAmounts[_currentQuotaIndex];
        }
        return -1;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeGameScene();
    }

    private void Initiate()
    {
        slider = FindFirstObjectByType<Slider>();
        mapGenerator = FindFirstObjectByType<MapGenerator>();
        expansionPanel = FindFirstObjectByType<ExpansionPanel>();
        cardInfoManager =  FindFirstObjectByType<CardInfoManager>();
        
        cameraActiveObject = GameObject.Find("Camera Active Object");
        _cameraActiveImg = cameraActiveObject.GetComponent<Image>();
        _cameraActiveText = cameraActiveObject.GetComponentInChildren<TMP_Text>();
        
        if (mapGenerator != null)
        {
            mapGenerator.GenerateMap();
        }
        if (expansionPanel != null)
        {
            expansionPanel.InitiateExpansionPanel();
        }

        InitializeSpawnersAndUnits();

        if (_quotaCoroutine != null)
        {
            StopCoroutine(_quotaCoroutine);
        }
        _quotaCoroutine = StartCoroutine(CheckResourceQuota());
    }

    private void InitializeSpawnersAndUnits()
    {
        BuildingSpawner[] buildingSpawners = FindObjectsByType<BuildingSpawner>(FindObjectsSortMode.None);
        foreach (BuildingSpawner spawner in buildingSpawners)
        {
            spawner.SpawnBuildings();
            if (spawner.BuildingTilemap != null)
            {
                spawner.BuildingTilemap.gameObject.SetActive(false);
            }
        }

        ResourceSpawner[] resourceSpawners = FindObjectsByType<ResourceSpawner>(FindObjectsSortMode.None);
        foreach (ResourceSpawner spawner in resourceSpawners)
        {
            spawner.SpawnResources();
            if (spawner.ResourceTilemap != null)
            {
                spawner.ResourceTilemap.gameObject.SetActive(false);
            }
        }

        Unit_Lifter[] allUnits = FindObjectsByType<Unit_Lifter>(FindObjectsSortMode.None);
        foreach (Unit_Lifter unit in allUnits)
        {
            unit.StartUnitActions();
        }
    }

    private IEnumerator CheckResourceQuota()
    {
        if (slider != null)
        {
            slider.maxValue = resourceCheckInterval;
            slider.value = 0f;
        }

        while (true)
        {
            float elapsedTime = 0f;
            while (elapsedTime < resourceCheckInterval)
            {
                elapsedTime += Time.deltaTime;
                if (slider != null)
                {
                    slider.value = elapsedTime;
                }
                yield return null;
            }

            if (slider != null)
            {
                slider.value = resourceCheckInterval;
            }

            if (_currentQuotaIndex >= requiredResourceAmounts.Count)
            {
                _quotaCoroutine = null;
                yield break;
            }

            int currentRequiredAmount = requiredResourceAmounts[_currentQuotaIndex];
            ResourceManager rm = ResourceManager.Instance;

            if (rm != null && rm.GetResource(requiredResourceType) >= currentRequiredAmount)
            {
                rm.SpendResources(requiredResourceType, currentRequiredAmount);
                _currentQuotaIndex++;
                
                if (ResourceManager.Instance != null) {
                    ResourceManager.Instance.UpdateAllResourceUI();
                }
            }
            else
            {
                GameOver();
                yield break;
            }

            if (slider != null)
            {
                slider.value = 0f;
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
        if (SceneManager.GetActiveScene().name != "GameScene") return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) Time.timeScale = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) Time.timeScale = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) Time.timeScale = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) Time.timeScale = 4;
    }
}