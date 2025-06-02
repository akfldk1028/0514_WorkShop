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
    public Action OnBackViewKey;
    public Action OnTopViewKey;
    public Action OnPlayerViewKey;
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
        // 1) 키보드 입력은 항상 체크
        HandleKeyboardInput();

        // 2) UI 위에 마우스가 있으면 마우스 로직을 건너뛰되, 드래그 상태는 초기화
        if (IsMouseOverUI)
        {
            _isDragging = false;
            return; // 키보드는 이미 처리했으므로, 여기서 마우스 관련은 종료
        }

        // 3) 마우스 클릭/드래그 처리
        HandleMouseInput();
    }

    private void HandleKeyboardInput()
    {
        Vector2 moveDir = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) moveDir.y += 1;
        if (Input.GetKey(KeyCode.S)) moveDir.y -= 1;
        if (Input.GetKey(KeyCode.A)) moveDir.x -= 1;
        if (Input.GetKey(KeyCode.D)) moveDir.x += 1;
        if (Input.GetKeyDown(KeyCode.P))
        {
            Managers.Game.CustomerCreator._tableManager.DebugPrintAllTableOrders();
        }
        if (Managers.Game != null)
        {
            moveDir = moveDir.normalized;
            if (moveDir != Managers.Game.MoveDir)
            {
                Managers.Game.MoveDir = moveDir;
            }
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            OnBackViewKey?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            OnTopViewKey?.Invoke();
        }
    }

    private void HandleMouseInput()
    {
        // -- 1) 마우스 클릭(왼쪽 버튼 Down)
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

                    _isDragging = true;
                    _lastDragCell = cellPos;
                }
            }
        }

        // -- 2) 마우스 드래그 중
        if (Input.GetMouseButton(0) && _isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                if (Managers.Map.CellGrid != null)
                {
                    Vector3Int cellPos = Managers.Map.World2Cell(hit.point);
                    if (cellPos != _lastDragCell)
                    {
                        OnMouseDragCell?.Invoke(cellPos);
                        _lastDragCell = cellPos;
                    }
                }
            }
        }

        // -- 3) 마우스 드래그 종료(왼쪽 버튼 Up)
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
