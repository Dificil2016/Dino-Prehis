using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelector : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    Inventory inventory;
    public ItemType selectedType = 0;

    List<int> itemsIndex;
    int itemsIndexPosition;
    public int selectedItemIndex;

    void Awake()
    {
        inventory = GameController.Instance.PlayerController.GetComponent<Inventory>();
    }

    void UpdateInventory()
    {
        inventory.SetItemIndex();

        itemsIndex = new List<int>();

        foreach (Transform child in itemList.transform)
        { Destroy(child.gameObject); }

        foreach (var itemSlot in inventory.Slots)
        {
            if (itemSlot.Item.type == selectedType)
            {
                var slot = Instantiate(itemSlotUI, itemList.transform);
                slot.SetData(itemSlot);

                itemsIndex.Add(slot.Index);
            }
        }

        itemsIndexPosition = 0;
        if (itemsIndex.Count > 0)
        {
            selectedItemIndex = itemsIndex[itemsIndexPosition];
        }
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameController.Instance.CloseItemSelection();
        }
    }

    public void OpenSelection(ItemType type)
    {
        gameObject.SetActive(true);

        selectedType = type;

        UpdateInventory();
    }

    public void CloseSelection()
    {
        gameObject.SetActive(false);
    }

    public ItemBase SendSelectedItem()
    {
        return inventory.Slots[selectedItemIndex].Item;
    }
}
