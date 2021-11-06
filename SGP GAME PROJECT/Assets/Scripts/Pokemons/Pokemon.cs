/*
    @author : SamirAli Mukhi
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public PokemonBase Base { 
        get {
            return _base;
        }
     }
    public int Level { 
        get{
            return level;
        }
    }

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove {get; set;}
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status {get; private set;}
    public int StatusTime{get; set;}
    public Queue<string> StatusChanges { get; private set; } 
    public bool HpChanged {get; set;}
    
    //To set volatile status
    public Condition VolatileStatus {get; private set;}
    //To set volatile status time
    public int VolatileStatusTime{get; set;}
    
    //To change the status of pokemon everytime 
    public event System.Action OnStatusChanged;

    public void Init()
    {
        //generate moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                break;
        }
    
        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    public void HealPokemon()
    {
        // Remove statuses, recover changes and reset stats
        ResetStatBoost();
        HPRestore(MaxHp);
        StatusHeal();        
        // reset move pp
        Moves.ForEach(m => m.RestorePP(m.Base.PP));
    }

    public void StatusHeal()
    {
        Status = null;
    }

    public void HPRestore(int hpToHeal)
    {
        HP += hpToHeal;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.Name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetPokemonByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if(saveData.statusId != null)
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(s => new Move(s)).ToList();
        
        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack , 0},
            {Stat.Defense , 0},
            {Stat.SpAttack , 0},
            {Stat.SpDefense , 0},
            {Stat.Speed , 0},
            {Stat.Accuracy , 0},
            {Stat.Evasion , 0},
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // Apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3, 3.5f, 4f};

        if(boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else    
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if(boost > 0)
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            else   
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");

            Debug.Log($"{Base.Name}'s {stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if(Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }

        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove moveToLearn)
    {
        if(Moves.Count > PokemonBase.MaxNumOfMoves)
            return;

        Moves.Add(new Move(moveToLearn.Base));
    }

    public int Attack {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHp {
        get; private set;
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25)
            critical = 2f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpAttack : Defense;

        float modifier = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifier);

        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage )
    {
        HP = Mathf.Clamp(HP - damage , 0 , MaxHp);
        HpChanged = true;
    }
    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null) return;

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    //Methofd to set volatile status
    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }
    public Move GetRandomMove()
    {
        //The logic to check the moves which have PP
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();//to convert it to a list

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        //canPerformMove is used which keeps track of volatile movements which occur before move
        bool canPerformMove = true;
        if(Status?.OnBeforeMove != null)
        {
            if(!Status.OnBeforeMove(this))
                canPerformMove = false;
        }

        if(VolatileStatus?.OnBeforeMove != null)
        {
            if(!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }
        return canPerformMove;
    }
    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }


    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
	public string name;
	public int hp;
	public int level;
	public int exp;
	public ConditionID? statusId;
    public List<MoveSaveData> moves; 
}