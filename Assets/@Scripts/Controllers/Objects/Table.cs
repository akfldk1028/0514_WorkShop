using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Image 사용을 위해 추가

public class Table : Item
{
    private static int _tableIdCounter = 1; // 1부터 시작
    public int tableId = 1;
    public List<Chair> chairs = new List<Chair>();
    public bool IsOccupied => chairs.Exists(c => c.IsOccupied);
    public bool IsFullyOccupied => chairs.TrueForAll(c => c.IsOccupied);

    [Header("UI")]
    public Canvas tableOrderCanvas;  // 테이블 UI Canvas
    // public GameObject orderButton;   // 기존 버튼, 필요하면 유지

    public Image readyToOrderImage; // 손님 꽉 찼을 때 (주문 가능) 이미지
    public UI_TimeCountdownSlider waitingForFoodSlider; // 주문 받고 음식 기다릴 때 타이머 슬라이더

    public enum ETableUIState 
    {
        Hidden,
        ReadyToOrder,
        WaitingForFood
    }
    private ETableUIState currentUIState = ETableUIState.Hidden;
    public ETableUIState CurrentUIState => currentUIState;

    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = Define.EObjectType.Table; // 필요시 Table 타입 추가
        tableId = _tableIdCounter++;

        Managers.Game.RegisterItem(this);

        // UI 초기화 - 처음엔 모든 상태 이미지 비활성화
        if (tableOrderCanvas != null)
            tableOrderCanvas.gameObject.SetActive(false); // Canvas 자체를 먼저 비활성화

        if (readyToOrderImage != null)
            readyToOrderImage.gameObject.SetActive(false);
        if (waitingForFoodSlider != null)
        {
            waitingForFoodSlider.gameObject.SetActive(false);
            // 슬라이더 이벤트 연결
            waitingForFoodSlider.OnTimeUp += OnFoodWaitingTimeUp;
        }
        
        currentUIState = ETableUIState.Hidden;
        return true;
    }

    private void UpdateUIState(ETableUIState newState)
    {
        if (tableOrderCanvas == null) return;

        currentUIState = newState;
        
        // Canvas 활성화/비활성화
        tableOrderCanvas.gameObject.SetActive(newState != ETableUIState.Hidden);

        // 모든 UI 요소를 먼저 비활성화
        if (readyToOrderImage != null)
            readyToOrderImage.gameObject.SetActive(false);
        if (waitingForFoodSlider != null)
            waitingForFoodSlider.gameObject.SetActive(false);

        // 현재 상태에 맞는 UI만 활성화
        switch (newState)
        {
            case ETableUIState.ReadyToOrder:
                if (readyToOrderImage != null)
                    readyToOrderImage.gameObject.SetActive(true);
                break;
                
            case ETableUIState.WaitingForFood:
                if (waitingForFoodSlider != null)
                    waitingForFoodSlider.gameObject.SetActive(true);
                break;
                
            case ETableUIState.Hidden:
                // 이미 위에서 모든 UI를 비활성화했으므로 추가 작업 없음
                break;
        }
    }

    public void ShowReadyToOrderUI() // 기존 ShowOrderUI에서 변경
    {
        if (currentUIState != ETableUIState.ReadyToOrder) // 현재 비활성화 상태일 때만 실행
        {
            UpdateUIState(ETableUIState.ReadyToOrder);
            Debug.Log($"<color=green>[Table {tableId}] UI: 주문 가능 상태로 변경!</color>");
        }
    }

    public void ShowWaitingForFoodUI() // 새로 추가된 메서드
    {
        if (currentUIState != ETableUIState.WaitingForFood)
        {
            // 타이머 슬라이더를 먼저 설정 (비활성화 상태에서)
            if (waitingForFoodSlider != null)
            {
                waitingForFoodSlider.SetTotalTime(30f); // 음식 대기 시간 15초로 설정
                waitingForFoodSlider.ResetTimer();
            }
            
            UpdateUIState(ETableUIState.WaitingForFood);
            
            // UI 활성화 후 바로 타이머 시작 (InitializeSlider에서 덮어쓰지 않음)
            if (waitingForFoodSlider != null)
            {
                waitingForFoodSlider.StartCountdown();
                Debug.Log($"<color=cyan>[Table {tableId}] 타이머 카운트다운 바로 시작!</color>");
            }
            
            Debug.Log($"<color=blue>[Table {tableId}] UI: 음식 대기 상태로 변경! ({waitingForFoodSlider?.GetTotalTime()}초 타이머 시작)</color>");
        }
    }
    
    /// <summary>
    /// 음식 대기 시간이 끝났을 때 호출되는 메서드
    /// </summary>
    private void OnFoodWaitingTimeUp()
    {
        Debug.Log($"<color=red>[Table {tableId}] 음식 대기 시간 종료! 고객들이 불만을 표시하고 떠납니다.</color>");
        
        // 1. 불만 고객들 수집
        List<Customer> complainingCustomers = new List<Customer>();
        
        // 앉아있는 고객들을 불만 상태로 만들고 떠나게 함
        foreach (var chair in chairs)
        {
            if (chair.IsOccupied && chair._currentCustomer != null)
            {
                var customer = chair._currentCustomer;
                complainingCustomers.Add(customer);
                
                Debug.Log($"<color=orange>[Table {tableId}] 고객 {customer.name}이 음식을 너무 오래 기다려서 불만스럽게 떠납니다!</color>");
                Debug.Log($"<color=cyan>[Table {tableId}] 고객 {customer.name} 현재 상태: {customer.CustomerState}</color>");
                Debug.Log($"<color=cyan>[Table {tableId}] Agent 상태: {(customer.agent != null ? customer.agent.enabled.ToString() : "null")}</color>");
                
                // 고객을 불만 상태로 변경 - Customer 클래스가 알아서 처리
                customer.CustomerState = ECustomerState.StandingUp;
                Debug.Log($"<color=yellow>[Table {tableId}] 고객 {customer.name} 상태를 StandingUp으로 변경 완료</color>");
            }
        }
        
        // 2. 불만 고객들의 주문 데이터 정리
        if (complainingCustomers.Count > 0)
        {
            Debug.Log($"<color=red>[Table {tableId}] {complainingCustomers.Count}명의 불만 고객 주문 데이터 정리 시작</color>");
            
            // TableManager를 통해 주문 데이터 정리
            Managers.Game.CustomerCreator.TableManager.RemoveOrdersByCustomers(complainingCustomers);
            
            Debug.Log($"<color=orange>[Table {tableId}] 불만 고객들의 주문 데이터 정리 완료</color>");
        }
        
        // 3. 테이블 UI 숨김
        HideOrderUI();
        
        Debug.Log($"<color=magenta>[Table {tableId}] 불만 고객 처리 완료 - 총 {complainingCustomers.Count}명 처리됨</color>");
    }
    
    /// <summary>
    /// 음식이 서빙되었을 때 타이머를 중지하는 메서드
    /// </summary>
    public void OnFoodServed()
    {
        if (waitingForFoodSlider != null && waitingForFoodSlider.IsCountingDown())
        {
            waitingForFoodSlider.PauseCountdown();
            Debug.Log($"<color=green>[Table {tableId}] 음식이 서빙되어 타이머를 중지했습니다.</color>");
        }
        
        // 음식 서빙 후 상태 변경 (필요시)
        // HideOrderUI(); // 또는 다른 상태로 변경
    }

    public void HideOrderUI() // 기존 HideOrderUI에서 변경
    {
        if (currentUIState != ETableUIState.Hidden) // 현재 활성화 상태일 때만 실행
        {
            UpdateUIState(ETableUIState.Hidden);
            Debug.Log($"<color=red>[Table {tableId}] UI: 숨김 상태로 변경</color>");
        }
    }

    // 의자 추가
    public void AddChair(Chair chair)
    {
        if (!chairs.Contains(chair))
        {
            chairs.Add(chair);
            chair.table = this;
        }
    }

    // 빈 의자 반환
    public Chair FindEmptyChair()
    {
        return chairs.Find(c => !c.IsOccupied);
    }

    public List<Order> CollectOrdersFromSeatedCustomers()
    {
        List<Order> collectedOrders = new List<Order>(); 
        Debug.Log($"[Table ID: {tableId} (객체 InstanceID: {this.GetInstanceID()})] 주문 수집 시작. 의자 수: {chairs.Count}");

        foreach (var chair in chairs)
        {
            if (chair.IsOccupied && chair._currentCustomer != null)
            {
                var customer = chair._currentCustomer;
                Debug.Log($"[Table ID: {tableId}] 의자 {chair.name}에 앉은 고객 {customer.name} (InstanceID: {customer.GetInstanceID()}) 확인.");

                if (customer.orderedFoods.ContainsKey(this)) // 'this'는 현재 Table 객체
                {
                    var foodsOrderedAtThisTable = customer.orderedFoods[this];
                    Debug.Log($"[Table ID: {tableId}] 고객 {customer.name}이 현재 테이블(InstanceID: {this.GetInstanceID()})에서 {foodsOrderedAtThisTable.Count} 종류의 음식을 주문했습니다.");

                    foreach (var foodItem in foodsOrderedAtThisTable)
                    {
                        Debug.Log($"[Table ID: {tableId}] 고객 {customer.name}의 주문 추가: {foodItem.RecipeName}, 수량: {foodItem.Quantity}");
                        collectedOrders.Add(new Order
                        {
                            customer = customer,
                            recipeId = foodItem.NO, // recipeName 대신 recipeId(NO) 사용
                            Quantity = foodItem.Quantity,
                        });
                    }
                }
            }
            else
            {
                Debug.Log($"[Table ID: {tableId}] 의자 {chair.name}은 비어있거나 고객 정보가 없습니다.");
            }
        }
        Debug.Log($"[Table ID: {tableId}] 주문 수집 완료. 총 {collectedOrders.Count}개의 주문 항목 수집.");
        return collectedOrders;
    }

    // 디버그: 테이블에 앉은 손님들의 주문 정보 출력
    public void DebugPrintOrders()
    {
        var orders = CollectOrdersFromSeatedCustomers();
        Debug.Log($"<color=yellow>[Table {tableId}] 주문 정보</color> (총 {orders.Count}건)");
        foreach (var order in orders)
        {
            string customerClass = order.customer != null ? order.customer.GetType().Name : "Unknown";
            string foodName = order.RecipeName; // 편의 프로퍼티 사용
            string color = "cyan";
            Debug.Log($"<color={color}>[주문] 음식: {foodName}, 주문자: {customerClass}, 요청: {order.requestText}, 추천: {order.isRecommendation}</color>");
        }
    }

    // 고객이 떠날 때 테이블 상태 리셋
    public void ResetTableAfterCustomerLeave()
    {
        Debug.Log($"<color=cyan>[Table {tableId}] 고객 이탈 후 테이블 상태 확인 중...</color>");
        
        // 테이블에 앉은 고객이 없으면 완전 리셋
        if (!IsOccupied)
        {
            // UI 상태 리셋
            HideOrderUI();
            
            // 음식 대기 타이머가 실행 중이면 중지
            if (waitingForFoodSlider != null && waitingForFoodSlider.IsCountingDown())
            {
                waitingForFoodSlider.PauseCountdown();
                waitingForFoodSlider.ResetTimer();
                Debug.Log($"<color=yellow>[Table {tableId}] 음식 대기 타이머 중지 및 리셋</color>");
            }
            
            Debug.Log($"<color=yellow>[Table {tableId}] 고객이 모두 떠나서 테이블 완전 리셋 완료</color>");
        }
        // 테이블이 다시 꽉 차면 주문 가능 상태로 변경 (새로운 고객들이 앉은 경우)
        else if (IsFullyOccupied)
        {
            // 현재 음식 대기 중이 아니라면 주문 가능 상태로 변경
            if (currentUIState != ETableUIState.WaitingForFood)
            {
                ShowReadyToOrderUI();
                Debug.Log($"<color=green>[Table {tableId}] 새로운 고객들로 테이블이 다시 꽉 참 - 주문 가능 상태로 변경</color>");
            }
        }
        // 일부 고객만 있는 경우 - UI만 숨김
        else
        {
            if (currentUIState == ETableUIState.ReadyToOrder)
            {
                HideOrderUI();
                Debug.Log($"<color=orange>[Table {tableId}] 테이블이 부분적으로 비어서 주문 UI 숨김</color>");
            }
        }
    }
} 