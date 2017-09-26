using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioAction
{
    public int scenarioActionId;
    public ScenarioActionType scenarioActionType;
    public List<CharacterTemplate> createActors;
    public Player controlActor;
    public bool isMoveCamera;
    public Vector3 setCameraPos;
    public HexTile.HexCoord targetMoveTile;
    public ScenarioActorPivotType actorPivot;
    public string dialogName;
    public string dialogText;

    public ScenarioAction() { }

    public ScenarioAction(int scenarioActionId, Vector3 setCameraPos, List<CharacterTemplate> createActors)
    {
        scenarioActionType = ScenarioActionType.CreateActor;
        this.scenarioActionId = scenarioActionId;
        this.setCameraPos = setCameraPos;
        this.createActors = createActors;
    }

    public ScenarioAction(int scenarioActionId, Player controlActor, HexTile.HexCoord targetMoveTile, ScenarioActorPivotType actorPivot)
    {
        scenarioActionType = ScenarioActionType.ControlActor;
        this.scenarioActionId = scenarioActionId;
        this.controlActor = controlActor;
        this.targetMoveTile = targetMoveTile;
        this.actorPivot = actorPivot;
    }

    public ScenarioAction(int scenarioActionId, bool isMoveCamera, Vector3 setCameraPos, HexTile.HexCoord targetMoveTile)
    {
        scenarioActionType = isMoveCamera? ScenarioActionType .ControlCamera: ScenarioActionType.SetCamera;
        this.scenarioActionId = scenarioActionId;
        this.setCameraPos = setCameraPos;
        this.targetMoveTile = targetMoveTile;
    }

    public ScenarioAction(int scenarioActionId, string dialogName, string dialogText)
    {
        scenarioActionType = ScenarioActionType.Dialog;
        this.scenarioActionId = scenarioActionId;
        this.dialogName = dialogName;
        this.dialogText = dialogText;
    }
}
