using UnityEngine;

public class Combo_0 : Damageable, IStorage
{
    public event System.Action<int, int> OnStorageChanged;
    
    [Header("UI Settings")]
    [SerializeField] private GameObject storageSliderPrefab;
    [SerializeField] private string canvasName = "ObjectUI_Canvas";
    [SerializeField] private Vector3 sliderOffset = new Vector3(0, 1.5f, 0);
    private GameObject _sliderInstance;
    
    [Header("Values")]
    [SerializeField] private int maxStorageAmount;
    
    private int _currentStorageAmount;

    protected override void OnEnable()
    {
        base.OnEnable();
        _currentStorageAmount = 0;
        
        if (storageSliderPrefab != null)
        {
            Canvas canvas = GameObject.Find(canvasName)?.GetComponent<Canvas>();
            if (canvas != null)
            {
                _sliderInstance = Instantiate(storageSliderPrefab, canvas.transform);
                var controller = _sliderInstance.GetComponent<StorageSlider>();
                controller?.Initialize(this, sliderOffset);
            }
        }
        
        OnStorageChanged?.Invoke(_currentStorageAmount, maxStorageAmount);
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.RemoveStorage(this);
        }
        
        if (_sliderInstance != null)
        {
            Destroy(_sliderInstance);
        }
    }

    public bool StorageIsFull()
    {
        return  _currentStorageAmount >= maxStorageAmount;
    }
    
    public void AddResource(ResourceType type, int amount)
    {
        _currentStorageAmount += amount;
        if (_currentStorageAmount > maxStorageAmount)
        {
            _currentStorageAmount = maxStorageAmount;
        }
        
        OnStorageChanged?.Invoke(_currentStorageAmount, maxStorageAmount);
        ResourceManager.Instance.AddResource(type, amount);
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}