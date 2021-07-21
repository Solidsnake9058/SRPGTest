using System.Collections;
using System.Collections.Generic;

public class Scenarion
{
    public int scenarionId;
    public ScenarioType scenarionType;
    public ScenarioConditionType scenarionConditionType;
    public int userPlayer = -1;
    public int enemyPlayer = -1;
    public bool isOnceEvent;
    public List<ScenarioAction> scenarionActions;
    public int scenarionActionStep = 0;
    public HexTile battleAfterEvent;

    public Scenarion() { }

    public Scenarion(int scenarionId, ScenarioType scenarionType, ScenarioConditionType scenarionConditionType, int userPlayer, int enemyPlayer, bool isOnceEvent, List<ScenarioAction> scenarionActions)
    {
        this.scenarionId = scenarionId;
        this.scenarionType = scenarionType;
        this.scenarionConditionType = scenarionConditionType;
        this.userPlayer = userPlayer;
        this.enemyPlayer = enemyPlayer;
        this.isOnceEvent = isOnceEvent;
        this.scenarionActions = scenarionActions;
    }

    public Scenarion(int scenarionId, ScenarioType scenarionType, ScenarioConditionType scenarionConditionType, bool isOnceEvent, List<ScenarioAction> scenarionActions)
    {
        this.scenarionId = scenarionId;
        this.scenarionType = scenarionType;
        this.scenarionConditionType = scenarionConditionType;
        this.isOnceEvent = isOnceEvent;
        this.scenarionActions = scenarionActions;
    }
}
