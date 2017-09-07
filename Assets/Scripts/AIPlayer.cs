using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            List<Tile> attackTilesInRange = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], attackRange);
            List<Tile> moveToAttackTilesInRange = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + attackRange);
            List<Tile> movementTilesInRange = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + 1000);

            //attack if in range and with lowest HP
            if (attackTilesInRange.Where(x => GameManager.instance.players.Where(y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            {
                List<Player> opponentsInRange = attackTilesInRange.Select(x => GameManager.instance.players.Where(y => y.HP > 0 && y != this && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
                opponentsInRange.RemoveAll(x => x == null);
                Player opponents = opponentsInRange.OrderBy(x => -x.HP).First();

                GameManager.instance.highlightTileAt(gridPosition, Color.red, attackRange);
                GameManager.instance.attackWithCurrentPlayer(GameManager.instance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y]);
            }
            //move toward nearest attack range if opponent
            else if (moveToAttackTilesInRange.Where(x => GameManager.instance.players.Where(y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            {
                List<Player> opponentsInRange = moveToAttackTilesInRange.Select(x => GameManager.instance.players.Where(y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
                opponentsInRange.RemoveAll(x => x == null);

                List<Tile> opponentTiles = new List<Tile>();
                int searchRange = attackRange;

                while (opponentTiles.Count == 0)
                {
                    for (int i = 0; i < opponentsInRange.Count; i++)
                    {
                        opponentTiles.AddRange(TileHighlight.FindHighlight(GameManager.instance.map[(int)opponentsInRange[i].gridPosition.x][(int)opponentsInRange[i].gridPosition.y], searchRange));
                    }
                    opponentTiles.RemoveAll(x => GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).Count() > 0);
                    opponentTiles = opponentTiles.Intersect(moveToAttackTilesInRange).ToList();
                    searchRange++;
                }

                List<TilePath> opponentPaths = new List<TilePath>();
                for (int i = 0; i < opponentTiles.Count; i++)
                {
                    opponentPaths.Add(TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponentTiles[i].gridPosition.x][(int)opponentTiles[i].gridPosition.y]));
                }
                opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();

                //Player opponents = opponentsInRange.OrderBy(x => -x.HP).OrderBy(x => -TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y], true).listOfTiles.Count).FirstOrDefault();

                GameManager.instance.highlightTileAt(gridPosition, Color.blue, movementPerActionPoint, false);

                //List<Tile> path = TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponents.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles;
                GameManager.instance.moveCurrentPlayer(opponentPaths[0].listOfTiles[Mathf.Min(opponentPaths[0].listOfTiles.Count - 1, movementPerActionPoint)]);
            }
            //move toward nearest opponent
            else if (movementTilesInRange.Where(x => GameManager.instance.players.Where(y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            {
                List<Player> opponentsInRange = movementTilesInRange.Select(x => GameManager.instance.players.Where(y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).FirstOrDefault()).ToList();
                opponentsInRange.RemoveAll(x => x == null);

                List<Tile> opponentTiles = new List<Tile>();
                int searchRange = attackRange;

                while (opponentTiles.Count == 0)
                {
                    for (int i = 0; i < opponentsInRange.Count; i++)
                    {
                        opponentTiles.AddRange(TileHighlight.FindHighlight(GameManager.instance.map[(int)opponentsInRange[i].gridPosition.x][(int)opponentsInRange[i].gridPosition.y], searchRange));
                    }
                    opponentTiles.RemoveAll(x => GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).Count() > 0);
                    //opponentTiles = opponentTiles.Intersect(moveToAttackTilesInRange).ToList();
                    searchRange++;
                }
                //for (int i = 0; i < opponentsInRange.Count; i++)
                //{
                //    opponentTiles.AddRange(GameManager.instance.map[(int)opponentsInRange[i].gridPosition.x][(int)opponentsInRange[i].gridPosition.y].neighbors);
                //}
                //opponentTiles.RemoveAll(x => GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).Count() > 0);
                //opponentTiles = opponentTiles.Intersect(moveToAttackTilesInRange).ToList();

                //Player opponents = opponentsInRange.OrderBy(x => -x.HP).OrderBy(x => -TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).listOfTiles.Count).FirstOrDefault();
                List<TilePath> opponentPaths = new List<TilePath>();
                for (int i = 0; i < opponentTiles.Count; i++)
                {
                    opponentPaths.Add(TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponentTiles[i].gridPosition.x][(int)opponentTiles[i].gridPosition.y]));
                }
                opponentPaths = opponentPaths.OrderBy(x => x.listOfTiles.Count).ToList();
                GameManager.instance.highlightTileAt(gridPosition, Color.blue, movementPerActionPoint, false);

                //List<Tile> path = TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponents.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles;
                GameManager.instance.moveCurrentPlayer(opponentPaths[0].listOfTiles[(int)Mathf.Min(Mathf.Max(opponentPaths[0].listOfTiles.Count - 1 - 1, 0), movementPerActionPoint)]);
            }
            //end turn if nothing can do
            else
            {
                GameManager.instance.removeHighlightTiles();
                actionPoint = 2;
                moving = false;
                attacking = false;
                GameManager.instance.nextTurn();
            }
        }

        base.TurnUpdate();
    }

    public override void TurnOnGUI()
    {


        base.TurnOnGUI();
    }
}
