using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyMenu : MonoBehaviour
{
    public List<BattleHud> memberSlots;

    public List<Monster> playerParty;

    private void OnEnable()
    {
        playerParty = GameController.Instance.PlayerController.MonsterParty.Party;
        SetPartyData();
    }

    public void SetPartyData()
    {
        for (int i = 0; i < memberSlots.Count; i++)
        {
            if (i < playerParty.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(playerParty[i]);
            }
            else
            { memberSlots[i].gameObject.SetActive(false); }
        }
    }

    public void SelectMonster(int MonsterID)
    {
       GameController.Instance.SelectMonster(MonsterID);
    }
        
}
