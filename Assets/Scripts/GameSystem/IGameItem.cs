using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IGameItem : MonoBehaviour
{
    protected GameManager m_GameManager = null;

    public virtual void Initialize(GameManager gameManager)
    {
        m_GameManager = gameManager;
    }

    public virtual void SystemUpdate() { }
    public virtual void SystemFixedUpdate() { }
    public virtual void SystemLateUpdate() { }
    public virtual void GameSetting() { }

    public virtual void Pause() { }
    public virtual void Resume() { }
}
