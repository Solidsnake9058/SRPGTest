using System.Collections;
using System.Collections.Generic;

public class BattleSendData
{
    public string attacker;
    public string target;
    public string m_BackGround;
    public string attackerTile;
    public string targetTile;

    public bool m_IsPlayerAttack;
    public bool m_IsHeal;
    public bool m_IsCounter;
    public bool m_IsDirect;
    public int attackerDefensRate;
    public int targetDefensRate;

    public int attackerMaxHP;
    public int attackerHP;
    public int damageByAttacker;
    public int targetMaxHP;
    public int targetHP;
    public int damageByTarget;

    public string m_GetItem;
    public int playerExp;
    public int m_GetExp;
    public int level;
    public string m_PlayerClass;

    public BattlePlayerData m_AttackerData;
    public BattlePlayerData m_TargetData;

    public PlayerRecord m_PlayerData;
    public PlayerRecord m_LvUpData;

    public BattleSendData()
    { }

    public BattleSendData(BattlePlayerData attackerData, BattlePlayerData targetData, string backGround, bool isPlayerAttack, bool isHeal, bool isDirect, bool isCounter, int getExp, string getItem, string playerClass, PlayerRecord lvUpData, PlayerRecord playerData)
    {
        m_AttackerData = attackerData;
        m_TargetData = targetData;
        m_BackGround = backGround;
        m_IsPlayerAttack = isPlayerAttack;
        m_IsHeal = isHeal;
        m_IsDirect = isDirect;
        m_IsCounter = isCounter;
        m_GetExp = getExp;
        m_GetItem = getItem;
        m_PlayerClass = playerClass;
        m_LvUpData = lvUpData;
        m_PlayerData = playerData;
    }

    public BattleSendData(string attacker, string target, string backGround, string attackerTile, string targetTile, bool isPlayerAttack, bool isHeal, bool isCounter, bool isDirect, int attackerDefensRate, int targetDefensRate, int attackerMaxHP, int attackerHP, int damageByAttacker, int targetMaxHP, int targetHP, int damageByTarget, string getItem, int playerExp, int getExp, int level, string playerClass, PlayerRecord playerData, PlayerRecord lvUpData)
    {
        this.attacker = attacker;
        this.target = target;
        this.m_BackGround = backGround;
        this.attackerTile = attackerTile;
        this.targetTile = targetTile;
        this.m_IsPlayerAttack = isPlayerAttack;
        this.m_IsHeal = isHeal;
        this.m_IsCounter = isCounter;
        this.m_IsDirect = isDirect;
        this.attackerDefensRate = attackerDefensRate;
        this.targetDefensRate = targetDefensRate;
        this.attackerMaxHP = attackerMaxHP;
        this.attackerHP = attackerHP;
        this.damageByAttacker = damageByAttacker;
        this.targetMaxHP = targetMaxHP;
        this.targetHP = targetHP;
        this.damageByTarget = damageByTarget;
        this.m_GetItem = getItem;
        this.playerExp = playerExp;
        this.m_GetExp = getExp;
        this.level = level;
        this.m_PlayerClass = playerClass;
        this.m_PlayerData = playerData;
        this.m_LvUpData = lvUpData;
    }
}

public struct BattlePlayerData
{
    public string m_PlayerName;
    public string m_TileName;
    public float m_DefensRate;
    public int m_MaxHP;
    public int m_HP;
    public int m_CauseDamage;
    public bool m_IsEnemy;
}