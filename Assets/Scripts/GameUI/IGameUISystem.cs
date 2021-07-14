using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IGameUISystem : IGameItem
{
    [SerializeField]
    private Transform m_UIBase = default;
    public bool m_IsShow { get { return m_UIBase ?? m_UIBase.gameObject.activeSelf; } }

    protected virtual void ShowEvent() { }
    protected virtual void HideEvent() { }

    public void ShowUI()
    {
        m_UIBase?.gameObject.SetActive(true);
        ShowEvent();
    }

    public void HideUI()
    {
        m_UIBase?.gameObject.SetActive(false);
        HideEvent();
    }
}
