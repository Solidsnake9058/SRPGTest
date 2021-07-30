using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : IGameItem
{
    protected static ElementManager m_ElementManager { get { return GameMidiator.m_Instance.m_ElementManager; } }
    protected static PlayerManager m_PlayerManager { get { return GameMidiator.m_Instance.m_PlayerManager; } }
    protected static GameUIManager m_GameUIManager { get { return GameMidiator.m_Instance.m_GameUIManager; } }


    private SaveDataStore m_SaveData;

    public Dictionary<int, int> m_PlayerItems
    {
        get
        {
            return m_SaveData.m_PlayerItems;
        }
    }
    public Dictionary<int, int> m_PlayerWeapons
    {
        get
        {
            return m_SaveData.m_PlayerWeapons;
        }
    }

    [SerializeField]
    private int m_PlayerGoldMax = 60000;
    public int m_PlayerGold
    {
        get
        {
            return m_SaveData.m_Gold;
        }
        set { m_SaveData.m_Gold = Mathf.Clamp(value, 0, m_PlayerGoldMax); }
    }

    public void SetPlayerItemData(SaveDataStore saveDataStore)
    {
        m_SaveData = saveDataStore;
        //_playerGold = saveDataStore.m_Gold;
        //m_PlayerItems = saveDataStore.m_PlayerItems;
        //m_PlayerWeapons = saveDataStore.m_PlayerWeapons;
    }

    private void IncreaseItem(int id)
    {
        if (m_PlayerItems.ContainsKey(id))
        {
            m_PlayerItems[id]++;
        }
        else
        {
            m_PlayerItems.Add(id, 1);
        }
    }
    private bool DecreaseItem(int id)
    {
        if (m_PlayerItems.ContainsKey(id) && m_PlayerItems[id] > 0)
        {
            m_PlayerItems[id]--;
            return true;
        }
        return false;
    }
    public bool SellItem(int id)
    {
        Item selectItem = m_ElementManager.GetItem(id);
        if (selectItem != null && DecreaseItem(id))
        {
            m_PlayerGold += selectItem.price;
            return true;
        }
        return false;
    }
    public bool UseItem(int id)
    {
        Item selectItem = m_ElementManager.GetItem(id);
        if (selectItem != null && !selectItem.m_IsKeyItem && DecreaseItem(id))
        {
            if (selectItem.m_FullCure)
            {
                List<Player> players = m_PlayerManager.GetUserPlayers(null, false, false);
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].m_Hp = players[i].m_MaxHP;
                }
            }
            else
            {
                int hp = selectItem.hp;
                Player player = GameManager.m_Instance.GetSelectedPlayer();
                player.m_Atk += selectItem.atk;
                player.m_Def += selectItem.def;
                player.m_Dex += selectItem.dex;
                player.m_Wis += selectItem.wis;
                player.m_MaxHP += selectItem.addHp;
                player.m_Hp = Mathf.Min(player.m_Hp + hp, player.m_MaxHP);

                if (selectItem.useCharType > 0)
                {
                    player.m_Race = selectItem.newCharType;
                    player.m_Level = 1;
                    player.m_Exp = 0;
                }
                string newRaceName = selectItem.useCharType > 0 ? m_ElementManager.GetRace(player.m_Race).name : "";
                m_GameUIManager.ShowUseItemInfo(player.m_PlayerName, hp, selectItem.atk, selectItem.def, selectItem.dex, selectItem.wis, selectItem.addHp, selectItem.gold, newRaceName);
            }
            return true;
        }
        return false;
    }

    public bool EquipWeapon(int id)
    {

        Player player = GameManager.m_Instance.GetSelectedPlayer();
        if (player == null || player.m_EquipWeapon.Equals(id))
        {
            return false;
        }
        Weapon selectedWeapon = m_ElementManager.GetWeapon(id);
        Weapon equipedWeapon = m_ElementManager.GetWeapon(player.m_EquipWeapon);

        player.m_EquipWeapon = selectedWeapon.id;
        DecreaseWeapon(selectedWeapon.id);
        IncreaseWeapon(equipedWeapon.id);

        m_GameUIManager.ShowEquipWeaponInfo(selectedWeapon.name, selectedWeapon.name);
        return true;
    }
    public bool SellWeapon(int id)
    {
        Weapon selectedWeapon = m_ElementManager.GetWeapon(id);
        if (selectedWeapon != null && DecreaseWeapon(id))
        {
            m_PlayerGold += selectedWeapon.price;
            return true;
        }
        return false;
    }
    private void IncreaseWeapon(int id)
    {
        if (m_PlayerWeapons.ContainsKey(id))
        {
            m_PlayerWeapons[id]++;
        }
        else
        {
            m_PlayerWeapons.Add(id, 1);
        }
    }
    private bool DecreaseWeapon(int id)
    {
        if (m_PlayerWeapons.ContainsKey(id) && m_PlayerWeapons[id] > 0)
        {
            int newCount = m_PlayerWeapons[id] - 1;
            m_PlayerWeapons[id] = newCount;
            return true;
        }
        return false;
    }

    public bool BuyItem(int id, bool isWeapon)
    {
        if (isWeapon)
        {
            Weapon selectWeapon = m_ElementManager.GetWeapon(id);
            if (selectWeapon != null)
            {
                IncreaseWeapon(selectWeapon.id);
                m_PlayerGold -= selectWeapon.price;
                return true;
            }
        }
        else
        {
            Item selectItem = m_ElementManager.GetItem(id);
            if (selectItem != null)
            {
                IncreaseItem(selectItem.id);
                m_PlayerGold -= selectItem.price;
                return true;
            }
        }
        return false;
    }

    public void GetChest(int gold, int itemId, int weaponId)
    {
        m_PlayerGold += gold;
        if (itemId >= 0)
        {
            if (m_PlayerItems.ContainsKey(itemId))
            {
                m_PlayerItems[itemId]++;
            }
            else
            {
                m_PlayerItems.Add(itemId, 1);
            }
        }

        if (weaponId >= 0)
        {
            if (m_PlayerWeapons.ContainsKey(weaponId))
            {
                m_PlayerWeapons[weaponId]++;
            }
            else
            {
                m_PlayerWeapons.Add(weaponId, 1);
            }
        }

        m_GameUIManager.ShowGetItemInfo(gold, itemId, weaponId);
    }

}
