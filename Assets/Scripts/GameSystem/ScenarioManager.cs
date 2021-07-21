using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : IGameItem
{
    private Scenarion m_StageOpenScenation;
    private Scenarion m_StageClearScenation;
    private List<Scenarion> m_StageScenatios = new List<Scenarion>();
    private Scenarion m_RunningScenarion;
    public List<int> m_RemoveScenaroList = new List<int>();

    private bool m_IsWaitingActor = false;
    private bool m_IsMoveCamera = false;

    public void SetScenarion(List<Scenarion> stageScenatios, List<int> removeScenaroList)
    {
        m_RemoveScenaroList = removeScenaroList;
        m_StageOpenScenation = null;
        m_StageClearScenation = null;
        m_StageScenatios.Clear();
        for (int i = 0; i < stageScenatios.Count; i++)
        {
            if (m_RemoveScenaroList.Contains(stageScenatios[i].scenarionId))
            {
                continue;
            }
            switch (stageScenatios[i].scenarionType)
            {
                case ScenarioType.Openning:
                    m_StageOpenScenation = stageScenatios[i];
                    break;
                case ScenarioType.Event:
                    m_StageScenatios.Add(stageScenatios[i]);
                    break;
                case ScenarioType.StageClear:
                    m_StageClearScenation = stageScenatios[i];
                    break;
            }
        }
    }

    public override void SystemUpdate()
    {
        if (m_IsWaitingActor|| m_IsMoveCamera)
        {
            return;
        }
        if (!SetScenarionAction())
        {
            //TODO Scenarion fin
        }
    }

    public bool SetScenarionAction()
    {
        if (m_RunningScenarion.scenarionActionStep < m_RunningScenarion.scenarionActions.Count)
        {
            ScenarioAction scenarioAction = m_RunningScenarion.scenarionActions[m_RunningScenarion.scenarionActionStep];
            //play action
            if (m_RunningScenarion.scenarionType == ScenarioType.Openning)
            {
                //Hide all players
                GameMidiator.m_Instance.m_PlayerManager.HidePlayers();
            }

            switch (scenarioAction.scenarioActionType)
            {
                case ScenarioActionType.Dialog:
                    //Show dialog
                    //EnableGroup(dialogGroup);
                    //dialogName.text = scenarioAction.dialogName;
                    //dialogText.text = scenarioAction.dialogText;
                    //isWaitingActor = true;
                    break;
                case ScenarioActionType.CreateActor:
                    GameMidiator.m_Instance.m_PlayerManager.GenetareActionPlayer(scenarioAction.createActors);
                    break;
                case ScenarioActionType.ControlActor:
                    if(GameMidiator.m_Instance.m_PlayerManager.MoveActorToTarget(scenarioAction.actorId, scenarioAction.targetMoveTile, scenarioAction.actorPivot))
                    {
                        m_IsWaitingActor = true;
                    }
                    break;
                case ScenarioActionType.SetCamera:
                    Vector3 tilePos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(scenarioAction.setCameraPos).HexTilePos();
                    ScreenController.instance.SetCameraPos(tilePos);
                    break;
                case ScenarioActionType.ControlCamera:
                    //moveCaremaPos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(scenarioAction.targetMoveTile).HexTilePos();
                    m_IsMoveCamera = true;
                    m_IsWaitingActor = true;
                    break;
                case ScenarioActionType.AddUserPlayer:
                    break;
                case ScenarioActionType.AddEnemyPlayer:
                    break;
            }
            //currentWaitingTime = 0;
            //isToDark = scenarioAction.isToDark;
            //waitingTime = scenarioAction.waitTime;
            m_RunningScenarion.scenarionActionStep++;
            return true;
        }
        else
        {
            //Scenario is end
            //SetStopWaiting();
            //EnableGroup(mapController);
            if (m_RunningScenarion.scenarionType == ScenarioType.Openning)
            {
                GameMidiator.m_Instance.m_PlayerManager.ShowPlayers();
                //HideActor();
                //ClearActorPlayer();
                Player temp = GameMidiator.m_Instance.m_PlayerManager.GetPlayerByID(0); //userPlayers.Values.Where(x => x.playerIndex == 0).FirstOrDefault();
                ScreenController.instance.SetCameraPos(temp.HexTilePos());// new Vector3(temp.transform.position.x, 0, temp.transform.position.z));
                //ShowStageInfo(false);
            }

            if (m_RunningScenarion.scenarionConditionType == ScenarioConditionType.BeforeBattle)
            {
                //TODO call attack
                //AttackWithCurrentPlayer(runningScenario.battleAfterEvent);
            }
            if (m_RunningScenarion.isOnceEvent)
            {
                m_RemoveScenaroList.Add(m_RunningScenarion.scenarionId);
                for (int i = 0; i < m_StageScenatios.Count; i++)
                {
                    if (m_StageScenatios[i].scenarionId.Equals(m_RunningScenarion.scenarionId))
                    {
                        m_StageScenatios.Remove(m_StageScenatios[i]);
                        break;
                    }
                }
            }
            m_RunningScenarion = null;
            return false;
        }
    }
}
