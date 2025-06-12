using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Linq 추가

public class Order
{
    public Customer customer;
    public int recipeId; // string recipeName 대신 int recipeId 사용
    public int Quantity; // << 수량 필드 추가
    public string requestText; // case2
    public bool isRecommendation; // true면 추천 요청
    public DateTime orderTime;
    
    // 편의 프로퍼티 - 레시피 데이터 조회
    public Data.RecipeData RecipeData 
    { 
        get 
        { 
            return Managers.Data?.RecipeDic?.ContainsKey(recipeId) == true ? 
                   Managers.Data.RecipeDic[recipeId] : null; 
        } 
    }
    
    // 편의 프로퍼티 - 레시피 이름
    public string RecipeName 
    { 
        get { return RecipeData?.RecipeName ?? "알 수 없는 레시피"; } 
    }
    
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
            Debug.LogWarning($"<color=red>[OrderManager]</color> 주문 큐가 가득함! 크기: {orderQueue.Count}");
            return;
        }

        // 중복 주문 체크 (같은 고객의 같은 음식)
        bool isDuplicate = orderQueue.Any(existingOrder => 
            existingOrder.customer == order.customer && 
            existingOrder.recipeId == order.recipeId);

        if (isDuplicate)
        {
            Debug.LogWarning($"<color=yellow>[OrderManager]</color> 중복 주문 감지됨: {order.customer?.name} - {order.RecipeName}");
            return;
        }

        orderQueue.Enqueue(order);
        Debug.Log($"<color=green>[OrderManager]</color> 주문 추가됨: {order.RecipeName} x{order.Quantity} (큐 크기: {orderQueue.Count})");
        Managers.PublishAction(ActionType.Customer_Ordered);
    }

    public Order GetNextOrder()
    {
        return orderQueue.Count > 0 ? orderQueue.Dequeue() : null;
    }

    public Order PeekNextOrder()
    {
        return orderQueue.Count > 0 ? orderQueue.Peek() : null;
    }

    public int GetOrderCount()
    {
        return orderQueue.Count;
    }

    public List<Order> GetAllOrders()
    {
        return orderQueue.ToList();
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
    
    /// <summary>
    /// 레시피 ID로 주문 생성 (편의 메서드)
    /// </summary>
    public Order CreateOrder(Customer customer, int recipeId, int quantity = 1, string requestText = null, bool isRecommendation = false)
    {
        return new Order
        {
            customer = customer,
            recipeId = recipeId,
            Quantity = quantity,
            requestText = requestText,
            isRecommendation = isRecommendation,
            orderTime = DateTime.Now
        };
    }
    
    /// <summary>
    /// 특정 고객의 모든 주문을 제거
    /// </summary>
    /// <param name="customer">주문을 제거할 고객</param>
    /// <returns>제거된 주문 수</returns>
    public int RemoveOrdersByCustomer(Customer customer)
    {
        if (customer == null) return 0;
        
        var ordersToRemove = orderQueue.Where(order => order.customer == customer).ToList();
        int removedCount = 0;
        
        // 큐에서 해당 고객의 주문들을 제거
        var tempQueue = new Queue<Order>();
        while (orderQueue.Count > 0)
        {
            var order = orderQueue.Dequeue();
            if (order.customer != customer)
            {
                tempQueue.Enqueue(order);
            }
            else
            {
                removedCount++;
                Debug.Log($"<color=red>[OrderManager]</color> {customer.name}의 주문 제거됨: {order.RecipeName} x{order.Quantity}");
            }
        }
        
        // 큐 재구성
        orderQueue = tempQueue;
        
        if (removedCount > 0)
        {
            Debug.Log($"<color=orange>[OrderManager]</color> {customer.name}의 주문 {removedCount}개 제거됨 (남은 주문: {orderQueue.Count})");
            // UI 업데이트 액션 호출
            Managers.PublishAction(ActionType.GameScene_UpdateOrderText);
        }
        
        return removedCount;
    }
    
    /// <summary>
    /// 특정 고객들의 모든 주문을 제거
    /// </summary>
    /// <param name="customers">주문을 제거할 고객들</param>
    /// <returns>제거된 주문 수</returns>
    public int RemoveOrdersByCustomers(List<Customer> customers)
    {
        if (customers == null || customers.Count == 0) return 0;
        
        int totalRemovedCount = 0;
        foreach (var customer in customers)
        {
            totalRemovedCount += RemoveOrdersByCustomer(customer);
        }
        
        // 다중 고객 주문 제거 시에도 UI 업데이트 (이미 개별적으로 호출되지만 확실히 하기 위해)
        if (totalRemovedCount > 0)
        {
            Managers.PublishAction(ActionType.GameScene_UpdateOrderText);
        }
        
        return totalRemovedCount;
    }
    
    /// <summary>
    /// 특정 레시피 ID의 주문들을 제거
    /// </summary>
    /// <param name="recipeId">제거할 레시피 ID</param>
    /// <returns>제거된 주문 수</returns>
    public int RemoveOrdersByRecipeId(int recipeId)
    {
        var tempQueue = new Queue<Order>();
        int removedCount = 0;
        
        while (orderQueue.Count > 0)
        {
            var order = orderQueue.Dequeue();
            if (order.recipeId != recipeId)
            {
                tempQueue.Enqueue(order);
            }
            else
            {
                removedCount++;
                Debug.Log($"<color=red>[OrderManager]</color> 레시피 ID {recipeId}의 주문 제거됨: {order.RecipeName} x{order.Quantity}");
            }
        }
        
        orderQueue = tempQueue;
        
        if (removedCount > 0)
        {
            Debug.Log($"<color=orange>[OrderManager]</color> 레시피 ID {recipeId}의 주문 {removedCount}개 제거됨");
            // UI 업데이트 액션 호출
            Managers.PublishAction(ActionType.GameScene_UpdateOrderText);
        }
        
        return removedCount;
    }
    
    /// <summary>
    /// 첫 번째 주문을 맨 뒤로 이동 (Tab키로 레시피 건너뛰기용)
    /// </summary>
    /// <returns>이동 성공 여부</returns>
    public bool MoveFirstOrderToBack()
    {
        if (orderQueue.Count <= 1) return false;
        
        var firstOrder = orderQueue.Dequeue();
        orderQueue.Enqueue(firstOrder);
        
        Debug.Log($"<color=cyan>[OrderManager]</color> 첫 번째 주문을 맨 뒤로 이동: {firstOrder.RecipeName}");
        return true;
    }
    
    /// <summary>
    /// 서빙된 주문을 처리합니다. (TableManager에서 호출)
    /// </summary>
    /// <param name="recipe">서빙할 레시피</param>
    /// <param name="order">처리할 주문</param>
    /// <returns>처리 성공 여부</returns>
    public bool ProcessServedOrder(GameManager.PlayerRecipe recipe, Order order)
    {
        Debug.Log($"<color=green>[OrderManager] 주문 처리: {recipe.recipeName} x{order.Quantity}</color>");
        
        // 1. 플레이어 인벤토리에서 제거
        Managers.Game.RemoveRecipeFromInventory(recipe.recipeId);
        
        // 2. UI에서 아이콘 제거
        RemoveRecipeFromUI(recipe.recipeId);
        
        return true;
    }
    
    /// <summary>
    /// UI에서 레시피 아이콘을 제거합니다.
    /// </summary>
    private void RemoveRecipeFromUI(int recipeId)
    {
        InGameManager.CompletedOrderData.LastCompletedRecipeId = recipeId;
        Managers.PublishAction(ActionType.GameScene_RemoveCompletedRecipe);
    }
}