using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    public Text playerName;
    public Text playerClass;
    public Text playerLevel;
    public Text playerHP;
    public Text playerActable;

    public void CilckActoin()
    {
        ScreenController.instance.SetCameraPos(new Vector3((float)Convert.ToDouble(name.Split(',')[0]), 0, (float)Convert.ToDouble(name.Split(',')[1])));
        GameManager.instance.DisableGroup(GameManager.instance.unitGroup);
    }

    public void SetText(string pName, string pClass, int pLevel, int pHP, int pMaxHP, bool pActable, Vector3 pPos)
    {
        playerName.text = pName;
        playerClass.text = pClass;
        playerLevel.text = pLevel.ToString();
        playerHP.text = string.Format("{0,3}/{1,3}",pHP, pMaxHP);
        playerActable.text = "<color=" + (pHP > 0 ? (pActable ? "lightblue>移動可" : "lime>移動済") : "red>行動不可") + "</color>";
        name = string.Format("{0},{1}", pPos.x, pPos.z);
    }

}
