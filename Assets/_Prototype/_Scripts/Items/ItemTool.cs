using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ToolType
{
    Shoe, Cloth, Wrench, Lighter
}
[CreateAssetMenu(fileName = "New Tool", menuName = "Items/Create New Tool")]
public class ItemTool : ItemBase
{
    public ToolType ToolType;

    public ItemTool()
    {
        itemType = ItemType.Tool;
    }
}
