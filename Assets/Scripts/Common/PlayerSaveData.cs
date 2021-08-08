using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class PlayerSaveData
{
    public List<SaveDataStore> m_SaveDataStores = new List<SaveDataStore>();
    public SaveDataStore m_SaveDataStoreAuto;
    public bool m_IsShowTile = true;
    public bool m_IsShowPlayerUI = true;

    public PlayerSaveData()
    {
        for (int i = 0; i < 15; i++)
        {
            m_SaveDataStores.Add(new SaveDataStore());
        }
        m_SaveDataStoreAuto = new SaveDataStore();
        m_IsShowTile = true;
        m_IsShowPlayerUI = true;
    }
}

[Serializable]
public class SaveDataStore
{
    public bool m_IsEmpty;
    //User data
    public int m_Gold;
    public Dictionary<int, PlayerRecord> m_UserPlayerRecords;
    public Dictionary<int, int> m_PlayerItems;
    public Dictionary<int, int> m_PlayerWeapons;
    //Stage data
    public int m_StageIndex;
    public int m_TurnCount;
    public Dictionary<int, PlayerRecord> m_StagePlayerRecords;
    public Dictionary<int, PlayerRecord> m_StageEnemyRecords;
    public List<int> m_DefeatedEnemyList;
    public List<int> m_RemoveScenaroList;
    public List<ChestState> m_ChestStates;
    public DateTime m_SaveTime;

    public SaveDataStore()
    {
        m_IsEmpty = true;
        m_StageIndex = -1;
        m_TurnCount = 1;
        m_Gold = 0;
        m_UserPlayerRecords = new Dictionary<int, PlayerRecord>();
        m_StagePlayerRecords = new Dictionary<int, PlayerRecord>();
        m_StageEnemyRecords = new Dictionary<int, PlayerRecord>();
        m_DefeatedEnemyList = new List<int>();
        m_RemoveScenaroList = new List<int>();
        m_PlayerItems = new Dictionary<int, int>();
        m_PlayerWeapons = new Dictionary<int, int>();
        m_ChestStates = new List<ChestState>();
    }

    public SaveDataStore Clone()
    {
        string json = JsonConvert.SerializeObject(this);
        return JsonConvert.DeserializeObject<SaveDataStore>(json);
    }

    public bool GetChestState(HexCoord hex)
    {
        for (int i = 0; i < m_ChestStates.Count; i++)
        {
            if (m_ChestStates[i].m_LocX.Equals(hex.m_Q) && m_ChestStates[i].m_LocY.Equals(hex.m_R))
            {
                return m_ChestStates[i].m_IsOpened;
            }
        }
        return false;
    }
}

public class ChestState
{
    public int m_LocX;
    public int m_LocY;
    public bool m_IsOpened;
}