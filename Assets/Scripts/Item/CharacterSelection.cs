using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField]
    private Text m_PlayerName;
    [SerializeField]
    private Text m_PlayerClass;
    [SerializeField]
    private Text m_PlayerLevel;
    [SerializeField]
    private Text m_PlayerHP;
    [SerializeField]
    private Text m_PlayerActable;
    [SerializeField]
    private Button m_ClickButton;
    private Vector3 m_FocusPos = Vector3.zero;
    private Action m_ClickEvent;

    public void SetClickEvent(Action action)
    {
        m_ClickEvent = action;
        m_ClickButton.onClick.AddListener(CilckActoin);
    }
    public void CilckActoin()
    {
        ScreenController.instance.SetCameraPos(m_FocusPos);
        m_ClickEvent?.Invoke();
        //GameManager.instance.DisableGroup(GameManager.instance.unitGroup);
    }

    public void SetText(string pName, string pClass, int pLevel, int pHP, int pMaxHP, bool pActable, Vector3 pPos)
    {
        m_PlayerName.text = pName;
        m_PlayerClass.text = pClass;
        m_PlayerLevel.text = pLevel.ToString();
        m_PlayerHP.text = string.Format("{0,3}/{1,3}",pHP, pMaxHP);
        m_PlayerActable.text = $"<color={(pHP > 0 ? (pActable ? "lightblue>移動可" : "lime>移動済") : "red>行動不可")}</color>";
        m_FocusPos = pPos;
        name = $"{pName}({pPos.x},{pPos.z})";
    }

}
