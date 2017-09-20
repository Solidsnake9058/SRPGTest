using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelection : MonoBehaviour {
    public Text itemName;
    public Text itemType;
    public Text itemCount;
    public bool isWeapon;

    public void ClickAction()
    {
        if (isWeapon)
        {
            if (GameManager.instance.itemSelectedId.ToString() == transform.name)
            {
                GameManager.instance.weaponSelectedId = -1;
            }
            else
            {
                GameManager.instance.weaponSelectedId = Convert.ToInt32(transform.name);
            }
            GameManager.instance.SetWeapon();
        }
        else
        {
            if (GameManager.instance.itemSelectedId.ToString() == transform.name)
            {
                GameManager.instance.itemSelectedId = -1;
            }
            else
            {
                GameManager.instance.itemSelectedId = Convert.ToInt32(transform.name);
            }
            GameManager.instance.SetItem();
        }
    }

    public void SetItemInfo(string name, string type, int count)
    {
        itemName.text = name;
        itemType.text = type;
        itemCount.text = count.ToString();
    }
}
