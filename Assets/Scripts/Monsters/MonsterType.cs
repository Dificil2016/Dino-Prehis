using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Type", menuName = "Dino-Prehis/ new type")]
public class MonsterType : ScriptableObject
{
    public string typeName;

    public Sprite typeIcon;
    public Color typeColor;
    public List<MonsterType> weakness;
    public List<MonsterType> resistences;
}

public class TypeChart
{
    public static float GetEffectiveness(MonsterType attType, MonsterType defType)
    {
        if (attType == null || defType == null)
        { return 1; }

        foreach (var weakness in defType.weakness) 
        { 
            if (attType == weakness)
            { return 2; }
        }
        foreach (var resistence in defType.resistences)
        {
            if (attType == resistence) 
            { return 0.5f; }
        }
        return 1;
    }
}
