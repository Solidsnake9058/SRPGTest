using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScenarionAction
{
    public int scenarioActionId;
    public ScenarionActionType scenarionActionType;
    public List<PlayerRecord> createActors;
    public int actorId;
    public bool isMoveCamera;
    public HexCoord setCameraPos;
    public HexCoord targetMoveTile;
    public ScenarionActorPivotType actorPivot;
    public string dialogName;
    public string dialogText;
    public float waitTime;
    public bool isToDark;

    public ScenarionAction() { }

    public ScenarionAction(int scenarioActionId, List<PlayerRecord> createActors,float waitTime,bool isToDark)
    {
        scenarionActionType = ScenarionActionType.CreateActor;
        this.scenarioActionId = scenarioActionId;
        this.createActors = createActors;
		this.waitTime = waitTime;
		this.isToDark = isToDark;
    }

    public ScenarionAction(int scenarioActionId, int actorId, HexCoord targetMoveTile, ScenarionActorPivotType actorPivot, float waitTime, bool isToDark)
    {
        scenarionActionType = ScenarionActionType.ControlActor;
        this.scenarioActionId = scenarioActionId;
        this.actorId = actorId;
        this.targetMoveTile = targetMoveTile;
        this.actorPivot = actorPivot;
		this.waitTime = waitTime;
		this.isToDark = isToDark;
	}

    public ScenarionAction(int scenarioActionId, bool isMoveCamera, HexCoord targetMoveTile, float waitTime, bool isToDark)
    {
        scenarionActionType = isMoveCamera? ScenarionActionType.ControlCamera: ScenarionActionType.SetCamera;
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

    public ScenarionAction(int scenarioActionId, string dialogName, string dialogText)
    {
        scenarionActionType = ScenarionActionType.Dialog;
        this.scenarioActionId = scenarioActionId;
        this.dialogName = dialogName;
        this.dialogText = dialogText;
    }
}
