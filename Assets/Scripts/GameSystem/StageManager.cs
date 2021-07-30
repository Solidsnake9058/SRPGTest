using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : IGameItem
{
    protected static StageMapManager m_StageMapManager { get { return GameMidiator.m_Instance.m_StageMapManager; } }
    protected static ScenarionManager m_ScenarionManager { get { return GameMidiator.m_Instance.m_ScenarionManager; } }
    protected static PlayerManager m_PlayerManager { get { return GameMidiator.m_Instance.m_PlayerManager; } }

    [SerializeField]
    private TextAsset[] m_StageAssets = default;

    public List<int> m_ShopItemList { get; private set; }
    public List<int> m_ShopWeaponList { get; private set; }

    private int m_StageIndex = 0;
    public MapContainer m_Container { get; private set; }

    public string StageTitle => m_Container != null ? m_Container.stageTitle : "";

    public bool LoadStageData(SaveDataStore saveData)
    {

        m_StageIndex = saveData.m_StageIndex - 1;
        if (m_StageIndex < 0 || m_StageIndex >= m_StageAssets.Length)
        {
            return false;
        }
        m_Container = ObjectSaveLoad.JsonDataLoad<MapContainer>(m_StageAssets[m_StageIndex].text);
        m_Container.InitTileDataMap();
        m_ShopItemList = m_Container.shopItemList;
        m_ShopWeaponList = m_Container.shopWeaponList;

        m_StageMapManager.CreateStageMap(m_Container);
        for (int i = 0; i < saveData.m_ChestStates.Count; i++)
        {
            HexCoord hex = new HexCoord(saveData.m_ChestStates[i].m_LocX, saveData.m_ChestStates[i].m_LocY);
            m_StageMapManager.GetMapTile(hex).SetChestState(saveData.GetChestState(hex));
        }

        List<PlayerRecord> userPlayerRecords = GetUserPlayerRecords(saveData.m_UserPlayerRecords, saveData.m_StagePlayerRecords);
        List<PlayerRecord> enemyPlayerRecords = GetEnemyPlayerRecords(saveData.m_StageEnemyRecords);

        m_PlayerManager.GenetarePlayers(userPlayerRecords);
        m_PlayerManager.GenetarePlayers(enemyPlayerRecords);
        m_ScenarionManager.SetScenarion(m_Container.scenarionList, saveData.m_RemoveScenaroList);
        return true;
    }

    public List<PlayerRecord> GetUserPlayerRecords(Dictionary<int, PlayerRecord> userDatas, Dictionary<int, PlayerRecord> userStageDatas)
    {
        List<PlayerRecord> playerRecords = new List<PlayerRecord>();
        for (int i = 0; i < m_Container.userPlayerRecords.Count; i++)
        {
            int id = m_Container.userPlayerRecords[i].characterId;
            PlayerRecord userdata = userDatas[id];
            PlayerRecord userStagedata = userStageDatas.ContainsKey(id) ? userStageDatas[id] : null;

            PlayerRecord playerRecord = userdata.Clone();
            playerRecord.SetPlayerLocate(userStagedata ?? m_Container.userPlayerRecords[i]);
            playerRecord.SetPlayerHp(userStagedata);
            playerRecords.Add(playerRecord);
        }
        return playerRecords;
    }

    public List<PlayerRecord> GetEnemyPlayerRecords(Dictionary<int, PlayerRecord> enemyStageDatas)
    {
        List<PlayerRecord> playerRecords = new List<PlayerRecord>();
        for (int i = 0; i < m_Container.enemyPlayerRecords.Count; i++)
        {
            int id = m_Container.enemyPlayerRecords[i].id;
            PlayerRecord enemyStagedata = enemyStageDatas.ContainsKey(id) ? enemyStageDatas[id] : null;

            PlayerRecord playerRecord = m_Container.enemyPlayerRecords[i].Clone();
            playerRecord.SetPlayerLocate(enemyStagedata ?? m_Container.enemyPlayerRecords[i]);
            playerRecord.SetPlayerHp(enemyStagedata);
            playerRecords.Add(playerRecord);
        }
        return playerRecords;
    }

    public bool StageClearCheck()
    {
        for (int i = 0; i < m_Container.stageClearConditionList.Count; i++)
        {
            StageClearCondition condition = m_Container.stageClearConditionList[i];
            switch (condition.stageClearConditionType)
            {
                case StageClearConditionType.EnemyDead:
                    int deadCount = 0;
                    int deadCondition = Mathf.Clamp(condition.enemyDeadCount, 1, condition.enemyDeadList.Count);
                    List<int> deatList = condition.enemyDeadList;
                    for (int j = 0; j < deatList.Count; j++)
                    {
                        Player tempPlayer = m_PlayerManager.m_EnemyPlayers[deatList[j]] ?? null;
                        if (tempPlayer == null || tempPlayer.m_Hp <= 0)
                        {
                            deadCount++;
                        }
                    }
                    if (deadCount >= deadCondition)
                    {
                        return true;
                    }
                    break;
                case StageClearConditionType.SpecifyTile:
                    if (m_PlayerManager.CheckUserPlayerTile(condition.specifyTile))
                    {
                        return true;
                    }
                    break;
            }
            //if (isWin)
            //{
            //    //Win
            //    Scenario temp = stageScenatios.Where(x => x.scenarioType == ScenarioType.StageClear).FirstOrDefault();
            //    if (temp != null)
            //    {
            //        runningScenario = temp;
            //        _Update = ScenarioMode;
            //    }
            //    else
            //    {
            //        Debug.Log("No Clear Scenario");
            //    }
            //    break;
            //}
        }
        return false;
    }
}
