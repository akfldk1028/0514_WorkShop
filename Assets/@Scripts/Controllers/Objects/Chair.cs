using UnityEngine;
using static Define;


public class Chair : Item
{
    public Table table; // Table과 연동
    public Customer _currentCustomer;
    public Customer _reservedBy;
    public bool IsOccupied => _currentCustomer != null;
    public bool IsReserved => _reservedBy != null;
    public bool IsAvailable => !IsOccupied && !IsReserved;
    public Transform placeToSit;     // Customer와 같은 이름으로 통일

    public void Reserve(Customer customer)
    {
        _reservedBy = customer;
    }
    public void Unreserve()
    {
        _reservedBy = null;
    }

    public override bool Init()
    {
        if (!base.Init()) return false;
        ObjectType = EObjectType.Chair;
        Managers.Game.RegisterItem(this);
        return true;
    }

    // 손님 앉히기
    public void SeatCustomer(Customer customer)
    {
        _currentCustomer = customer;
        Unreserve(); // 예약 해제
        customer.transform.position = placeToSit.position;
        // customer.transform.LookAt(platePlace);
        Managers.PublishAction(ActionType.Chair_OccupiedChanged); // chair 자신을 넘김

    }

    // 손님 일어나기
    public void VacateSeat()
    {
    if (_currentCustomer != null)
    {
        _currentCustomer = null;
        Managers.PublishAction(ActionType.Chair_OccupiedChanged);
    }
    }
}

