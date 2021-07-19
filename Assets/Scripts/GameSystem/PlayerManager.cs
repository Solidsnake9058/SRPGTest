using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : IGameItem
{
    [SerializeField]
    private Transform m_PlayerTransform = default;
    [SerializeField]
    private Transform m_PlayerUITransform = default;

    [SerializeField]
    private PlayerUI m_PlayerUIPrefab = default;
    public Dictionary<int, Player> m_DicUserPlayers { get; private set; }
    public List<Player> m_UserPlayers { get; private set; }

    public Dictionary<int, Player> m_DicEnemyPlayers { get; private set; }
    public List<Player> m_EnemyPlayers { get; private set; }

    public List<Player> m_ActorPlayers { get; private set; }
    private List<int> m_DefeatedEnemyList = new List<int>();

    private string m_UserPlayerNameFormat = "UserPlayer{0}";
    private string m_EnemyPlayerNameFormat = "EnemyPlayer{0}";

    public override void GameSetting()
    {
        m_DicUserPlayers = new Dictionary<int, Player>();
        m_UserPlayers = new List<Player>();
        m_DicEnemyPlayers = new Dictionary<int, Player>();
        m_EnemyPlayers = new List<Player>();
        m_ActorPlayers = new List<Player>();
    }

    public void ClearPlayer()
    {
        if (m_DicUserPlayers != null)
        {
            for (int i = 0; i < m_DicUserPlayers.Count; i++)
            {
                Destroy(m_DicUserPlayers[i].gameObject);
            }
        }
        if (m_DicEnemyPlayers != null)
        {
            for (int i = 0; i < m_DicEnemyPlayers.Count; i++)
            {
                Destroy(m_DicEnemyPlayers[i].gameObject);
            }
        }
        m_DicUserPlayers.Clear();
        m_DicEnemyPlayers.Clear();
    }

    public void GenetarePlayers(List<PlayerRecord> playerRecords)
    {
        for (int i = 0; i < playerRecords.Count; i++)
        {
            PlayerRecord playerRecord = playerRecords[i];
            if (playerRecord.isEnemy && m_DefeatedEnemyList.Contains(i))
            {
                continue;
            }
            Vector3 tilePos = m_GameManager.GetMapTile(playerRecord.locX, playerRecord.locY).HexTilePos();
            CharacterTemplate playerData = m_GameManager.GetPlayerType(playerRecord.characterId, playerRecord.isEnemy);
            Player playerPrefab = playerRecord.isEnemy ? PlayerPrefabHolder.instance.m_EnemyPlayerPrefab : PlayerPrefabHolder.instance.m_UserPlayerPrefab;
            Player player = Instantiate(playerPrefab, new Vector3(tilePos.x, 0, tilePos.z), Quaternion.Euler(new Vector3(0, 180, 0)), m_PlayerTransform);
            string nameFormat = playerRecord.isEnemy ? m_EnemyPlayerNameFormat : m_UserPlayerNameFormat;
            player.gameObject.name = string.Format(nameFormat, i);

            player.SetPlayerModel();
            player.SetPlayerValue(playerRecord, playerData, tilePos);
            player.playerIndex = i;

            PlayerUI playerUI = Instantiate(m_PlayerUIPrefab, new Vector3(tilePos.x, 1, tilePos.z), m_PlayerUIPrefab.transform.rotation, m_PlayerUITransform);
            playerUI.player = player;
            playerUI.SetPlayerIndex(playerRecord.isEnemy, i);
            playerUI.gameObject.name = string.Format(nameFormat + "UI", i);


            if (playerRecord.isEnemy)
            {
                m_DicEnemyPlayers.Add(player.playerIndex, player);
                m_EnemyPlayers.Add(player);
            }
            else
            {
                m_DicUserPlayers.Add(player.playerIndex, player);
                m_UserPlayers.Add(player);
            }
        }

        ScreenController.instance.SetPlayerUIs();
    }

    public void HidePlayers()
    {
        for (int i = 0; i < m_UserPlayers.Count; i++)
        {
            if (m_UserPlayers[i].hp <= 0)
            {
                continue;
            }
            m_UserPlayers[i].HidePlayer();
        }
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            if (m_EnemyPlayers[i].hp <= 0)
            {
                continue;
            }
            m_EnemyPlayers[i].HidePlayer();
        }
    }

    public void ShowPlayers()
    {
        for (int i = 0; i < m_UserPlayers.Count; i++)
        {
            if (m_UserPlayers[i].hp <= 0)
            {
                continue;
            }
            m_UserPlayers[i].ShowPlayer();
        }
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            if (m_EnemyPlayers[i].hp <= 0)
            {
                continue;
            }
            m_EnemyPlayers[i].ShowPlayer();
        }
    }

    public void RemoveDeadEnemy()
    {
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            Player player = m_EnemyPlayers[i];
            if (player.hp <= 0)
            {
                ScreenController.instance.RemoveUI(player.m_IsEnemy, player.playerIndex);
                m_DicEnemyPlayers.Remove(m_EnemyPlayers[i].playerIndex);
                m_EnemyPlayers.Remove(m_EnemyPlayers[i]);
                Destroy(player.gameObject);
            }
        }
    }

    public bool CheckUserPlayerAlive()
    {
        for (int i = 0; i < m_UserPlayers.Count; i++)
        {
            if (m_UserPlayers[i].hp > 0)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckEnemyPlayerAlive()
    {
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            if (m_EnemyPlayers[i].hp > 0)
            {
                return true;
            }
        }
        return false;
    }
}