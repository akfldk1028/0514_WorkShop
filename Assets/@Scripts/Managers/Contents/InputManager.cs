using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public bool IsMouseOverUI => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    
    public Action<Vector3> OnMouseClickWorld;
    public Action<Vector3Int> OnMouseClickCell;
    public Action<Vector3Int> OnMouseDragCell;
    public Action<Vector3Int> OnMouseDragEndCell;
    
    private bool _isDragging = false;
    private Vector3Int _lastDragCell = Vector3Int.zero;
    private IDisposable _updateSubscription;
    private bool _initialized = false;

    
    public InputManager()
    {
        Debug.Log("<color=cyan>[InputManager]</color> 생성됨");
    }
    
    public void Init()
    {
        if (_initialized) return;
        _initialized = true;
        
        if (Managers.ActionMessage != null)
        {
            // Managers.UpdateHandler += OnUpdate;  // Instance를 통해 접근 /
            _updateSubscription = Managers.Subscribe(ActionType.Managers_Update, OnUpdate);
            Debug.Log("<color=cyan>[InputManager]</color> Update 구독 성공");
        }
 
    }
    
    ~InputManager()
    {
        _updateSubscription?.Dispose();
    }
    



    private void OnUpdate()
    {
        Debug.Log("<color=cyan>[InputManager]</color> OnUpdate() 호출");
        // UI 위에 마우스가 있는 경우 처리하지 않음
        if (IsMouseOverUI)
        {
            _isDragging = false;
            return;
        }
        
        // 마우스 클릭 감지 (왼쪽 버튼)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                OnMouseClickWorld?.Invoke(hit.point);
                
                if (Managers.Map.CellGrid != null)
                {
                    Vector3Int cellPos = Managers.Map.World2Cell(hit.point);
                    OnMouseClickCell?.Invoke(cellPos);
                    
                    // 드래그 시작
                    _isDragging = true;
                    _lastDragCell = cellPos;
                }
            }
        }
        
        // 마우스 드래그 감지
        if (Input.GetMouseButton(0) && _isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                if (Managers.Map.CellGrid != null)
                {
                    Vector3Int cellPos = Managers.Map.World2Cell(hit.point);
                    
                    // 셀이 변경된 경우에만 이벤트 발생
                    if (cellPos != _lastDragCell)
                    {
                        OnMouseDragCell?.Invoke(cellPos);
                        _lastDragCell = cellPos;
                    }
                }
            }
        }
        
        // 마우스 드래그 종료 감지
        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                if (Managers.Map.CellGrid != null)
                {
                    Vector3Int cellPos = Managers.Map.World2Cell(hit.point);
                    OnMouseDragEndCell?.Invoke(cellPos);
                }
            }
            
            _isDragging = false;
        }
    }
}