using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

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
    public UIManager uiManager;
    public CardDragger cardDragger;
    
    private int _currentQuotaIndex;
    private Coroutine _quotaCoroutine;
    private CardDragger _activeCardDragger;
    private DisplayableData _activeCardData;

    [HideInInspector] public UnityEvent<DisplayableData> onStartDrag;
    [HideInInspector] public UnityEvent onEndDrag;
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
            InitializeAllManagers();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        SetTimeScale();
        ToggleExpansionPanel();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void ToggleExpansionPanel()
    {
        if (Input.GetKeyDown(KeyCode.M) && expansionPanel != null)
        {
            expansionPanel.TogglePanelVisibility();
        }
    }
    
    public void StartDrag(DisplayableData data)
    {
        if (cardDragger == null) 
        {
            return;
        }

        _activeCardData = data;
        cardDragger.StartDrag(_activeCardData);
        onStartDrag?.Invoke(_activeCardData);
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
    
    public void EndDrag()
    {
        if (cardDragger != null)
        {
            cardDragger.EndDrag();
        }
        _activeCardData = null;
        onEndDrag?.Invoke();
        uiManager.UnpinAndHideCardPanel();
    }
    
    public bool IsDragging()
    {
        return _activeCardDragger  != null && _activeCardDragger .IsDragging;
    }
    
    public DisplayableData GetActiveData()
    {
        return _activeCardData;
    }

    private void InitializeAllManagers()
    {
        slider = FindFirstObjectByType<Slider>();
        mapGenerator = FindFirstObjectByType<MapGenerator>();
        expansionPanel = FindFirstObjectByType<ExpansionPanel>();
        uiManager = FindFirstObjectByType<UIManager>();
        cardDragger = FindFirstObjectByType<CardDragger>();

        mapGenerator?.GenerateMap();
        expansionPanel?.InitiateExpansionPanel();

        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        yield return null;

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
            spawner.BuildingTilemap?.gameObject.SetActive(false);
        }

        ResourceSpawner[] resourceSpawners = FindObjectsByType<ResourceSpawner>(FindObjectsSortMode.None);
        foreach (ResourceSpawner spawner in resourceSpawners)
        {
            spawner.SpawnResources();
            spawner.ResourceTilemap?.gameObject.SetActive(false);
        }

        Unit_Lifter[] allUnits = FindObjectsByType<Unit_Lifter>(FindObjectsSortMode.None);
        foreach (Unit_Lifter unit in allUnits)
        {
            unit.TryStartActions();
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

            int currentRequiredAmount = requiredResourceAmounts[_currentQuotaIndex];
            ResourceManager rm = ResourceManager.Instance;

            if (rm != null && rm.GetResource(requiredResourceType) >= currentRequiredAmount)
            {
                rm.SpendResources(requiredResourceType, currentRequiredAmount);
                _currentQuotaIndex++;
                ResourceManager.Instance?.UpdateAllResourceUI();
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
        _currentQuotaIndex = 0;
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