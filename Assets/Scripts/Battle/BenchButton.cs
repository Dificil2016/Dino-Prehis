using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BenchButton : MonoBehaviour
{
    public int monsterIndex;
    public Text text;
    public HPBar HPBar;

    public void SelectBenchMonster()
    {
        GameController.Instance.BattleSystem.SelectMonster(monsterIndex);
    }
}
