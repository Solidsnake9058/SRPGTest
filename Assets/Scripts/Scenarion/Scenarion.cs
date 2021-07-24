using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Scenarion
{
    public int scenarionId;
    public ScenarionType scenarionType;
    public ScenarionConditionType scenarionConditionType;
    public int userPlayer = -1;
    public int enemyPlayer = -1;
    public bool isOnceEvent;
    public List<ScenarionAction> scenarionActions;
    public int scenarionActionStep = 0;
    public HexCoord? battleAfterEvent;

    public Scenarion() { }

    public Scenarion(int scenarionId, ScenarionType scenarionType, ScenarionConditionType scenarionConditionType, int userPlayer, int enemyPlayer, bool isOnceEvent, List<ScenarionAction> scenarionActions)
    {
        this.scenarionId = scenarionId;
        this.scenarionType = scenarionType;
        this.scenarionConditionType = scenarionConditionType;
        this.userPlayer = userPlayer;
        this.enemyPlayer = enemyPlayer;
        this.isOnceEvent = isOnceEvent;
        this.scenarionActions = scenarionActions;
    }

    public Scenarion(int scenarionId, ScenarionType scenarionType, ScenarionConditionType scenarionConditionType, bool isOnceEvent, List<ScenarionAction> scenarionActions)
    {
        this.scenarionId = scenarionId;
        this.scenarionType = scenarionType;
        this.scenarionConditionType = scenarionConditionType;
        this.isOnceEvent = isOnceEvent;
        this.scenarionActions = scenarionActions;
    }
}
