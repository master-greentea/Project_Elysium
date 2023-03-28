using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
[CreateAssetMenu(fileName = "New Key", menuName = "Items/Create New Key")]
public class ItemKey : ItemBase
{
    public string keyID;

    public ItemKey()
    {
        itemType = ItemType.Key;
    }
}
