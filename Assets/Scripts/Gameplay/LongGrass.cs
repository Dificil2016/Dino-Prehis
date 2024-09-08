using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, PlayerTrigger
{
    public void OnPlayerTrigger(PlayerController player)
    {
        if (UnityEngine.Random.Range(0, 21) < 1)
        {
            player.StopMovement();
            GameController.Instance.StartBattle();
        }
    }
}
