using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : InitBase
{
	private BaseObject _target;
	public BaseObject Target
	{
		get { return _target; }
		set { _target = value; }
	}

	// 여러 뷰용 offset
	public Vector3 backViewOffset = new Vector3(0f, -7f, -40f); // Y값을 -10f에서 -7f로 변경하여 카메라를 더 높게

	public Vector3 backViewPosition = new Vector3(5f, -20f, 0f); // 더 뒤로 (Z값 증가)
	public Vector3 backViewRotation = new Vector3(0f, 0f, 0f);     // X값 5도로 올림
	// 투시적 탑뷰: 주점 전체가 잘 보이도록 실제 카메라 위치/회전값 적용
	public Vector3 topViewPosition = new Vector3(35.3f, 113.5f, 124.7f); // 예시: 주점 위쪽/뒤쪽
	public Vector3 topViewRotation = new Vector3(45f, -147.18f, 0f);     // 예시: 약간 기울어진 각도

	private Vector3 currentOffset;
	private enum ViewMode { BackView, TopView }
	private ViewMode currentViewMode = ViewMode.BackView;
    public float positionSmoothSpeed = 0.125f;  // 0.05f에서 증가 (더 부드럽게)
    public float rotationSmoothSpeed = 2f;      // 7f에서 감소 (너무 빠른 회전 방지)

	private bool justSwitchedToBackView = false;

	public CameraController()
	{
		Debug.Log("<color=magenta>[CameraController]</color> 생성됨");
	}

	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		// Camera.main.orthographicSize = 15.0f;
		currentOffset = backViewOffset;
		currentViewMode = ViewMode.BackView;

		// InputManager의 이벤트 구독
		Managers.Input.OnBackViewKey += () => {
			currentOffset = backViewOffset;
			currentViewMode = ViewMode.BackView;
			justSwitchedToBackView = true; // 전환 플래그
		};
		Managers.Input.OnTopViewKey += () => { currentViewMode = ViewMode.TopView; };

		return true;
	}

	
	void LateUpdate()
	{
		if (currentViewMode == ViewMode.BackView)
		{
			if (Target == null)
				return;

			if (justSwitchedToBackView)
			{
				// 1회만 즉시 이동
				transform.position = backViewPosition;
				transform.rotation = Quaternion.Euler(backViewRotation);
				justSwitchedToBackView = false;
				return;
			}

			// 이후에는 offset 기반 부드러운 따라가기
			Vector3 desiredPosition = Target.transform.position + Target.transform.rotation * currentOffset;
			transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed);

			Quaternion desiredRotation = Quaternion.LookRotation(Target.transform.position + Target.transform.forward * 25f - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
		}
		else if (currentViewMode == ViewMode.TopView)
		{
			// 주점 전체가 보이도록 고정 위치/회전
			transform.position = topViewPosition;
			transform.rotation = Quaternion.Euler(topViewRotation);
		}
	}
}
