using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : IGameItem
{
    protected static GameUIManager m_GameUIManager { get { return GameMidiator.m_Instance.m_GameUIManager; } }

    [SerializeField]
    private Transform m_PlayerTransform = default;
    [SerializeField]
    private Transform m_PlayerUITransform = default;

    [SerializeField]
    private Transform m_ActorPlayerTransform = default;

    [SerializeField]
    private PlayerUI m_PlayerUIPrefab = default;
    public Dictionary<int, Player> m_DicUserPlayers { get; private set; }
    public List<Player> m_UserPlayers { get; private set; }
    public int UserPlayerCount => m_UserPlayers != null ? m_UserPlayers.Count : 0;

    public Dictionary<int, Player> m_DicEnemyPlayers { get; private set; }
    public List<Player> m_EnemyPlayers { get; private set; }
    public int EnemyPlayerCount => m_EnemyPlayers != null ? m_EnemyPlayers.Count : 0;


    public Dictionary<int, Player> m_DicActorPlayers { get; private set; }

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
        m_DicActorPlayers = new Dictionary<int, Player>();
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
            Vector3 tilePos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(playerRecord.locX, playerRecord.locY).HexTilePos();
            CharacterTemplate playerData = GameMidiator.m_Instance.m_ElementManager.GetPlayerType(playerRecord.characterId, playerRecord.isEnemy);
            Player playerPrefab = playerRecord.isEnemy ? PlayerPrefabHolder.instance.m_EnemyPlayerPrefab : PlayerPrefabHolder.instance.m_UserPlayerPrefab;
            Player player = Instantiate(playerPrefab, m_PlayerTransform);
            string nameFormat = playerRecord.isEnemy ? m_EnemyPlayerNameFormat : m_UserPlayerNameFormat;
            player.gameObject.name = string.Format(nameFormat, i);

            player.SetPlayerModel();
            player.SetPlayerValue(playerRecord, playerData);
            player.SetPivot(playerRecord.scenarionActorPivotType);
            player.SetPivot();
            player.playerIndex = i;

            PlayerUI playerUI = Instantiate(m_PlayerUIPrefab, m_PlayerUITransform);
            playerUI.gameObject.name = string.Format(nameFormat + "UI", i);
            player.SetPlayerUI(playerUI);
            playerUI.SetPlayerIndex(playerRecord.isEnemy, i);
            m_GameUIManager.m_ScreenControlUI.SetPlayerUI(playerUI);

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
    }

    public void HidePlayers()
    {
        for (int i = 0; i < m_UserPlayers.Count; i++)
        {
            if (m_UserPlayers[i].m_Hp <= 0)
            {
                continue;
            }
            m_UserPlayers[i].HidePlayer();
        }
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            if (m_EnemyPlayers[i].m_Hp <= 0)
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
            if (m_UserPlayers[i].m_Hp <= 0)
            {
                continue;
            }
            m_UserPlayers[i].ShowPlayer();
        }
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            if (m_EnemyPlayers[i].m_Hp <= 0)
            {
                continue;
            }
            m_EnemyPlayers[i].ShowPlayer();
        }
        RemoveActorPlayers();
    }

    private void RemoveActorPlayers()
    {
        for (int i = 0; i < m_ActorPlayers.Count; i++)
        {
            Destroy(m_ActorPlayers[i].gameObject);
        }
        m_ActorPlayers.Clear();
    }

    public void RemoveDeadEnemy()
    {
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            Player player = m_EnemyPlayers[i];
            if (player.m_Hp <= 0)
            {
                m_GameUIManager.m_ScreenControlUI.RemoveUI(player.m_IsEnemy, player.playerIndex);
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
            if (m_UserPlayers[i].m_Hp > 0)
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
            if (m_EnemyPlayers[i].m_Hp > 0)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckUserPlayerTile(HexCoord hex)
    {
        for (int i = 0; i < m_UserPlayers.Count; i++)
        {
            if (m_UserPlayers[i].m_Hex.Equals(hex))
            {
                return true;
            }
        }
        return false;
    }

    public List<Player> GetUserPlayers(List<HexCoord> hexes, bool isCheckHex = true, bool isCheckAlive = true)
    {
        List<Player> result = new List<Player>();
        for (int i = 0; i < m_UserPlayers.Count; i++)
        {
            if ((!isCheckAlive || m_UserPlayers[i].m_Hp > 0) && (!isCheckHex || hexes.Contains(m_UserPlayers[i].m_Hex)))
            {
                result.Add(m_UserPlayers[i]);
            }
        }
        return result;
    }

    public List<Player> GetEnemyPlayers(List<HexCoord> hexes, bool isCheckHex = true, bool isCheckAlive = true)
    {
        List<Player> result = new List<Player>();
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            if ((!isCheckAlive || m_EnemyPlayers[i].m_Hp > 0) && (!isCheckHex || hexes.Contains(m_EnemyPlayers[i].m_Hex)))
            {
                result.Add(m_EnemyPlayers[i]);
            }
        }
        return result;
    }

    public List<HexCoord> GetPlayerHexes(HexCoord hexExclude)
    {
        List<HexCoord> hexes = new List<HexCoord>();
        for (int i = 0; i < m_UserPlayers.Count; i++)
        {
            if (!m_UserPlayers[i].m_Hex.Equals(hexExclude))
            {
                hexes.Add(m_UserPlayers[i].m_Hex);
            }
        }
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            if (m_EnemyPlayers[i].m_Hp > 0 && !m_EnemyPlayers[i].m_Hex.Equals(hexExclude))
            {
                hexes.Add(m_EnemyPlayers[i].m_Hex);
            }
        }

        return hexes;
    }

    public Player GetPlayer(HexCoord hex)
    {
        for (int i = 0; i < m_UserPlayers.Count; i++)
        {
            if (m_UserPlayers[i].m_Hex.Equals(hex))
            {
                return m_UserPlayers[i];
            }
        }
        for (int i = 0; i < m_EnemyPlayers.Count; i++)
        {
            if (m_EnemyPlayers[i].m_Hex.Equals(hex))
            {
                return m_EnemyPlayers[i];
            }
        }
        return null;
    }

    public bool CheckHexTileEmpty(HexCoord hex)
    {
        List<HexCoord> hexes = GetPlayerHexes(new HexCoord(999, 999));
        return hexes.Contains(hex);
    }

    public List<HexCoord> GetEmptyHex(List<HexCoord> range)
    {
        List<HexCoord> hexes = new List<HexCoord>();
        List<HexCoord> playerHexes = GetPlayerHexes(new HexCoord(999, 999));
        for (int i = 0; i < range.Count; i++)
        {
            if (!playerHexes.Contains(range[i]))
            {
                hexes.Add(range[i]);
            }
        }
        return hexes;
    }

    public Player GetPlayerByID(int id)
    {
        return m_DicUserPlayers.ContainsKey(id) ? m_DicUserPlayers[id] : null;
    }

    public Player GetPlayerByList(int index)
    {
        return index < m_UserPlayers.Count ? m_UserPlayers[index] : null;
    }

    public Player GetEnemyByList(int index)
    {
        return index < m_EnemyPlayers.Count ? m_EnemyPlayers[index] : null;
    }

    public bool CheckPlayerAction(Player player)
    {
        if (player.GetIsCanHeal())
        {
            List<HexCoord> hexTiles = GetUserTargetHex(player, true);
            if (hexTiles.Count>0)
            {
                return true;
            }
        }
        List<HexCoord> hexAtkTiles = GetUserTargetHex(player, false);
        if (hexAtkTiles.Count > 0)
        {
            return true;
        }
        return false;
    }

    private List<HexCoord> GetUserTargetHex(Player player, bool isHeal)
    {
        List<HexCoord> hexes = new List<HexCoord>();
        List<HexCoord> result = new List<HexCoord>();
        hexes = isHeal ? player.GetHealRange() : player.GetAttackRangeHex();
        for (int i = 0; i < hexes.Count; i++)
        {
            Player targetPlayer = GetPlayer(hexes[i]);
            if (targetPlayer && ((isHeal && !targetPlayer.m_IsEnemy && targetPlayer.m_Hp < targetPlayer.m_MaxHP) ||
                (!isHeal && targetPlayer.m_IsEnemy && targetPlayer.m_Hp > 0)))
            {
                result.Add(targetPlayer.m_Hex);
            }
        }
        return result;
    }

    #region Scenario
    public void GenetareActionPlayer(List<PlayerRecord> playerRecords)
    {
        for (int i = 0; i < playerRecords.Count; i++)
        {
            PlayerRecord temp = playerRecords[i];
            ActorPlayer player = Instantiate(PlayerPrefabHolder.instance.m_ActorPlayerPrefab, m_ActorPlayerTransform);
            player.playerIndex = temp.id;
            player.gameObject.name = temp.id.ToString();
            player.SetPivot(temp.scenarionActorPivotType);
            player.SetPivot();
            player.SetPlayerModel();
            player.SetPlayerValue(temp, null);
            m_ActorPlayers.Add(player);
            m_DicActorPlayers.Add(temp.id, player);
        }
    }

    public void TurnActive(bool isUserPlayer)
    {
        List<Player> players = isUserPlayer ? m_UserPlayers : m_EnemyPlayers;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].TurnActive();
        }
    }


    public Player GetActorPlayerByID(int id)
    {
        return m_DicActorPlayers.ContainsKey(id) ? m_DicActorPlayers[id] : null;
    }

    public Player MoveActorToTarget(int actorId, HexCoord hex, ScenarionActorPivotType pivotType)
    {
        Player player = GetActorPlayerByID(actorId);
        player.SetHexTarget(hex);
        player.SetPivot(pivotType);
        if (!player.m_Hex.Equals(hex))
        {
            player.SetPositionQueue(GameMidiator.m_Instance.m_StageMapManager.FindPath(player.m_Hex, hex, true));
            return player;
        }
        player.SetPivot();
        return null;
    }
    #endregion
}
