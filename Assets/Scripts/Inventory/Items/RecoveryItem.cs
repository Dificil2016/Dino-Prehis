using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dino-Prehis/ new Item / new recovery Item")]
public class RecoveryItem : ItemBase
{
    [Header ("HP Heal")]
    [SerializeField] int HPAmount;
    [SerializeField] bool RestoreMaxHP;
    
    [Header("Status Heal")]
    [SerializeField] ConditionID statusHeal;
    [SerializeField] bool recoverAllStatus;

    [Header("Use Condition")]
    [SerializeField] bool DefeatedUsable;

    public override bool Use(Monster monster)
    {
        if (DefeatedUsable && monster.hp > 0)
        {
            return false;
        }
        else if (!DefeatedUsable && monster.hp < 0)
        {
            return false;
        }
        else
        {
            if (monster.hp < monster.maxHP)
            {
                if (RestoreMaxHP)
                {
                    monster.hp = monster.maxHP;
                    return true;
                }
                else if (HPAmount > 0)
                {
                    monster.IncreaseHP(HPAmount);
                    return true;
                }
            }

            if (monster.status != null)
            {
                if (recoverAllStatus || statusHeal == monster.status.ID)
                {
                    monster.status = null;
                    return true;
                }
            }
        }

        return false;
    }
}
