using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : IGameItem
{
    [SerializeField]
    private TextAsset[] m_StageAssets;

    private int m_StageIndex = 0;
    public MapContainer m_Container { get; private set; }

    public bool LoadStageData(int stageIndex)
    {
        m_StageIndex = stageIndex - 1;
        if (m_StageIndex < 0 || m_StageIndex >= m_StageAssets.Length)
        {
            return false;
        }

        Vector3 connerPointA = Vector3.zero;
        Vector3 connerPointB = Vector3.zero;
        Vector3 connerPointC = Vector3.zero;
        Vector3 connerPointD = Vector3.zero;

        m_Container = ObjectSaveLoad.JsonDataLoad<MapContainer>(m_StageAssets[m_StageIndex].text);
        m_Container.InitTileDataMap();
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
}
