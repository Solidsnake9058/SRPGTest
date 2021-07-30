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

        //TODO process-> state Active:check attack target to attack or move; state move:check attacktarget to attack.



        GameManager.m_Instance.isSetCamera = true;

        bool isGoToMove = false;

        //priority queue
        List<HexCoord> attackHexesDirect = HexCoord.HexRing(m_Hex, 1);
        List<HexCoord> attackHexesIndirect = HexCoord.HexRing(m_Hex, 2);

        List<HexCoord> moveRange = m_StageMapManager.FindHighlight(m_Hex, m_MovementPerActionPoint, false);
        moveRange = m_PlayerManager.GetEmptyHex(moveRange);
        List<HexCoord> moveRangeBySearchRange = m_StageMapManager.FindHighlight(m_Hex, m_SearchRange, false);
        moveRangeBySearchRange = m_PlayerManager.GetEmptyHex(moveRangeBySearchRange);

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

        opponets = new List<Player>();

        //Get player who cannot be counter in range
        if (directAtk > 0)
        {
            GetOpponet(attackHexesDirect, attackHexesInRange, opponets, opponetsNotCanBeCount, true);
        }

        if (indirectAtk > 0)
        {
            GetOpponet(attackHexesIndirect, attackHexesInRange, opponets, opponetsNotCanBeCount, false);
        }

        Player opponent = null;
        //Attack safe target
        if (opponetsNotCanBeCount.Count > 0)
        {
            opponent = opponetsNotCanBeCount.OrderBy(x => x.m_Hp).FirstOrDefault();
            m_StageMapManager.HighlightTileAt(attackHexesInRange);
            GameManager.m_Instance.AttackWithCurrentPlayer(opponent.m_Hex);

            m_IsAttackable = false;
        }
        else
        {
            //Move to player who cannot be counter in move range
            opponetsNotCanBeCount = new List<Player>();

            //Get all of players that cannot be counter
            if (directAtk > 0)
            {
                GetMoveOpponet(opponetsNotCanBeCountDirect, opponetsCanBeAttackDirect, opponetsNotCanBeCount, true);
            }
            if (indirectAtk > 0)
            {
                GetMoveOpponet(opponetsNotCanBeCountIndirect, opponetsCanBeAttackIndirect, opponetsNotCanBeCount, false);
            }

            //Get move target that in move range
            List<HexCoord> moveTarget = new List<HexCoord>();
            List<HexCoord> moveTargetNotCanBeConter = opponetsNotCanBeCountDirect.Union(opponetsNotCanBeCountIndirect).ToList();
            List<HexCoord> moveTargetCanBeConter = opponetsCanBeAttackDirect.Union(opponetsCanBeAttackIndirect).ToList();

            if (m_EnemyAIType == EnemyAIType.Attacker)
            {
                moveTarget = opponetsNotCanBeCountDirect.Union(opponetsNotCanBeCountIndirect).Intersect(moveRange).ToList();
                moveTargetNotCanBeConter = moveTargetNotCanBeConter.Intersect(moveRange).ToList();
                moveTargetCanBeConter = moveTargetCanBeConter.Intersect(moveRange).ToList();
            }
            else if (m_EnemyAIType == EnemyAIType.Defanser)
            {
                moveTarget = opponetsNotCanBeCountDirect.Union(opponetsNotCanBeCountIndirect).Intersect(moveRangeBySearchRange).ToList();
                moveTargetNotCanBeConter = moveTargetNotCanBeConter.Intersect(moveRangeBySearchRange).ToList();
                moveTargetCanBeConter = moveTargetCanBeConter.Intersect(moveRangeBySearchRange).ToList();
            }

            List<HexTilePath> opponentPathsNotCanBeCounter = new List<HexTilePath>();
            for (int i = 0; i < moveTargetNotCanBeConter.Count; i++)
            {
                opponentPathsNotCanBeCounter.Add(m_StageMapManager.FindPath(m_Hex, moveTargetNotCanBeConter[i]));
            }

            //Move to target
            if (opponentPathsNotCanBeCounter.Count > 0)
            {
                opponentPathsNotCanBeCounter = opponentPathsNotCanBeCounter.OrderBy(x => x.listOfTiles.Count).ToList();
                m_StageMapManager.HighlightTileAt(m_Hex, (int)m_MovementPerActionPoint, false);
                HexTile destTile = null;
                float cost = 0;
                for (int i = 0; i < opponentPathsNotCanBeCounter[0].listOfTiles.Count; i++)
                {
                    HexTile tile = opponentPathsNotCanBeCounter[0].listOfTiles[i];
                    cost += tile.m_MovementCost;
                    if (cost <= m_MovementPerActionPoint)
                    {
                        destTile = tile;
                    }
                }
                GameManager.m_Instance.MoveCurrentPlayer(destTile);
                m_IsMovable = false;
            }
            //Attack player in range
            else if (opponets.Count > 0)
            {
                opponent = opponets.OrderBy(x => -x.m_Hp).FirstOrDefault();
                if (opponent != null && opponent != default(Player))
                {
                    m_StageMapManager.HighlightTileAt(attackHexesInRange);
                    GameManager.m_Instance.AttackWithCurrentPlayer(opponent.m_Hex);
                    m_IsAttackable = false;
                }
            }
        }

        //if no player can be attack and cannot be counter player who not in move range
        //change action to moving?
        isGoToMove = m_IsAttackable ? (m_EnemyAIType == EnemyAIType.Stander ? false : m_IsMovable) : m_IsAttackable;

        if (isGoToMove)
        {
            List<HexCoord> opponentTiles = new List<HexCoord>();
            if (m_EnemyAIType == EnemyAIType.Attacker)
            {
                opponentTiles.AddRange(opponetsCanBeAttackDirect.Union(opponetsCanBeAttackIndirect).Intersect(moveRange));
            }
            else if (m_EnemyAIType == EnemyAIType.Defanser)
            {
                opponentTiles.AddRange(opponetsCanBeAttackDirect.Union(opponetsCanBeAttackIndirect).Intersect(moveRangeBySearchRange));
            }

            List<HexTilePath> opponentPaths = new List<HexTilePath>();
            List<Player> temp = m_PlayerManager.GetUserPlayers(null, false);
            List<HexCoord> opponentLeastHPTiles = new List<HexCoord>();

            //sort all target by distance and hp
            temp.OrderBy(x => HexCoord.HexDistance(x.m_Hex, m_Hex)).ThenBy(x => x.m_Hp);
            for (int i = 0; i < temp.Count; i++)
            {
                //get target in attack range after move
                opponentLeastHPTiles = GetAttackRangeWithTarget(temp[i].m_Hex, opponentTiles);
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
                opponentPaths = opponentPaths.OrderBy(x => -x.lastTile.m_DefenseRate).ThenBy(x => x.listOfTiles.Count).ToList();

                m_StageMapManager.HighlightTileAt(m_Hex, (int)m_MovementPerActionPoint, false);
                HexTile destTile = null;
                float cost = 0;
                for (int i = 0; i < opponentPaths[0].listOfTiles.Count; i++)
                {
                    HexTile tile = opponentPaths[0].listOfTiles[i];
                    cost += tile.m_MovementCost;
                    if (cost <= m_MovementPerActionPoint)
                    {
                        destTile = tile;
                    }
                }
                GameManager.m_Instance.MoveCurrentPlayer(destTile);
            }
            else if (m_EnemyAIType == EnemyAIType.Attacker)
            {
                opponentTiles = new List<HexCoord>();
                opponentTiles.AddRange(opponetsCanBeAttackDirect.Union(opponetsCanBeAttackIndirect));

                int minDistance = 9999;
                int userPlayerCount = m_PlayerManager.UserPlayerCount;
                for (int i = 0; i < userPlayerCount; i++)
                {
                    Player userPlayer = m_PlayerManager.GetPlayerByList(i);
                    if (userPlayer.m_Hp == 0)
                    {
                        continue;
                    }
                    for (int j = 0; j < moveRange.Count; j++)
                    {
                        int distance = HexCoord.HexDistance(moveRange[j], userPlayer.m_Hex);//  GameManager.m_Instance.userPlayers[i].m_Hex);
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

                List<HexTile> path = new List<HexTile>();
                for (int i = 0; i < opponentPaths[0].listOfTiles.Count; i++)
                {
                    if (!moveRange.Contains(opponentPaths[0].listOfTiles[i].m_Hex))
                    {
                        path.Add(opponentPaths[0].listOfTiles[i]);
                    }
                    m_StageMapManager.HighlightTileAt(moveRange);
                    GameManager.m_Instance.MoveCurrentPlayer(path[Mathf.Min(path.Count - 1, (int)m_MovementPerActionPoint)]);
                }
                m_IsMovable = false;
            }
            else
            {
                if (m_IsAttackable)
                {
                    TurnEnd();
                }
            }
        }
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
