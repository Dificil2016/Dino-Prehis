using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Dino-Prehis/ new move")]
public class Move : ScriptableObject
{
    [Header("Basic Info")]
    public string moveName;
    [TextArea] 
    public string moveDescription;

    public MonsterType type;
    public int power;
    public int cost;
    public int priority;
    public MoveCategory moveCategory;

    [Header("Effects")]
    public List<SecondaryEffects> secondaryEffects;

    public MoveSaveData GetSaveData()
    {
        var moveData = new MoveSaveData()
        {
            moveName = moveName
        };

        return moveData;
    }

}
public enum MoveCategory
{
    Physical,
    Special
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance { get { return chance; } }
    public MoveTarget Target { get { return target; } }
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> statBoost;
    [SerializeField] ConditionID statusEffect;

    public List<StatBoost> StatBoost { get { return statBoost; } }
    public ConditionID Status { get { return statusEffect; } }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveTarget
{
    Target,
    User,
    All
}

[Serializable]
public class MoveSaveData
{
    public string moveName;
}
