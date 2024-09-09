using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    public string objName;

    [TextArea]
    public string objDescription;

    public ItemType type = 0;

    public virtual bool Use(Monster monster)
    {
        return false;
    }

    public virtual MonsterBase GetRandomMonster()
    {
        return null;
    }
}
public enum ItemType
{
    Recuperación, Movimientos, Fósil
}
