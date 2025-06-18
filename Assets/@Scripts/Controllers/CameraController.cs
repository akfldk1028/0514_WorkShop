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
	private Vector3 topViewPosition = new Vector3(91.6f, 120.5f, -57.9f); // 예시: 주점 위쪽/뒤쪽
	private Vector3 topViewRotation = new Vector3(52f, -48f, -4f);     // 예시: 약간 기울어진 각도


	public Vector3 fixedCameraPosition = new Vector3(33f, 3.65f, -15.9f);
	public Vector3 fixedCameraRotation = new Vector3(7.87f, -90f, 0f);


	private Vector3 currentOffset;
	private enum ViewMode { BackView, TopView, FixedView }
	private ViewMode currentViewMode = ViewMode.BackView;
	
	// 현재 뷰 모드를 외부에서 확인할 수 있는 프로퍼티
	public bool IsTopView => currentViewMode == ViewMode.TopView;
    public float positionSmoothSpeed = 0.125f;  // 0.05f에서 0.15f로 증가 (더 빠르게 따라가기)
    public float rotationSmoothSpeed = 5f;      // 7f에서 감소 (너무 빠른 회전 방지)

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

	public void SetFixedView(bool enable)
	{
		if (enable)
		{
			currentViewMode = ViewMode.FixedView;
		}
		else
		{
			currentViewMode = ViewMode.BackView; // 또는 이전 모드로 복원
		}
	}

	// 또는 더 유연하게
	public void SetFixedView(Vector3 position, Vector3 rotation)
	{
		fixedCameraPosition = position;
		fixedCameraRotation = rotation;
		currentViewMode = ViewMode.FixedView;
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

		else if (currentViewMode == ViewMode.FixedView)
		{
			transform.position = fixedCameraPosition;
			transform.rotation = Quaternion.Euler(fixedCameraRotation);
		}
	}
}
