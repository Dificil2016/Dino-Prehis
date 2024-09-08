using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Monster
{
    [SerializeField] MonsterBase _monsterBase;
    [SerializeField] string _nameTag;
    [SerializeField] int _level;
    [SerializeField] int _development;

    //Basic info
    public MonsterBase monsterBase { get { return _monsterBase; } }
    public string nameTag { get { return _nameTag; } }
    public int level { get { return _level; } }
    public int development { get { return _development; } }

    //Stats
    public int XP { get; set; }
    public int hp { get; set; }
    public int maxHP { get { return _monsterBase.GetMaxHPForLevel(level); } }
    public int Attack { get { return GetStat(Stat.Ataque); } }
    public int Defense { get { return GetStat(Stat.Defensa); } }
    public int Dexterity { get { return GetStat(Stat.Destreza); } }
    public int Speed { get { return GetStat(Stat.Rapidez); } }
    public Dictionary<Stat, int> _Stats { get; private set; }
    public Dictionary<Stat, int> _StatBost { get; private set; }

    //Exta
    public List<Move> Moves { get; set; }
    public Move currentMove { get; set; }
    public Queue<String> StatusChanges { get; private set; }
    public Condition status { get; set; }

    public bool LevelUp()
    {
        // Debug.Log($"{XP} / {monsterBase.GetXPForLevel(_level + 1)}");

        if (XP > monsterBase.GetXPForLevel(_level + 1) && _level < 99)
        {
            _level ++;
            return true;
        }
        return false;
    }

    public bool DevelopUp(int developIncrease)
    {
        bool isBaby = (_development < monsterBase.maxDevelopment);

        _development = Mathf.Clamp(_development+Mathf.FloorToInt(developIncrease * monsterBase.developmentSpeed), 0, monsterBase.maxDevelopment*2);

        if (_development > monsterBase.maxDevelopment && isBaby)
        { return true; }
        return false;
    }

    public LearnableMove GetLearnableMove()
    {
       return monsterBase.learnableMoves.Where(x => x.level == level).FirstOrDefault();
    }

    public void Init()
    {
        hp = maxHP;

        if (XP <= 0)
        {
            XP = monsterBase.GetXPForLevel(level);
        }

        SetMoves();

        StatusChanges = new Queue<String>();
        CalculateStats();

        ResetStatBoost();
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (status == null)
        {
            status = ConditionsDB.Conditions[conditionID];
            StatusChanges.Enqueue($"{nameTag} {status.startMessage}");
        }
        else
        {
            StatusChanges.Enqueue($"{nameTag} ya sufre {status.conditionName}");
        }
    }

    void CalculateStats()
    {
        _Stats = new Dictionary<Stat, int>();
        _Stats.Add(Stat.Ataque, Mathf.FloorToInt((monsterBase.attack * level) / 100f) + 5);
        _Stats.Add(Stat.Defensa, Mathf.FloorToInt((monsterBase.defense * level) / 100f) + 5);
        _Stats.Add(Stat.Destreza, Mathf.FloorToInt((monsterBase.dexterity * level) / 100f) + 5);
        _Stats.Add(Stat.Rapidez, Mathf.FloorToInt((monsterBase.speed * level) / 100f) + 5);

    }

    void ResetStatBoost()
    {
        _StatBost = new Dictionary<Stat, int>()
        {
            {Stat.Ataque, 0 },
            {Stat.Defensa, 0 },
            {Stat.Destreza, 0 },
            {Stat.Rapidez, 0 },
            {Stat.Precisión, 0 },
            {Stat.Evasión, 0 },
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = _Stats[stat];

        //Aumento de estadísticas
        int boost = _StatBost[stat];

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal + boost);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal - boost);
        }

        return statVal;
    }

    public void ApplyBoost(List<StatBoost> statBoost)
    {
        foreach (var boost in statBoost) 
        {
           _StatBost[boost.stat] = Mathf.Clamp(_StatBost[boost.stat] + boost.boost, -50, 50);

            if (boost.boost > 0)
            { StatusChanges.Enqueue($"{boost.stat} de {nameTag} aumentó en {boost.boost}"); }
            else
            { StatusChanges.Enqueue($"{boost.stat} de {nameTag} disminuyó en {-boost.boost}"); }
        }
    }

    public bool OnBeforeMove()
    {
        if (status?.OnBeforeMove != null)
        {
            return status.OnBeforeMove.Invoke(this);
        }
        return true;
    }

    public void OnAfterTurn()
    {
        status?.OnAfterTurn?.Invoke(this);
    }

    public void DecreaseHP(int damage)
    {
        hp = Mathf.Clamp(hp - damage, 0, maxHP);
    }

    public void IncreaseHP(int heal)
    {
        hp = Mathf.Clamp(hp + heal, 0, maxHP);
    }

    public DamageDetails TakeDamage(Move move, Monster attacker)
    {
        float crit = 1;
        if (UnityEngine.Random.value * 100f <= 5) { crit = 2; }

        float type = TypeChart.GetEffectiveness(move.type, this.monsterBase.Type1) * TypeChart.GetEffectiveness(move.type, this.monsterBase.Type2);

        float modifiers = UnityEngine.Random.Range(0.85f, 1f) * type * crit;
        float a = (2 * attacker.level + 10) / 250f;
        float d = 0;
        if (move.moveCategory == MoveCategory.Physical)
        { d = a * move.power * ((float)attacker.Attack / Defense) + 2; }
        else
        { d = a * move.power * ((float)attacker.Dexterity / Defense) + 2; }
        
        int damage = Mathf.FloorToInt(d * modifiers);

        var damageDetails = new DamageDetails()
        {
            TypeEffect = type,
            Critical = crit,
            Fainted = false,
            Damage = damage
        };

        hp -= damage;
        if (hp <= 0)
        { 
            hp = 0;
            damageDetails.Fainted = true; 
        }
        return damageDetails;
    }

    public MonsterSaveData GetSaveData()
    {
        var monsterData = new MonsterSaveData()
        {
            baseName = monsterBase.monsterName,
            nameTag = nameTag,
            hp = hp,
            level = level,
            development = development,
            exp = XP,
            status = status?.ID,
            moves = Moves.Select(m => m.GetSaveData()).ToList(),
        };


        return monsterData;
    }

    List<Move> SaveMoves = null;

    void SetMoves()
    {
        if (SaveMoves == null)
        {
            SaveMoves = monsterBase.GenerateMoves(level);
        }
        Moves = SaveMoves;
    }

    public Monster(MonsterSaveData saveData)
    {   
        _monsterBase = MonsterDB.monsters[saveData.baseName];

        _nameTag = saveData.nameTag;
        _level = saveData.level;
        _development = saveData.development;
        XP = Mathf.Clamp(saveData.exp, 0, monsterBase.GetXPForLevel(99));

        if (saveData.status != null) 
        { status = ConditionsDB.Conditions[saveData.status.Value]; }
        else
        {  status = null; }

        SaveMoves = saveData.moves.Select(m => MoveDB.moves[m.moveName]).ToList();

        Init();
        hp = Mathf.Clamp(saveData.hp,0,maxHP);
    }

    public Move GetRandomMove()
    {
        int r = UnityEngine.Random.Range(0, Moves.Count);
        return Moves[r];
    }

    public void OnBattleOver()
    {
        ResetStatBoost();
    }
}

[Serializable]
public class MonsterSaveData 
{
    public string baseName;
    public string nameTag;
    public int hp;
    public int level;
    public int development;
    public int exp;
    public ConditionID? status;
    public List<MoveSaveData> moves;
}

public class DamageDetails
{
    public bool Fainted;
    public float Critical;
    public float TypeEffect;
    public int Damage;
}
