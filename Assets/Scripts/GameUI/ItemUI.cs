using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : IGameUISystem
{
    protected static ElementManager m_ElementManager { get { return GameMidiator.m_Instance.m_ElementManager; } }
    protected static PlayerDataManager m_PlayerDataManager { get { return GameMidiator.m_Instance.m_PlayerDataManager; } }


    [Header("Prefab")]
    [SerializeField]
    protected ItemSelection m_ItemUIPrefab;

    [Header("UI Item")]
    [SerializeField]
    protected int m_ItemSelectedId = -1;
    [SerializeField]
    protected Text m_ItemGold;
    [SerializeField]
    protected Text m_ItemNotice;
    [SerializeField]
    protected RectTransform m_ItemList;
    [SerializeField]
    protected Button m_ButtonUseItem;
    [SerializeField]
    protected Button m_ButtonSellItem;
    [SerializeField]
    protected Button m_ConfirmButton = default;

    protected List<ItemSelection> m_ItemSelections = new List<ItemSelection>();

    public override void GameSetting()
    {
        m_ButtonUseItem.onClick.AddListener(UseItem);
        m_ButtonSellItem.onClick.AddListener(SellItem);
        m_ConfirmButton.onClick.AddListener(HideUI);
    }

    protected override void ShowEvent()
    {
        m_ItemSelectedId = -1;
        m_ItemNotice.text = "";
        SetItem();
    }
    protected virtual void SetItem()
    {
        int tempSelID = m_ItemSelectedId;
        m_ItemSelectedId = -1;
        m_ItemGold.text = $"所持金 <color=yellow>{ m_PlayerDataManager.m_PlayerGold}</color> Gold";

        m_ButtonUseItem.enabled = false;
        m_ButtonSellItem.enabled = false;

        ClearItemList();
        Dictionary<int, int> playerItems = m_PlayerDataManager.m_PlayerItems;
        foreach (var item in playerItems)
        {
            if (item.Value > 0)
            {
                Item setItem = GameMidiator.m_Instance.m_ElementManager.GetItem(item.Key);
                ItemSelection newObject = Instantiate(m_ItemUIPrefab, m_ItemList, false);
                string typeName = "";
                switch (setItem.itemType)
                {
                    case ItemType.Cure:
                        typeName = "回復";
                        break;
                    case ItemType.Resurge:
                        typeName = "復活";
                        break;
                    case ItemType.Special:
                        typeName = "特殊";
                        break;
                }
                newObject.SetItemInfo(this, item.Key, setItem.name, typeName, item.Value);
                m_ItemSelections.Add(newObject);
            }
        }
        SetSelectedItem(tempSelID, false);
    }

    public virtual void SetSelectedItem(int id, bool isWeapon)
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
        Item selectItem = GameMidiator.m_Instance.m_ElementManager.GetItem(m_ItemSelectedId);
        if (selectItem != null)
        {
            m_ItemNotice.text = $"<color=yellow>{selectItem.name}</color>\r\n{selectItem.notice}";
            Player player = GameManager.m_Instance.GetSelectedPlayer();
            if (player != null)
            {
                switch (selectItem.itemType)
                {
                    case ItemType.Cure:
                        if (player.m_Hp > 0 && player.m_Hp < player.m_MaxHP)
                        {
                            m_ButtonUseItem.enabled = true;
                        }
                        break;
                    case ItemType.Resurge:
                        if (player.m_Hp == 0)
                        {
                            m_ButtonUseItem.enabled = true;
                        }
                        break;
                    case ItemType.Special:
                        if (player.m_Hp > 0)
                        {
                            if (!selectItem.m_IsKeyItem && (selectItem.useCharType == -1 || (selectItem.useCharType == player.m_Race && player.m_Level >= 10)))
                            {
                                m_ButtonUseItem.enabled = true;
                            }
                        }
                        break;
                }
            }
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

    protected virtual void SellItem()
    {
        if (m_PlayerDataManager.SellItem(m_ItemSelectedId))
        {
            SetItem();
        }
    }

    private void UseItem()
    {
        if (m_PlayerDataManager.UseItem(m_ItemSelectedId))
        {
            HideUI();
        }
    }

    protected virtual void ClearItemList()
    {
        for (int i = 0; i < m_ItemSelections.Count; i++)
        {
            Destroy(m_ItemSelections[i].gameObject);
        }
        m_ItemSelections.Clear();
    }
}
