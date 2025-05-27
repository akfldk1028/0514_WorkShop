using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    public string foodName;
    public int price;
    public int cost; // 원가
    public List<string> recipe;
    public string description;
    public Sprite icon;
    public float cookTime;
    public int difficulty;
    public bool isUnlocked;
    public List<string> tags;
    // ... 기타 정보

    public Food(string name, int price, int cost, List<string> recipe, string desc, float cookTime = 0, int difficulty = 1, bool isUnlocked = true)
    {
        this.foodName = name;
        this.price = price;
        this.cost = cost;
        this.recipe = recipe;
        this.description = desc;
        this.cookTime = cookTime;
        this.difficulty = difficulty;
        this.isUnlocked = isUnlocked;
        this.tags = new List<string>();
    }
}