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


    public override PlayerRecord LevelUp(int levelMax, int hpMax, int statusMax)
    {
        //hp atk def dex wis mdef
        int targetLevel = (m_Exp / 100) + 1;
        if (m_Level < levelMax && m_Hp > 0 && targetLevel > m_Level)
        {
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
                        addValue = addHP = Mathf.Min(m_MaxHP + addValue, hpMax) - m_MaxHP;
                        break;
                    case LevelProp.atk:
                        addValue = addAtk = Mathf.Min(m_Atk + addValue, statusMax) - m_Atk;
                        break;
                    case LevelProp.def:
                        addValue = addDef = Mathf.Min(m_Def + addValue, statusMax) - m_Def;
                        break;
                    case LevelProp.dex:
                        addValue = addDex = Mathf.Min(m_Dex + addValue, statusMax) - m_Dex;
                        break;
                    case LevelProp.wis:
                        addValue = addWis = Mathf.Min(m_Wis + addValue, statusMax) - m_Wis;
                        break;
                    case LevelProp.mdef:
                        addValue = addMDef = Mathf.Min(m_MDef + addValue, statusMax) - m_MDef;
                        break;
                }
                if (addValue == 0 && upCount <= (int)LevelProp.max)
                {
                    upCount++;
                }
            }

            return new PlayerRecord(addHP, 0, addAtk, addDef, addWis, addDex, addMDef);
        }
        return null;
    }

}
