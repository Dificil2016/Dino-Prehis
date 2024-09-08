using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterParty : MonoBehaviour
{
    public List<Monster> Party;
    private void Awake()
    {
        foreach (Monster monster in Party) 
        { monster.Init(); }
    }

    public Monster GetFirstMonster()
    {
        for (int i = 0; i < Party.Count; i++)
        {
            if (Party[i].hp > 0)
            { 
                return Party[i]; 
            }
        }
        return null;
    }

    public void AddMonster(ItemBase phosil)
    {
        List<string> lines = new List<string>() { "" };

        MonsterBase mBase = phosil.GetRandomMonster();

        List<MoveSaveData> moveSaveDatas = mBase.GenerateMoves(5).Select(m => m.GetSaveData()).ToList();

        var mon = new MonsterSaveData
        {
            baseName = mBase.monsterName,
            nameTag = mBase.monsterName,
            hp = mBase.GetMaxHPForLevel(1),
            level = 1,
            development = 1,
            exp = mBase.GetXPForLevel(1),
            status = null,
            moves = moveSaveDatas
        };
        Monster newMonster = new Monster(mon);
        
        lines[0] = ($"Has obtenido un {mBase.monsterName}");

        if (Party.Count < 4) 
        {
            lines.Add($"{newMonster.nameTag} se une al equipo");
            Party.Add(newMonster);
            newMonster.Init();
        }
        else 
        {
            lines.Add($"tu equipo ya está lleno");
            lines.Add($"{newMonster.nameTag} fue enviado al rancho");
        }
        Dialog dialog = new Dialog(lines);

        StartCoroutine( DialogManager.Instance.ShowDialog(dialog, () => { GameController.Instance.CloseItemSelection(); }));
        
    }
}
