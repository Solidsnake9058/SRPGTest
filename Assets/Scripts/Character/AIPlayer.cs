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

    public override void Update()
    {
        //if (GameManager.instance.players[GameManager.instance.currentPlayerIndex] == this)
        //{
        //    transform.GetComponent<Renderer>().material.color = Color.green;
        //}
        //else
        //{
        //    transform.GetComponent<Renderer>().material.color = Color.white;
        //}
        if (m_Hp <= 0)
        {
            HidePlayer();
        }

        base.Update();
    }

    public override void TurnUpdate()
    {
        //if (positionQueue.Count > 0)
        //{
        //    transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
        //    transform.LookAt(positionQueue[0]);
        //    animator.SetBool("walk", true);

        //    if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f)
        //    {
        //        transform.position = positionQueue[0];
        //        positionQueue.RemoveAt(0);
        //        if (positionQueue.Count == 0)
        //        {
        //            animator.SetBool("walk", false);
        //        }
        //    }
        //}
        //else

        base.TurnUpdate();

        {
            GameManager.m_Instance.isSetCamera = true;

            bool isGoToMove = false;

            //priority queue
            List<HexTile> attackTilesDirect = HexTile.GetCubeRingTile(m_Hex, 1);
            List<HexTile> attackTilesIndirect = HexTile.GetCubeRingTile(m_Hex, 2);

            List<HexTile> moveRange = HexTileHighlight.FindHighlight(GameMidiator.m_Instance.m_StageMapManager.GetMapTile(m_Hex), m_MovementPerActionPoint, false);
            moveRange = moveRange.Where(x => (GameManager.m_Instance.userPlayers.Values.Union(GameManager.m_Instance.enemyPlayers.Values)).Where(y => y.gridPosition == x.m_GridPosition).Count() == 0).ToList();
            List<HexTile> moveRangeBySearchRange = HexTileHighlight.FindHighlight(GameMidiator.m_Instance.m_StageMapManager.GetMapTile(m_Hex), m_SearchRange, false);
            moveRangeBySearchRange = moveRangeBySearchRange.Where(x => (GameManager.m_Instance.userPlayers.Values.Union(GameManager.m_Instance.enemyPlayers.Values)).Where(y => y.gridPosition == x.m_GridPosition).Count() == 0).ToList();

            List<HexTile> attackTilesInRange = new List<HexTile>();// HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], attackRange, true);
            //List<HexTile> moveToAttackTilesInRange = HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], (int)(movementPerActionPoint + attackRange), true);
            //List<HexTile> movementTilesInRange = HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], (int)(movementPerActionPoint + searchRange), true);

            //List<HexTile> movementTilesRange = HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], (int)movementPerActionPoint);


            List<HexTile> opppnetsCanBeAttackDirect = new List<HexTile>();
            List<HexTile> opppnetsCanBeAttackIndirect = new List<HexTile>();

            List<HexTile> opppnetsNotCanBeCountDirect = new List<HexTile>();
            List<HexTile> opppnetsNotCanBeCountIndirect = new List<HexTile>();


            int directAtk = 0;
            int indirectAtk = 0;

            GetWeaponAttack(out directAtk, out indirectAtk);

            List<Player> opppnets = new List<Player>();
            List<Player> opppnetsNotCanBeCount = new List<Player>();

            opppnets = new List<Player>();

            //Get player who cannot be counter in range
            if (directAtk > 0)
            {
                List<Player> temp = attackTilesDirect.Select(x => GameManager.m_Instance.userPlayers.Values.Where(y => y.m_Hp > 0 && y.gridPosition == x.m_GridPosition).FirstOrDefault()).ToList();
                temp.RemoveAll(x => x == null);
                if (temp.Count > 0)
                {
                    opppnets.AddRange(temp);
                    opppnetsNotCanBeCount.AddRange(temp.Where(x => !x.GetIsCanAttack(true)).ToList());
                    attackTilesInRange.AddRange(attackTilesDirect);
                }
            }

            if (indirectAtk > 0)
            {
                List<Player> temp = attackTilesIndirect.Select(x => GameManager.m_Instance.userPlayers.Values.Where(y => y.m_Hp > 0 && y.gridPosition == x.m_GridPosition).FirstOrDefault()).ToList();
                temp.RemoveAll(x => x == null);
                if (temp.Count > 0)
                {
                    opppnets.AddRange(temp);
                    opppnetsNotCanBeCount.AddRange(temp.Where(x => !x.GetIsCanAttack(false)).ToList());
                    attackTilesInRange.AddRange(attackTilesIndirect);
                }
            }

            Player opponent = null;
            if (opppnetsNotCanBeCount.Count > 0)
            {
                opponent = opppnetsNotCanBeCount.OrderBy(x => x.m_Hp).FirstOrDefault();
            }

            if (opponent != null && opponent != default(Player))
            {
                GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.attackTileColor, attackTilesInRange);
                GameManager.m_Instance.AttackWithCurrentPlayer(GameMidiator.m_Instance.m_StageMapManager.GetMapTile(opponent.m_Hex));
                m_IsAttackable = false;
            }
            else
            {
                //Move to player who cannot be counter in move range
                opppnetsNotCanBeCount = new List<Player>();

                //Get all of players that cannot be counter
                if (directAtk > 0)
                {
                    List<Player> tempAll = GameManager.m_Instance.userPlayers.Values.Where(x => x.m_Hp > 0).ToList();
                    List<Player> temp = tempAll.Where(x => !x.GetIsCanAttack(true)).ToList();
                    opppnetsNotCanBeCount.AddRange(temp);

                    foreach (var p in temp)
                    {
                        opppnetsNotCanBeCountDirect.AddRange(HexTile.GetCubeRingTile(p.m_Hex, 1).Except(opppnetsNotCanBeCountDirect));
                    }
                    opppnetsNotCanBeCountDirect.RemoveAll(x => GameManager.m_Instance.userPlayers.Values.Union(GameManager.m_Instance.enemyPlayers.Values.Where(z => z.m_Hp > 0)).Where(y => y.gridPosition == x.m_GridPosition).Count() > 0);

                    foreach (var p in tempAll)
                    {
                        opppnetsCanBeAttackDirect.AddRange(HexTile.GetCubeRingTile(p.m_Hex, 1).Except(opppnetsCanBeAttackDirect));
                    }
                    opppnetsCanBeAttackDirect.RemoveAll(x => GameManager.m_Instance.userPlayers.Values.Union(GameManager.m_Instance.enemyPlayers.Values.Where(z => z.m_Hp > 0)).Where(y => y.gridPosition == x.m_GridPosition).Count() > 0);

                }
                if (indirectAtk > 0)
                {
                    List<Player> tempAll = GameManager.m_Instance.userPlayers.Values.Where(x => x.m_Hp > 0).ToList();
                    List<Player> temp = tempAll.Where(x => !x.GetIsCanAttack(true)).ToList();
                    opppnetsNotCanBeCount.AddRange(temp);

                    foreach (var p in temp)
                    {
                        opppnetsNotCanBeCountIndirect.AddRange(HexTile.GetCubeRingTile(p.m_Hex, 2).Except(opppnetsNotCanBeCountIndirect));
                    }
                    opppnetsNotCanBeCountIndirect.RemoveAll(x => GameManager.m_Instance.userPlayers.Values.Union(GameManager.m_Instance.enemyPlayers.Values.Where(z => z.m_Hp > 0)).Where(y => y.gridPosition == x.m_GridPosition).Count() > 0);

                    foreach (var p in tempAll)
                    {
                        opppnetsCanBeAttackIndirect.AddRange(HexTile.GetCubeRingTile(p.m_Hex, 2).Except(opppnetsCanBeAttackIndirect));
                    }
                    opppnetsCanBeAttackIndirect.RemoveAll(x => GameManager.m_Instance.userPlayers.Values.Union(GameManager.m_Instance.enemyPlayers.Values.Where(z => z.m_Hp > 0)).Where(y => y.gridPosition == x.m_GridPosition).Count() > 0);
                }

                //Get move target that in move range
                List<HexTile> moveTarget = new List<HexTile>();
                if (m_EnemyAIType == EnemyAIType.Attacker)
                {
                    moveTarget = opppnetsNotCanBeCountDirect.Union(opppnetsNotCanBeCountIndirect).Intersect(moveRange).ToList();
                }
                else if (m_EnemyAIType == EnemyAIType.Defanser)
                {
                    moveTarget = opppnetsNotCanBeCountDirect.Union(opppnetsNotCanBeCountIndirect).Intersect(moveRangeBySearchRange).ToList();
                }

                List<HexTilePath> opponentPathsCanBeCounter = new List<HexTilePath>();
                for (int i = 0; i < moveTarget.Count; i++)
                {
                    opponentPathsCanBeCounter.Add(GameMidiator.m_Instance.m_StageMapManager.FindPath(m_Hex, moveTarget[i].m_Hex));
                }

                //Move to target
                if (opponentPathsCanBeCounter.Count > 0)
                {
                    opponentPathsCanBeCounter = opponentPathsCanBeCounter.OrderBy(x => x.listOfTiles.Count).ToList();
                    GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.moveTileColor, (int)m_MovementPerActionPoint, false);
                    GameManager.m_Instance.MoveCurrentPlayer(opponentPathsCanBeCounter[0].listOfTiles[Mathf.Min(opponentPathsCanBeCounter[0].listOfTiles.Count - 1, (int)m_MovementPerActionPoint)]);
                    m_IsMovable = false;
                }
                //Attack player in range
                else if (opppnets.Count > 0)
                {
                    opponent = opppnets.OrderBy(x => -x.m_Hp).FirstOrDefault();
                    if (opponent != null && opponent != default(Player))
                    {
                        GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.attackTileColor, attackTilesInRange);
                        GameManager.m_Instance.AttackWithCurrentPlayer(GameMidiator.m_Instance.m_StageMapManager.GetMapTile(opponent.m_Hex));
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
                    opponentTiles.AddRange((opppnetsCanBeAttackDirect.Union(opppnetsCanBeAttackIndirect)).Intersect(moveRange));
                }
                else if (m_EnemyAIType == EnemyAIType.Defanser)
                {
                    opponentTiles.AddRange((opppnetsCanBeAttackDirect.Union(opppnetsCanBeAttackIndirect)).Intersect(moveRangeBySearchRange));
                }

                List<HexTilePath> opponentPaths = new List<HexTilePath>();
                List<Player> temp = GameManager.m_Instance.userPlayers.Values.Where(x => x.m_Hp > 0).ToList();
                List<HexTile> opponentLeastHPTiles = new List<HexTile>();

                //sort all target by distance and hp
                temp.OrderBy(x => HexTile.Distance(x.m_Hex, m_Hex)).ThenBy(x => x.m_Hp);
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
                    GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.moveTileColor, m_MovementPerActionPoint, false);
                    GameManager.m_Instance.MoveCurrentPlayer(opponentPaths[0].listOfTiles[Mathf.Min(opponentPaths[0].listOfTiles.Count - 1, (int)m_MovementPerActionPoint)]);
                }
                else if (m_EnemyAIType == EnemyAIType.Attacker)
                {
                    opponentTiles = new List<HexTile>();
                    //opponentTiles.AddRange(opppnetsCanBeAttackDirect.Union(opppnetsCanBeAttackIndirect));

                    int minDistance = 9999;
                    for (int i = 0; i < GameManager.m_Instance.userPlayers.Count; i++)
                    {
                        if (GameManager.m_Instance.userPlayers[i].m_Hp == 0)
                        {
                            continue;
                        }
                        for (int j = 0; j < moveRange.Count; j++)
                        {
                            int distance = HexTile.Distance(new HexTile.HexCoord((int)moveRange[j].m_GridPosition.x, (int)moveRange[j].m_GridPosition.y), new HexTile.HexCoord((int)GameManager.m_Instance.userPlayers[i].gridPosition.x, (int)GameManager.m_Instance.userPlayers[i].gridPosition.y));
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
                    opponentTiles = opponentTiles.OrderBy(x => HexTile.HexCoord.Distance(new HexTile.HexCoord((int)gridPosition.x, (int)gridPosition.y), new HexTile.HexCoord((int)x.m_GridPosition.x, (int)x.m_GridPosition.y))).ToList();

                    //HexTilePath opponentPath = HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)opponentTiles[0].mapHexIndex.y][(int)opponentTiles[0].mapHexIndex.x]);

                    for (int i = 0; i < opponentTiles.Count; i++)
                    {
                        opponentPaths.Add(GameMidiator.m_Instance.m_StageMapManager.FindPath(m_Hex, opponentTiles[i].m_Hex));
                    }

                    //opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();
                    opponentPaths[0].listOfTiles = opponentPaths[0].listOfTiles.Intersect(moveRange).ToList();
                    GameManager.m_Instance.HighlightTileAt(gridPosition, GameManager.m_Instance.attackTileColor, moveRange);
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



            //attack if in range and with lowest HP
            //if (isAttackable && attackTilesInRange.Where(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            //{
            //    List<Player> opponentsInRange = attackTilesInRange.Select(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
            //    opponentsInRange.RemoveAll(x => x == null);
            //    Player opponents = opponentsInRange.OrderBy(x => -x.hp).First();

            //    GameManager.instance.HighlightTileAt(gridPosition, Color.red, attackRange);
            //    GameManager.instance.AttackWithCurrentPlayer(GameManager.instance.mapHex[(int)opponents.mapHexIndex.y][(int)opponents.mapHexIndex.x]);
            //    isActable = false;
            //    isAttackable = false;
            //    GameManager.instance.NextEnemyTurn();
            //}
            ////move toward nearest attack range if opponent
            //else if (isMovable && moveToAttackTilesInRange.Where(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            //{
            //    List<Player> opponentsInRange = moveToAttackTilesInRange.Select(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
            //    opponentsInRange.RemoveAll(x => x == null);

            //    List<HexTile> opponentTiles = new List<HexTile>();
            //    List<HexTile> temp = new List<HexTile>();

            //    int searchRange = attackRange;

            //    for (int i = 0; i < opponentsInRange.Count; i++)
            //    {
            //        searchRange = attackRange;
            //        while (temp.Count == 0)
            //        {
            //            temp = new List<HexTile>();
            //            temp.AddRange(HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)opponentsInRange[i].mapHexIndex.y][(int)opponentsInRange[i].mapHexIndex.x], searchRange, true));
            //            temp.RemoveAll(x => GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers).Where(y => y.gridPosition == x.gridPosition).Count() > 0);

            //            opponentTiles.AddRange(temp);
            //            searchRange++;
            //        }
            //        //opponentTiles = opponentTiles.Intersect(moveToAttackTilesInRange).ToList();
            //    }

            //    List<HexTilePath> opponentPaths = new List<HexTilePath>();
            //    for (int i = 0; i < opponentTiles.Count; i++)
            //    {
            //        opponentPaths.Add(HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)opponentTiles[i].mapHexIndex.y][(int)opponentTiles[i].mapHexIndex.x]));
            //    }
            //    opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();

            //    //Player opponents = opponentsInRange.OrderBy(x => -x.HP).OrderBy(x => -TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y], true).listOfTiles.Count).FirstOrDefault();

            //    GameManager.instance.HighlightTileAt(gridPosition, Color.blue, (int)movementPerActionPoint, false);

            //    //List<Tile> path = TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponents.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles;
            //    for (int i = 0; i < opponentPaths.Count; i++)
            //    {
            //        opponentPaths[i].listOfTiles = opponentPaths[i].listOfTiles.Intersect(movementTilesRange).ToList();
            //    }

            //    GameManager.instance.MoveCurrentPlayer(opponentPaths[0].listOfTiles[Mathf.Min(opponentPaths[0].listOfTiles.Count - 1, (int)movementPerActionPoint)]);
            //    isMovable = false;
            //}
            ////move toward nearest opponent
            //else if (isMovable && movementTilesInRange.Where(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            //{
            //    List<Player> opponentsInRange = movementTilesInRange.Select(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
            //    opponentsInRange.RemoveAll(x => x == null);

            //    List<HexTile> opponentTiles = new List<HexTile>();
            //    List<HexTile> temp = new List<HexTile>();

            //    int searchRange = attackRange;

            //    for (int i = 0; i < opponentsInRange.Count; i++)
            //    {
            //        searchRange = attackRange;
            //        while (temp.Count == 0)
            //        {
            //            temp = new List<HexTile>();
            //            temp.AddRange(HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)opponentsInRange[i].mapHexIndex.y][(int)opponentsInRange[i].mapHexIndex.x], searchRange, true));
            //            temp.RemoveAll(x => GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers).Where(y => y.gridPosition == x.gridPosition).Count() > 0);

            //            opponentTiles.AddRange(temp);
            //            searchRange++;
            //        }
            //        //opponentTiles = opponentTiles.Intersect(moveToAttackTilesInRange).ToList();
            //    }
            //    //for (int i = 0; i < opponentsInRange.Count; i++)
            //    //{
            //    //    opponentTiles.AddRange(GameManager.instance.map[(int)opponentsInRange[i].gridPosition.x][(int)opponentsInRange[i].gridPosition.y].neighbors);
            //    //}
            //    //opponentTiles.RemoveAll(x => GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).Count() > 0);
            //    //opponentTiles = opponentTiles.Intersect(moveToAttackTilesInRange).ToList();

            //    //Player opponents = opponentsInRange.OrderBy(x => -x.HP).OrderBy(x => -TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).listOfTiles.Count).FirstOrDefault();
            //    List<HexTilePath> opponentPaths = new List<HexTilePath>();
            //    for (int i = 0; i < opponentTiles.Count; i++)
            //    {
            //        opponentPaths.Add(HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)opponentTiles[i].mapHexIndex.y][(int)opponentTiles[i].mapHexIndex.x]));
            //    }
            //    opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();
            //    GameManager.instance.HighlightTileAt(gridPosition, Color.blue, (int)movementPerActionPoint, false);

            //    //List<Tile> path = TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponents.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles;
            //    for (int i = 0; i < opponentPaths.Count; i++)
            //    {
            //        opponentPaths[i].listOfTiles = opponentPaths[i].listOfTiles.Intersect(movementTilesRange).ToList();
            //    }
            //    GameManager.instance.MoveCurrentPlayer(opponentPaths[0].listOfTiles[(int)Mathf.Min(Mathf.Max(opponentPaths[0].listOfTiles.Count - 1, 0), movementPerActionPoint)]);
            //    isMovable = false;
            //}
            //end turn if nothing can do
            //else
            //{
            //}

        }

        //base.TurnUpdate();
    }

    public override void TurnEnd()
    {
        GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
        m_Moving = false;
        m_Attacking = false;
        GameManager.m_Instance.NextEnemyTurn();

        base.TurnEnd();
    }

    public List<HexTile> GetCanBeAttackTile(int attackRange)
    {
        List<HexTile> tiles = new List<HexTile>();

        foreach (var player in GameManager.m_Instance.userPlayers.Values)
        {
            if (player.m_Hp <= 0)
            {
                continue;
            }
            tiles.AddRange(HexTile.GetCubeRingTile(player.m_Hex, attackRange));
        }
        tiles.RemoveAll(x => ((GameManager.m_Instance.userPlayers.Values.Union(GameManager.m_Instance.enemyPlayers.Values)).Select(y => y.gridPosition).ToList()).Contains(x.m_GridPosition));
        return tiles;
    }

    public override void TurnOnGUI()
    {
        base.TurnOnGUI();
    }

}
