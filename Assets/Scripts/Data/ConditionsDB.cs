using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D.IK;

public class ConditionsDB
{
    public static Dictionary<ConditionID, Condition> Conditions = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                ID = ConditionID.psn,
                conditionName = "Veneno",
                conditionIcon = "[P]",
                startMessage = "Ha sido envenenado",
                OnAfterTurn = (Monster monster) =>
                {
                    monster.DecreaseHP(monster.maxHP/ 8);
                    monster.StatusChanges.Enqueue($"{monster.nameTag} sufre por el veneno");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                ID = ConditionID.brn,
                conditionName = "Quemadura",
                conditionIcon = "[B]",
                startMessage = "Ha sido quemado",
                OnAfterTurn = (Monster monster) =>
                {
                    monster.DecreaseHP(monster.maxHP/ 16);
                    monster.StatusChanges.Enqueue($"{monster.nameTag} sufre por quemaduras");
                }
            }
        },
        {
            ConditionID.cnf,
            new Condition()
            {
                ID = ConditionID.cnf,
                conditionName = "Confusión",
                conditionIcon = "[C]",
                startMessage = "Ha sido confundido",
                OnBeforeMove = (Monster monster) =>
                {
                    if (Random.Range(0, 6) <= 1)
                    {
                        monster.StatusChanges.Enqueue($"{monster.nameTag} está confuso y no puede atacar");
                        return false;
                    }
                    return true;
                }
            }
        }
    };

}

public enum ConditionID 
{
    none,
    psn,
    brn,
    slp,
    cnf,
    frz
}