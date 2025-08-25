using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    [Header("Controls")]
    public float smoothing = 5f;
    public float panSpeed = 10f;

    private Camera _cam;
    private bool _isManualMode;
    private Vector3 _manualDirection;
    private Bounds? _worldBounds;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) {
            _cam = Camera.main;
        }
    }

    private void Update()
    {
        CameraMove();
    }

    private void CameraMove()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        _manualDirection = new Vector3(moveX, moveY, 0).normalized;

        if (_manualDirection != Vector3.zero) {
            _isManualMode = true;
        }
    }

    private void LateUpdate()
    {
        if (!GameManager.Instance.isCameraActive) return;
        
        if (_isManualMode)
        {
            transform.Translate(_manualDirection * panSpeed * Time.unscaledDeltaTime,
                Space.World);
        }
        else if (target != null)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y,
                transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition,
                smoothing * Time.unscaledDeltaTime);
        }

        ClampToBounds();
    }

    public void SetBounds(Bounds bounds)
    {
        _worldBounds = bounds;
        ClampToBounds();
    }

    private void ClampToBounds()
    {
        if (_cam == null || !_cam.orthographic || _worldBounds == null) {
            return;
        }

        Bounds bounds = _worldBounds.Value;

        float halfHeight = _cam.orthographicSize;
        float halfWidth = halfHeight * _cam.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;
        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }
}
