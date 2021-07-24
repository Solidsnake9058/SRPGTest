using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageClearCondition
{
    public int id;
    public StageClearConditionType stageClearConditionType;
    public List<int> enemyDeadList = new List<int>();
    public int enemyDeadCount = 0;
    public HexCoord specifyTile;

    public StageClearCondition() { }

    public StageClearCondition(List<int> enemyDeadList, int enemyDeadCount)
    {
        stageClearConditionType = StageClearConditionType.EnemyDead;
        this.enemyDeadList = enemyDeadList;
        this.enemyDeadCount = enemyDeadCount;
    }

    public StageClearCondition(HexCoord specifyTile)
    {
        stageClearConditionType = StageClearConditionType.SpecifyTile;
        this.specifyTile = specifyTile;
    }
}

public enum StageClearConditionType
{
    EnemyDead,
    SpecifyTile,
    Max
}
