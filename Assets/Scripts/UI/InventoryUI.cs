using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { Selection, MoveToForget, Busy}

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] PartyMenu partyMenu;
    [SerializeField] Text typeText;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    Inventory inventory;

    List<int> itemsIndex;
    int itemsIndexPosition;

    public ItemType selectedType = 0;
    public int selectedItemIndex;
    InventoryUIState state;


    public InventoryUIState State { get { return state; } }

    void Awake()
    {
        inventory = GameController.Instance.PlayerController.GetComponent<Inventory>();
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            changeBagType(1);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            changeBagType(-1);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if (itemsIndexPosition > 0)
            { itemsIndexPosition--;}

            selectedItemIndex = itemsIndex[itemsIndexPosition];
            SetItemInfo();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (itemsIndexPosition < itemsIndex.Count-1) 
            { itemsIndexPosition++;}

            selectedItemIndex = itemsIndex[itemsIndexPosition];
            SetItemInfo();
        }
    }


    public void changeBagType(int change)
    {
        selectedType += change;

        if (selectedType > ItemType.Fósil)
        { selectedType = 0; }
        else if (selectedType < 0)
        { selectedType = ItemType.Fósil; }

        UpdateInventory();
    }

    void UpdateInventory()
    {
        inventory.SetItemIndex();

        itemsIndex = new List<int>();

        typeText.text = selectedType.ToString();

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
        { selectedItemIndex = itemsIndex[itemsIndexPosition]; }
        else
        { selectedItemIndex = inventory.Slots.Count; }
        SetItemInfo();
    }

    public ItemBase SendSelectedItem()
    {
        return inventory.Slots[selectedItemIndex].Item;
    }

    public void OpenBag()
    {
        gameObject.SetActive(true);
        partyMenu.gameObject.SetActive(true);

        DialogManager.Instance.SetDialog("");

        state = InventoryUIState.Selection;
        UpdateInventory();
    }

    public void CloseBag()
    {
        gameObject.SetActive(false);
        DialogManager.Instance.CloseDialog();
        if(GameController.Instance.state == GameState.Battle)
        {
            partyMenu.gameObject.SetActive(false);
        }
    }

    public void SetItemInfo()
    {
        if (state == InventoryUIState.Selection)
        {
            DialogManager.Instance.SetDialog("");

            if (inventory.Slots.Count > selectedItemIndex)
            { DialogManager.Instance.SetDialog($"[{SendSelectedItem().objName}] {SendSelectedItem().objDescription}"); }
        }
    }

    public void SelectItem(int ItemIndex)
    {
        if (state == InventoryUIState.Selection)
        {
            selectedItemIndex = ItemIndex;
        }
    }

    public IEnumerator SelectMonster(Monster monster) 
    {
        if (state == InventoryUIState.Selection && inventory.Slots.Count > selectedItemIndex)
        {
            state = InventoryUIState.Busy;

            var item = SendSelectedItem();

            yield return HandleTmItems(monster);

            if (item.Use(monster) == true)
            {
                inventory.sustractItem(selectedItemIndex, 1);
                GameController.Instance.PartyMenu.SetPartyData();
                UpdateInventory();

                if (item is RecoveryItem)
                {
                    yield return DialogManager.Instance.TypeDialog($"{GameController.Instance.PlayerController.trainerName} ha usado {item.objName} en {monster.nameTag}");
                    yield return new WaitForSeconds(0.3f);
                }
            }
            else
            {
                yield return DialogManager.Instance.TypeDialog($"No ha tenido ningún efecto");
                yield return new WaitForSeconds(0.3f);
            }
            state = InventoryUIState.Selection;
            SetItemInfo();
        }
    }

    Move moveToLearn;
    Monster monster;

    IEnumerator HandleTmItems(Monster selectedMonster)
    {
        var item = SendSelectedItem() as TmItem;

        monster = selectedMonster;
        moveToLearn = item.Move;

        if (item == null)
        { yield break; }
        if (monster.HasMove(moveToLearn))
        {
            yield return DialogManager.Instance.TypeDialog($"{monster.nameTag} ya conoce {moveToLearn.moveName}");
            yield return new WaitForSeconds(0.3f);
        }
        else if (monster.Moves.Count < 4)
        {
            monster.LearnMove(moveToLearn);
            yield return DialogManager.Instance.TypeDialog($"{monster.nameTag} ha aprendido {moveToLearn.moveName}");
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return DialogManager.Instance.TypeDialog($"{monster.nameTag} intenta aprender {moveToLearn.moveName}");
            yield return new WaitForSeconds(0.3f);
            yield return DialogManager.Instance.TypeDialog($"pero {monster.nameTag} ya conoce 4 movimientos");
            yield return new WaitForSeconds(0.3f);
            yield return LearningMoveSelection(monster, moveToLearn);
            
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
        }
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator LearningMoveSelection(Monster monster, Move newMove)
    {
        state = InventoryUIState.MoveToForget;
        yield return DialogManager.Instance.TypeDialog("Elije que movimiento quieres olvidar");
        moveSelectionUI.SetMoveData(monster.Moves, newMove);
        moveSelectionUI.gameObject.SetActive(true);
    }

    public IEnumerator SelectLearningMove(int moveID)
    {
        moveSelectionUI.gameObject.SetActive(false);

        if (moveID < monster.Moves.Count)
        {
            yield return DialogManager.Instance.TypeDialog($"{monster.nameTag} olvidó {monster.Moves[moveID].moveName} para aprender {moveToLearn.moveName}");
            monster.Moves[moveID] = moveToLearn;
        }
        else
        { yield return DialogManager.Instance.TypeDialog($"{monster.nameTag} no ha aprendido {moveToLearn.moveName}"); }
        yield return new WaitForEndOfFrame();

        state = InventoryUIState.Selection;
    }

    public bool UseItemInBattle(Monster monster)
    {
        ItemBase item = SendSelectedItem();

        if (item.Use(monster) == true)
        {
            inventory.sustractItem(selectedItemIndex, 1);
            return true;
        }
        else
        {
            return false;
        }
    }
}
