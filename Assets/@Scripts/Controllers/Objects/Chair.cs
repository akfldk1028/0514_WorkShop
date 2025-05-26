using UnityEngine;
using static Define;


public class Chair : Item
{
    private Customer _currentCustomer;
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
        Managers.Game.RegisterItem(this);
        ObjectType = EObjectType.Chair;
        return true;
    }

    // 손님 앉히기
    public void SeatCustomer(Customer customer)
    {
        _currentCustomer = customer;
        Unreserve(); // 예약 해제
        customer.transform.position = placeToSit.position;
        // customer.transform.LookAt(platePlace);
    }

    // 손님 일어나기
    public void VacateSeat()
    {
        if (_currentCustomer != null)
            _currentCustomer = null;
    }
}

