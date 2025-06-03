using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    public int NO;
    public string RecipeID_eng;
    public string RecipeName;      
    public int Diffic;            
    public string Description;     
    public List<string> KeyCombination; 
    public int BasePrice;         
    public List<string> Tags;          
    public string Category;
    public List<string> RequiredIngredientsVisual;
    public string CompletedVisualResourceID; 
    public string OpenOption;

    // FoodOrderInfo에서 가져온 필드들
    public int Quantity { get; set; } = 1;
    public string SpecialRequest { get; set; } = null;
    public bool IsRecommended { get; set; } = false;

    public Food(
        int no, string recipeID_eng, string recipeName, int diffic, string description,
        List<string> keyCombination, int basePrice, List<string> tags, string category,
        List<string> requiredIngredientsVisual, string completedVisualResourceID, string openOption,
        int initialQuantity = 1, string initialSpecialRequest = null, bool initialIsRecommended = false)
    {
        this.NO = no;
        this.RecipeID_eng = recipeID_eng;
        this.RecipeName = recipeName;
        this.Diffic = diffic;
        this.Description = description;
        this.KeyCombination = keyCombination != null ? new List<string>(keyCombination) : new List<string>();
        this.BasePrice = basePrice;
        this.Tags = tags != null ? new List<string>(tags) : new List<string>();
        this.Category = category;
        this.RequiredIngredientsVisual = requiredIngredientsVisual != null ? new List<string>(requiredIngredientsVisual) : new List<string>();
        this.CompletedVisualResourceID = completedVisualResourceID;
        this.OpenOption = openOption;

        this.Quantity = initialQuantity;
        this.SpecialRequest = initialSpecialRequest;
        this.IsRecommended = initialIsRecommended;
    }

    // 복사 생성자
    public Food(Food other)
    {
        this.NO = other.NO;
        this.RecipeID_eng = other.RecipeID_eng;
        this.RecipeName = other.RecipeName;
        this.Diffic = other.Diffic;
        this.Description = other.Description;
        this.KeyCombination = new List<string>(other.KeyCombination);
        this.BasePrice = other.BasePrice;
        this.Tags = new List<string>(other.Tags);
        this.Category = other.Category;
        this.RequiredIngredientsVisual = new List<string>(other.RequiredIngredientsVisual);
        this.CompletedVisualResourceID = other.CompletedVisualResourceID;
        this.OpenOption = other.OpenOption;

        this.Quantity = other.Quantity;
        this.SpecialRequest = other.SpecialRequest;
        this.IsRecommended = other.IsRecommended;
    }

    // Clone 메서드
    public Food Clone()
    {
        return new Food(this);
    }

    public Sprite IconSprite
    {
        get
        {
            if (!string.IsNullOrEmpty(CompletedVisualResourceID))
                return Managers.Resource.Load<Sprite>(CompletedVisualResourceID);
            return null;
        }
    }

    public bool IsUnlocked
    {
        get
        {
            return OpenOption != null && !OpenOption.Equals("LOCKED", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}