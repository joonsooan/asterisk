using UnityEngine;
using System.Collections;

public class Combo_1 : MonoBehaviour, ICombo
{
    [Header("Values")]
    [SerializeField] private float generationInterval = 5f;
    [SerializeField] private int resourceAmount = 1;
    [SerializeField] private ResourceType resourceType;
    
    [Header("VFX")]
    [SerializeField] private string canvasName = "FloatingText Canvas";
    [SerializeField] private GameObject floatingNumTextPrefab;
    
    private Canvas _canvas;
    private Coroutine _productionCoroutine;

    private void Awake()
    {
        GameObject canvasObject = GameObject.Find(canvasName);
        
        if (canvasObject != null)
        {
            _canvas = canvasObject.GetComponent<Canvas>();
        }
        
        ActivateComboCard();
    }

    public void ActivateComboCard()
    {
        if (_productionCoroutine != null)
        {
            StopCoroutine(_productionCoroutine);
        }
        _productionCoroutine = StartCoroutine(ProduceResource());
    }

    public void DeactivateComboCard()
    {
        if (_productionCoroutine != null)
        {
            StopCoroutine(_productionCoroutine);
        }
    }

    private IEnumerator ProduceResource()
    {
        while (true)
        {
            yield return new WaitForSeconds(generationInterval);
            
            GenerateResource();
        }
    }

    private void GenerateResource()
    {
        ResourceManager.Instance.AddResource(resourceType, resourceAmount);
        ShowFloatingText(resourceAmount);
    }
    
    private void ShowFloatingText(int amount)
    {
        if (floatingNumTextPrefab == null || _canvas == null) return;

        GameObject textInstance = Instantiate(floatingNumTextPrefab, transform.position, Quaternion.identity, _canvas.transform);

        FloatingNumText floatingText = textInstance.GetComponent<FloatingNumText>();
        if (floatingText != null) {
            floatingText.SetText($"+{amount}");
        }
    }
}