using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    public string foodName;
    public int price;
    public List<string> recipe; // 간단히 string 리스트로
    public string description;

    public Food(string name, int price, List<string> recipe, string desc)
    {
        this.foodName = name;
        this.price = price;
        this.recipe = recipe;
        this.description = desc;
    }
} 