using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrowButton : IGameItem,IPointerDownHandler,IPointerUpHandler,IPointerEnterHandler,IPointerExitHandler {

    protected static GameUIManager m_GameUIManager { get { return GameMidiator.m_Instance.m_GameUIManager; } }

    [SerializeField]
    private Image m_Image;
    [SerializeField]
    private PivotType m_Pivot = PivotType.Right;
    private PivotType m_PivotOri;
    private bool isPress = false;

    public override void GameSetting()
    {
        m_PivotOri = m_Pivot;
        HideImage();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_Image.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideImage();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isPress = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isPress = false;
    }

    public void HideImage()
    {
        m_Image.color = Color.clear;
    }

    public void NextPivot()
    {
        m_Pivot = m_Pivot.Next();
    }

    public void LastPivot()
    {
        m_Pivot = m_Pivot.Last();
    }

    public void Reset()
    {
        m_Pivot = m_PivotOri;
    }


    public override void SystemUpdate()
    {
        if (isPress)
        {
            m_GameUIManager.m_ScreenControlUI.MoveCamera(m_Pivot);
        }
    }
}
