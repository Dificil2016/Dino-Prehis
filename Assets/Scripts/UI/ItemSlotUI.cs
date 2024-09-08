using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    public int Index = 0;
    ItemBase item;
    [SerializeField] Text nameText;
    [SerializeField] Text countText;
    
    InventoryUI inventoryUI;

    public void SetData(itemSlot slot)
    {
        inventoryUI = GameController.Instance.InventoryUI;
        item = slot.Item;
        Index = slot.ItemIndex;
        nameText.text = slot.Item.objName;
        countText.text = $"x{slot.count}";
    }

    public void SendItemData()
    {
        GameController.Instance.SelectItem(Index);
        if (GameController.Instance.state == GameState.Bag) 
        {
            inventoryUI.SetItemInfo();
        }
        
    }
}