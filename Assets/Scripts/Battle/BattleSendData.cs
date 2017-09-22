using System.Collections;
using System.Collections.Generic;

public class BattleSendData
{
    public string attacker;
    public string target;
    public string backGround;
    public string attackerTile;
    public string targetTile;

    public bool isPlayerAttack;
    public bool isHeal;
    public bool isCounter;
    public bool isDirect;
    public int attackerDefensRate;
    public int targetDefensRate;

    public int attackerMaxHP;
    public int attackerHP;
    public int damageByAttacker;
    public int targetMaxHP;
    public int targetHP;
    public int damageByTarget;

    public string getItem;
    public int playerExp;
    public int getExp;
    public int level;
    public string playerClass;

    public PlayerRecord playerData;
    public PlayerRecord lvUpData;

    public BattleSendData()
    { }

    public BattleSendData(string attacker, string target, string backGround, string attackerTile, string targetTile, bool isPlayerAttack, bool isHeal, bool isCounter, bool isDirect, int attackerDefensRate, int targetDefensRate, int attackerMaxHP, int attackerHP, int damageByAttacker, int targetMaxHP, int targetHP, int damageByTarget, string getItem, int playerExp, int getExp, int level, string playerClass, PlayerRecord playerData, PlayerRecord lvUpData)
    {
        this.attacker = attacker;
        this.target = target;
        this.backGround = backGround;
        this.attackerTile = attackerTile;
        this.targetTile = targetTile;
        this.isPlayerAttack = isPlayerAttack;
        this.isHeal = isHeal;
        this.isCounter = isCounter;
        this.isDirect = isDirect;
        this.attackerDefensRate = attackerDefensRate;
        this.targetDefensRate = targetDefensRate;
        this.attackerMaxHP = attackerMaxHP;
        this.attackerHP = attackerHP;
        this.damageByAttacker = damageByAttacker;
        this.targetMaxHP = targetMaxHP;
        this.targetHP = targetHP;
        this.damageByTarget = damageByTarget;
        this.getItem = getItem;
        this.playerExp = playerExp;
        this.getExp = getExp;
        this.level = level;
        this.playerClass = playerClass;
        this.playerData = playerData;
        this.lvUpData = lvUpData;
    }
}
