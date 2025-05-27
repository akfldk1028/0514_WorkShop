using System;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public Customer customer;
    public string recipeName; // case1
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

    public void AddOrder(Order order)
    {
        orderQueue.Enqueue(order);
        UpdateOrderUI();
    }

    public Order GetNextOrder()
    {
        return orderQueue.Count > 0 ? orderQueue.Dequeue() : null;
    }

    public void UpdateOrderUI()
    {
        // 우측 상단 UI에 주문 목록 표시
    }
}