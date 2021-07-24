using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarionManager : IGameItem
{
    private Scenarion m_StageOpenScenarion;
    private Scenarion m_StageClearScenarion;
    private List<Scenarion> m_StageScenarions = new List<Scenarion>();
    private Scenarion m_RunningScenarion;
    private List<int> m_RemoveScenarionList = new List<int>();

    private bool m_IsWaitAction = false;
    private bool m_IsMoveCamera { get { return ScreenController.m_Instance.m_IsCameraMoving; } }
    private bool m_IsEnding = false;
    public bool m_IsScenarionRunning { get { return m_RunningScenarion != null; } }
    private Player m_ControlPlayer = null;

    public override void GameSetting()
    {
        m_IsWaitAction = false;
        m_IsEnding = false;
    }

    public void SetScenarion(List<Scenarion> stageScenations, List<int> removeScenaronList)
    {
        m_RemoveScenarionList = removeScenaronList;
        m_StageOpenScenarion = null;
        m_StageClearScenarion = null;
        m_StageScenarions.Clear();
        for (int i = 0; i < stageScenations.Count; i++)
        {
            if (m_RemoveScenarionList.Contains(stageScenations[i].scenarionId))
            {
                continue;
            }
            switch (stageScenations[i].scenarionType)
            {
                case ScenarionType.Openning:
                    m_StageOpenScenarion = stageScenations[i];
                    break;
                case ScenarionType.Event:
                    m_StageScenarions.Add(stageScenations[i]);
                    break;
                case ScenarionType.StageClear:
                    m_StageClearScenarion = stageScenations[i];
                    break;
            }
        }
    }

    public override void SystemUpdate()
    {
        bool isWaitActor = false;
        if (m_ControlPlayer != null)
        {
            m_ControlPlayer.SystemUpdate();
            isWaitActor = !m_ControlPlayer.IsTurnEnd;
        }
        if (isWaitActor || m_IsWaitAction || m_IsMoveCamera)
        {
            return;
        }
        if (!SetScenarionAction())
        {
            //TODO Scenarion fin
            GameManager.m_Instance.ScenationFin(m_IsEnding);
        }
    }

    public bool SetOpenScenarion()
    {
        m_RunningScenarion = m_StageOpenScenarion;
        return m_IsScenarionRunning;
    }

    public bool SetClearScenarion()
    {
        m_RunningScenarion = m_StageClearScenarion;
        m_IsEnding = true;
        return m_IsScenarionRunning;
    }

    public bool SetBeforeScenarion(int userID, int enemyID, HexCoord hex)
    {
        for (int i = 0; i < m_StageScenarions.Count; i++)
        {
            Scenarion scenarion = m_StageScenarions[i];
            if (scenarion.scenarionConditionType.Equals(ScenarionConditionType.BeforeBattle) && ComparePlayerID(scenarion.userPlayer, userID) && ComparePlayerID(scenarion.enemyPlayer, enemyID))
            {
                m_RunningScenarion = scenarion;
                m_RunningScenarion.battleAfterEvent = hex;
                break;
            }
        }
        return m_IsScenarionRunning;
    }

    public bool SetAfterScenarion(int enemyID, HexCoord hex)
    {
        for (int i = 0; i < m_StageScenarions.Count; i++)
        {
            Scenarion scenarion = m_StageScenarions[i];
            if (scenarion.scenarionConditionType.Equals(ScenarionConditionType.AfterBattle) && ComparePlayerID(scenarion.enemyPlayer, enemyID))
            {
                m_RunningScenarion = scenarion;
                m_RunningScenarion.battleAfterEvent = hex;
                break;
            }
        }
        return m_IsScenarionRunning;
    }

    public bool SetAfterShopScenarion()
    {
        for (int i = 0; i < m_StageScenarions.Count; i++)
        {
            Scenarion scenarion = m_StageScenarions[i];
            if (scenarion.scenarionType.Equals(ScenarionType.Event) && scenarion.scenarionConditionType.Equals(ScenarionConditionType.AfterShop))
            {
                m_RunningScenarion = scenarion;
                break;
            }
        }
        return m_IsScenarionRunning;
    }

    private bool ComparePlayerID(int id, int userID)
    {
        return id.Equals(-1) || id.Equals(userID);
    }

    public void DialogClick()
    {
        m_IsWaitAction = false;
    }

    private bool SetScenarionAction()
    {
        m_ControlPlayer = null;
        if (m_RunningScenarion.scenarionActionStep < m_RunningScenarion.scenarionActions.Count)
        {
            ScenarionAction scenarioAction = m_RunningScenarion.scenarionActions[m_RunningScenarion.scenarionActionStep];
            if (m_RunningScenarion.scenarionActionStep.Equals(0) && m_RunningScenarion.scenarionType == ScenarionType.Openning)
            {
                GameMidiator.m_Instance.m_PlayerManager.HidePlayers();
            }

            switch (scenarioAction.scenarionActionType)
            {
                case ScenarionActionType.Dialog:
                    GameMidiator.m_Instance.m_GameUIManager.SetDialog(scenarioAction.dialogName, scenarioAction.dialogText);
                    m_IsWaitAction = true;
                    break;
                case ScenarionActionType.CreateActor:
                    GameMidiator.m_Instance.m_PlayerManager.GenetareActionPlayer(scenarioAction.createActors);
                    break;
                case ScenarionActionType.ControlActor:
                    m_ControlPlayer = GameMidiator.m_Instance.m_PlayerManager.MoveActorToTarget(scenarioAction.actorId, scenarioAction.targetMoveTile, scenarioAction.actorPivot);
                    break;
                case ScenarionActionType.SetCamera:
                    Vector3 tilePos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(scenarioAction.setCameraPos).HexTilePos();
                    ScreenController.m_Instance.MoveCameraPos(tilePos);
                    break;
                case ScenarionActionType.ControlCamera:
                    Vector3 moveCaremaPos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(scenarioAction.targetMoveTile).HexTilePos();
                    ScreenController.m_Instance.MoveCameraPos(moveCaremaPos, scenarioAction.waitTime);
                    break;
                case ScenarionActionType.AddUserPlayer:
                    break;
                case ScenarionActionType.AddEnemyPlayer:
                    break;
            }
            //currentWaitingTime = 0;
            //isToDark = scenarioAction.isToDark;
            //waitingTime = scenarioAction.waitTime;
            m_RunningScenarion.scenarionActionStep++;
        }
        else
        {
            //Scenario is end
            //SetStopWaiting();
            //EnableGroup(mapController);
            m_IsWaitAction = false;
            if (m_RunningScenarion.scenarionType == ScenarionType.Openning)
            {
                GameMidiator.m_Instance.m_PlayerManager.ShowPlayers();
                Player temp = GameMidiator.m_Instance.m_PlayerManager.GetPlayerByID(0); //userPlayers.Values.Where(x => x.playerIndex == 0).FirstOrDefault();
                ScreenController.m_Instance.SetCameraPos(temp.HexTilePos());// new Vector3(temp.transform.position.x, 0, temp.transform.position.z));
            }

            if (m_RunningScenarion.scenarionConditionType == ScenarionConditionType.BeforeBattle)
            {
                //TODO call attack
                //AttackWithCurrentPlayer(runningScenario.battleAfterEvent);
            }
            if (m_RunningScenarion.isOnceEvent)
            {
                m_RemoveScenarionList.Add(m_RunningScenarion.scenarionId);
                SetScenarion(m_StageScenarions, m_RemoveScenarionList);
            }
            m_RunningScenarion = null;
        }
        return m_IsScenarionRunning;
    }
}
