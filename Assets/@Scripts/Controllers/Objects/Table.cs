using System.Collections.Generic;
using UnityEngine;

public class Table : Item
{
    public int tableId;
    public List<Chair> chairs = new List<Chair>();
    public bool IsOccupied => chairs.Exists(c => c.IsOccupied);
    public bool IsFullyOccupied => chairs.TrueForAll(c => c.IsOccupied);

    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = Define.EObjectType.Table; // 필요시 Table 타입 추가


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
} 