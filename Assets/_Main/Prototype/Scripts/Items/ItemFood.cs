using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FoodType
{
    MRE, Snack, Soda, Water, Trash
}
[CreateAssetMenu(fileName = "New Food", menuName = "Items/Create New Food")]
public class ItemFood : ItemBase
{
    public FoodType FoodType;
    public float HealthRestore;

    public ItemFood()
    {
        itemType = ItemType.Food;
    }
}
