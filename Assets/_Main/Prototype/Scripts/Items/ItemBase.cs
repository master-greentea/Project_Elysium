using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ItemType
{
    Food, Tool, Furniture, Key
}

public class ItemBase : ScriptableObject
{
    protected ItemType itemType;
    public bool canPickUp { get; protected set; } = true;

    public string Name;
    public Sprite Icon;

    private bool _isInteractable;
}
