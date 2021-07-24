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
        if (m_PlayerState.Equals(PlayerState.Move))
        {
            return;
        }

        GameManager.m_Instance.isSetCamera = true;

        bool isGoToMove = false;

        //priority queue
        List<HexCoord> attackHexesDirect = HexCoord.HexRing(m_Hex, 1);
        List<HexCoord> attackHexesIndirect = HexCoord.HexRing(m_Hex, 2);

        List<HexCoord> moveRange = GameMidiator.m_Instance.m_StageMapManager.FindHighlight(m_Hex, m_MovementPerActionPoint, false);
        moveRange = GameMidiator.m_Instance.m_PlayerManager.GetEmptyHex(moveRange);
        List<HexCoord> moveRangeBySearchRange = GameMidiator.m_Instance.m_StageMapManager.FindHighlight(m_Hex, m_SearchRange, false);
        moveRangeBySearchRange = GameMidiator.m_Instance.m_PlayerManager.GetEmptyHex(moveRangeBySearchRange);

        List<HexCoord> attackHexesInRange = new List<HexCoord>();

        List<HexCoord> opponetsCanBeAttackDirect = new List<HexCoord>();
        List<HexCoord> opponetsCanBeAttackIndirect = new List<HexCoord>();
        List<HexCoord> opponetsNotCanBeCountDirect = new List<HexCoord>();
        List<HexCoord> opponetsNotCanBeCountIndirect = new List<HexCoord>();

        int directAtk = 0;
        int indirectAtk = 0;

        GetWeaponAttack(out directAtk, out indirectAtk);

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
        if (opponetsNotCanBeCount.Count > 0)
        {
            opponent = opponetsNotCanBeCount.OrderBy(x => x.m_Hp).FirstOrDefault();
            //GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.attackTileColor, attackTilesInRange1);

            GameMidiator.m_Instance.m_StageMapManager.HighlightTileAt(attackHexesInRange);
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
            List<HexTile> moveTarget = new List<HexTile>();
            List<HexCoord> moveTargetNotCanBeConter = opponetsNotCanBeCountDirect.Union(opponetsNotCanBeCountIndirect).ToList();
            List<HexCoord> moveTargetCanBeConter = opponetsCanBeAttackDirect.Union(opponetsCanBeAttackIndirect).ToList();

            if (m_EnemyAIType == EnemyAIType.Attacker)
            {
                //moveTarget = opppnetsNotCanBeCountDirect.Union(opppnetsNotCanBeCountIndirect).Intersect(moveRange).ToList();
                moveTargetNotCanBeConter = moveTargetNotCanBeConter.Intersect(moveRange).ToList();
                moveTargetCanBeConter = moveTargetCanBeConter.Intersect(moveRange).ToList();
            }
            else if (m_EnemyAIType == EnemyAIType.Defanser)
            {
                //moveTarget = opppnetsNotCanBeCountDirect.Union(opppnetsNotCanBeCountIndirect).Intersect(moveRangeBySearchRange).ToList();
                moveTargetNotCanBeConter = moveTargetNotCanBeConter.Intersect(moveRangeBySearchRange).ToList();
                moveTargetCanBeConter = moveTargetCanBeConter.Intersect(moveRangeBySearchRange).ToList();
            }

            List<HexTilePath> opponentPathsNotCanBeCounter = new List<HexTilePath>();
            for (int i = 0; i < moveTargetNotCanBeConter.Count; i++)
            {
                opponentPathsNotCanBeCounter.Add(GameMidiator.m_Instance.m_StageMapManager.FindPath(m_Hex, moveTargetNotCanBeConter[i]));
            }

            //Move to target
            if (opponentPathsNotCanBeCounter.Count > 0)
            {
                opponentPathsNotCanBeCounter = opponentPathsNotCanBeCounter.OrderBy(x => x.listOfTiles.Count).ToList();
                GameMidiator.m_Instance.m_StageMapManager.HighlightTileAt(m_Hex, (int)m_MovementPerActionPoint, false);
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
                //GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.moveTileColor, (int)m_MovementPerActionPoint, false);
                //GameManager.m_Instance.MoveCurrentPlayer(opponentPathsNotCanBeCounter[0].listOfTiles[Mathf.Min(opponentPathsNotCanBeCounter[0].listOfTiles.Count - 1, (int)m_MovementPerActionPoint)]);
                m_IsMovable = false;
            }
            //Attack player in range
            else if (opponets.Count > 0)
            {
                opponent = opponets.OrderBy(x => -x.m_Hp).FirstOrDefault();
                if (opponent != null && opponent != default(Player))
                {
                    //GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.attackTileColor, attackTilesInRange);

                    GameMidiator.m_Instance.m_StageMapManager.HighlightTileAt(attackHexesInRange);
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
            List<HexTile> opponentTiles = new List<HexTile>();
            if (m_EnemyAIType == EnemyAIType.Attacker)
            {
                //opponentTiles.AddRange((opppnetsCanBeAttackDirect.Union(opppnetsCanBeAttackIndirect)).Intersect(moveRange));
            }
            else if (m_EnemyAIType == EnemyAIType.Defanser)
            {
                //opponentTiles.AddRange((opppnetsCanBeAttackDirect.Union(opppnetsCanBeAttackIndirect)).Intersect(moveRangeBySearchRange));
            }

            List<HexTilePath> opponentPaths = new List<HexTilePath>();
            List<Player> temp = GameManager.m_Instance.userPlayers.Values.Where(x => x.m_Hp > 0).ToList();
            List<HexTile> opponentLeastHPTiles = new List<HexTile>();

            //sort all target by distance and hp
            temp.OrderBy(x => HexCoord.HexDistance(x.m_Hex, m_Hex)).ThenBy(x => x.m_Hp);
            for (int i = 0; i < temp.Count; i++)
            {
                //get target in attack range after move
                opponentLeastHPTiles = GetAttackRangeWhitTarget(temp[i].m_Hex).Intersect(opponentTiles).ToList();
                if (opponentLeastHPTiles != null && opponentLeastHPTiles.Count > 0)
                {
                    break;
                }
            }

            for (int i = 0; i < opponentLeastHPTiles.Count; i++)
            {
                opponentPaths.Add(GameMidiator.m_Instance.m_StageMapManager.FindPath(m_Hex, opponentLeastHPTiles[i].m_Hex));
            }

            if (opponentPaths.Count > 0)
            {
                //get highest defense rate tile and least step path
                opponentPaths = opponentPaths.OrderBy(x => -x.lastTile.m_DefenseRate).ThenBy(x => x.listOfTiles.Count).ToList();

                GameMidiator.m_Instance.m_StageMapManager.HighlightTileAt(m_Hex, (int)m_MovementPerActionPoint, false);
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

                //GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.moveTileColor, m_MovementPerActionPoint, false);
                //GameManager.m_Instance.MoveCurrentPlayer(opponentPaths[0].listOfTiles[Mathf.Min(opponentPaths[0].listOfTiles.Count - 1, (int)m_MovementPerActionPoint)]);
            }
            else if (m_EnemyAIType == EnemyAIType.Attacker)
            {
                opponentTiles = new List<HexTile>();
                //opponentTiles.AddRange(opppnetsCanBeAttackDirect.Union(opppnetsCanBeAttackIndirect));

                int minDistance = 9999;
                int userPlayerCount = GameMidiator.m_Instance.m_PlayerManager.UserPlayerCount;
                for (int i = 0; i < userPlayerCount; i++)
                {
                    Player userPlayer = GameMidiator.m_Instance.m_PlayerManager.GetPlayerByList(i);
                    if (userPlayer.m_Hp == 0)
                    {
                        continue;
                    }
                    //for (int j = 0; j < moveRange.Count; j++)
                    //{
                    //    int distance = HexCoord.HexDistance(moveRange[j].m_Hex, userPlayer.m_Hex);//  GameManager.m_Instance.userPlayers[i].m_Hex);
                    //    if (distance < minDistance)
                    //    {
                    //        minDistance = distance;
                    //        opponentTiles.Clear();
                    //        opponentTiles.Add(moveRange[j]);
                    //    }
                    //    else if (distance == minDistance)
                    //    {
                    //        opponentTiles.Add(moveRange[j]);
                    //    }
                    //}
                }
                opponentTiles = opponentTiles.OrderBy(x => HexCoord.HexDistance(m_Hex, x.m_Hex)).ToList();

                //////////HexTilePath opponentPath = HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)opponentTiles[0].mapHexIndex.y][(int)opponentTiles[0].mapHexIndex.x]);

                for (int i = 0; i < opponentTiles.Count; i++)
                {
                    opponentPaths.Add(GameMidiator.m_Instance.m_StageMapManager.FindPath(m_Hex, opponentTiles[i].m_Hex));
                }

                ///////////opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();
                //opponentPaths[0].listOfTiles = opponentPaths[0].listOfTiles.Intersect(moveRange).ToList();
                //GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.attackTileColor, moveRange);

                GameMidiator.m_Instance.m_StageMapManager.HighlightTileAt(moveRange);
                GameManager.m_Instance.MoveCurrentPlayer(opponentPaths[0].listOfTiles[Mathf.Min(opponentPaths[0].listOfTiles.Count - 1, (int)m_MovementPerActionPoint)]);
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

    private void GetOpponet(List<HexCoord> attackTiles, List<HexCoord> attackTilesRange, List<Player> opponets, List<Player> opponetsNotCanBeCount, bool isDirect)
    {
        List<Player> temp = GameMidiator.m_Instance.m_PlayerManager.GetUserPlayerTile(attackTiles);
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
        List<Player> tempUser = GameMidiator.m_Instance.m_PlayerManager.GetUserPlayerTile(null, false);
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
                if (!tiles.Contains(hex) && GameMidiator.m_Instance.m_StageMapManager.GetMapTile(hex) != null && !tempPlayers.Contains(hex))
                {
                    tiles.Add(hex);
                }
            }
        }
    }

    public override void TurnEnd()
    {
        GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
        m_Moving = false;
        m_Attacking = false;
        GameManager.m_Instance.NextEnemyTurn();

        base.TurnEnd();
    }

    //public List<HexTile> GetCanBeAttackTile(int attackRange)
    //{
    //    List<HexTile> tiles = new List<HexTile>();

    //    foreach (var player in GameManager.m_Instance.userPlayers.Values)
    //    {
    //        if (player.m_Hp <= 0)
    //        {
    //            continue;
    //        }
    //        tiles.AddRange(HexTile.GetCubeRingTile(player.m_Hex, attackRange));
    //    }
    //    tiles.RemoveAll(x => ((GameManager.m_Instance.userPlayers.Values.Union(GameManager.m_Instance.enemyPlayers.Values)).Select(y => y.m_Hex).ToList()).Contains(x.m_Hex));
    //    return tiles;
    //}

    public override void TurnOnGUI()
    {
        base.TurnOnGUI();
    }

}
