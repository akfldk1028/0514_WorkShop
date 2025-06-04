using System.Collections.Generic;
using UnityEngine;

public class FoodManager 
{
    private List<Food> _foods = new List<Food>();

    public FoodManager()
    {
        Debug.Log("<color=orange>[FoodManager]</color> 생성됨");
    }

    public void SetInfo()
    {
        _foods.Clear();
        
        if (Managers.Data.RecipeDic == null)
        {
            Debug.LogError("[FoodManager] Managers.Data.RecipeDic이 null입니다!");
            return;
        }
        Debug.Log($"[FoodManager] RecipeDic에서 Food 객체 변환 시작. 총 {Managers.Data.RecipeDic.Count}개의 레시피 데이터.");

        foreach (Data.RecipeData rd in Managers.Data.RecipeDic.Values)
        {
            Food newFood = new Food(
                no: rd.NO,
                recipeID_eng: rd.RecipeID_eng,
                recipeName: rd.RecipeName,
                diffic: rd.Diffic,
                description: rd.Description,
                keyCombination: rd.KeyCombination,
                basePrice: rd.BasePrice,
                tags: rd.Tags,
                category: rd.Category,
                requiredIngredientsVisual: rd.RequiredIngredientsVisual,
                completedVisualResourceID: rd.CompletedVisualResourceID,
                openOption: rd.OpenOption
            );

            AddFood(newFood);
        }
        Debug.Log($"[FoodManager] Food 객체 변환 완료. 총 {_foods.Count}개의 Food가 로드됨.");
    }

    public void AddFood(Food food)
    {
        if (!_foods.Exists(f => f.NO == food.NO || f.RecipeID_eng == food.RecipeID_eng))
        {
            _foods.Add(food);
        }
    }

    public Food GetRandomFood()
    {
        var unlocked = _foods.FindAll(f => f.IsUnlocked);
        if (unlocked.Count == 0) 
        {
            return null;
        }
        return unlocked[Random.Range(0, unlocked.Count)];
    }

    public Food GetFoodByName(string name)
    {
        return _foods.Find(f => f.RecipeName == name);
    }

    public List<Food> GetFoodsByTag(string tag)
    {
        return _foods.FindAll(f => f.Tags != null && f.Tags.Contains(tag));
    }

    public void UnlockFood(string recipeNameOrId)
    {
        var food = GetFoodByName(recipeNameOrId); 
        if (food != null)
        {
            if (food.OpenOption == "LOCKED") 
            {
                food.OpenOption = "UNLOCKED"; 
            }
        }
    }
    public void LockFood(string recipeNameOrId)
    {
        var food = GetFoodByName(recipeNameOrId);
        if (food != null)
        {
            if (food.OpenOption != "LOCKED")
            {
                food.OpenOption = "LOCKED";
            }
        }
    }
    
    public List<Food> GetAllFoods() => _foods;
}