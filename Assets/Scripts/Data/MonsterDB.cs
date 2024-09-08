using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDB 
{
    public static Dictionary<string, MonsterBase> monsters;

    public static void Init()
    {
        monsters = new Dictionary<string, MonsterBase>();

        var MonsterArray = Resources.LoadAll<MonsterBase>("");
        foreach (var monster in MonsterArray)
        {
            if (monsters.ContainsKey(monster.monsterName))
            {
                Debug.LogError("Hay 2 mountruos con el mismo nombre");
                continue;
            }
            monsters[monster.monsterName] = monster;
        }
    }
}
