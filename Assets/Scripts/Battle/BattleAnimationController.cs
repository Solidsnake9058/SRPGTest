using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimationController : MonoBehaviour
{
    private bool m_SendTargetCheck;

    public void SetSendTargetCheck(bool isPlayer)
    {
        m_SendTargetCheck = !isPlayer;
    }
    public void SendDamage()
    {
        BattleManager.m_Instance.SendDamage(m_SendTargetCheck);
    }

    public void RunPreactionFin()
    {
        BattleManager.m_Instance.RunPreactionFin();
    }

    public void EndBattle()
    {
        BattleManager.m_Instance.EndBattle();
    }
}
