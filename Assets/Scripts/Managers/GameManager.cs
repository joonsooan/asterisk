using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel;

    private int _currentQuotaIndex;
    private Coroutine _quotaCoroutine;
    private DisplayableData _activeCardData;
    private bool _isPaused;

    [HideInInspector] public UnityEvent<DisplayableData> onStartDrag;
    [HideInInspector] public UnityEvent onEndDrag;
    
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        HandleGameInput();
    }
    
    private void HandleGameInput()
    {
        if (SceneManager.GetActiveScene().name != "GameScene") return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }

        if (_isPaused) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) Time.timeScale = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) Time.timeScale = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) Time.timeScale = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) Time.timeScale = 4;
        
        if (Input.GetKeyDown(KeyCode.M) && expansionPanel != null)
        {
            expansionPanel.TogglePanelVisibility();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(_isPaused);
        }
    }
    
    public int GetRequiredAmountForCurrentQuota()
    {
        return (_currentQuotaIndex >= 0 && _currentQuotaIndex < requiredResourceAmounts.Count)
            ? requiredResourceAmounts[_currentQuotaIndex]
            : -1;
    }

    public void GameOver()
    {
        _currentQuotaIndex = 0;
        Time.timeScale = 1;
        SceneManager.LoadScene("TitleScene");
    }
    
    public void StartDrag(DisplayableData data)
    {
        if (cardDragger == null) return;

        _activeCardData = data;
        cardDragger.StartDrag(_activeCardData);
        onStartDrag?.Invoke(_activeCardData);
    }

    public void EndDrag()
    {
        if (cardDragger != null)
        {
            cardDragger.EndDrag();
        }
        _activeCardData = null;
        onEndDrag?.Invoke();
        uiManager?.UnpinAndHideCardPanel();
    }

    public bool IsDragging() => cardDragger != null && cardDragger.IsDragging;
    public DisplayableData GetActiveData() => _activeCardData;
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            _isPaused = false;
            Time.timeScale = 1f;
            InitializeGameScene();
        }
    }

    private void InitializeGameScene()
    {
        slider = FindFirstObjectByType<Slider>();
        mapGenerator = FindFirstObjectByType<MapGenerator>();
        expansionPanel = FindFirstObjectByType<ExpansionPanel>();
        uiManager = FindFirstObjectByType<UIManager>();
        cardDragger = FindFirstObjectByType<CardDragger>();
        
        GameObject pausePanelObject = GameObject.Find("PausePanel");
        if (pausePanelObject != null)
        {
            pausePanel = pausePanelObject;
            pausePanel.SetActive(false);
        }

        mapGenerator?.GenerateMap();
        expansionPanel?.InitiateExpansionPanel();
        
        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        yield return null;

        InitializeSpawnersAndUnits();

        if (_quotaCoroutine != null) StopCoroutine(_quotaCoroutine);
        _quotaCoroutine = StartCoroutine(CheckResourceQuota());
    }

    private void InitializeSpawnersAndUnits()
    {
        foreach (var spawner in FindObjectsByType<BuildingSpawner>(FindObjectsSortMode.None))
        {
            spawner.SpawnBuildings();
            if(spawner.BuildingTilemap != null) spawner.BuildingTilemap.gameObject.SetActive(false);
        }

        foreach (var spawner in FindObjectsByType<ResourceSpawner>(FindObjectsSortMode.None))
        {
            spawner.SpawnResources();
            if(spawner.ResourceTilemap != null) spawner.ResourceTilemap.gameObject.SetActive(false);
        }

        foreach (var unit in FindObjectsByType<Unit_Lifter>(FindObjectsSortMode.None))
        {
            unit.TryStartActions();
        }
    }

    private IEnumerator CheckResourceQuota()
    {
        if (slider != null)
        {
            slider.maxValue = resourceCheckInterval;
        }

        while (_currentQuotaIndex < requiredResourceAmounts.Count)
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
                rm.UpdateAllResourceUI();
            }
            else
            {
                GameOver();
                yield break;
            }
        }
        
        Debug.Log("All quotas met! Game Win!");
    }
}