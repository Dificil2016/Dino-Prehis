using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    Monster _monster;

    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.nameTag + $" {monster.status?.conditionIcon}";
        levelText.text = "Lvl " + monster.level;
        hpBar.SetHP((float) monster.hp / monster.maxHP);
    }
}
