using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitListUI : IGameUISystem
{
    [Header("Prefab")]
    [SerializeField]
    private CharacterSelection m_CharacterSelectionPrefab;

    [Header("UI Item")]
    [SerializeField]
    private RectTransform m_UserPanel;
    [SerializeField]
    private GameObject m_UserContent;
    [SerializeField]
    private RectTransform m_EnemyPanel;
    [SerializeField]
    private GameObject m_EnemyContent;
    [SerializeField]
    private RectTransform m_UserPlayerList;
    [SerializeField]
    private RectTransform m_EnemyPlayerList;
    [SerializeField]
    private Text m_TurnText;

    [SerializeField]
    private Button m_UserPanelButton;
    [SerializeField]
    private Button m_EnemyPanelButton;
    [SerializeField]
    private Button m_ConfirmButton = default;

    private List<CharacterSelection> m_PlayerList = new List<CharacterSelection>();
    private List<CharacterSelection> m_EnemyList = new List<CharacterSelection>();

    public override void GameSetting()
    {
        m_UserPanelButton.onClick.AddListener(()=> SetShowPanel(true));
        m_EnemyPanelButton.onClick.AddListener(() => SetShowPanel(false));
        m_ConfirmButton.onClick.AddListener(HideUI);
    }
    protected override void ShowEvent()
    {
        SetUI();
    }
    private void SetUI()
    {
        m_TurnText.text = string.Format("ターン{0}", m_GameManager.m_TurnCount);
        SetCharacterList(m_GameManager.userPlayers, m_UserPlayerList, m_PlayerList, false);
        SetCharacterList(m_GameManager.enemyPlayers, m_EnemyPlayerList, m_EnemyList, true);
        SetShowPanel(true);
    }

    private void SetCharacterList(Dictionary<int, Player> players, RectTransform content, List<CharacterSelection> playerList, bool isSkipDead)
    {
        ClearCharacterList(playerList);
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            if (isSkipDead && player.m_Hp <= 0)
            {
                continue;
            }
            CharacterSelection item = Instantiate(m_CharacterSelectionPrefab, content, false);
            item.SetText(player.m_PlayerName, m_GameManager.GetRace(player.m_Race).name, player.m_Level, player.m_Hp, player.m_MaxHP, player.m_IsActable, player.transform.position);
            item.SetClickEvent(HideUI);
            playerList.Add(item);
        }
    }

    private void ClearCharacterList(List<CharacterSelection> playerList)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            Destroy(playerList[i].gameObject);
        }
        playerList.Clear();
    }

    private void SetShowPanel(bool isPlayer)
    {
        if (isPlayer)
        {
            m_UserPanel.SetAsLastSibling();
        }
        else
        {
            m_EnemyPanel.SetAsLastSibling();
        }
        m_UserContent.gameObject.SetActive(isPlayer);
        m_EnemyContent.gameObject.SetActive(!isPlayer);
    }
}
