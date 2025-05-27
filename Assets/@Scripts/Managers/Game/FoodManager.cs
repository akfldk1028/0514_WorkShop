using System.Collections.Generic;
using UnityEngine;

public class FoodManager 
{
    private List<Food> _foods = new List<Food>();

    public FoodManager()
    {
        Debug.Log("<color=orange>[FoodManager]</color> 생성됨");
    }

    // 음식 데이터 초기화
    public void SetInfo()
    {
        _foods.Clear();
        _foods.Add(new Food("A!", 5000, 2000, new List<string>{"소주", "맥주"}, "국민조합 술", 2.5f, 1, true));
        _foods.Add(new Food("B!", 7000, 3000, new List<string>{"잭다니엘", "콜라"}, "위스키 칵테일", 3.0f, 2, true));
        _foods.Add(new Food("C!", 6000, 2500, new List<string>{"떡", "고추장", "어묵"}, "매콤한 분식", 4.0f, 1, true));
        // ... 등등
    }

    // 음식 추가
    public void AddFood(Food food)
    {
        _foods.Add(food);
    }

    // 음식 삭제
    public void RemoveFood(Food food)
    {
        _foods.Remove(food);
    }

    // 랜덤 음식 반환 (unlock된 것만)
    public Food GetRandomFood()
    {
        var unlocked = _foods.FindAll(f => f.isUnlocked);
        if (unlocked.Count == 0) return null;
        return unlocked[UnityEngine.Random.Range(0, unlocked.Count)];
    }

    // 이름으로 음식 찾기
    public Food GetFoodByName(string name)
    {
        return _foods.Find(f => f.foodName == name);
    }

    // 태그로 음식 리스트 반환
    public List<Food> GetFoodsByTag(string tag)
    {
        return _foods.FindAll(f => f.tags.Contains(tag));
    }

    // unlock/lock 관리
    public void UnlockFood(string name)
    {
        var food = GetFoodByName(name);
        if (food != null) food.isUnlocked = true;
    }
    public void LockFood(string name)
    {
        var food = GetFoodByName(name);
        if (food != null) food.isUnlocked = false;
    }

    // 전체 음식 리스트 반환
    public List<Food> GetAllFoods() => _foods;

  
}