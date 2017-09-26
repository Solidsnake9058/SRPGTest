using System.Collections;
using System.Collections.Generic;

public class Scenario
{
    public int scenarioId;
    public ScenarioType scenarioType;
    public ScenarioConditionType scenarioConditionType;
    public Player userPlayer;
    public Player enemyPlayer;
    public bool isOnceEvent;
    public List<ScenarioAction> scenarioActions;
    public int scenarioActionStep = 0;
    public HexTile battleAfterEvent;

    public Scenario() { }

    public Scenario(int scenarioId, ScenarioType scenarioType, ScenarioConditionType scenarioConditionType, Player userPlayer, Player enemyPlayer, bool isOnceEvent, List<ScenarioAction> scenarioActions, HexTile battleAfterEvent)
    {
        this.scenarioId = scenarioId;
        this.scenarioType = scenarioType;
        this.scenarioConditionType = scenarioConditionType;
        this.userPlayer = userPlayer;
        this.enemyPlayer = enemyPlayer;
        this.isOnceEvent = isOnceEvent;
        this.scenarioActions = scenarioActions;
        this.battleAfterEvent = battleAfterEvent;
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
