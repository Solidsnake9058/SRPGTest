﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUI : ItemUI
{
    public override void GameSetting()
    {
        m_ButtonUseItem.onClick.AddListener(EquipWeapon);
        m_ButtonSellItem.onClick.AddListener(SellItem);
        m_ConfirmButton.onClick.AddListener(HideUI);
    }

    protected override void SetItem()
    {
        m_ButtonUseItem.enabled = false;
        m_ButtonSellItem.enabled = false;

        ClearItemList();
        Dictionary<int, int> playerItems = m_PlayerDataManager.m_PlayerWeapons;
        foreach (var item in playerItems)
        {
            if (item.Value > 0)
            {
                Weapon selWeapon = m_ElementManager.GetWeapon(item.Key);
                ItemSelection newObject = Instantiate(m_ItemUIPrefab, m_ItemList, false);

                List<string> weaponRangeText = new List<string>();
                if (selWeapon.directAtk > 0)
                {
                    weaponRangeText.Add($"<color=orange>直接</color>");
                }
                if (selWeapon.indirectAtk > 0)
                {
                    weaponRangeText.Add($"<color=lime>間接</color>");
                }

                newObject.SetItemInfo(selWeapon.name, string.Join("<color=white>/</color>", weaponRangeText.ToArray()), item.Value);
                m_ItemSelections.Add(newObject);
            }
        }
    }

    public override void SetSelectedItem(int id, bool isWeapon)
    {
        if (m_ItemSelectedId.Equals(id))
        {
            m_ItemSelectedId = -1;
        }
        else
        {
            m_ItemSelectedId = id;
        }
        for (int i = 0; i < m_ItemSelections.Count; i++)
        {
            m_ItemSelections[i].SetSelectColor(m_ItemSelections[i].m_Id.Equals(m_ItemSelectedId));
        }

        m_ButtonUseItem.enabled = false;
        m_ButtonSellItem.enabled = false;
        bool isEquipable;
        Weapon selectItem;
        Player player = GameManager.m_Instance.GetSelectedPlayer();
        string[] raceNames = m_ElementManager.GetEquipableRace(player != null ? player.m_Race : -1, m_ItemSelectedId, out selectItem, out isEquipable);
        if (selectItem != null)
        {
            m_ItemNotice.text = $"<color=yellow>{selectItem.name}</color>\r\n{selectItem.notice}\r\n使えるクラスは、<color=yellow>{string.Join("</color><color=white>、</color><color=yellow>", raceNames)}</color>です。";
            m_ButtonUseItem.enabled = isEquipable;
            if (selectItem.price > 0)
            {
                m_ButtonSellItem.enabled = true;
            }
        }
        else
        {
            m_ItemNotice.text = "";
        }
    }

    private void EquipWeapon()
    {
        if (m_PlayerDataManager.EquipWeapon(m_ItemSelectedId))
        {
            HideUI();
        }
    }

    protected override void SellItem()
    {
        if (m_PlayerDataManager.SellWeapon(m_ItemSelectedId))
        {
            SetItem();
        }
    }
}
