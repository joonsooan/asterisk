using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform target;
    public float smoothing = 5f;

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = new(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }
    }
}