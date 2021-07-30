using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private Transform m_UIBase = default;

    [SerializeField]
    private Image m_PlayerHP = default;
    [SerializeField]
    private Text m_PlayerHPText = default;
    [SerializeField]
    private Text m_PlayerEnable = default;
    [SerializeField]
    private float m_StandradUIHeight = 1f;
    private bool m_IsEnemy = default;
    private int m_PlayerIndex = default;

    public void SetPosition(Vector3 pos)
    {
        pos.y = m_StandradUIHeight;
        transform.position = pos;
    }
    public void RefreshUI(float hp, float hpMax, PlayerState playerState)
    {
        m_PlayerHP.fillAmount = hp / hpMax;
        m_PlayerHPText.text = $"{hp:0}/{hpMax:0}";
        m_PlayerEnable.enabled = !playerState.Equals(PlayerState.Active);
        m_PlayerEnable.text = hp > 0 ? "<color=lightblue>E</color>" : "<color=red>D</color>";
    }

    public void SetShowUI()
    {
        m_UIBase.localPosition = Vector3.zero;
    }

    public void SetHideUI()
    {
        m_UIBase.localPosition = new Vector3(0,-100,0);
    }

    public void SetPlayerIndex(bool isEnemy, int index)
    {
        m_IsEnemy = isEnemy;
        m_PlayerIndex = index;
    }

    public bool CheckID(bool isEnemy, int index)
    {
        return !(m_IsEnemy & isEnemy) && m_PlayerIndex.Equals(index);
    }
}
