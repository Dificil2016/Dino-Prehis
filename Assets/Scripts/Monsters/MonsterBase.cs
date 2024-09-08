using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Dino-Prehis/ new monster")]
public class MonsterBase : ScriptableObject
{
    [Header("Name")]
    public string monsterName;
    [TextArea]
    public string monsterDescription;

    [Header("Sprites")]
    public Sprite allySprite;
    public Sprite allySpriteBaby;
    public Sprite enemySprite;
    public Sprite enemySpriteBaby;

    [Header("Stats")]
    public MonsterType Type1;
    public MonsterType Type2;
    public int maxHP;
    public int attack;
    public int defense;
    public int dexterity;
    public int speed;

    public int xpYield;

    [Header("Development")]
    public int maxDevelopment;
    public float developmentSpeed;

    public List<LearnableMove> learnableMoves;

    public int GetXPForLevel(int level)
    { return level * level * level; }

    public List<Move> GenerateMoves(int level)
    {
        var moves = new List<Move>();
        foreach (var move in learnableMoves)
        {
            if (move.level <= level)
            { moves.Add(move.moveBase); }

            if (moves.Count >= 4)
            { break; }
        }

        return moves;
    }

    public int GetMaxHPForLevel(int level)
    { return Mathf.FloorToInt((maxHP * level) / 100f) + level + 10; }
}

[System.Serializable]
public class LearnableMove 
{
    public Move moveBase;
    public int level;
}

public enum Stat
{
    Ataque,
    Defensa,
    Destreza,
    Rapidez,

    //Stats extra
    Precisión,
    Evasión
}


