using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{

    public Vector2 gridPostion = Vector2.zero;

    public int movementCost = 1;

    public List<Tile> neighbors = new List<Tile>();

    // Use this for initialization
    void Start()
    {
        generateNeighbors();
    }

    void generateNeighbors()
    {
        neighbors = new List<Tile>();

        //up
        if (gridPostion.y > 0)
        {
            Vector2 n = new Vector2(gridPostion.x, gridPostion.y - 1);
            neighbors.Add(GameManager.inatance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
        }
        //down
        if (gridPostion.y < GameManager.inatance.mapHeight - 1)
        {
            Vector2 n = new Vector2(gridPostion.x, gridPostion.y + 1);
            neighbors.Add(GameManager.inatance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
        }

        //left
        if (gridPostion.x > 0)
        {
            Vector2 n = new Vector2(gridPostion.x - 1, gridPostion.y);
            neighbors.Add(GameManager.inatance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
        }

        //right
        if (gridPostion.x < GameManager.inatance.mapWeight - 1)
        {
            Vector2 n = new Vector2(gridPostion.x + 1, gridPostion.y);
            neighbors.Add(GameManager.inatance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseEnter()
    {
        //string msg = "";
        //foreach (Tile t in neighbors)
        //{
        //    msg += "(" + t.gridPostion.x + "," + t.gridPostion.y + ") ";
        //}
        //Debug.Log("Pos: (" + gridPostion.x + "," + gridPostion.y + ") ,Neighbors: " + msg);

        //if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex].moving)
        //{
        //    transform.GetComponent<Renderer>().material.color = Color.blue;
        //}
        //else if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex].attacking)
        //{
        //    transform.GetComponent<Renderer>().material.color = Color.red;
        //}

        //Debug.Log("Pos (" + gridPostion.x + "," + gridPostion.y + ")");
    }

    private void OnMouseExit()
    {
        //transform.GetComponent<Renderer>().material.color = Color.white;

    }

    private void OnMouseDown()
    {
        if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex].moving)
        {
            GameManager.inatance.moveCurrentPlayer(this);
        }
        else if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex].attacking)
        {
            GameManager.inatance.attackWithCurrentPlayer(this);
        }

    }
}
