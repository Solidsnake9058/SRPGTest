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
                if (positionQueue.Count == 0)
                {
                    actionPoint--;
                }
            }
        }
        else
        {
            //priority queue
            List<HexTile> attackTilesInRange = HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], attackRange, true);
            List<HexTile> moveToAttackTilesInRange = HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], (int)(movementPerActionPoint + attackRange), true);
            List<HexTile> movementTilesInRange = HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], (int)(movementPerActionPoint + 1000), true);

            List<HexTile> movementTilesRange = HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], (int)movementPerActionPoint);

            //attack if in range and with lowest HP
            if (isAttackable && attackTilesInRange.Where(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            {
                List<Player> opponentsInRange = attackTilesInRange.Select(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
                opponentsInRange.RemoveAll(x => x == null);
                Player opponents = opponentsInRange.OrderBy(x => -x.hp).First();

                GameManager.instance.HighlightTileAt(gridPosition, Color.red, attackRange);
                GameManager.instance.AttackWithCurrentPlayer(GameManager.instance.mapHex[(int)opponents.mapHexIndex.y][(int)opponents.mapHexIndex.x]);
                isActable = false;
                isAttackable = false;
                GameManager.instance.NextEnemyTurn();
            }
            //move toward nearest attack range if opponent
            else if (isMovable && moveToAttackTilesInRange.Where(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            {
                List<Player> opponentsInRange = moveToAttackTilesInRange.Select(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
                opponentsInRange.RemoveAll(x => x == null);

                List<HexTile> opponentTiles = new List<HexTile>();
                List<HexTile> temp = new List<HexTile>();

                int searchRange = attackRange;

                for (int i = 0; i < opponentsInRange.Count; i++)
                {
                    searchRange = attackRange;
                    while (temp.Count == 0)
                    {
                        temp = new List<HexTile>();
                        temp.AddRange(HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)opponentsInRange[i].mapHexIndex.y][(int)opponentsInRange[i].mapHexIndex.x], searchRange, true));
                        temp.RemoveAll(x => GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers).Where(y => y.gridPosition == x.gridPosition).Count() > 0);

                        opponentTiles.AddRange(temp);
                        searchRange++;
                    }
                    //opponentTiles = opponentTiles.Intersect(moveToAttackTilesInRange).ToList();
                }

                List<HexTilePath> opponentPaths = new List<HexTilePath>();
                for (int i = 0; i < opponentTiles.Count; i++)
                {
                    opponentPaths.Add(HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)opponentTiles[i].mapHexIndex.y][(int)opponentTiles[i].mapHexIndex.x]));
                }
                opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();

                //Player opponents = opponentsInRange.OrderBy(x => -x.HP).OrderBy(x => -TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y], true).listOfTiles.Count).FirstOrDefault();

                GameManager.instance.HighlightTileAt(gridPosition, Color.blue, (int)movementPerActionPoint, false);

                //List<Tile> path = TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponents.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles;
                for (int i = 0; i < opponentPaths.Count; i++)
                {
                    opponentPaths[i].listOfTiles = opponentPaths[i].listOfTiles.Intersect(movementTilesRange).ToList();
                }

                GameManager.instance.MoveCurrentPlayer(opponentPaths[0].listOfTiles[Mathf.Min(opponentPaths[0].listOfTiles.Count - 1, (int)movementPerActionPoint)]);
                isMovable = false;
            }
            //move toward nearest opponent
            else if (isMovable && movementTilesInRange.Where(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            {
                List<Player> opponentsInRange = movementTilesInRange.Select(x => GameManager.instance.userPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
                opponentsInRange.RemoveAll(x => x == null);

                List<HexTile> opponentTiles = new List<HexTile>();
                List<HexTile> temp = new List<HexTile>();

                int searchRange = attackRange;

                for (int i = 0; i < opponentsInRange.Count; i++)
                {
                    searchRange = attackRange;
                    while (temp.Count == 0)
                    {
                        temp = new List<HexTile>();
                        temp.AddRange(HexTileHighlight.FindHighlight(GameManager.instance.mapHex[(int)opponentsInRange[i].mapHexIndex.y][(int)opponentsInRange[i].mapHexIndex.x], searchRange, true));
                        temp.RemoveAll(x => GameManager.instance.userPlayers.Union(GameManager.instance.enemyPlayers).Where(y => y.gridPosition == x.gridPosition).Count() > 0);

                        opponentTiles.AddRange(temp);
                        searchRange++;
                    }
                    //opponentTiles = opponentTiles.Intersect(moveToAttackTilesInRange).ToList();
                }
                //for (int i = 0; i < opponentsInRange.Count; i++)
                //{
                //    opponentTiles.AddRange(GameManager.instance.map[(int)opponentsInRange[i].gridPosition.x][(int)opponentsInRange[i].gridPosition.y].neighbors);
                //}
                //opponentTiles.RemoveAll(x => GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).Count() > 0);
                //opponentTiles = opponentTiles.Intersect(moveToAttackTilesInRange).ToList();

                //Player opponents = opponentsInRange.OrderBy(x => -x.HP).OrderBy(x => -TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).listOfTiles.Count).FirstOrDefault();
                List<HexTilePath> opponentPaths = new List<HexTilePath>();
                for (int i = 0; i < opponentTiles.Count; i++)
                {
                    opponentPaths.Add(HexTilePathFinder.FindPath(GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x], GameManager.instance.mapHex[(int)opponentTiles[i].mapHexIndex.y][(int)opponentTiles[i].mapHexIndex.x]));
                }
                opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();
                GameManager.instance.HighlightTileAt(gridPosition, Color.blue, (int)movementPerActionPoint, false);

                //List<Tile> path = TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponents.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles;
                for (int i = 0; i < opponentPaths.Count; i++)
                {
                    opponentPaths[i].listOfTiles = opponentPaths[i].listOfTiles.Intersect(movementTilesRange).ToList();
                }
                GameManager.instance.MoveCurrentPlayer(opponentPaths[0].listOfTiles[(int)Mathf.Min(Mathf.Max(opponentPaths[0].listOfTiles.Count - 1, 0), movementPerActionPoint)]);
                isMovable = false;
            }
            //end turn if nothing can do
            else
            {
                GameManager.instance.RemoveHighlightTiles();
                actionPoint = 2;
                isActable = false;
                isAttackable = false;
                moving = false;
                attacking = false;
                GameManager.instance.NextEnemyTurn();
            }
        }

        base.TurnUpdate();
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
