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
    public float interactionDistance = 5f;  // 인터랙션 가능 거리

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
                    // 테이블이 비면 리셋 처리
                    table.ResetTableAfterCustomerLeave(); // 테이블 리셋 메서드 호출
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

    /// <summary>
    /// 특정 고객들의 주문을 누적 목록에서 제거
    /// </summary>
    /// <param name="customers">주문을 제거할 고객들</param>
    public void RemoveOrdersByCustomers(List<Customer> customers)
    {
        if (customers == null || customers.Count == 0) return;
        
        // OrderManager에서 주문 제거
        int removedFromQueue = Managers.Game.CustomerCreator.OrderManager.RemoveOrdersByCustomers(customers);
        
        // 누적 주문 목록에서도 제거 (간단히 해당 고객들의 주문 패턴 제거)
        List<string> ordersToRemove = new List<string>();
        
        foreach (var customer in customers)
        {
            // Customer의 주문 데이터에서 주문 문자열 생성하여 누적 목록에서 찾아 제거
            if (customer.orderedFoods != null)
            {
                foreach (var tableOrders in customer.orderedFoods.Values)
                {
                    foreach (var food in tableOrders)
                    {
                        string orderString = $"{food.RecipeName} x{food.Quantity}";
                        if (_accumulatedOrders.Contains(orderString))
                        {
                            ordersToRemove.Add(orderString);
                        }
                    }
                }
            }
        }
        
        // 누적 목록에서 제거
        foreach (var orderToRemove in ordersToRemove)
        {
            if (_accumulatedOrders.Remove(orderToRemove))
            {
                Debug.Log($"<color=red>[TableManager]</color> 누적 주문에서 제거됨: {orderToRemove}");
            }
        }
        
        // UI 업데이트
        this.LastOrderSummary = string.Join("\n", _accumulatedOrders);
        Managers.PublishAction(ActionType.GameScene_UpdateOrderText);
        
        Debug.Log($"<color=orange>[TableManager]</color> {customers.Count}명의 고객 주문 정리됨. 큐에서 {removedFromQueue}개, 누적에서 {ordersToRemove.Count}개 제거");
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

    #region 서빙 관련 기능
    
    /// <summary>
    /// 플레이어 위치에서 가장 가까운 서빙 가능한 테이블을 찾습니다.
    /// </summary>
    /// <param name="playerPos">플레이어 위치</param>
    /// <returns>서빙 가능한 테이블 (없으면 null)</returns>
    public Table GetNearestServableTable(Vector3 playerPos)
    {
        Table nearestTable = null;
        float nearestDistance = interactionDistance;

        foreach (var table in _tables)
        {
            if (table.CurrentUIState == Table.ETableUIState.WaitingForFood)
            {
                float distance = Vector3.Distance(playerPos, table.transform.position);
                
                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTable = table;
                }
            }
        }
        return nearestTable;
    }
    
    /// <summary>
    /// 테이블에 서빙을 시도합니다.
    /// </summary>
    /// <param name="table">서빙할 테이블</param>
    /// <returns>서빙 성공 여부</returns>
    public bool TryServeTable(Table table)
    {
        if (table == null)
        {
            Debug.LogError("[TableManager] 테이블이 null입니다!");
            return false;
        }
        
        var tableOrders = table.CollectOrdersFromSeatedCustomers();
        if (tableOrders?.Count == 0)
        {
            Debug.Log("<color=yellow>[TableManager] 테이블에 주문이 없습니다.</color>");
            return false;
        }
        
        Debug.Log($"<color=cyan>[TableManager] 테이블 {table.tableId}에 {tableOrders.Count}개의 주문 항목이 있습니다.</color>");
        
        int servedCount = 0;
        
        // 모든 주문에 대해 서빙 시도
        foreach (var order in tableOrders)
        {
            if (TryServeOrder(table, order))
            {
                servedCount++;
            }
        }
        
        if (servedCount == 0)
        {
            Debug.Log("<color=yellow>[TableManager] 플레이어가 이 테이블의 주문에 맞는 음식을 가지고 있지 않습니다.</color>");
            return false;
        }
        
        // 서빙 완료 후 고객 상태 업데이트
        UpdateCustomersToEating(table);
        
        Debug.Log($"<color=green>[TableManager] 총 {servedCount}개의 주문을 서빙했습니다.</color>");
        return true;
    }
    
    /// <summary>
    /// 개별 주문을 서빙합니다.
    /// </summary>
    private bool TryServeOrder(Table table, Order order)
    {
        Debug.Log($"<color=cyan>[TableManager] 주문 확인: {order.RecipeName} x{order.Quantity} (ID: {order.recipeId})</color>");
        
        if (!Managers.Game.HasRecipe(order.recipeId))
        {
            Debug.Log($"<color=yellow>[TableManager] 플레이어가 {order.RecipeName}을(를) 가지고 있지 않습니다.</color>");
            return false;
        }
        
        var recipe = Managers.Game.GetRecipe(order.recipeId);
        if (!recipe.HasValue)
        {
            Debug.LogError($"[TableManager] 레시피 정보를 가져올 수 없습니다: {order.recipeId}");
            return false;
        }
        
        // OrderManager에게 주문 처리 위임
        bool orderProcessed = Managers.Game.CustomerCreator.OrderManager.ProcessServedOrder(recipe.Value, order);
        
        if (orderProcessed)
        {
            // 테이블에 음식 스폰
            SpawnFoodOnTable(table, recipe.Value.prefabName, order.Quantity);
            Debug.Log($"<color=green>[TableManager] 주문 서빙 완료: {order.RecipeName} x{order.Quantity}</color>");
        }
        
        return orderProcessed;
    }
    
    /// <summary>
    /// 테이블에 음식을 스폰합니다.
    /// </summary>
    private void SpawnFoodOnTable(Table table, string prefabName, int quantity)
    {
        // plate-over 사운드 재생
        Managers.Sound.Play(Define.ESound.Effect, "plate-over");
        
        for (int i = 0; i < quantity; i++)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                Debug.LogError("[TableManager] prefabName이 비어있습니다!");
                continue;
            }
            
            // 테이블 위에서 랜덤 위치에 1.5f 격차로 배치
            float randomX = UnityEngine.Random.Range(-1.5f, 1.5f);
            float randomZ = UnityEngine.Random.Range(-1.5f, 1.5f);
            Vector3 offset = new Vector3(randomX, 0, randomZ);
            Vector3 spawnPos = table.transform.position + Vector3.up * 1.5f + offset;
            
            // 리소스 매니저를 통해 prefab 스폰 (테이블을 부모로 설정)
            GameObject spawnedFood = Managers.Resource.Instantiate(prefabName, spawnPos, Quaternion.identity, table.transform);
            
            if (spawnedFood != null)
            {
                // 스케일을 1.3배로 키우기
                spawnedFood.transform.localScale = Vector3.one * 1.5f;
                Debug.Log($"<color=cyan>[TableManager] 음식 스폰 완료: {prefabName} at {spawnPos} (#{i + 1})</color>");
            }
            else
            {
                Debug.LogError($"<color=red>[TableManager] 음식 스폰 실패: {prefabName}</color>");
            }
        }
    }
    
    /// <summary>
    /// 테이블의 고객들을 식사 상태로 업데이트합니다.
    /// </summary>
    private void UpdateCustomersToEating(Table table)
    {
        if (table?.chairs == null) return;
        
        int customersServed = 0;
        
        // 테이블에 앉은 모든 고객을 Eating 상태로 전환
        foreach (var chair in table.chairs)
        {
            var customer = chair?._currentCustomer;
            if (customer == null || !chair.IsOccupied) continue;
            
            if (customer.CustomerState == ECustomerState.WaitingForFood)
            {
                // 음식 받음 액션 발행
                Managers.PublishAction(ActionType.Customer_ReceivedFood);
                
                // 식사 시작 상태로 전환
                customer.CustomerState = ECustomerState.Eating;
                customersServed++;
                
                Debug.Log($"<color=green>[TableManager] 고객 {customer.name}이 음식을 받고 식사를 시작합니다.</color>");
            }
        }
        
        if (customersServed > 0)
        {
            // 🆕 음식이 서빙되었으므로 테이블의 대기 슬라이더 숨기기
            table.OnFoodServed();
            
            Debug.Log($"<color=cyan>[TableManager] 테이블 {table.tableId}에서 총 {customersServed}명의 고객이 식사를 시작했습니다.</color>");
        }
    }
    
    #endregion
}