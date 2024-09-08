using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(menuName = "Dino-Prehis/ new Item / new phosil")]
public class PhosilItem : ItemBase
{
    public List<MonsterBase> monsters;
    public override bool Use(Monster monster)
    { return false; }

    public override MonsterBase GetRandomMonster()
    {
        int index = Random.Range(0, monsters.Count);

        return monsters[index];
    }
}

