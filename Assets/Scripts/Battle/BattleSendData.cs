using System.Collections;
using System.Collections.Generic;

public class BattleSendData
{
    string attacker;
    string target;
    string backGround;
    bool isPlayerAttack;
    bool isHeal;
    bool isCounter;
    bool isDirect;
    int attackerDefensRate;
    int targetDefensRate;

    int attackerMaxHP;
    int attackerHP;
    int damageByAttacker;
    int targetMaxHP;
    int targetHP;
    int damageByTarget;

    string getItem;
    int getExp;
    PlayerRecord playerData;
    PlayerRecord luUpData;
}
