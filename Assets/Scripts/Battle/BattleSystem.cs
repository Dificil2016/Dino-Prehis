using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, MonsterSelection, ItemSelection, LearningMoveSelection, RunningTurn, Busy, BattleOver }
public enum BattleAction { Attack, Defense, Move, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;

    [SerializeField] BattleUnit enemyUnit;

    [SerializeField] DialogBox dialogBox;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    [SerializeField] Image playerPortrait;
    [SerializeField] Image enemyPortrait;
    [SerializeField] GameObject TrainersHud;
    [SerializeField] InventoryUI inventoryUI;

    [SerializeField] PlayerController player;
    TrainerController trainer;

    Move moveToLearn;

    public event Action<bool> onBattleOver;

    BattleState state;
    MonsterParty playerParty;
    MonsterParty enemyParty;

    bool isTrainerBattle = false;

    public void StartBattle(MonsterParty _playerParty, MonsterParty _enemyParty, bool isTrainerBattle)
    {
        playerParty = _playerParty;
        enemyParty = _enemyParty;
        inventoryUI = GameController.Instance.InventoryUI;
        this.isTrainerBattle = isTrainerBattle;

        player = playerParty.GetComponent<PlayerController>();
        trainer = enemyParty.GetComponent<TrainerController>();
        
        StartCoroutine(SetupBattle());
    }

    public void HandleUpdate()
    {
        if (state == BattleState.MoveSelection || state == BattleState.MonsterSelection && playerUnit.monster.hp > 0)
        {
            if (playerUnit.monster.hp > 0)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ActionSelection();
                }
            }
        }
        if (state == BattleState.ItemSelection)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                inventoryUI.CloseBag();
                state = BattleState.ActionSelection;
                ActionSelection();
            }
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log($"Estado: {state}");
        }
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (isTrainerBattle) 
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            TrainersHud.SetActive(true);
            playerPortrait.sprite = player.trainerPortrait;
            enemyPortrait.sprite = trainer.trainerPortrait;
            
            yield return dialogBox.TypeDialog($"{trainer.trainerName} quiere luchar");
            yield return new WaitForSeconds(0.5f);

            TrainersHud.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            enemyUnit.gameObject.SetActive(true);

            playerUnit.SetUp(playerParty.GetFirstMonster());
            playerUnit.battleHud.SetData(playerUnit.monster);

            enemyUnit.SetUp(enemyParty.GetFirstMonster());
            enemyUnit.battleHud.SetData(enemyUnit.monster);
            yield return dialogBox.TypeDialog($"{trainer.trainerName} envía a {enemyUnit.monster.nameTag}");
        }
        else
        {
            playerUnit.SetUp(playerParty.GetFirstMonster());
            playerUnit.battleHud.SetData(playerUnit.monster);

            enemyUnit.SetUp(enemyParty.GetFirstMonster());
            enemyUnit.battleHud.SetData(enemyUnit.monster);
            yield return dialogBox.TypeDialog($"un {enemyUnit.monster.nameTag} salvaje te corta el paso");
        }
        

        dialogBox.SetMoveNames(playerUnit.monster.Moves);
        ActionSelection();
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableBenchSelector(false);
        GameController.Instance.PartyMenu.gameObject.SetActive(false);
        dialogBox.EnableDialogText(true);
        dialogBox.SetDialog($"¿Qué va a hacer {playerUnit.monster.nameTag}?");
        dialogBox.EnableActionSelector(true);
    }

    void MoveSelection()
    { 
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);

        dialogBox.ResetMoveDetails();
    }

    IEnumerator OpenBenchScreen()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelector(false);

        bool ChangePosible = dialogBox.SetBenchDetails(playerParty.Party, playerUnit.monster);
        if (ChangePosible)
        {
            yield return dialogBox.TypeDialog($"¿Por quién va a cambiar {playerUnit.monster.nameTag}?");
            dialogBox.EnableBenchSelector(true);
            state = BattleState.MonsterSelection;
        }
        else
        {
            yield return dialogBox.TypeDialog($"No hay por quién cambiar");
            ActionSelection();
        }
    }

    IEnumerator RunTurns(BattleAction playerAction, int ID)
    {
        dialogBox.EnableActionSelector(false);
        state = BattleState.RunningTurn;

        enemyUnit.monster.currentMove = enemyUnit.monster.GetRandomMove();
        if (playerAction == BattleAction.Move)
        {
            playerUnit.monster.currentMove = playerUnit.monster.Moves[ID];

            bool playerGoesFirst = true;

            int playerMovePriority = playerUnit.monster.currentMove.priority;
            int enemyMovePriority = enemyUnit.monster.currentMove.priority;

            if (playerMovePriority == enemyMovePriority) 
            { playerGoesFirst = playerUnit.monster.Speed > enemyUnit.monster.Speed; }
            else
            { playerGoesFirst = playerMovePriority > enemyMovePriority; }

            
            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            yield return RunMove(firstUnit, secondUnit, firstUnit.monster.currentMove);
            yield return RunAfterTurn(firstUnit, secondUnit);
            yield return new WaitForSeconds(0.2f);

            yield return RunMove(secondUnit, firstUnit, secondUnit.monster.currentMove);
            yield return RunAfterTurn(secondUnit, firstUnit);
            yield return new WaitForSeconds(0.2f);
        }
        else if (playerAction == BattleAction.Run)
        {
            yield return Fly();

            yield return RunMove(enemyUnit, playerUnit, enemyUnit.monster.currentMove);
            yield return RunAfterTurn(enemyUnit, playerUnit);
            yield return new WaitForSeconds(0.2f);
        }
        else if (playerAction == BattleAction.UseItem)
        {
            Monster selectedMonster = playerParty.Party[ID];

            if (inventoryUI.UseItemInBattle(selectedMonster)) 
            {
                yield return dialogBox.TypeDialog($"{player.trainerName} ha usado {inventoryUI.SendSelectedItem().objName} sobre {selectedMonster.nameTag}");
            }
            else
            {
                yield return dialogBox.TypeDialog($"{player.trainerName} intenta usar {inventoryUI.SendSelectedItem().objName} sobre {selectedMonster.nameTag}");
                yield return dialogBox.TypeDialog($"Pero no ha surtido efecto");
            }

            playerUnit.battleHud.SetData(playerUnit.monster);
            enemyUnit.battleHud.SetData(enemyUnit.monster);

            yield return RunMove(enemyUnit, playerUnit, enemyUnit.monster.currentMove);
            yield return RunAfterTurn(enemyUnit, playerUnit);
            yield return new WaitForSeconds(0.2f);
        }

        if (enemyUnit.monster.hp > 0)
        {
            enemyUnit.monster.OnAfterTurn();
            yield return ShowStatusChanges(enemyUnit);
            yield return CheckMonsterDeath(enemyUnit);
        }
        if (playerUnit.monster.hp > 0)
        {
            playerUnit.monster.OnAfterTurn();
            yield return ShowStatusChanges(playerUnit);
            yield return CheckMonsterDeath(playerUnit);
        }

        if (enemyUnit.monster.hp <= 0) { yield return CheckForBattleOver(enemyUnit); }
        if (playerUnit.monster.hp <= 0) { yield return CheckForBattleOver(playerUnit); }
        else
        { ActionSelection(); }
    }

    IEnumerator RunMove(BattleUnit userUnit, BattleUnit targetUnit, Move move)
    {

        if (userUnit.monster.hp <= 0)
        {  yield break; }

        state = BattleState.RunningTurn;
        yield return dialogBox.TypeDialog($"{userUnit.monster.nameTag} ha usado {move.moveName}");

        if(userUnit.monster.OnBeforeMove() == true)
        {
            DamageDetails damageDetails;

            if (CheckMoveHits(userUnit.monster, targetUnit.monster) == true && targetUnit.monster.hp > 0)
            {
                //if the attack deals damage
                if (move.power > 0)
                {
                    targetUnit.PlayHitAnimation(move);
                    damageDetails = targetUnit.monster.TakeDamage(move, userUnit.monster);
                    targetUnit.battleHud.SetData(targetUnit.monster);
                    yield return ShowDamageDetails(damageDetails);
                }
                //if the attack deals a secondary effect
                if (move.secondaryEffects != null && move.secondaryEffects.Count > 0 && targetUnit.monster.hp > 0)
                {
                    foreach (var secondary in move.secondaryEffects) 
                    {
                        var rng = UnityEngine.Random.Range(1, 101);
                        if (rng <= secondary.Chance)
                        { yield return RunMoveEffects(userUnit.monster, targetUnit.monster, secondary, secondary.Target); }
                    }
                }
                yield return CheckMonsterDeath(userUnit);
                yield return CheckMonsterDeath(targetUnit);
            }
            else
            {
                yield return dialogBox.TypeDialog($"el ataque ha fallado");
            }
        }
        
    }

    IEnumerator RunAfterTurn(BattleUnit userUnit, BattleUnit targetUnit)
    {
        yield return ShowStatusChanges(userUnit);
        yield return ShowStatusChanges(targetUnit);
    }

    IEnumerator CheckMonsterDeath(BattleUnit unit)
    {
        if (unit.monster.hp <= 0)
        {
            unit.PlayFaintAnimation();
            yield return dialogBox.TypeDialog($"{unit.monster.nameTag} ha sido derrotado");

            if (!unit.isPlayerUnit)
            {
                int exp = unit.monster.monsterBase.xpYield;
                int level = unit.monster.level;
                float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

                int xpGain = Mathf.FloorToInt((exp * level * trainerBonus) / 7);

                playerUnit.monster.XP += xpGain;
                yield return dialogBox.TypeDialog($"{playerUnit.monster.nameTag} recibe {xpGain} puntos de experiencia");

                while (playerUnit.monster.LevelUp())
                {
                    playerUnit.battleHud.SetData(playerUnit.monster);
                    yield return dialogBox.TypeDialog($"{playerUnit.monster.nameTag} ha subido a nivel {playerUnit.monster.level}");

                    var newMove = playerUnit.monster.GetLearnableMove();

                    if (newMove != null)
                    {
                        moveToLearn = newMove.moveBase;

                        if (playerUnit.monster.Moves.Count < 4)
                        {
                            playerUnit.monster.Moves.Add(newMove.moveBase);
                            yield return dialogBox.TypeDialog($"{playerUnit.monster.nameTag} ha aprendido {newMove.moveBase.moveName}");
                            dialogBox.SetMoveNames(playerUnit.monster.Moves);
                        }
                        else
                        {
                            yield return dialogBox.TypeDialog($"{playerUnit.monster.nameTag} intenta aprender {newMove.moveBase.moveName}");
                            yield return dialogBox.TypeDialog($"pero {playerUnit.monster.nameTag} ya conoce 4 movimientos");
                            yield return LearningMoveSelection(playerUnit.monster, newMove.moveBase);
                            yield return new WaitUntil(() => state != BattleState.LearningMoveSelection);
                        }
                    }
                }

                if (playerUnit.monster.DevelopUp(1))
                {
                    yield return dialogBox.TypeDialog($"{playerUnit.monster.nameTag} está cambiando...");
                    yield return playerUnit.PlayGrowAnimation();
                    yield return dialogBox.TypeDialog($"{playerUnit.monster.nameTag} ha alcanzado su etapa adulta");
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    bool CheckMoveHits(Monster user, Monster target)
    {
        int precision = user._StatBost[Stat.Precisión];
        int evasion = target._StatBost[Stat.Evasión];
        float accuracy = 100;

        //Esto funciona como el culo y siempre da 100/100, arreglar más tarde

        if (precision > 0) 
        { accuracy += precision * 10; }
        else if (evasion < 0) { accuracy -= precision*10; }

        if (evasion > 0) { accuracy -= evasion*10; }
        else if (evasion < 0) { accuracy += evasion*10; }

        // Debug.Log($"Precisión: {accuracy} / 100. Precisión del usario: {precision}");

        return UnityEngine.Random.Range(1,101) <= accuracy;
    }

    IEnumerator RunMoveEffects(Monster sourceMonster, Monster targetMonster, MoveEffects effects, MoveTarget target)
    {
        //Stat Boost
        if (target == MoveTarget.User || target == MoveTarget.All)
        { sourceMonster.ApplyBoost(effects.StatBoost); }
        if (target == MoveTarget.Target || target == MoveTarget.All)
        { targetMonster.ApplyBoost(effects.StatBoost); }

        //Status Effect
        if (effects.Status != ConditionID.none)
        {
            if (target == MoveTarget.User || target == MoveTarget.All)
            { sourceMonster.SetStatus(effects.Status); }
            if (target == MoveTarget.Target || target == MoveTarget.All)
            { targetMonster.SetStatus(effects.Status); }
        }

        yield break;
    }

    IEnumerator ShowStatusChanges(BattleUnit unit)
    {
        unit.battleHud.SetData(unit.monster);
        while (unit.monster.StatusChanges.Count > 0)
        {
            unit.PlayStatusAnimation();
            var message = unit.monster.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void BattleOver(bool won)
    { 
        state = BattleState.BattleOver;
        playerParty.Party.ForEach(m => m.OnBattleOver());
        enemyParty.Party.ForEach(m => m.OnBattleOver());
        onBattleOver(won);
    }

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.isPlayerUnit == true)
        {
            Monster m = playerParty.GetFirstMonster();
            if (m != null)
            {
                yield return SwitchPlayerMonster(playerUnit, m);
            }
            else
            { BattleOver(false); }
        }
        else
        {
            Monster m = enemyParty.GetFirstMonster();
            if (m != null)
            {
                yield return SwitchEnemyMonster(enemyUnit, m);
            }
            else
            { BattleOver(true); }
        }
        
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Damage > 0)
        {
            if (damageDetails.TypeEffect > 1)
            {
                yield return dialogBox.TypeDialog($"Golpeó su punto débil");
            }
            else if (damageDetails.TypeEffect < 1)
            {
                yield return dialogBox.TypeDialog($"No es muy efectivo");
            }
            if (damageDetails.Critical > 1)
            {
                yield return dialogBox.TypeDialog($"Ha sido un golpe crítico");
            }
        } 
    }

    IEnumerator LearningMoveSelection(Monster monster, Move newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog("Elije que movimiento quieres olvidar");
        moveSelectionUI.SetMoveData(monster.Moves, newMove);
        moveSelectionUI.gameObject.SetActive(true);
        state = BattleState.LearningMoveSelection;
    }

    IEnumerator SwitchEnemyMonster(BattleUnit targetUnit, Monster m)
    {
        if (targetUnit.monster.hp > 0)
        {
            GameController.Instance.PartyMenu.gameObject.SetActive(false);
            dialogBox.EnableActionSelector(false);
            targetUnit.PlayExitAnimation();
            yield return dialogBox.TypeDialog($"{targetUnit.monster.nameTag} se retira");
        }

        targetUnit.monster = m;
        targetUnit.SetUp(m);
        dialogBox.SetMoveNames(playerUnit.monster.Moves);
        targetUnit.battleHud.SetData(targetUnit.monster);
        yield return dialogBox.TypeDialog($"le toca a {targetUnit.monster.nameTag}");

    }

    IEnumerator SwitchPlayerMonster(BattleUnit targetUnit, Monster m)
    {
        if (targetUnit.monster.hp > 0)
        {
            GameController.Instance.PartyMenu.gameObject.SetActive(false);
            dialogBox.EnableActionSelector(false);
            targetUnit.PlayExitAnimation();
            yield return dialogBox.TypeDialog($"{targetUnit.monster.nameTag} se retira");
        }

        targetUnit.monster = m;
        targetUnit.SetUp(m);
        dialogBox.SetMoveNames(playerUnit.monster.Moves);
        targetUnit.battleHud.SetData(targetUnit.monster);
        yield return dialogBox.TypeDialog($"le toca a {targetUnit.monster.nameTag}");

        ActionSelection();
    }

    IEnumerator LearnMove(int moveID)
    {
        moveSelectionUI.gameObject.SetActive(false);

        if (moveID < playerUnit.monster.Moves.Count)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.monster.nameTag} olvidó {playerUnit.monster.Moves[moveID].moveName} para aprender {moveToLearn.moveName}");
            playerUnit.monster.Moves[moveID] = moveToLearn;
        }
        else
        { yield return dialogBox.TypeDialog($"{playerUnit.monster.nameTag} no ha aprendido {moveToLearn.moveName}"); }
        yield return new WaitForEndOfFrame();

        state = BattleState.RunningTurn;
    }

    public IEnumerator SelectMonsterInBag(int selectedMonsterID)
    {
        var selectedMonster = GameController.Instance.PlayerController.MonsterParty.Party[selectedMonsterID];
        var monsterID = IsMonsterInBattle(selectedMonster);
        Debug.Log($"XAS; {monsterID}");

        if (monsterID < playerParty.Party.Count) 
        {
            inventoryUI.CloseBag();
            yield return RunTurns(BattleAction.UseItem, monsterID);
        }
        else
        {
            yield return DialogManager.Instance.TypeDialog($"{selectedMonster.nameTag} no está en batalla");
        }
    }

    public int IsMonsterInBattle(Monster monster) 
    {
        for (int i = 0; i < playerParty.Party.Count; i++)
        {
            if (playerParty.Party[i] == monster)
            {
                return i;
            }
        }

        return playerParty.Party.Count + 1;
    }

    public void highlightMove(int moveID)
    {
        if (state == BattleState.MoveSelection )
        {
            if (moveID < playerUnit.monster.Moves.Count)
            { dialogBox.SetMoveDetails(playerUnit.monster.Moves[moveID]); }
            else
            { dialogBox.ResetMoveDetails(); }
        }
    }

    public void SelectFight()
    {
        if (state == BattleState.ActionSelection)
        {
            MoveSelection();
        }
        else
        { Debug.LogWarning("La función de luchar fue llamada fuera de la selección de acción"); }
    }

    public void SelectBag()
    {
        if (state == BattleState.ActionSelection)
        {
            state = BattleState.ItemSelection;
            inventoryUI.OpenBag();
        }
        else
        { Debug.LogWarning("La función de bolsa fue llamada fuera de la selección de acción"); }
    }


    public void SelectParty()
    {
        if (state == BattleState.ActionSelection)
        {
            StartCoroutine(OpenBenchScreen());
        }
        else
        { Debug.LogWarning("La función de equipo fue llamada fuera de la selección de acción"); }
    }

    public void SelectLearningMove(int moveID)
    {
        if (state == BattleState.LearningMoveSelection)
        {
            StartCoroutine(LearnMove(moveID));
        }
        else
        { Debug.LogWarning("La función aprender movimiento fue llamada fuera de la acción de aprender movimiento"); }
    }

    public void SelectFly()
    {
        StartCoroutine(RunTurns(BattleAction.Run, 0));
    }

    IEnumerator Fly()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelector(false);
        if(isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("No puedes huir de una pelea entre entrenadores");
            state = BattleState.RunningTurn;
        }
        else
        {
            yield return dialogBox.TypeDialog("El equipo huye de la pelea");
            BattleOver(true);
        }
    }

    public void SelectMove(int moveID)
    {
        if (state == BattleState.MoveSelection)
        {
            if(moveID < playerUnit.monster.Moves.Count)
            {
                // seleccionado movimiento "moveID"
                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(RunTurns(BattleAction.Move, moveID));
            }
            else 
            { dialogBox.ResetMoveDetails();}
        }
        else
        { Debug.LogWarning("Un movimiento fue llamado fuera de la selección de movimiento"); }
    }

    public void SelectMonster(int monsterID)
    {

        if (playerParty.Party[monsterID].hp <= 0)
        { return; }

        dialogBox.EnableBenchSelector(false);
        GameController.Instance.PartyMenu.gameObject.SetActive(false);
        StartCoroutine(SwitchPlayerMonster(playerUnit, playerParty.Party[monsterID]));
    }

}
