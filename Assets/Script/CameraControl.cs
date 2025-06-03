using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset = new Vector3(0f, 4f, -1.5f); 
    public float positionSmoothSpeed = 0.05f;
    public float rotationSmoothSpeed = 7f;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + target.rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed);

        Quaternion desiredRotation = Quaternion.LookRotation(target.position + target.forward * 10f - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
