using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Define;

// TODO 의자 지속적  찾기 로직추가해야함
public class CustomerCreator
{    
    private static CustomerCreator s_instance;
    private static CustomerCreator Instance { get { return s_instance; } }

    private FoodManager _foodManager = new FoodManager();
    private OrderManager _orderManager = new OrderManager();
    public TableManager _tableManager = new TableManager();

    public  FoodManager FoodManager { get { Instance._foodManager.SetInfo(); return Instance._foodManager; } }
    public  OrderManager OrderManager { get { Instance._orderManager.SetInfo(); return Instance._orderManager; } }
    public TableManager TableManager { get { Instance._tableManager.SetInfo(); return Instance._tableManager; } }
    private List<Customer> _customers = new List<Customer>();
    public IReadOnlyList<Customer> Customers => _customers;

    public float spawnInterval = 2.0f;
    private float lastSpawnTime = 0f;
    private IDisposable updateSubscription;
    private IDisposable customerSubscription;
    private bool isActive = false;
    
    [Header("고객 스폰 제한 설정")]
    public int maxCustomersToSpawn = 20; // 최대 스폰할 고객 수
    private int spawnedCustomersCount = 0; // 현재까지 스폰된 고객 수
    
    private int[] characterIdList = { CUSTOMER_ID_2, CUSTOMER_ID_3, CUSTOMER_ID_4, CUSTOMER_ID_5, CUSTOMER_ID_6, CUSTOMER_ID_7 }; // 예시: 원하는 id들로 채우세요

    public CustomerCreator()
    {
        s_instance = this; // 싱글톤 인스턴스 할당
        Debug.Log("<color=orange>[CustomerCreator]</color> 생성됨");

    }
    
    public void StartAutoSpawn()
    {
        if (isActive) return;
        
        isActive = true;
        lastSpawnTime = Time.time;
        updateSubscription = Managers.Subscribe(ActionType.Managers_Update, OnUpdate);
        Debug.Log($"[CustomerCreator] 자동 스폰 시작 (최대 {maxCustomersToSpawn}명)");
    }
    
    public void StopAutoSpawn()
    {
        isActive = false;
        updateSubscription?.Dispose();
        updateSubscription = null;
        Debug.Log("[CustomerCreator] 자동 스폰 중지");
    }
    

    private void OnUpdate()
    {
        if (!isActive) return;
        
        // 최대 고객 수에 도달했으면 스폰 중지
        if (spawnedCustomersCount >= maxCustomersToSpawn)
        {
            Debug.Log($"<color=yellow>[CustomerCreator]</color> 최대 고객 수 ({maxCustomersToSpawn}명)에 도달하여 스폰을 중지합니다.");
            StopAutoSpawn();
            return;
        }
        
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnCustomer();
            lastSpawnTime = Time.time;
        }
    }
    
    public void SetInfo()
    {
        // 여러 Customer 이벤트를 한번에 구독
        customerSubscription = Managers.SubscribeMultiple(OnCustomerAction, 
            ActionType.Customer_Spawned,
            ActionType.Customer_WaitingForTable,
            ActionType.Customer_MovedToTable,
            ActionType.Customer_Seated,
            ActionType.Customer_Ordered,
            ActionType.Customer_WaitingForFood,
            ActionType.Customer_ReceivedFood,
            ActionType.Customer_StartedEating,
            ActionType.Customer_FinishedEating,
            ActionType.Customer_Left
        );
    }
    // 플레이어가 손님을 클릭해서 주문을 받는 함수
    public void OnPlayerTakeOrder(Table table)
    {
        var orders = table.CollectOrdersFromSeatedCustomers();
        foreach (var order in orders)
        {
            _orderManager.AddOrder(order); // OrderManager의 주문 큐에 추가
        }
    }

    private void OnCustomerAction(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Customer_Spawned:
                Debug.Log("[GameManager] 고객이 들어옴옴!");
                // TODO: 전체 손님 수 증가, 대기열 UI 갱신, 입장 효과음/이펙트, 튜토리얼 안내 등
                // Managers.Game.IncrementCustomerCount();
                // Managers.UI.UpdateWaitingQueue();
                // Managers.Sound.Play("CustomerArrive");
                break;
            case ActionType.Customer_WaitingForTable:
                Debug.Log("[GameManager] 고객이 테이블 대기 중!");
                // TODO: 대기열 UI 강조, 대기 손님 수 표시, 안내 메시지 등
                // Managers.UI.HighlightWaitingArea();
                break;
            case ActionType.Customer_MovedToTable:
                Debug.Log("[GameManager] 고객이 테이블로 이동!");
                // TODO: 테이블 하이라이트, 안내 메시지, 손님 상태 UI 갱신 등
                // Managers.UI.HighlightTable();
                // Managers.UI.ShowMessage("손님이 자리에 앉으러 이동 중!");
                break;
                
            case ActionType.Customer_Seated:
                Debug.Log("[GameManager] 고객이 자리에 앉음!");
                // TODO: 착석 효과음, 착석 수 카운트, 테이블 UI 갱신, 업적/퀘스트 체크 등
                // Managers.Sound.Play("Seat");
                // Managers.Game.IncrementSeatedCount();
                break;
            case ActionType.Customer_TableFullyOccupied:
                Debug.Log("[GameManager] 테이블 만석!");
                // TODO: 테이블 만석 알림, 주문 버튼 활성화, UI 갱신 등
                // Managers.UI.ShowTableFullMessage();
                break;
            case ActionType.Customer_Ordered:
                Debug.Log("[GameManager] 고객이 주문함!");
                _orderManager.UpdateOrderUI();
                // TODO: 전체 주문 리스트 UI 갱신, 주문 알림, 사운드, 통계 등
                // Managers.OrderManager.AddOrder(...);
                // Managers.UI.UpdateOrderList();
                // Managers.Sound.Play("Order");
                break;
            case ActionType.Customer_WaitingForFood:
                Debug.Log("[GameManager] 고객이 음식을 기다림!");
                // TODO: 대기 상태 UI, 음식 준비 알림, 주방/서빙 시스템 연동 등
                // Managers.Kitchen.NotifyOrderWaiting(...);
                break;
            case ActionType.Customer_ReceivedFood:
                Debug.Log("[GameManager] 고객이 음식을 받음!");
                // TODO: 음식 전달 이펙트/사운드, 만족도 증가, UI 갱신 등
                // Managers.Sound.Play("FoodReceived");
                // Managers.Game.IncrementFoodServed();
                break;
            case ActionType.Customer_StartedEating:
                Debug.Log("[GameManager] 고객이 먹기 시작함!");
                // TODO: 식사 애니메이션, 식사 중 UI, 만족도/점수 증가 등
                // Managers.UI.ShowEatingState(...);
                break;
            case ActionType.Customer_FinishedEating:
                Debug.Log("[GameManager] 고객이 식사를 마침!");
                // TODO: 점수/골드 증가, 손님 만족도 평가, 업적/퀘스트 체크 등
                // Managers.Game.AddScore(...);
                // Managers.UI.ShowSatisfaction(...);
                break;
            case ActionType.Customer_Left:
                Debug.Log("[GameManager] 고객 퇴장!");
                // TODO: 전체 손님 수 감소, 대기열 UI 갱신, 퇴장 효과음/이펙트, 통계 등
                // Managers.Game.DecrementCustomerCount();
                // Managers.UI.UpdateWaitingQueue();
                // Managers.Sound.Play("CustomerLeave");
                break;
        }
    }


    void OnDestroy()
    {
        customerSubscription?.Dispose();
        _tableManager?.Dispose(); // TableManager 구독 해제 추가
    }

    private void SpawnCustomer()
    {
        // 2. 고객 타입 선택
        int randomCharacterIndex = UnityEngine.Random.Range(0, characterIdList.Length);
        int randomCharacterId = characterIdList[randomCharacterIndex];
        
        // 3. 대기 위치 할당받기 (없으면 기본 위치 사용)
        Vector3 waitingPosition = Managers.Map.GetNextAvailableWaitingPositionForSpawn();
        if (waitingPosition == Vector3.zero)
        {
            // 대기 위치가 없으면 기본 대기 위치 사용
            waitingPosition = Managers.Map.WaitingCellPos;
            Debug.Log($"<color=yellow>[CustomerCreator]</color> 대기 위치가 꽉 참! 기본 위치에 스폰: {waitingPosition}");
        }
        
        // 4. 대기 위치에서 바로 Customer 생성
        Customer customer = Managers.Object.Spawn<Customer>(waitingPosition, randomCharacterId, pooling: true);
        if (customer == null)
        {
            Debug.LogWarning("[CustomerCreator] Customer 생성 실패!");
            return;
        }
        
        // 5. Agent 확인 및 기본 설정
        NavMeshAgent agent = customer.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"<color=red>[CustomerCreator]</color> {customer.name}에 NavMeshAgent가 없습니다!");
            Managers.Object.Despawn(customer);
            return;
        }
        
        agent.enabled = true;
        agent.speed = 3.5f;
        
        // 6. 대기열에 정식 등록
        Managers.Map.RegisterCustomerInQueue(customer, waitingPosition);
        
        // 7. 스폰된 고객 수 증가
        spawnedCustomersCount++;
        
        Debug.Log($"<color=green>[CustomerCreator]</color> {customer.name} 대기줄에 직접 스폰 완료! 위치: {waitingPosition}");
        Debug.Log($"<color=cyan>[CustomerCreator]</color> 현재 대기자: {Managers.Map.WaitingCustomerCount}명, 총 스폰된 고객: {spawnedCustomersCount}/{maxCustomersToSpawn}명");
        
        Managers.PublishAction(ActionType.Customer_Spawned);
    }
    
    /// <summary>
    /// 스폰 카운터를 리셋합니다.
    /// </summary>
    public void ResetSpawnCounter()
    {
        spawnedCustomersCount = 0;
        Debug.Log($"<color=green>[CustomerCreator]</color> 스폰 카운터가 리셋되었습니다.");
    }
    
    /// <summary>
    /// 최대 스폰 수를 변경합니다.
    /// </summary>
    /// <param name="newMaxCount">새로운 최대 스폰 수</param>
    public void SetMaxCustomersToSpawn(int newMaxCount)
    {
        maxCustomersToSpawn = newMaxCount;
        Debug.Log($"<color=green>[CustomerCreator]</color> 최대 스폰 수가 {newMaxCount}명으로 변경되었습니다.");
    }
    
    /// <summary>
    /// 현재 스폰 상태를 반환합니다.
    /// </summary>
    /// <returns>현재까지 스폰된 고객 수와 최대 스폰 수</returns>
    public (int spawned, int max) GetSpawnStatus()
    {
        return (spawnedCustomersCount, maxCustomersToSpawn);
    }

    // private void SpawnCustomer()
    // {
    //     var waitingCells = Managers.Map.WaitingCells;
    //     var waitingCellPos = Managers.Map.WaitingCellPos;

    //     Debug.Log("[CustomerCreator] 고객 생성: " + waitingCellPos);
    //     waitingCellPos.y = 0f;
    //     Customer customer = Managers.Object.Spawn<Customer>(waitingCellPos, CUSTOMER_ID, pooling: true);
    //     Debug.Log("[CustomerCreator] 고객 생성: " + customer);
    //     if (customer != null)
    //     {
    //         Managers.PublishAction(ActionType.Customer_Spawned);
    //     }


    //     // if (waitingCells.Count > 0)
    //     // {
    //     //     int randomIndex = UnityEngine.Random.Range(0, waitingCells.Count);
    //     //     Vector3 cellPos = waitingCells[randomIndex];
    //     //     Debug.Log("[CustomerCreator] 고객 생성: " + cellPos);
    //     //     // Vector3 worldPos = Managers.Map.GetCellCenterWorld(cellPos);
    //     //     cellPos.y = 0f;
            
    //     //     Customer customer = Managers.Object.Spawn<Customer>(cellPos, CUSTOMER_ID, pooling: true);
    //     //     Debug.Log("[CustomerCreator] 고객 생성: " + customer);
    //     //     if (customer != null)
    //     //     {
    //     //         Managers.PublishAction(ActionType.Customer_Spawned);
    //     //     }
    //     // }
    // }

}