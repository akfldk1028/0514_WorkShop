using System;
using UnityEngine;
using static Define;

public class CustomerCreator
{
    public CustomerCreator()
    {
        Debug.Log("<color=orange>[CustomerCreator]</color> 생성됨");
    }
    
    public float spawnInterval = 2.0f;
    private float lastSpawnTime = 0f;
    private IDisposable updateSubscription;
    private IDisposable customerSubscription;

    private bool isActive = false;
    
    public void StartAutoSpawn()
    {
        if (isActive) return;
        
        isActive = true;
        lastSpawnTime = Time.time;
        updateSubscription = Managers.Subscribe(ActionType.Managers_Update, OnUpdate);
        Debug.Log("[CustomerCreator] 자동 스폰 시작");
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
        
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnCustomer();
            lastSpawnTime = Time.time;
        }
    }
    
    public void Init()
    {
        // 여러 Customer 이벤트를 한번에 구독
        customerSubscription = Managers.SubscribeMultiple(OnCustomerAction, 
            ActionType.Customer_Spawned,
            ActionType.Customer_MovedToTable,
            ActionType.Customer_Left);
    }

    private void OnCustomerAction(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Customer_Spawned:
                Debug.Log("[GameManager] 새 고객 도착!");
                // 점수, UI 업데이트 등
                break;
                
            case ActionType.Customer_MovedToTable:
                Debug.Log("[GameManager] 고객이 테이블로 이동!");
                break;
                
            case ActionType.Customer_Left:
                Debug.Log("[GameManager] 고객 퇴장!");
                break;
        }
    }

    void OnDestroy()
    {
        customerSubscription?.Dispose();
    }


    private void SpawnCustomer()
    {
        var waitingCells = Managers.Map.WaitingCells;
        if (waitingCells.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, waitingCells.Count);
            Vector3Int cellPos = waitingCells[randomIndex];
            Vector3 worldPos = Managers.Map.GetCellCenterWorld(cellPos);
            worldPos.y = 0f;
            
            Customer customer = Managers.Object.Spawn<Customer>(worldPos, CUSTOMER_ID, pooling: true);
            
            if (customer != null)
            {
                Managers.PublishAction(ActionType.Customer_Spawned);
            }
        }
    }
}