using System.Collections.Generic;
using UnityEngine;

public class Table : Item
{
    private static int _tableIdCounter = 1; // 1부터 시작
    public int tableId = 1;
    public List<Chair> chairs = new List<Chair>();
    public bool IsOccupied => chairs.Exists(c => c.IsOccupied);
    public bool IsFullyOccupied => chairs.TrueForAll(c => c.IsOccupied);

    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = Define.EObjectType.Table; // 필요시 Table 타입 추가
        tableId = _tableIdCounter++;

        Managers.Game.RegisterItem(this);


        return true;
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
        List<Order> orders = new List<Order>();
        foreach (var chair in chairs)
        {
            if (chair.IsOccupied && chair._currentCustomer != null)
            {
                var customer = chair._currentCustomer;
                foreach (var kvp in customer.orderedFoods)
                {
                    var food = kvp.Key;
                    var info = kvp.Value;
                    orders.Add(new Order
                    {
                        customer = customer,
                        recipeName = food.foodName,
                        requestText = info.specialRequest,
                        isRecommendation = info.isRecommended,
                        orderTime = System.DateTime.Now
                    });
                }
            }
        }
        return orders;
    }

    // 디버그: 테이블에 앉은 손님들의 주문 정보 출력
    public void DebugPrintOrders()
    {
        var orders = CollectOrdersFromSeatedCustomers();
        Debug.Log($"<color=yellow>[Table {tableId}] 주문 정보</color> (총 {orders.Count}건)");
        foreach (var order in orders)
        {
            string customerClass = order.customer != null ? order.customer.GetType().Name : "Unknown";
            string foodName = order.recipeName;
            string color = "cyan";
            Debug.Log($"<color={color}>[주문] 음식: {foodName}, 주문자: {customerClass}, 요청: {order.requestText}, 추천: {order.isRecommendation}</color>");
        }
    }
} 