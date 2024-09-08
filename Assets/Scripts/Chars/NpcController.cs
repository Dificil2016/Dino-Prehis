using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState { Idle, Walking, Dialog }

public class NpcController : MonoBehaviour, Interactables
{
    [SerializeField] Dialog dialog;
    [SerializeField] Character character;
    [SerializeField] List<Vector2> MovePattern;
    [SerializeField] float WaitTime;

    NPCState state;
    float idleTimer = 0;
    int currentPaternStep = 0;

    void Start()
    {
        character.SetPositionAndSnap(transform.position);
    }

    public void Interact(Transform playerTransform)
    {
        if (state == NPCState.Idle)
        { 
            state = NPCState.Dialog;
            if (dialog.Lines.Count > 0)
            {
                character.LookTowards(playerTransform.position);
                StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => { state = NPCState.Idle; }));
            }
        }
    }

    private void Update()
    {
        if ( GameController.Instance.state == GameState.FreeRoaming)
        {
            if (state == NPCState.Idle)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer > WaitTime)
                {
                    idleTimer = 0;
                    if (MovePattern.Count > 0 && state != NPCState.Dialog)
                    { StartCoroutine(Walk()); }
                }
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(MovePattern[currentPaternStep]);

        if (oldPos != transform.position) 
        { currentPaternStep = (currentPaternStep + 1) % MovePattern.Count; }
        

        state = NPCState.Idle;
    }
}
