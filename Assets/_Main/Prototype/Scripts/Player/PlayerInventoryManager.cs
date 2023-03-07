using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    private List<ItemBase> itemsList = new List<ItemBase>();

    private void Awake()
    {
        
    }

    public void AddItem(ItemBase item)
    {
        if (item.canPickUp) itemsList.Add(item);
    }
    public void RemoveItem(ItemBase item)
    {
        itemsList.Remove(item);
    }
}
