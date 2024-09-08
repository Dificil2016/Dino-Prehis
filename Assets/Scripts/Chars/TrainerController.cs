using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class TrainerController : MonoBehaviour, Interactables, ISavable
{
    public string trainerName;
    public Sprite trainerPortrait;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject FOV;
    [SerializeField] Character character;
    public bool hasBeenFought = false;
    [SerializeField] Dialog battleDialog;
    [SerializeField] Dialog postBattleDialog;

    NPCState state = NPCState.Idle;

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        if (hasBeenFought)
        { yield break; }

        state = NPCState.Walking;
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        exclamation.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2 (Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);
        state = NPCState.Idle;

        Interact(player.transform);

    }

    private void Start()
    {
        character.SetPositionAndSnap(transform.position);
        SetFOVrotation(character.animator.lookDir);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public void Interact(Transform playerTransform)
    {
        if (state == NPCState.Idle)
        {
            if (hasBeenFought) 
            {
                state = NPCState.Dialog;
                if (postBattleDialog.Lines.Count > 0)
                {
                    character.LookTowards(playerTransform.position);
                    StartCoroutine(DialogManager.Instance.ShowDialog(postBattleDialog, () => { state = NPCState.Idle; }));
                }
            }
            else
            {
                state = NPCState.Dialog;
                if (battleDialog.Lines.Count > 0)
                {
                    character.LookTowards(playerTransform.position);
                    StartCoroutine(DialogManager.Instance.ShowDialog(battleDialog, () =>
                    {
                        state = NPCState.Idle;
                        GameController.Instance.StartTarinerBattle(this);
                    }));
                }
            }
            
        }
    }

    public void BattleLost()
    {
        hasBeenFought = true;
        FOV.gameObject.SetActive(false);

        state = NPCState.Dialog;
        if (postBattleDialog.Lines.Count > 0)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(postBattleDialog, () => { state = NPCState.Idle; }));
        }
    }

    public void SetFOVrotation(FaceDir faceDir)
    {
        float angle = 0f;
        if (faceDir == FaceDir.up)
        { angle = 180; }
        else if (faceDir == FaceDir.left)
        { angle = 270; }
        else if (faceDir == FaceDir.right)
        { angle = 90; }

        FOV.transform.eulerAngles = new Vector3 (0f, 0f, angle);
    }

    public object CaptureState()
    {
        return hasBeenFought;
    }

    public void RestoreState(object state)
    {
        hasBeenFought = (bool)state;

        if (hasBeenFought) 
        { FOV.gameObject.SetActive(false); }
    }
}
