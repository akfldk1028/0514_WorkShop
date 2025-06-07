using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System; // IDisposable 추가

public class TableManager
{
    public TableManager()
    {
        Debug.Log("<color=orange>[TableManager]</color> 생성됨");
    }
    private List<Table> _tables = new List<Table>();
    public IReadOnlyList<Table> Tables => _tables;

    [Header("Interaction")]
    public float interactionDistance = 3f;  // 인터랙션 가능 거리

    public string LastOrderSummary { get; private set; } // 주문 요약 저장용 프로퍼티

    // 구독 해제용 변수들 추가
    private IDisposable _chairSubscription;
    private IDisposable _playerInteractSubscription;
    private bool _isInitialized = false; // 중복 초기화 방지
    
    // 누적 주문 목록 관리
    private List<string> _accumulatedOrders = new List<string>();

    public void SetInfo(){
        // 중복 초기화 방지
        if (_isInitialized) return;
        _isInitialized = true;
        
        // 1. 의자 상태 변경 시 UI 자동 업데이트
        _chairSubscription = Managers.Subscribe(ActionType.Chair_OccupiedChanged, () => {
            foreach (var table in _tables)
            {
                if (table.IsFullyOccupied)
                {
                    // 🚨 이미 주문을 받은 테이블(WaitingForFood)은 상태 유지
                    if (table.CurrentUIState != Table.ETableUIState.WaitingForFood)
                    {
                        table.ShowReadyToOrderUI(); // 테이블 다 차면 "주문 가능" UI 표시
                        Managers.PublishAction(ActionType.Customer_TableFullyOccupied);
                    }
                }
                else
                {
                    // 테이블이 비면 무조건 숨김
                    table.HideOrderUI(); // 테이블 비면 UI 숨김
                }
            }
        });

        // 2. K키 누르면 주문 받기 (UI는 이미 떠 있는 상태)
        _playerInteractSubscription = Managers.Subscribe(ActionType.Player_InteractKey, HandlePlayerInteraction);
        
        Debug.Log("<color=orange>[TableManager]</color> 이벤트 구독 완료");
    }

    // 구독 해제 메서드 추가
    public void Dispose()
    {
        _chairSubscription?.Dispose();
        _playerInteractSubscription?.Dispose();
        _accumulatedOrders.Clear(); // 누적 주문 목록도 정리
        _isInitialized = false;
        Debug.Log("<color=orange>[TableManager]</color> 구독 해제 완료");
    }

    private void HandlePlayerInteraction()
    {
        //queue 에 넣어야할듯  CustomerCreator.cs  OnPlayerTakeOrder
        if (Managers.Game?.Player == null) return;
        Vector3 playerPos = Managers.Game.Player.transform.position;
        Table nearestTable = GetNearestInteractableTable(playerPos);
        
        if (nearestTable != null)
        {
            Debug.Log($"<color=green>[TableManager] 테이블 {nearestTable.tableId}에서 주문 받기!</color>");
            TakeOrderFromTable(nearestTable);
        }
        else
        {
            Debug.Log("<color=yellow>[TableManager] 근처에 주문 가능한 테이블이 없습니다.</color>");
        }
    }

    private Table GetNearestInteractableTable(Vector3 playerPos)
    {
        Table nearestTable = null;
        float nearestDistance = interactionDistance; // 최대 거리로 초기화

        foreach (var table in _tables)
        {
            // 테이블이 다 찼고, UI Canvas가 활성화되어 있는지 확인
            if (!table.IsFullyOccupied || table.tableOrderCanvas == null || !table.tableOrderCanvas.gameObject.activeSelf) 
                continue; 

            float distance = Vector3.Distance(playerPos, table.transform.position);
            
            if (distance <= nearestDistance) // 범위 내에서만 검색
            {
                nearestDistance = distance;
                nearestTable = table;
            }
        }
        return nearestTable;
    }

    private void TakeOrderFromTable(Table table)
    {
        var orders = table.CollectOrdersFromSeatedCustomers();

        // 새로운 주문들을 누적 목록에 추가
        foreach (var order in orders)
        {
            Managers.Game.CustomerCreator.OrderManager.AddOrder(order);
            string orderString = $"{order.RecipeName} x{order.Quantity}";
            _accumulatedOrders.Add(orderString);
            Debug.Log($"<color=cyan>[TableManager] 누적 주문 추가: {orderString}</color>");
        }
        
        // 누적된 모든 주문을 표시
        this.LastOrderSummary = string.Join("\n", _accumulatedOrders);
        
        table.ShowWaitingForFoodUI(); 
        
        Debug.Log($"<color=cyan>[TableManager] 테이블 {table.tableId}에서 {orders.Count}개 종류, 총 {orders.Sum(o => o.Quantity)}개 음식 주문 받음! UI를 음식 대기 상태로 변경.</color>");
        Debug.Log($"<color=magenta>[TableManager] 전체 누적 주문: {this.LastOrderSummary}</color>");

        Managers.PublishAction(ActionType.GameScene_UpdateOrderText); // ActionType만 전달
    }

    // 주문 완료 처리 메서드 추가 (음식이 완성되면 호출)
    public void CompleteOrder(string orderString)
    {
        if (_accumulatedOrders.Remove(orderString))
        {
            this.LastOrderSummary = string.Join("\n", _accumulatedOrders);
            Managers.PublishAction(ActionType.GameScene_UpdateOrderText);
            Debug.Log($"<color=green>[TableManager] 주문 완료: {orderString}</color>");
        }
    }

    // 모든 주문 초기화 메서드
    public void ClearAllOrders()
    {
        _accumulatedOrders.Clear();
        this.LastOrderSummary = "";
        Managers.PublishAction(ActionType.GameScene_UpdateOrderText);
        Debug.Log("<color=orange>[TableManager] 모든 주문 초기화</color>");
    }

    public void DebugPrintAllTableOrders()
    {
        Debug.Log("<color=magenta>===== 모든 테이블 주문 정보 출력 =====</color>");
        foreach (var table in _tables)
        {
            table.DebugPrintOrders();
        }
    }
    
    public void RegisterTable(Table table)
    {
        if (!_tables.Contains(table))
        {
            _tables.Add(table);
        }
        Debug.Log($"<color=cyan>[TableManager] 테이블 등록: {table.tableId}</color>");
    }

    public void UnregisterTable(Table table)
    {
        if (_tables.Contains(table))
            _tables.Remove(table);
    }
}