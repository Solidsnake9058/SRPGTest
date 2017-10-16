using System.Collections;
using System.Collections.Generic;

public class Scenario
{
    public int scenarioId;
    public ScenarioType scenarioType;
    public ScenarioConditionType scenarioConditionType;
    public int userPlayer;
    public int enemyPlayer;
    public bool isOnceEvent;
    public List<ScenarioAction> scenarioActions;
    public int scenarioActionStep = 0;
    public HexTile battleAfterEvent;

    public Scenario() { }

    public Scenario(int scenarioId, ScenarioType scenarioType, ScenarioConditionType scenarioConditionType, int userPlayer, int enemyPlayer, bool isOnceEvent, List<ScenarioAction> scenarioActions)
    {
        this.scenarioId = scenarioId;
        this.scenarioType = scenarioType;
        this.scenarioConditionType = scenarioConditionType;
        this.userPlayer = userPlayer;
        this.enemyPlayer = enemyPlayer;
        this.isOnceEvent = isOnceEvent;
        this.scenarioActions = scenarioActions;
    }

    public Scenario(int scenarioId, ScenarioType scenarioType, ScenarioConditionType scenarioConditionType, bool isOnceEvent, List<ScenarioAction> scenarioActions)
    {
        this.scenarioId = scenarioId;
        this.scenarioType = scenarioType;
        this.scenarioConditionType = scenarioConditionType;
        this.isOnceEvent = isOnceEvent;
        this.scenarioActions = scenarioActions;
    }
}
