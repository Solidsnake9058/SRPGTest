using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioAction
{
    public int scenarioActionId;
    public ScenarioActionType scenarioActionType;
    public List<PlayerRecord> createActors;
    public int actorId;
    public bool isMoveCamera;
    public HexTile.HexCoord setCameraPos;
    public HexTile.HexCoord targetMoveTile;
    public ScenarioActorPivotType actorPivot;
    public string dialogName;
    public string dialogText;
    public float waitTime;
    public bool isToDark;

    public ScenarioAction() { }

    public ScenarioAction(int scenarioActionId, List<PlayerRecord> createActors,float waitTime,bool isToDark)
    {
        scenarioActionType = ScenarioActionType.CreateActor;
        this.scenarioActionId = scenarioActionId;
        this.createActors = createActors;
		this.waitTime = waitTime;
		this.isToDark = isToDark;
    }

    public ScenarioAction(int scenarioActionId, int actorId, HexTile.HexCoord targetMoveTile, ScenarioActorPivotType actorPivot, float waitTime, bool isToDark)
    {
        scenarioActionType = ScenarioActionType.ControlActor;
        this.scenarioActionId = scenarioActionId;
        this.actorId = actorId;
        this.targetMoveTile = targetMoveTile;
        this.actorPivot = actorPivot;
		this.waitTime = waitTime;
		this.isToDark = isToDark;
	}

    public ScenarioAction(int scenarioActionId, bool isMoveCamera, HexTile.HexCoord targetMoveTile, float waitTime, bool isToDark)
    {
        scenarioActionType = isMoveCamera? ScenarioActionType.ControlCamera: ScenarioActionType.SetCamera;
        this.scenarioActionId = scenarioActionId;
        if (isMoveCamera)
        {
            this.targetMoveTile = targetMoveTile;
        }
        else
        {
            setCameraPos = targetMoveTile;
        }
        this.waitTime = waitTime;
		this.isToDark = isToDark;
	}

    public ScenarioAction(int scenarioActionId, string dialogName, string dialogText)
    {
        scenarioActionType = ScenarioActionType.Dialog;
        this.scenarioActionId = scenarioActionId;
        this.dialogName = dialogName;
        this.dialogText = dialogText;
    }
}
