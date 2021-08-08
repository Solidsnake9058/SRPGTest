using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarionManager : IGameItem
{
    protected static GameUIManager m_GameUIManager { get { return GameMidiator.m_Instance.m_GameUIManager; } }
    protected static StageMapManager m_StageMapManager { get { return GameMidiator.m_Instance.m_StageMapManager; } }
    protected static PlayerManager m_PlayerManager { get { return GameMidiator.m_Instance.m_PlayerManager; } }

    private Scenarion m_StageOpenScenarion;
    private Scenarion m_StageClearScenarion;
    private List<Scenarion> m_StageScenarions = new List<Scenarion>();
    private Scenarion m_RunningScenarion;
    private List<int> m_RemoveScenarionList = new List<int>();

    private bool m_IsWaitAction = false;
    private bool m_IsMoveCamera { get { return m_GameUIManager.m_ScreenControlUI.m_IsCameraMoving; } }
    private bool m_IsOpening = false;
    private bool m_IsEnding = false;
    public bool m_IsScenarionRunning { get { return m_RunningScenarion != null; } }
    private Player m_ControlPlayer = null;
    private bool m_ToDark = false;
    private bool m_ToLight = false;

    public override void GameSetting()
    {
        m_IsWaitAction = false;
        m_IsEnding = false;
        m_IsOpening = false;
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
        if (isWaitActor || m_IsWaitAction || m_IsMoveCamera || (m_ToLight && m_GameUIManager.m_BlackFrontUI.IsDark))
        {
            return;
        }
        if (!SetScenarionAction())
        {
            GameManager.m_Instance.ScenationFin(m_IsOpening, m_IsEnding);
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

    public bool SetBeforeScenarion(Player attacker, Player target, HexCoord hex)
    {
        if (attacker.m_IsEnemy.Equals(target.m_IsEnemy))
        {
            return m_IsScenarionRunning;
        }
        int userID = attacker.m_IsEnemy ? target.playerIndex : attacker.playerIndex;
        int enemyID = attacker.m_IsEnemy ? attacker.playerIndex : target.playerIndex;
        return SetBeforeScenarion(userID, enemyID, hex);
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
        bool isToLight = m_ToDark;
        m_ToDark = false;
        m_ToLight = false;
        if (m_RunningScenarion.scenarionActionStep < m_RunningScenarion.scenarionActions.Count)
        {
            ScenarionAction scenarioAction = m_RunningScenarion.scenarionActions[m_RunningScenarion.scenarionActionStep];
            m_ToDark = scenarioAction.isToDark;
            if (m_RunningScenarion.scenarionActionStep.Equals(0) && m_RunningScenarion.scenarionType == ScenarionType.Openning)
            {
                m_PlayerManager.HidePlayers();
                m_GameUIManager.m_ScreenControlUI.SetPlayerUIShow(false);
            }

            switch (scenarioAction.scenarionActionType)
            {
                case ScenarionActionType.Dialog:
                    m_GameUIManager.SetDialog(scenarioAction.dialogName, scenarioAction.dialogText);
                    m_IsWaitAction = true;
                    break;
                case ScenarionActionType.CreateActor:
                    m_PlayerManager.GenetareActionPlayer(scenarioAction.createActors);
                    break;
                case ScenarionActionType.ControlActor:
                    m_ControlPlayer = m_PlayerManager.MoveActorToTarget(scenarioAction.actorId, scenarioAction.targetMoveTile, scenarioAction.actorPivot);
                    break;
                case ScenarionActionType.SetCamera:
                    Vector3 tilePos = m_StageMapManager.GetMapTile(scenarioAction.setCameraPos).HexTilePos();
                    m_GameUIManager.m_ScreenControlUI.MoveCameraPos(tilePos, scenarioAction.isToDark, scenarioAction.waitTime, scenarioAction.waitTimeDark);
                    m_ToDark = scenarioAction.isToDark;
                    break;
                case ScenarionActionType.ControlCamera:
                    Vector3 moveCaremaPos = m_StageMapManager.GetMapTile(scenarioAction.targetMoveTile).HexTilePos();
                    m_GameUIManager.m_ScreenControlUI.MoveCameraPos(moveCaremaPos, scenarioAction.isToDark, scenarioAction.waitTime, scenarioAction.waitTimeDark);
                    m_ToDark = scenarioAction.isToDark;
                    break;
                case ScenarionActionType.AddUserPlayer:
                    break;
                case ScenarionActionType.AddEnemyPlayer:
                    break;
            }
            //currentWaitingTime = 0;
            //isToDark = scenarioAction.isToDark;
            //waitingTime = scenarioAction.waitTime;
            m_ToLight = scenarioAction.isToLight;
            if (scenarioAction.isToLight)
            {
                m_ToDark = false;
                m_GameUIManager.m_BlackFrontUI.SetToLight();
            }
            m_RunningScenarion.scenarionActionStep++;
        }
        else
        {
            //Scenario is end
            //EnableGroup(mapController);
            m_IsWaitAction = false;
            if (m_RunningScenarion.scenarionType == ScenarionType.Openning)
            {
                m_IsOpening = true;
            }
            if (m_RunningScenarion.scenarionConditionType == ScenarionConditionType.BeforeBattle)
            {
                if (m_RunningScenarion.battleAfterEvent.HasValue)
                {
                    GameManager.m_Instance.AttackWithCurrentPlayer(m_RunningScenarion.battleAfterEvent.Value);
                }
            }
            if (m_RunningScenarion.isOnceEvent)
            {
                m_RemoveScenarionList.Add(m_RunningScenarion.scenarionId);
                SetScenarion(m_StageScenarions, m_RemoveScenarionList);
            }
            m_RunningScenarion = null;
        }
        isToLight &= m_RunningScenarion == null;
        if (isToLight)
        {
            m_GameUIManager.m_BlackFrontUI.SetToLight();
        }
        return m_IsScenarionRunning;
    }
}
