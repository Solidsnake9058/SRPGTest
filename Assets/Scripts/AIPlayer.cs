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
        //if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex] == this)
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
            List<Tile> attackTilesInRange = TileHighlight.FindHighlight(GameManager.inatance.map[(int)gridPosition.x][(int)gridPosition.y], attackRange);
            List<Tile> moveToAttackTilesInRange = TileHighlight.FindHighlight(GameManager.inatance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + attackRange);
            List<Tile> movementTilesInRange = TileHighlight.FindHighlight(GameManager.inatance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + 1000);

            //attack if in range and with lowest HP
            if (attackTilesInRange.Where(x => GameManager.inatance.players.Where(y => y.HP > 0 && y != this && y.gridPosition == x.gridPostion).Count() > 0).Count() > 0)
            {
                List<Player> opponentsInRange = attackTilesInRange.Select(x => GameManager.inatance.players.Where(y => y.HP > 0 && y != this && y.gridPosition == x.gridPostion).FirstOrDefault()).ToList();
                opponentsInRange.RemoveAll(x => x == null);
                Player opponents = opponentsInRange.OrderBy(x => -x.HP).First();

                GameManager.inatance.highlightTileAt(gridPosition, Color.red, attackRange);
                GameManager.inatance.attackWithCurrentPlayer(GameManager.inatance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y]);
            }
            //move toward nearest attack range if opponent
            else if (moveToAttackTilesInRange.Where(x => GameManager.inatance.players.Where(y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPostion).Count() > 0).Count() > 0)
            {
                List<Player> opponentsInRange = moveToAttackTilesInRange.Select(x => GameManager.inatance.players.Where(y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPostion).FirstOrDefault()).ToList();
                opponentsInRange.RemoveAll(x => x == null);
                Player opponents = opponentsInRange.OrderBy(x => -x.HP).OrderBy(x => -TilePathFinder.FindPath(GameManager.inatance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.inatance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).listOfTiles.Count).FirstOrDefault();

                GameManager.inatance.highlightTileAt(gridPosition, Color.blue, movementPerActionPoint);

                List<Tile> path = TilePathFinder.FindPath(GameManager.inatance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.inatance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y], GameManager.inatance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponents.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles;
                GameManager.inatance.moveCurrentPlayer(path[(int)Mathf.Max(0, path.Count - 1 - attackRange)]);
            }
            //move toward nearest opponent
            else if (movementTilesInRange.Where(x => GameManager.inatance.players.Where(y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPostion).Count() > 0).Count() > 0)
            {
                List<Player> opponentsInRange = movementTilesInRange.Select(x => GameManager.inatance.players.Where(y => y.GetType() != typeof(AIPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPostion).FirstOrDefault()).ToList();
                opponentsInRange.RemoveAll(x => x == null);
                Player opponents = opponentsInRange.OrderBy(x => -x.HP).OrderBy(x => -TilePathFinder.FindPath(GameManager.inatance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.inatance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).listOfTiles.Count).FirstOrDefault();

                GameManager.inatance.highlightTileAt(gridPosition, Color.blue, movementPerActionPoint);

                List<Tile> path = TilePathFinder.FindPath(GameManager.inatance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.inatance.map[(int)opponents.gridPosition.x][(int)opponents.gridPosition.y], GameManager.inatance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponents.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles;
                GameManager.inatance.moveCurrentPlayer(path[(int)Mathf.Min(Mathf.Max(path.Count - 1 - 1, 0), movementPerActionPoint - 1)]);
            }
            //end turn if nothing can do
            else
            {
                GameManager.inatance.removeHighlightTiles();
                actionPoint = 2;
                moving = false;
                attacking = false;
                GameManager.inatance.nextTurn();
            }
        }

        base.TurnUpdate();
    }

    public override void TurnOnGUI()
    {


        base.TurnOnGUI();
    }
}
