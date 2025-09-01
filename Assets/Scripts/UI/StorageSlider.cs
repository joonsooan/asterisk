using UnityEngine;
using UnityEngine.UI;

public class StorageSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullColor = Color.red;

    private IStorage _targetStorage;
    private Color _defaultFillColor;

    private void Awake()
    {
        if (fillImage != null)
        {
            _defaultFillColor = fillImage.color;
        }
    }

    public void Initialize(IStorage storage, Vector3 uiOffset)
    {
        _targetStorage = storage;
        _targetStorage.OnStorageChanged += UpdateUI;

        Transform targetTransform = (storage as Component).transform;

        if (targetTransform != null)
        {
            transform.position = targetTransform.position + uiOffset;
        }
    }

    private void OnDestroy()
    {
        if (_targetStorage != null)
        {
            _targetStorage.OnStorageChanged -= UpdateUI;
        }
    }

    private void UpdateUI(int currentAmount, int maxAmount)
    {
        if (maxAmount == 0) return;
        
        slider.value = (float)currentAmount / maxAmount;
        fillImage.color = (currentAmount >= maxAmount) ? fullColor : _defaultFillColor;
    }
}