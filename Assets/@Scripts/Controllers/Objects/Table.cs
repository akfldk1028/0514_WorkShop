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
    public Image waitingForFoodImage; // 주문 받고 음식 기다릴 때 이미지

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
        if (waitingForFoodImage != null)
            waitingForFoodImage.gameObject.SetActive(false);
        
        currentUIState = ETableUIState.Hidden;
        return true;
    }

    private void UpdateUIState(ETableUIState newState)
    {
        if (tableOrderCanvas == null) return;

        currentUIState = newState;
        tableOrderCanvas.gameObject.SetActive(newState != ETableUIState.Hidden);

        if (readyToOrderImage != null)
            readyToOrderImage.gameObject.SetActive(newState == ETableUIState.ReadyToOrder);
        if (waitingForFoodImage != null)
            waitingForFoodImage.gameObject.SetActive(newState == ETableUIState.WaitingForFood);
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
            UpdateUIState(ETableUIState.WaitingForFood);
            Debug.Log($"<color=blue>[Table {tableId}] UI: 음식 대기 상태로 변경!</color>");
        }
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
        // 테이블에 앉은 고객이 없으면 UI 숨김
        if (!IsOccupied)
        {
            HideOrderUI();
            Debug.Log($"<color=yellow>[Table {tableId}] 고객이 모두 떠나서 테이블 상태 리셋</color>");
        }
        // 테이블이 다시 꽉 차면 주문 가능 상태로 변경
        else if (IsFullyOccupied && currentUIState == ETableUIState.WaitingForFood)
        {
            // 음식을 다 먹었으면 다시 주문 가능 상태로 (필요시)
            // ShowReadyToOrderUI();
        }
    }
} 