using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Furniture", menuName = "Items/Create New Furniture")]
public class ItemFurniture : ItemBase
{
    public ItemFurniture()
    {
        itemType = ItemType.Furniture;
        canPickUp = false;
    }
}
