using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingUI : IGameUISystem
{
    [SerializeField]
    private Toggle m_ShowTileLine;
    [SerializeField]
    private Toggle m_ShowPlayHP;
    [SerializeField]
    private Button m_ConfirmButton;

    public override void GameSetting()
    {
        m_ConfirmButton.onClick.AddListener(ConfirmClick);
    }

    protected override void ShowEvent()
    {
        m_ShowTileLine.isOn = SaveManager.GetIsShowTile();
        m_ShowPlayHP.isOn = SaveManager.GetIsShowPlayerUI();
    }

    private void ConfirmClick()
    {
        GameManager.m_Instance.GameSettingConfirm(m_ShowTileLine.isOn, m_ShowPlayHP.isOn);
        HideUI();
    }
}
