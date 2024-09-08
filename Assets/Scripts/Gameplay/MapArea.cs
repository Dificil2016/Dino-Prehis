using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<MonsterBase> WildMonsters;
    [SerializeField] Vector2 levelRange;
    [SerializeField] Vector2 developmentRange;
    [SerializeField] MonsterParty wildParty;

    public MonsterParty GetRandomMonsterParty(int count)
    {
        wildParty.Party = new List<Monster>();
        for (int i = 0; i < count; i++)
        {
            var wildMonsterBase = WildMonsters[Random.Range(0, WildMonsters.Count)];
            int level = (int)UnityEngine.Random.Range(levelRange.x, levelRange.y);
            int development = (int)UnityEngine.Random.Range(developmentRange.x, developmentRange.y);

            var monsterData = new MonsterSaveData()
            {
                baseName = wildMonsterBase.monsterName,
                nameTag = wildMonsterBase.monsterName,
                hp = wildMonsterBase.GetMaxHPForLevel(level),
                level = level,
                development = development,
                exp = wildMonsterBase.GetXPForLevel(level),
                status = null,
                moves = wildMonsterBase.GenerateMoves(level).Select(m => m.GetSaveData()).ToList()
            };
            var wildMonster = new Monster(monsterData);

            wildParty.Party.Add(wildMonster);
        }
        return wildParty;
    }
}
