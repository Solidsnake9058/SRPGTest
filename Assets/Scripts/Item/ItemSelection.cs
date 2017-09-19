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

    public void SetItemInfo(string name, ItemType type, int count)
    {
        itemName.text = name;
        itemType.text = Enum.GetName(type.GetType(), type);
        itemCount.text = count.ToString();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
