using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementManager : IGameItem
{
    [SerializeField]
    public TextAsset m_ElementData;

    public GameElement m_GameElement { get; private set; }
    private Dictionary<int, CharacterTemplate> m_DicPlayerTypes;
    private Dictionary<int, CharacterTemplate> m_DicEnemyTypes;

    public override void GameSetting()
    {
        LoadGameElements();
    }


    private void LoadGameElements()
    {
        if (m_GameElement != null)
        {
            return;
        }
        m_GameElement = ObjectSaveLoad.JsonDataLoad<GameElement>(m_ElementData.text);

        m_DicPlayerTypes = new Dictionary<int, CharacterTemplate>();
        m_DicEnemyTypes = new Dictionary<int, CharacterTemplate>();
        for (int i = 0; i < m_GameElement.characters.Count; i++)
        {
            if (m_GameElement.characters[i].enemy)
            {
                m_DicEnemyTypes.Add(m_GameElement.characters[i].id, m_GameElement.characters[i]);
            }
            else
            {
                m_DicPlayerTypes.Add(m_GameElement.characters[i].id, m_GameElement.characters[i]);
            }
        }
    }
    public CharacterTemplate GetPlayerType(int id, bool isEnemy)
    {
        if (isEnemy)
        {
            return m_DicEnemyTypes[id];
        }
        else
        {
            return m_DicPlayerTypes[id];
        }
    }

    public Item GetItem(int id)
    {
        return m_GameElement.m_Items.ContainsKey(id) ? m_GameElement.m_Items[id] : null;
    }

    public Dictionary<int, int> GetInitItems()
    {
        Dictionary<int, int> items = new Dictionary<int, int>();
        for (int i = 0; i < m_GameElement.items.Count; i++)
        {
            items.Add(m_GameElement.items[i].id, m_GameElement.items[i].initialCount);
        }
        return items;
    }

    public Weapon GetWeapon(int id)
    {
        return m_GameElement.m_Weapons.ContainsKey(id) ? m_GameElement.m_Weapons[id] : null;
    }
    public CharacterType GetRace(int id)
    {
        return m_GameElement.m_Races.ContainsKey(id) ? m_GameElement.m_Races[id] : null;
    }

    public Dictionary<int, PlayerRecord> GetInitPlayerValue()
    {
        Dictionary<int, PlayerRecord> records = new Dictionary<int, PlayerRecord>();
        for (int i = 0; i < m_GameElement.characters.Count; i++)
        {
            if (!m_GameElement.characters[i].enemy)
            {
                records.Add(m_GameElement.characters[i].id, new PlayerRecord(m_GameElement.characters[i]));
            }
        }
        return records;
    }

    public string[] GetEquipableRace(int targetRaceId, int id, out Weapon weapon, out bool isEquipable)
    {
        isEquipable = false;
        List<string> raceNames = new List<string>();
        weapon = GetWeapon(id);
        if (weapon != null)
        {
            for (int i = 0; i < m_GameElement.races.Count; i++)
            {
                if (m_GameElement.races[i].equipWeapon.Equals(id))
                {
                    raceNames.Add(m_GameElement.races[i].name);
                    if (targetRaceId.Equals(m_GameElement.races[i].id))
                    {
                        isEquipable = true;
                    }
                }
            }
        }
        return raceNames.ToArray();
    }

    public void GetWeaponAttack(int weaponID, ref int derictAtk, ref int inderictAtk)
    {
        Weapon weapon = m_GameElement.m_Weapons[weaponID];
        if (weapon != null)
        {
            derictAtk = weapon.directAtk;
            inderictAtk = weapon.indirectAtk;
        }
    }

    public int GetWeaponAttack(int weaponID, bool isDirect)
    {
        Weapon weapon = m_GameElement.m_Weapons[weaponID];
        if (weapon != null)
        {
            if (isDirect)
            {
                return weapon.directAtk;
            }
            return weapon.indirectAtk;
        }
        return 0;
    }
}
