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
    public float waitTimeDark;
    public bool isToDark;
    public bool isToLight;


    public ScenarionAction() { }

    public ScenarionAction(int scenarioActionId, List<PlayerRecord> createActors,float waitTime, float waitTimeDark, bool isToDark, bool isToLight)
    {
        scenarionActionType = ScenarionActionType.CreateActor;
        this.scenarioActionId = scenarioActionId;
        this.createActors = createActors;
		this.waitTime = waitTime;
        this.waitTimeDark = waitTimeDark;
        this.isToDark = isToDark;
        this.isToLight = isToLight;
    }

    public ScenarionAction(int scenarioActionId, int actorId, HexCoord targetMoveTile, ScenarionActorPivotType actorPivot, float waitTime, float waitTimeDark, bool isToDark, bool isToLight)
    {
        scenarionActionType = ScenarionActionType.ControlActor;
        this.scenarioActionId = scenarioActionId;
        this.actorId = actorId;
        this.targetMoveTile = targetMoveTile;
        this.actorPivot = actorPivot;
		this.waitTime = waitTime;
        this.waitTimeDark = waitTimeDark;
        this.isToDark = isToDark;
        this.isToLight = isToLight;
    }

    public ScenarionAction(int scenarioActionId, bool isMoveCamera, HexCoord targetMoveTile, float waitTimeDark, float waitTime, bool isToDark, bool isToLight)
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
        this.waitTimeDark = waitTimeDark;
        this.waitTime = waitTime;
		this.isToDark = isToDark;
        this.isToLight = isToLight;
    }

    public ScenarionAction(int scenarioActionId, string dialogName, string dialogText, bool isToLight)
    {
        scenarionActionType = ScenarionActionType.Dialog;
        this.scenarioActionId = scenarioActionId;
        this.dialogName = dialogName;
        this.dialogText = dialogText;
        this.isToLight = isToLight;
    }
}
