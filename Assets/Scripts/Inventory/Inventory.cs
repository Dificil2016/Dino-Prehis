using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<itemSlot> slots;

    public List<itemSlot> Slots { get { return slots; } }

    public void SetItemIndex()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetIndex(i);
        }
    }

    public void sustractItem(int Index, int sutractCount)
    {
        slots[Index].count -= sutractCount;

        if (slots[Index].count <= 0)
        {
            Slots.RemoveAt(Index);
        }
    }

    public void AddItem(ItemBase item, int addCount)
    {
        bool newItem = true;

        foreach (var slot in slots)
        {
            if (slot.Item == item)
            {
                slot.count += addCount;
                newItem = false;
            }
        }

        if (newItem)
        {
            itemSlot itemSlot = new itemSlot();
            itemSlot.SetItemSlotData(item, addCount);

            slots.Add(itemSlot);

            SetItemIndex();
        }
    }
}

[Serializable]
public class itemSlot
{
    [SerializeField] ItemBase item;
    public int count;

    int itemIndex;

    public ItemBase Item { get { return item; } }
    public int ItemIndex { get { return itemIndex; } }

    public void SetItemSlotData(ItemBase item, int count)
    {
        this.item = item;
        this.count = count;
    }

    public void SetIndex(int Index)
    { this.itemIndex = Index; }
    
}
    
