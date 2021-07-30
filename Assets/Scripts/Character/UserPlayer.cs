using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserPlayer : Player
{
    public override void SystemUpdate()
    {
        TurnUpdate();
    }

    public override void TurnEnd()
    {
        HexTile tile = m_StageMapManager.GetMapTile(m_Hex);
        {
            tile.OpenChest();
        }
        base.TurnEnd();
    }

    protected override void MoveToPointAction()
    {
        GameManager.m_Instance.ShowConfirmMenu();
    }


    public override PlayerRecord LevelUp()
    {
        //hp atk def dex wis mdef
        if (m_Level >= 20)
        {
            return null;
        }

        List<LevelProp> lvprops = new List<LevelProp>();
        for (int i = 0; i < (int)LevelProp.max; i++)
        {
            lvprops.Add((LevelProp)i);
        }
        lvprops = GenericFunction.Shuffle(lvprops);
        int upCount = Random.Range(1, 5);

        int addHP = 0;
        int addAtk = 0;
        int addDef = 0;
        int addDex = 0;
        int addWis = 0;
        int addMDef = 0;

        m_Level++;
        int addValue = 0;
        for (int i = 0; i < upCount; i++)
        {
            addValue = Random.Range(1, 3);
            switch (lvprops[i])
            {
                case LevelProp.hp:
                    if (m_MaxHP + addValue > 150)
                    {
                        addValue = 150 - m_MaxHP;
                    }
                    m_MaxHP += addHP = addValue;
                    break;
                case LevelProp.atk:
                    if (m_Atk + addValue > 45)
                    {
                        addValue = 45 - m_Atk;
                    }
                    m_Atk += addAtk = addValue;
                    break;
                case LevelProp.def:
                    if (m_Atk + addValue > 45)
                    {
                        addValue = 45 - m_Def;
                    }
                    m_Def += addDef = addValue;
                    break;
                case LevelProp.dex:
                    if (m_Atk + addValue > 45)
                    {
                        addValue = 45 - m_Dex;
                    }
                    m_Dex += addDex = addValue;
                    break;
                case LevelProp.wis:
                    if (m_Atk + addValue > 45)
                    {
                        addValue = 45 - m_Wis;
                    }
                    m_Wis += addWis = addValue;
                    break;
                case LevelProp.mdef:
                    if (m_Atk + addValue > 45)
                    {
                        addValue = 45 - m_MDef;
                    }
                    m_MDef += addMDef = addValue;
                    break;
            }
            if (addValue == 0 && upCount <= (int)LevelProp.max)
            {
                upCount++;
            }
        }

        return new PlayerRecord(addHP, 0, addAtk, addDef, addWis, addDex, addMDef);
    }

}
