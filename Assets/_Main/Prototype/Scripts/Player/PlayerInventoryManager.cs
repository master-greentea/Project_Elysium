using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance;
    private List<ItemBase> itemsList = new List<ItemBase>();

    private void Awake()
    {
        Instance = this;
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
