using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class Tile : MonoBehaviour
{
    GameObject Prefab;
    public GameObject visual;

    public TileType type = TileType.Normal;

    public Vector2 gridPosition = Vector2.zero;

    public int movementCost = 1;
    public bool impassible = false;

    public List<Tile> neighbors = new List<Tile>();

    // Use this for initialization
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            generateNeighbors();
        }
    }

    void generateNeighbors()
    {
        neighbors = new List<Tile>();

        //up
        if (gridPosition.y > 0)
        {
            Vector2 n = new Vector2(gridPosition.x, gridPosition.y - 1);
            neighbors.Add(GameMidiator.m_Instance.m_StageMapManager.m_Map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
        }
        //down
        if (gridPosition.y < GameManager.m_Instance.mapSizeY - 1)
        {
            Vector2 n = new Vector2(gridPosition.x, gridPosition.y + 1);
            neighbors.Add(GameMidiator.m_Instance.m_StageMapManager.m_Map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
        }

        //left
        if (gridPosition.x > 0)
        {
            Vector2 n = new Vector2(gridPosition.x - 1, gridPosition.y);
            neighbors.Add(GameMidiator.m_Instance.m_StageMapManager.m_Map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
        }

        //right
        if (gridPosition.x < GameManager.m_Instance.mapSizeX - 1)
        {
            Vector2 n = new Vector2(gridPosition.x + 1, gridPosition.y);
            neighbors.Add(GameMidiator.m_Instance.m_StageMapManager.m_Map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseEnter()
    {
        if (SceneManager.GetActiveScene().name == "MapCreatorScene" && Input.GetMouseButton(0))
        {
            setType(MapCreatorManager.instance.pallerSelection);
        }
        //string msg = "";
        //foreach (Tile t in neighbors)
        //{
        //    msg += "(" + t.gridPostion.x + "," + t.gridPostion.y + ") ";
        //}
        //Debug.Log("Pos: (" + gridPostion.x + "," + gridPostion.y + ") ,Neighbors: " + msg);

        //if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving)
        //{
        //    transform.GetComponent<Renderer>().material.color = Color.blue;
        //}
        //else if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking)
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
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            //if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving)
            //{
            //    //GameManager.instance.moveCurrentPlayer(this);
            //}
            //else if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking)
            //{
            //    //GameManager.instance.attackWithCurrentPlayer(this);
            //}
            //else
            {
                impassible = impassible ? false : true;
                if (impassible)
                {
                    transform.GetComponentInChildren<Renderer>().material.color = new Color(0.5f, 0.5f, 0.0f);
                }
                else
                {
                    transform.GetComponentInChildren<Renderer>().material.color = Color.white;
                }
            }
        }
        else if (SceneManager.GetActiveScene().name == "MapCreatorScene")
        {
            setType(MapCreatorManager.instance.pallerSelection);
        }
    }

    public void setType(TileType t)
    {
        type = t;
        //definition of TileType properties
        switch (t)
        {
            case TileType.Normal:
                movementCost = 1;
                impassible = false;
                Prefab = PrefabHolder.instance.tile_Normal_prefab;
                break;
            case TileType.Difficult:
                movementCost = 2;
                impassible = false;
                Prefab = PrefabHolder.instance.tile_Difficult_prefab;
                break;
            case TileType.VeryDifficult:
                movementCost = 4;
                impassible = false;
                Prefab = PrefabHolder.instance.tile_VeryDifficult_prefab;
                break;
            case TileType.Impassible:
                movementCost = 9999;
                impassible = true;
                Prefab = PrefabHolder.instance.tile_Impassible_prefab;
                break;
        }

        generateVisuals();
    }

    public void generateVisuals()
    {
        GameObject container = transform.Find("Visuals").gameObject;
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Destroy(container.transform.GetChild(i).gameObject);
        }
        GameObject newVisual = (GameObject)Instantiate(Prefab, transform.position, Quaternion.Euler(new Vector3(0,0,0)));
        newVisual.transform.parent = container.transform;
    }
}
