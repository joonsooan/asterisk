using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject cameraActiveObject;

    private Image _cameraActiveImg;
    private TMP_Text _cameraActiveText;

    private readonly Color _cameraActiveColor = Color.green;
    private readonly Color _cameraInactiveColor = Color.red;

    public static CardManager Instance { get; private set; }

    [HideInInspector] public bool isCameraActive;

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
        if (cameraActiveObject != null)
        {
            _cameraActiveImg = cameraActiveObject.GetComponent<Image>();
            _cameraActiveText = cameraActiveObject.GetComponentInChildren<TMP_Text>();
        }
    }

    // public void ToggleCameraModeUI(bool isActive)
    // {
    //     isCameraActive = isActive;
    //
    //     if (_cameraActiveImg != null && _cameraActiveText != null)
    //     {
    //         _cameraActiveImg.color = isActive ? _cameraActiveColor : _cameraInactiveColor;
    //         _cameraActiveText.text = isActive ? "Camera" : "Build";
    //     }
    // }
}