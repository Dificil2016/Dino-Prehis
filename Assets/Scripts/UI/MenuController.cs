using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] PartyMenu partyMenu;
    public void OpenMenu()
    {
        partyMenu = GameController.Instance.PartyMenu;
        menu.SetActive(true);
        partyMenu.gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
        partyMenu.gameObject.SetActive(false);
        GameController.Instance.InventoryUI.gameObject.SetActive(false);
    }
    public void SelectItem()
    { GameController.Instance.RunMenuOption(menuOptions.Items); }

    public void SelectSave()
    { GameController.Instance.RunMenuOption(menuOptions.Save); }

    public void SelectLoad()
    { GameController.Instance.RunMenuOption(menuOptions.Load); }
}

[System.Serializable]
public enum menuOptions
{
    Items,
    Save,
    Load,
}
