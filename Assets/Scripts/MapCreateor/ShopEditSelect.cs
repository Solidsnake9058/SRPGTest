using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopEditSelect : MonoBehaviour
{
    public Text itemName;
    public Button removeBtn;

    public void SetName(string iName, bool isWeapon,int iId)
    {
        itemName.text = iName;
        removeBtn.name = string.Format("{0}:{1}", isWeapon ? "Weapon" : "Item", iId);
    }

    public void RemoveItem(Button btn)
    {
        string[] id = btn.name.Split(':');
        MapCreatorManager.instance.RemoveShopItem(id.Equals("Weapon"), id[1]);
    }
}
