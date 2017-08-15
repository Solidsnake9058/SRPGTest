using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager inatance;

    public GameObject tilePrefab;
    public GameObject userPlayerPrefab;
    public GameObject aiPlayerPrefab;


    public int mapHeight = 32;
    public int mapWeight = 38;

    public float cubeSize = 1;

    public List<List<Tile>> map = new List<List<Tile>>();
    //List<List<HexTile>> map = new List<List<HexTile>>();

    public List<Player> players = new List<Player>();
    public int currentPlayerIndex = 0;

    void Awake()
    {
        inatance = this;
    }

    private void Start()
    {
        genetareMap();
        genetarePlayers();

    }

    void Update()
    {
        if (players[currentPlayerIndex].HP > 0)
        {
            players[currentPlayerIndex].TurnUpdate();
        }
        else
        {
            nextTurn();
        }
    }

    private void OnGUI()
    {
        players[currentPlayerIndex].TurnOnGUI();
    }

    public void nextTurn()
    {
        if (currentPlayerIndex + 1 < players.Count)
        {
            currentPlayerIndex++;
        }
        else
        {
            currentPlayerIndex = 0;
        }
    }

    public void moveCurrentPlayer(Tile destTile)
    {
        if (destTile.GetComponent<Renderer>().material.color !=Color.white && !destTile.impassible)
        {
            removeHighlightTiles();
            players[currentPlayerIndex].moving = false;
            foreach (Tile t in TilePathFinder.FindPath(map[(int)(players[currentPlayerIndex].gridPosition.x)][(int)(players[currentPlayerIndex].gridPosition.y)], destTile).listOfTiles)
            {
                //Debug.Log("move (" + t.gridPostion.x + "," + t.gridPostion.y + ") ,position (" + map[(int)t.gridPostion.x][(int)t.gridPostion.y].transform.position.ToString() + ")");

                players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPostion.x][(int)t.gridPostion.y].transform.position + 1.5f * Vector3.up);
                //Debug.Log(players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].x + "," + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].z);
            }
            players[currentPlayerIndex].gridPosition = destTile.gridPostion;
            players[currentPlayerIndex].positionQueue.RemoveAt(0);
            //players[currentPlayerIndex].positionQueue.Reverse();
        }
        else
        {
            Debug.Log("destination invalid");
        }
    }

    public void attackWithCurrentPlayer(Tile destTile)
    {
        if (destTile.GetComponent<Renderer>().material.color == Color.white && !destTile.impassible)
        {
            Debug.Log("destination invalid");
            return;
        }

        Player target = null;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].gridPosition == destTile.gridPostion)
            {
                target = players[i];
                break;
            }
        }

        if (target != null)
        {
            //Debug.Log("p: (" + players[currentPlayerIndex].gridPosition.x + "," + players[currentPlayerIndex].gridPosition.y + ") t:(" + target.gridPosition.x + "," + target.gridPosition.y + ")");

            if (players[currentPlayerIndex].gridPosition.x >= target.gridPosition.x - 1 && players[currentPlayerIndex].gridPosition.x <= target.gridPosition.x + 1 &&
                players[currentPlayerIndex].gridPosition.y >= target.gridPosition.y - 1 && players[currentPlayerIndex].gridPosition.y <= target.gridPosition.y + 1)
            {
                players[currentPlayerIndex].actionPoint--;

                bool hit = Random.Range(0f, 1f) <= players[currentPlayerIndex].attackChance;
                if (hit)
                {
                    int amountOfDamage = (int)Mathf.Floor(players[currentPlayerIndex].damageBase + Random.Range(0, players[currentPlayerIndex].damageRollSides));

                    target.HP -= amountOfDamage;

                    Debug.Log(players[currentPlayerIndex].playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage!");
                }
                else
                {
                    Debug.Log(players[currentPlayerIndex].playerName + " missed " + target.playerName + "!");
                }
            }
            else
            {
                Debug.Log("Target is not adjacent!");
            }
        }
    }

    public void highlightTileAt(Vector2 originPosition, Color highlightColor ,int distance)
    {
        List<Tile> highlightTiles = TileHighlight.FindHighlight(map[(int)originPosition.x][(int)originPosition.y], distance);

        foreach (Tile t in highlightTiles)
        {
            t.transform.GetComponent<Renderer>().material.color = highlightColor;
        }
    }

    public void removeHighlightTiles()
    {
        for (int i = 0; i < mapWeight; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (!map[i][j].impassible)
                {
                    map[i][j].transform.GetComponent<Renderer>().material.color = Color.white;
                }
            }
        }
    }

    void genetareMap()
    {
        Vector3 pos = Vector3.zero;
        map = new List<List<Tile>>();
        //map = new List<List<HexTile>>();

        //for (int i = 0; i < mapHeight; i++)
        //{
        //    int offset = i >> 1;
        //    List<HexTile> row = new List<HexTile>();
        //    for (int j = -offset; j < mapWeight - offset; j++)
        //    {
        //        int Z = -i - j;
        //        pos.x = ((float)(j - Z) / 2.0f);
        //        pos.z = -i;

        //        HexTile tile = ((GameObject)Instantiate(tilePrefab, cubeSize * pos, Quaternion.Euler(new Vector3()))).GetComponent<HexTile>();

        //        tile.hex.q = j;
        //        tile.hex.r = i;
        //        row.Add(tile);
        //    }
        //    map.Add(row);
        //}

        for (int i = 0; i < mapWeight; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = 0; j < mapHeight; j++)
            {
                Tile tile = ((GameObject)Instantiate(tilePrefab, new Vector3(i - Mathf.Floor(mapWeight / 2), 0, -j + Mathf.Floor(mapHeight / 2)), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
                tile.gridPostion = new Vector2(i, j);
                row.Add(tile);
            }
            map.Add(row);
        }
    }

    void genetarePlayers()
    {
        UserPlayer player;

        player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(0 - Mathf.Floor(mapWeight / 2), 1.5f, -0 + Mathf.Floor(mapHeight / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(0, 0);
        player.playerName = "A";

        players.Add(player);

        player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3((mapWeight - 1) - Mathf.Floor(mapWeight / 2), 1.5f, -(mapHeight - 1) + Mathf.Floor(mapHeight / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(mapWeight - 1, mapHeight - 1);
        player.playerName = "B";

        players.Add(player);

        player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3((4-Mathf.Floor( mapWeight/2)), 1.5f, -4 + Mathf.Floor(mapHeight / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(4, 4);
        player.playerName = "C";

        players.Add(player);

        //AIPlayer aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(6 - Mathf.Floor(mapWeight / 2), 1.5f, -4 + Mathf.Floor(mapHeight / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();

        //players.Add(aiplayer);

    }
}
