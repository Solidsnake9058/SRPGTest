using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class AIPlayer : Player
{
    public override void SetPlayerValue(PlayerRecord playerRecord, CharacterTemplate playerData)
    {
        base.SetPlayerValue(playerRecord, playerData);
        CharacterLevelTemplate playerLvData = playerData.GetCharacterLevelData((int)playerRecord.level);
        m_Level = (int)playerLvData.level;
        m_Exp = (int)playerLvData.exp;
        m_Hp = m_MaxHP = (int)playerLvData.hp;
        m_Gold = (int)playerLvData.gold;
        m_EquipWeapon = playerLvData.equipWeapon;
        m_EnemyAIType = playerRecord.aiType;
        m_SearchRange = playerRecord.searchRange;
        _IsEnemy = true;
    }

    public override void SystemUpdate()
    {
        if (TurnUpdate())
        {
            EnemyAction();
        }
    }

    protected override void SetPlayerDead()
    {
        HidePlayer();
    }

    private void EnemyAction()
    {
        if (m_IsMoving)
        {
            return;
        }
        //priority queue
        List<HexCoord> attackHexesDirect = HexCoord.HexRing(m_Hex, 1);
        List<HexCoord> attackHexesIndirect = HexCoord.HexRing(m_Hex, 2);
        List<HexCoord> moveRange = new List<HexCoord>();

        if (m_PlayerState.Equals(PlayerState.Action))
        {
            if (m_EnemyAIType == EnemyAIType.Attacker)
            {
                moveRange = m_StageMapManager.FindHighlight(m_Hex, m_MovementPerActionPoint, false);
                moveRange = m_PlayerManager.GetEmptyHex(moveRange);
            }
            else if (m_EnemyAIType == EnemyAIType.Defanser)
            {
                moveRange = m_StageMapManager.FindHighlight(m_Hex, m_SearchRange, false);
                moveRange = m_PlayerManager.GetEmptyHex(moveRange);
            }
        }

        List<HexCoord> attackHexesInRange = new List<HexCoord>();

        List<HexCoord> opponetsCanBeAttackDirect = new List<HexCoord>();
        List<HexCoord> opponetsCanBeAttackIndirect = new List<HexCoord>();
        List<HexCoord> opponetsNotCanBeCountDirect = new List<HexCoord>();
        List<HexCoord> opponetsNotCanBeCountIndirect = new List<HexCoord>();

        int directAtk = 0;
        int indirectAtk = 0;

        GetWeaponAttack(ref directAtk, ref indirectAtk);

        List<Player> opponets = new List<Player>();
        List<Player> opponetsNotCanBeCount = new List<Player>();
        List<Player> moveOpponets = new List<Player>();
        List<Player> moveOpponetsNotCanBeCount = new List<Player>();

        //Get player who can (not) counter in range
        if (directAtk > 0)
        {
            GetOpponet(attackHexesDirect, attackHexesInRange, opponets, opponetsNotCanBeCount, true);
            GetMoveOpponet(opponetsNotCanBeCountDirect, opponetsCanBeAttackDirect, moveOpponetsNotCanBeCount, true);
        }
        if (indirectAtk > 0)
        {
            GetOpponet(attackHexesIndirect, attackHexesInRange, opponets, opponetsNotCanBeCount, false);
            GetMoveOpponet(opponetsNotCanBeCountIndirect, opponetsCanBeAttackIndirect, moveOpponetsNotCanBeCount, false);
        }

        //Attack player who cannot counter
        if (AttackOpponent(opponetsNotCanBeCount))
        {
            return;
        }
        //Move to player who cannot counter in move range
        if (MoveToOpponent(opponetsNotCanBeCountDirect.Union(opponetsNotCanBeCountIndirect).Intersect(moveRange).ToList()))
        {
            return;
        }
        //Attack player in range
        if (AttackOpponent(opponets))
        {
            return;
        }
        //Move to player who can counter in move range
        if (MoveToOpponent(opponetsCanBeAttackDirect.Union(opponetsCanBeAttackIndirect).Intersect(moveRange).ToList()))
        {
            return;
        }

        if (m_PlayerState.Equals(PlayerState.Active) && m_EnemyAIType == EnemyAIType.Attacker)
        {
            //Move to nearist target
            List<HexCoord> opponentTiles = new List<HexCoord>();
            List<HexTilePath> opponentPaths = new List<HexTilePath>();

            int minDistance = 9999;
            List<Player> temp = m_PlayerManager.GetUserPlayers(null, false);
            for (int i = 0; i < temp.Count; i++)
            {
                Player userPlayer = temp[i];
                for (int j = 0; j < moveRange.Count; j++)
                {
                    int distance = HexCoord.HexDistance(moveRange[j], userPlayer.m_Hex);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        opponentTiles.Clear();
                        opponentTiles.Add(moveRange[j]);
                    }
                    else if (distance == minDistance)
                    {
                        opponentTiles.Add(moveRange[j]);
                    }
                }
            }
            opponentTiles = opponentTiles.OrderBy(x => HexCoord.HexDistance(m_Hex, x)).ToList();

            for (int i = 0; i < opponentTiles.Count; i++)
            {
                opponentPaths.Add(m_StageMapManager.FindPath(m_Hex, opponentTiles[i]));
            }

            HexTile destTile = null;
            float cost = 0;
            List<HexTile> path = new List<HexTile>();
            for (int i = 0; i < opponentPaths[0].listOfTiles.Count; i++)
            {
                HexTile tile = opponentPaths[0].listOfTiles[i];
                cost += tile.m_MovementCost;
                if (cost <= m_MovementPerActionPoint)
                {
                    destTile = tile;
                }
                else
                {
                    break;
                }
            }
            m_StageMapManager.HighlightTileAt(moveRange);
            GameManager.m_Instance.MoveCurrentPlayer(destTile);
            return;
        }
        //Nothing can do.
        TurnEnd();
    }

    private bool AttackOpponent(List<Player> opponets)
    {
        Player opponent = null;
        if (opponets.Count > 0)
        {
            opponent = opponets.OrderBy(x => x.m_Hp).FirstOrDefault();
            GameManager.m_Instance.AttackWithCurrentPlayer(opponent.m_Hex);
            if (m_EnemyAIType == EnemyAIType.Defanser)
            {
                m_EnemyAIType = EnemyAIType.Attacker;
            }
            return true;
        }
        return false;
    }

    private bool MoveToOpponent(List<HexCoord> moveTarget)
    {
        if (m_PlayerState.Equals(PlayerState.Active))
        {
            List<HexTilePath> opponentPaths = new List<HexTilePath>();
            List<Player> temp = m_PlayerManager.GetUserPlayers(null, false);
            List<HexCoord> opponentLeastHPTiles = new List<HexCoord>();

            //sort all target by distance and hp
            temp.OrderBy(x => HexCoord.HexDistance(x.m_Hex, m_Hex)).ThenBy(x => x.m_Hp);
            for (int i = 0; i < temp.Count; i++)
            {
                //get target in attack range after move
                opponentLeastHPTiles = GetAttackRangeWithTarget(temp[i].m_Hex, moveTarget);
                if (opponentLeastHPTiles != null && opponentLeastHPTiles.Count > 0)
                {
                    break;
                }
            }

            for (int i = 0; i < opponentLeastHPTiles.Count; i++)
            {
                opponentPaths.Add(m_StageMapManager.FindPath(m_Hex, opponentLeastHPTiles[i]));
            }

            if (opponentPaths.Count > 0)
            {
                //get highest defense rate tile and least step path
                opponentPaths = opponentPaths.OrderBy(x => -x.lastTile.m_DefenseRate).ThenBy(x => x.costOfPath).ToList();
                m_StageMapManager.HighlightTileAt(m_Hex, (int)m_MovementPerActionPoint, false);
                HexTile destTile = opponentPaths[0].lastTile;
                GameManager.m_Instance.MoveCurrentPlayer(destTile);
                GameManager.m_Instance.isSetCamera = true;
                return true;
            }
        }
        return false;
    }

    private void GetOpponet(List<HexCoord> attackTiles, List<HexCoord> attackTilesRange, List<Player> opponets, List<Player> opponetsNotCanBeCount, bool isDirect)
    {
        List<Player> temp = GameMidiator.m_Instance.m_PlayerManager.GetUserPlayers(attackTiles);
        if (temp.Count > 0)
        {
            attackTilesRange.AddRange(attackTiles);
        }
        for (int i = 0; i < temp.Count; i++)
        {
            opponets.Add(temp[i]);
            if (!temp[i].GetIsCanAttack(isDirect))
            {
                opponetsNotCanBeCount.Add(temp[i]);
            }
        }
    }

    private void GetMoveOpponet(List<HexCoord> attackTilesNotCanBeCount, List<HexCoord> attackTilesCanBeCount, List<Player> opponetsNotCanBeCount, bool isDirect)
    {
        List<Player> tempUser = GameMidiator.m_Instance.m_PlayerManager.GetUserPlayers(null, false);
        List<HexCoord> tempPlayers = GameMidiator.m_Instance.m_PlayerManager.GetPlayerHexes(new HexCoord(-999, -999));
        for (int i = 0; i < tempUser.Count; i++)
        {
            Player player = tempUser[i];
            List<HexCoord> playerRing = HexCoord.HexRing(player.m_Hex, isDirect ? 1 : 2);
            List<HexCoord> tiles = attackTilesCanBeCount;
            if (!player.GetIsCanAttack(isDirect))
            {
                opponetsNotCanBeCount.Add(player);
                tiles = attackTilesNotCanBeCount;
            }
            for (int j = 0; j < playerRing.Count; j++)
            {
                HexCoord hex = playerRing[j];
                if (!tiles.Contains(hex) && m_StageMapManager.GetMapTile(hex) != null && !tempPlayers.Contains(hex))
                {
                    tiles.Add(hex);
                }
            }
        }
    }

    public override void TurnEnd()
    {
        m_StageMapManager.RemoveHighlightTiles();
        m_Moving = false;
        m_Attacking = false;
        GameManager.m_Instance.NextEnemyTurn();

        base.TurnEnd();
    }

    public override void TurnOnGUI()
    {
        base.TurnOnGUI();
    }

}
