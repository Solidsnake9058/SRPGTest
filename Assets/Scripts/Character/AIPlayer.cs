using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class AIPlayer : Player
{

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
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
        if (hp <= 0)
        {
            transform.position = new Vector3(transform.position.x, -100, transform.position.x);
        }

        base.Update();
    }

    public override void TurnUpdate()
    {
        if (positionQueue.Count > 0)
        {
            transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f)
            {
                transform.position = positionQueue[0];
                positionQueue.RemoveAt(0);
            }
        }
        else
        {
            GameManager.instance.isSetCamera = true;

            bool isGoToMove = false;

            //priority queue
            List<HexTile> attackTilesDirect = HexTile.GetCubeRingTile(gridPosition, 1, GameManager.instance.mapSizeX, GameManager.instance.mapSizeY);
            List<HexTile> attackTilesIndirect = HexTile.GetCubeRingTile(gridPosition, 2, GameManager.instance.mapSizeX, GameManager.instance.mapSizeY);

            List<HexTile> moveRange = HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], movementPerActionPoint, false);
            moveRange = moveRange.Where(x=>(GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers)).Where(y=>y.gridPosition==x.gridPosition).Count()==0).ToList();
            List<HexTile> moveRangeBySearchRange = HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], searchRange, false);
            moveRangeBySearchRange = moveRangeBySearchRange.Where(x => (GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers)).Where(y => y.gridPosition == x.gridPosition).Count() == 0).ToList();

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

            GetWeaponAttack(ref directAtk, ref indirectAtk);

            List<Player> opppnets = new List<Player>();
            List<Player> opppnetsNotCanBeCount = new List<Player>();

            opppnets = new List<Player>();

            //Get player who cannot be counter in range
            if (directAtk > 0)
            {
                List<Player> temp = attackTilesDirect.Select(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
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
                List<Player> temp = attackTilesIndirect.Select(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
                temp.RemoveAll(x => x == null);
                if (temp.Count > 0)
                {
                    opppnets.AddRange(temp);
                    opppnetsNotCanBeCount.AddRange(temp.Where(x => !x.GetIsCanAttack(false)).ToList());
                    attackTilesInRange.AddRange(attackTilesIndirect);
                }
            }

            Player opponent = new Player();
            if (opppnetsNotCanBeCount.Count > 0)
            {
                opponent = opppnetsNotCanBeCount.OrderBy(x => -x.hp).FirstOrDefault();
            }

            if (opponent != null && opponent != default(Player))
            {
                GameManager.instance.HighlightTileAt(gridPosition, GameManager.instance.attackTileColor, attackTilesInRange);
                GameManager.instance.AttackWithCurrentPlayer(GameManager.instance.mapHex[(int)opponent.mapHexIndex.y][(int)opponent.mapHexIndex.x]);
                isAttackable = false;
            }
            else
            {
                //Move to player who cannot be counter in move range
                opppnetsNotCanBeCount = new List<Player>();

                //Get all of players that cannot be counter
                if (directAtk > 0)
                {
                    List<Player> tempAll = GameManager.instance.userPlayers.Where(x => x.hp > 0).ToList();
                    List<Player> temp = tempAll.Where(x => !x.GetIsCanAttack(true)).ToList();
                    opppnetsNotCanBeCount.AddRange(temp);

                    foreach (var p in temp)
                    {
                        opppnetsNotCanBeCountDirect.AddRange(HexTile.GetCubeRingTile(p.gridPosition, 1, GameManager.instance.mapSizeX, GameManager.instance.mapSizeY).Except(opppnetsNotCanBeCountDirect));
                    }
                    opppnetsNotCanBeCountDirect.RemoveAll(x => GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers).Where(y => y.gridPosition == x.gridPosition).Count() > 0);

                    foreach (var p in tempAll)
                    {
                        opppnetsCanBeAttackDirect.AddRange(HexTile.GetCubeRingTile(p.gridPosition, 1, GameManager.instance.mapSizeX, GameManager.instance.mapSizeY).Except(opppnetsCanBeAttackDirect));
                    }
                    opppnetsCanBeAttackDirect.RemoveAll(x => GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers).Where(y => y.gridPosition == x.gridPosition).Count() > 0);

                }
                if (indirectAtk > 0)
                {
                    List<Player> tempAll = GameManager.instance.userPlayers.Where(x => x.hp > 0).ToList();
                    List<Player> temp = tempAll.Where(x => !x.GetIsCanAttack(true)).ToList();
                    opppnetsNotCanBeCount.AddRange(temp);

                    foreach (var p in temp)
                    {
                        opppnetsNotCanBeCountIndirect.AddRange(HexTile.GetCubeRingTile(p.gridPosition, 2, GameManager.instance.mapSizeX, GameManager.instance.mapSizeY).Except(opppnetsNotCanBeCountIndirect));
                    }
                    opppnetsNotCanBeCountIndirect.RemoveAll(x => GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers).Where(y => y.gridPosition == x.gridPosition).Count() > 0);

                    foreach (var p in tempAll)
                    {
                        opppnetsCanBeAttackIndirect.AddRange(HexTile.GetCubeRingTile(p.gridPosition, 2, GameManager.instance.mapSizeX, GameManager.instance.mapSizeY).Except(opppnetsCanBeAttackIndirect));
                    }
                    opppnetsCanBeAttackIndirect.RemoveAll(x => GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers).Where(y => y.gridPosition == x.gridPosition).Count() > 0);
                }

                //Get move target that in move range
                List<HexTile> moveTarget = new List<HexTile>();
                if (enemyAIType == EnemyAIType.Attacker)
                {
                    moveTarget = opppnetsNotCanBeCountDirect.Union(opppnetsNotCanBeCountIndirect).Intersect(moveRange).ToList();
                }
                else if (enemyAIType == EnemyAIType.Defanser)
                {
                    moveTarget = opppnetsNotCanBeCountDirect.Union(opppnetsNotCanBeCountIndirect).Intersect(moveRangeBySearchRange).ToList();
                }

                List<HexTilePath> opponentPathsCanBeCounter = new List<HexTilePath>();
                for (int i = 0; i < moveTarget.Count; i++)
                {
                    opponentPathsCanBeCounter.Add(HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)moveTarget[i].mapHexIndex.y][(int)moveTarget[i].mapHexIndex.x]));
                }

                //Move to target
                if (opponentPathsCanBeCounter.Count > 0)
                {
                    opponentPathsCanBeCounter = opponentPathsCanBeCounter.OrderBy(x => x.listOfTiles.Count).ToList();
                    GameManager.instance.HighlightTileAt(gridPosition, GameManager.instance.moveTileColor, (int)movementPerActionPoint, false);
                    GameManager.instance.MoveCurrentPlayer(opponentPathsCanBeCounter[0].listOfTiles[Mathf.Min(opponentPathsCanBeCounter[0].listOfTiles.Count - 1, (int)movementPerActionPoint)]);
                    isMovable = false;
                }
                //Attack player in range
                else if (opppnets.Count > 0)
                {
                    opponent = opppnets.OrderBy(x => -x.hp).FirstOrDefault();
                    if (opponent != null && opponent != default(Player))
                    {
                        GameManager.instance.HighlightTileAt(gridPosition, GameManager.instance.attackTileColor, attackTilesInRange);
                        GameManager.instance.AttackWithCurrentPlayer(GameManager.instance.mapHex[(int)opponent.mapHexIndex.y][(int)opponent.mapHexIndex.x]);
                        isAttackable = false;
                    }
                }
            }
            
            //if no player can be attack and cannot be counter player who not in move range
            //change action to moving?
            isGoToMove = isAttackable ? (enemyAIType == EnemyAIType.Stander ? false : isMovable) : isAttackable;

            if (isGoToMove)
            {
                List<HexTile> opponentTiles = new List<HexTile>();
                if (enemyAIType == EnemyAIType.Attacker)
                {
                    opponentTiles.AddRange((opppnetsCanBeAttackDirect.Union(opppnetsCanBeAttackIndirect)).Intersect(moveRange));
                }
                else if (enemyAIType == EnemyAIType.Defanser)
                {
                    opponentTiles.AddRange((opppnetsCanBeAttackDirect.Union(opppnetsCanBeAttackIndirect)).Intersect(moveRangeBySearchRange));
                }

                List<HexTilePath> opponentPaths = new List<HexTilePath>();
                for (int i = 0; i < opponentTiles.Count; i++)
                {
                    opponentPaths.Add(HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)opponentTiles[i].mapHexIndex.y][(int)opponentTiles[i].mapHexIndex.x]));
                }

                if (opponentPaths.Count > 0)
                {
                    opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();
                    GameManager.instance.HighlightTileAt(gridPosition, GameManager.instance.moveTileColor, movementPerActionPoint, false);
                    GameManager.instance.MoveCurrentPlayer(opponentPaths[0].listOfTiles[Mathf.Min(opponentPaths[0].listOfTiles.Count - 1, (int)movementPerActionPoint)]);
                }
                else if(enemyAIType == EnemyAIType.Attacker)
                {
                    opponentTiles = new List<HexTile>();
                    //opponentTiles.AddRange(opppnetsCanBeAttackDirect.Union(opppnetsCanBeAttackIndirect));

                    int minDistance = 9999;
                    for (int i = 0; i < GameManager.instance.userPlayers.Count; i++)
                    {
                        if (GameManager.instance.userPlayers[i].hp == 0)
                        {
                            continue;
                        }
                        for (int j = 0; j < moveRange.Count; j++)
                        {
                            int distance = HexTile.Distance(new HexTile.HexCoord((int)moveRange[j].gridPosition.x, (int)moveRange[j].gridPosition.y), new HexTile.HexCoord((int)GameManager.instance.userPlayers[i].gridPosition.x, (int)GameManager.instance.userPlayers[i].gridPosition.y));
                            if (distance< minDistance)
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
                    opponentTiles = opponentTiles.OrderBy(x => HexTile.HexCoord.Distance(new HexTile.HexCoord((int)gridPosition.x, (int)gridPosition.y), new HexTile.HexCoord((int)x.gridPosition.x, (int)x.gridPosition.y))).ToList();

                    //HexTilePath opponentPath = HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)opponentTiles[0].mapHexIndex.y][(int)opponentTiles[0].mapHexIndex.x]);

                    for (int i = 0; i < opponentTiles.Count; i++)
                    {
                        opponentPaths.Add(HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)opponentTiles[i].mapHexIndex.y][(int)opponentTiles[i].mapHexIndex.x]));
                    }

                    //opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();
                    opponentPaths[0].listOfTiles = opponentPaths[0].listOfTiles.Intersect(moveRange).ToList();
                    GameManager.instance.HighlightTileAt(gridPosition, GameManager.instance.attackTileColor, moveRange);
                    GameManager.instance.MoveCurrentPlayer(opponentPaths[0].listOfTiles[Mathf.Min(opponentPaths[0].listOfTiles.Count - 1, (int)movementPerActionPoint)]);
                }
                isMovable = false;
            }
            else
            {
                if (isAttackable)
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

        base.TurnUpdate();
    }

    public override void TurnEnd()
    {
        GameManager.instance.RemoveHighlightTiles();
        moving = false;
        attacking = false;
        GameManager.instance.NextEnemyTurn();

        base.TurnEnd();
    }

    public List<HexTile> GetCanBeAttackTile(int attackRange)
    {
        List<HexTile> tiles = new List<HexTile>();

        foreach (var player in GameManager.instance.userPlayers)
        {
            if (player.hp <= 0)
            {
                continue;
            }
            tiles.AddRange(HexTile.GetCubeRingTile(player.gridPosition, attackRange, GameManager.instance.mapSizeX, GameManager.instance.mapSizeY));
        }
        tiles.RemoveAll(x =>((GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers)).Select(y => y.gridPosition).ToList()).Contains(x.gridPosition));
        return tiles;
    }

    public override void TurnOnGUI()
    {
        base.TurnOnGUI();
    }

    public override void OnMouseDown()
    {
        //if (GameManager.instance.attacking)
        //{
        //    HexTile attTile = GameManager.instance.mapHex[(int)gridPosition.y][(int)gridPosition.x + (((int)gridPosition.y) >> 1)];
        //    GameManager.instance.AttackWithCurrentPlayer(attTile);
        //}

        base.OnMouseDown();
    }
}
