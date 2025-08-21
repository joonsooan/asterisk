using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    
    [Header("Controls")]
    public float smoothing = 5f;
    public float panSpeed = 10f;
    
    private bool isManualMode = false;
    private Vector3 manualDirection;

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        manualDirection = new Vector3(moveX, moveY, 0).normalized;
        
        if (manualDirection != Vector3.zero) {
            isManualMode = true;
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            isManualMode = false;
        }
    }

    private void LateUpdate()
    {
        if (isManualMode) {
            transform.Translate(manualDirection * panSpeed * Time.deltaTime, Space.World);
        } else if (target != null) {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }
    }
}