using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : ItemUI
{
    private List<ItemSelection> m_WeaponSelections = new List<ItemSelection>();
    private bool m_IsWeapon = false;
    public override void GameSetting()
    {
        m_ButtonUseItem.onClick.AddListener(BuyItemWeapon);
        m_ConfirmButton.onClick.AddListener(HideUI);
    }

    protected override void HideEvent()
    {
        //TODO SetShopDialog

    }
    protected override void SetItem()
    {
        m_ItemGold.text = $"所持金 <color=yellow>{m_GameManager.m_PlayerGold}</color> Gold";

        m_ButtonUseItem.enabled = false;

        ClearItemList();
        List<int> shopItems = m_GameManager.m_ShopItemList;
        List<int> shopWeapons = m_GameManager.m_ShopWeaponList;

        for (int i = 0; i < shopItems.Count; i++)
        {
            Item selItem = m_GameManager.GetItem(shopItems[i]);
            ItemSelection newObject = Instantiate(m_ItemUIPrefab, m_ItemList, false);
            string itemTypeName = "";
            switch (selItem.itemType)
            {
                case ItemType.Cure:
                    itemTypeName = "<color=red>回復</color>";
                    break;
                case ItemType.Resurge:
                    itemTypeName = "<color=red>復活</color>";
                    break;
                case ItemType.Special:
                    itemTypeName = "<color=lightblue>特殊</color>";
                    break;
            }
            newObject.SetItemInfo(this, shopItems[i], selItem.name, itemTypeName, selItem.price);
            m_ItemSelections.Add(newObject);
        }

        for (int i = 0; i < shopWeapons.Count; i++)
        {
            Weapon selWeapon = m_GameManager.GetWeapon(shopWeapons[i]);
            ItemSelection newObject = Instantiate(m_ItemUIPrefab, m_ItemList, false);
            List<string> weaponRangeText = new List<string>();
            if (selWeapon.directAtk > 0)
            {
                weaponRangeText.Add("<color=orange>直接</color>");
            }
            if (selWeapon.indirectAtk > 0)
            {
                weaponRangeText.Add("<color=lime>間接</color>");
            }
            newObject.SetItemInfo(this, shopItems[i], selWeapon.name, string.Join("<color=white>/</color>", weaponRangeText.ToArray()), selWeapon.price, true);
            m_WeaponSelections.Add(newObject);
        }
    }

    public override void SetSelectedItem(int id, bool isWeapon)
    {
        if (m_ItemSelectedId.Equals(id))
        {
            m_ItemSelectedId = -1;
            m_ItemNotice.text = "";
        }
        else
        {
            m_ItemSelectedId = id;
        }
        m_IsWeapon = isWeapon;
        for (int i = 0; i < m_WeaponSelections.Count; i++)
        {
            m_WeaponSelections[i].SetSelectColor(m_IsWeapon && m_WeaponSelections[i].m_Id.Equals(m_ItemSelectedId));
        }
        for (int i = 0; i < m_ItemSelections.Count; i++)
        {
            m_ItemSelections[i].SetSelectColor(!m_IsWeapon && m_ItemSelections[i].m_Id.Equals(m_ItemSelectedId));
        }

        m_ButtonUseItem.enabled = false;

        int price = 999999999;

        if (m_IsWeapon)
        {
            Weapon selectWeapon = m_GameManager.GetWeapon(m_ItemSelectedId);
            if (selectWeapon != null)
            {
                price = selectWeapon.price;
                m_ItemNotice.text = selectWeapon.notice;
            }
        }
        else
        {
            Item selectItem = m_GameManager.GetItem(m_ItemSelectedId);
            if (selectItem != null)
            {
                price = selectItem.price;
                m_ItemNotice.text = selectItem.notice;
            }
        }

        if (price <= m_GameManager.m_PlayerGold)
        {
            m_ButtonUseItem.enabled = true;
        }
    }

    private void BuyItemWeapon()
    {
        if (m_GameManager.BuyItem(m_ItemSelectedId, m_IsWeapon))
        {
            HideUI();
        }
    }

    protected override void ClearItemList()
    {
        base.ClearItemList();
        m_WeaponSelections.Clear();
    }
}
