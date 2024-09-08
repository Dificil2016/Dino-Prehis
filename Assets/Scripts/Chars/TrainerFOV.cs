using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFOV : MonoBehaviour, PlayerTrigger
{
    public void OnPlayerTrigger(PlayerController player)
    {
        GameController.Instance.OnTrainerBattle(GetComponentInParent<Collider2D>());
    }
}
