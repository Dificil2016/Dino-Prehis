using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState { FreeRoaming, Battle, Dialog, ItemSelection, Menu, Bag, TrainerTrigger, Pause}
public class GameController : MonoBehaviour
{
    public GameState state {  get; private set; }

    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera mainCamera;
    [SerializeField] MapArea mapArea;
    [SerializeField] MenuController menuController;
    [SerializeField] PartyMenu partyMenu;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ItemSelector itemSelector;

    public BattleSystem BattleSystem { get {  return battleSystem; }  }
    public PartyMenu PartyMenu { get { return partyMenu; } }

    public InventoryUI InventoryUI { get { return inventoryUI; } }

    GameState prevState;

    public SceneDetails currentScene;
    public SceneDetails prevScene;

    public PlayerController PlayerController { get { return playerController; } }

    TrainerController trainer;

    public static GameController Instance { get; private set; }
    private void Awake()
    {
        menuController = GetComponent<MenuController>();
        Instance = this;
        MonsterDB.Init();
        MoveDB.Init();
    }

    private void Start()
    {
        battleSystem.onBattleOver += EndBattle;
        partyMenu.SetPartyData();
    }

    public void OnTrainerBattle(Collider2D collider)
    {
        var trainer = collider.GetComponentInParent<TrainerController>();

        if (trainer != null && trainer.hasBeenFought == false)
        {
            menuController.CloseMenu();

            state = GameState.TrainerTrigger;
            playerController.StopMovement(collider.transform);
            StartCoroutine(trainer.TriggerTrainerBattle(playerController));
        }
    }

    public void SetMapArea()
    {
        MapArea newMapArea = currentScene.GetComponent<MapArea>();
        if (newMapArea != null) 
        { mapArea = newMapArea; }
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        { 
            if (state != GameState.Pause)
            { prevState = state; }
            
            state = GameState.Pause; 
        }
        else
        { 
            state = prevState;
        }
    }

    public void StartBattle()
    {
        playerController.StopMovement();

        var playerParty = playerController.GetComponent<MonsterParty>();

        Monster m = playerParty.GetFirstMonster();
        if (m != null)
        {
            var wildParty = mapArea.GetRandomMonsterParty(Random.Range(1,4));
            trainer = null;
            state = GameState.Battle;
            battleSystem.gameObject.SetActive(true);
            mainCamera.gameObject.SetActive(false);

            battleSystem.StartBattle(SetBattleParty(playerParty), wildParty, false);
        }
    }

    //Provisional, sustituir con la selección de miembros de batalla
    MonsterParty SetBattleParty(MonsterParty monsterParty)
    {
        var battleParty = monsterParty;

        for (int i = 3; i < monsterParty.Party.Count; i++)
        {
            battleParty.Party.RemoveAt(i);
        }
        return battleParty;
    }

    public void StartTarinerBattle(TrainerController trainer)
    {
        if (state != GameState.Battle)
        {
            playerController.StopMovement();

            var playerParty = playerController.GetComponent<MonsterParty>();
            var enemyParty = trainer.GetComponent<MonsterParty>();
            Monster m = playerParty.GetFirstMonster();
            if (m != null)
            {
                this.trainer = trainer;
                state = GameState.Battle;
                battleSystem.gameObject.SetActive(true);
                mainCamera.gameObject.SetActive(false);
                battleSystem.StartBattle(SetBattleParty(playerParty), enemyParty, true);
            }
        }
    }

    private void EndBattle(bool playerWon)
    {
        state = GameState.FreeRoaming;
        battleSystem.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        if (trainer != null && playerWon == true)
        {
            trainer.BattleLost();
            trainer = null;
        }
    }

    public void ShowDialog()
    {
        playerController.StopMovement();
        if (state != GameState.Battle) 
        {
            state = GameState.Dialog;
        }
    }
    public void CloseDialog()
    {
        if (state == GameState.Dialog)
        { state = GameState.FreeRoaming; }
    }

    public void CloseItemSelection()
    {
        if (state == GameState.ItemSelection)
        { state = GameState.FreeRoaming; }
        itemSelector.CloseSelection();
    }

    private void Update()
    {
        if (state == GameState.FreeRoaming)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            { 
                menuController.OpenMenu();
                playerController.StopMovement();
                state = GameState.Menu;
            }

            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle) 
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Bag)
        {
            inventoryUI.HandleUpdate();
        }
        else if (state == GameState.ItemSelection)
        {
            itemSelector.HandleUpdate();
        }

        if (state == GameState.Menu)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                menuController.CloseMenu();
                state = GameState.FreeRoaming;
            }
        }

        if (state == GameState.Bag)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (inventoryUI.State == InventoryUIState.Selection)
                {
                    inventoryUI.CloseBag();
                    state = GameState.Menu;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"Estado: {state}");
        }
    }

    public void RunMenuOption(menuOptions option)
    {
        if (state == GameState.Menu && playerController.Character.isMoving == false)
        {
            if (option == menuOptions.Save)
            {
                SavingSystem.i.Save("SaveSlot_1");
            }
            else if (option == menuOptions.Load)
            {
                SavingSystem.i.Load("SaveSlot_1");
            }
            else if (option == menuOptions.Items)
            {
                inventoryUI.OpenBag();
                
                state = GameState.Bag;
            }
        }
    }

    public void OpenItemSelection(ItemType type)
    {
        state = GameState.ItemSelection;
        itemSelector.OpenSelection(type);
    }

    public void SelectMonster(int monsterID)
    {
        if (state == GameState.Bag)
        {
            StartCoroutine(inventoryUI.SelectMonster(playerController.MonsterParty.Party[monsterID]));
        }
        else if (state == GameState.Battle)
        {
            StartCoroutine(battleSystem.SelectMonsterInBag(monsterID));
        }
    }

    public void SelectItem(int itemIndex) 
    {
        if (state == GameState.Bag || state == GameState.Battle)
        {
            inventoryUI.SelectItem(itemIndex);
        }
        else if (state == GameState.ItemSelection)
        {
            var phosil = itemSelector.SendSelectedItem();
            playerController.Inventory.sustractItem(itemIndex, 1);
            itemSelector.CloseSelection();
            playerController.MonsterParty.AddMonster(phosil);
        }
    }

    public void SetCurrentScene(SceneDetails newScene)
    {
        prevScene = currentScene;
        currentScene = newScene;
    }
    
}
