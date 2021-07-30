using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveManager
{
    private static PlayerSaveData m_GameData
    {
        get
        {
            if (_GameData == null)
            {
                Load();
            }
            return _GameData;
        }
    }
    private static PlayerSaveData _GameData;

    public static void Load()
    {
        _GameData = null;
        _GameData = PlayerPrefsX.GetClassDecrypt("PlayerSaveData", new PlayerSaveData());
    }

    public static void Save()
    {
        PlayerPrefsX.SetClassEncrypt("PlayerSaveData", m_GameData);
    }

    public static void Clear()
    {
        Debug.Log("Clear Save Data");
        PlayerPrefsX.DeleteAllKey();
        Load();
    }

    public static bool GetIsShowTile()
    {
        return m_GameData.m_IsShowTile;
    }
    public static void SetIsShowTile(bool value)
    {
        m_GameData.m_IsShowTile = value;
    }

    public static bool GetIsShowPlayerUI()
    {
        return m_GameData.m_IsShowPlayerUI;
    }
    public static void SetIsShowPlayerUI(bool value)
    {
        m_GameData.m_IsShowPlayerUI = value;
    }

    public static SaveDataStore GetSaveDataStore(int index)
    {
        if (index >= 0 && index < m_GameData.m_SaveDataStores.Count)
        {
            return m_GameData.m_SaveDataStores[index];
        }
        else
        {
            return new SaveDataStore();
        }
    }

    public static SaveDataStore GetSaveDataStore()
    {
        return m_GameData.m_SaveDataStoreAuto;
    }

    public static void SetSaveDataStore(int index, SaveDataStore saveDataStore)
    {
        if (index >= 0 && index < m_GameData.m_SaveDataStores.Count)
        {
            m_GameData.m_SaveDataStores[index] = saveDataStore;
        }
    }

    public static void SetSaveDataStore(SaveDataStore saveDataStore)
    {
        m_GameData.m_SaveDataStoreAuto = saveDataStore;
    }
}
