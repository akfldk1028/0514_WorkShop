using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq 추가

public class Order
{
    public Customer customer;
    public string recipeName; // case1
    public int Quantity; // << 수량 필드 추가
    public string requestText; // case2
    public bool isRecommendation; // true면 추천 요청
    public DateTime orderTime;
    // ... 기타 정보
}


public class OrderManager 
{

    public OrderManager()
    {
        Debug.Log("<color=orange>[OrderManager]</color> 생성됨");
    }

    public void SetInfo()
    {

    }
    private Queue<Order> orderQueue = new Queue<Order>();
    private const int MAX_QUEUE_SIZE = 50; // 최대 주문 큐 크기

    public void AddOrder(Order order)
    {
        // 큐 크기 제한 체크
        if (orderQueue.Count >= MAX_QUEUE_SIZE)
        {
            Debug.LogWarning($"<color=red>[OrderManager]</color> 주문 큐가 가득참! 크기: {orderQueue.Count}");
            return;
        }

        // 중복 주문 체크 (같은 고객의 같은 음식)
        bool isDuplicate = orderQueue.Any(existingOrder => 
            existingOrder.customer == order.customer && 
            existingOrder.recipeName == order.recipeName);

        if (isDuplicate)
        {
            Debug.LogWarning($"<color=yellow>[OrderManager]</color> 중복 주문 감지됨: {order.customer?.name} - {order.recipeName}");
            return;
        }

        orderQueue.Enqueue(order);
        Debug.Log($"<color=green>[OrderManager]</color> 주문 추가됨: {order.recipeName} x{order.Quantity} (큐 크기: {orderQueue.Count})");
        Managers.PublishAction(ActionType.Customer_Ordered);
    }

    public Order GetNextOrder()
    {
        return orderQueue.Count > 0 ? orderQueue.Dequeue() : null;
    }

    public int GetOrderCount()
    {
        return orderQueue.Count;
    }

    public void ClearOrders()
    {
        orderQueue.Clear();
        Debug.Log("<color=orange>[OrderManager]</color> 모든 주문 삭제됨");
    }

    public void UpdateOrderUI()
    {
        // 우측 상단 UI에 주문 목록 표시
        Debug.Log($"<color=cyan>[OrderManager]</color> UI 업데이트 - 주문 수: {orderQueue.Count}");
    }
}