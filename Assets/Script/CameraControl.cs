using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform target; // 플레이어
    public Vector3 offset = new Vector3(0f, 4f, -1.5f); // 오른쪽 어깨 뒤쪽 위
    public float positionSmoothSpeed = 0.05f;
    public float rotationSmoothSpeed = 7f;

    void LateUpdate()
    {
        // 플레이어 기준 방향으로 회전한 offset 적용
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // 부드럽게 위치 이동
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed);

        // 플레이어 방향을 따라 카메라 회전
        Quaternion desiredRotation = Quaternion.LookRotation(target.position + target.forward * 10f - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
