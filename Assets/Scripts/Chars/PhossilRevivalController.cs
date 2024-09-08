using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PhossilRevivalController : MonoBehaviour, Interactables
{

    [SerializeField] Dialog dialog;
    [SerializeField] Character character;

    NPCState state;

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
                StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => { OpenItemSelectionMenu(); }));
            }
        }
    }

    public void OpenItemSelectionMenu()
    {
        state = NPCState.Idle;
        GameController.Instance.OpenItemSelection(ItemType.Fósil);
    }

    private void Update()
    {
        character.HandleUpdate();
    }
}
