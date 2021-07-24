using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : IGameUISystem
{
    [Header("UI Item")]
    [SerializeField]
    private Text m_PlayerName;
    [SerializeField]
    private Text m_DialogContent;
    [SerializeField]
    private Button m_ConfirmButton = default;

    [SerializeField]
    private float m_ConfirmWaitTime = 1;

    public override void GameSetting()
    {
        m_ConfirmButton.onClick.AddListener(ConfirmDialog);
    }

    private void ConfirmDialog()
    {
        HideUI();
        GameMidiator.m_Instance.m_ScenarionManager.DialogClick();
    }

    public void SetDialog(string name,string content)
    {
        m_PlayerName.text = name;
        m_DialogContent.text = content;
        ShowUI();
        StartCoroutine(SetWaitButton());
    }

    private IEnumerator SetWaitButton()
    {
        m_ConfirmButton.enabled = false;
        yield return new WaitForSeconds(m_ConfirmWaitTime);
        m_ConfirmButton.enabled = true;
    }
}
