using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelection : MonoBehaviour
{
    private ItemUI m_ItemUI;

    [SerializeField]
    private Image m_Image = default;
    [SerializeField]
    private Button m_Button = default;
    [SerializeField]
    private Text itemName;
    [SerializeField]
    private Text itemType;
    [SerializeField]
    private Text itemCount;

    public bool m_IsWeapon;
    public bool isShop;
    public int m_Id { get; private set; }

    public void ClickAction()
    {
        //if (isShop)
        //{
        //    if (GameManager.instance.shopSelectedId == transform.name)
        //    {
        //        GameManager.instance.shopSelectedId = "";
        //    }
        //    else
        //    {
        //        GameManager.instance.shopSelectedId = transform.name;
        //    }
        //    GameManager.instance.SetShop();
        //}
        //else
        //{
        //    if (isWeapon)
        //    {
        //        if (GameManager.instance.itemSelectedId.ToString() == transform.name)
        //        {
        //            GameManager.instance.weaponSelectedId = -1;
        //        }
        //        else
        //        {
        //            GameManager.instance.weaponSelectedId = Convert.ToInt32(transform.name);
        //        }
        //        GameManager.instance.SetWeapon();
        //    }
        //    else
        //    {
        //        if (GameManager.instance.itemSelectedId.ToString() == transform.name)
        //        {
        //            GameManager.instance.itemSelectedId = -1;
        //        }
        //        else
        //        {
        //            GameManager.instance.itemSelectedId = Convert.ToInt32(transform.name);
        //        }
        //        GameManager.instance.SetItem();
        //    }
        //}
        m_ItemUI.SetSelectedItem(m_Id, m_IsWeapon);
    }

    public void SetItemInfo(string name, string type, int count)
    {
        itemName.text = name;
        itemType.text = type;
        itemCount.text = count.ToString();
    }

    public void SetItemInfo(ItemUI itemUI, int id, string name, string type, int count, bool isWeapon = false)
    {
        m_ItemUI = itemUI;
        m_Id = id;
        itemName.text = name;
        itemType.text = type;
        itemCount.text = count.ToString();
        m_IsWeapon = isWeapon;
        m_Button.onClick.AddListener(ClickAction);
    }

    public void SetSelectColor(bool isSelected)
    {
        Color newColor = m_Image.color;
        newColor.a = isSelected ? 0.5f : 1f;
        m_Image.color = newColor;
    }
}
