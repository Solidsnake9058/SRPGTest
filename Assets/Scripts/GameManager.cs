using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager inatance;

    public GameObject tilePrefab;
    public GameObject userPlayerPrefab;
    public GameObject aiPlayerPrefab;
    Transform mapTransform;

    public int mapSizeX = 32;
    public int mapSizeY = 38;

    public float cubeSize = 1;

    public List<List<Tile>> map = new List<List<Tile>>();
    //List<List<HexTile>> map = new List<List<HexTile>>();

    public List<Player> players = new List<Player>();
    public int currentPlayerIndex = 0;

    void Awake()
    {
        inatance = this;
        mapTransform = transform.Find("Map");

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
        currentPlayerIndex++;
        currentPlayerIndex = currentPlayerIndex % players.Count;
    }

    public void moveCurrentPlayer(Tile destTile)
    {
        if (destTile.visual.transform.GetComponentInChildren<Renderer>().materials[0].color !=Color.white && !destTile.impassible)
        {
            removeHighlightTiles();
            players[currentPlayerIndex].moving = false;
            foreach (Tile t in TilePathFinder.FindPath(map[(int)(players[currentPlayerIndex].gridPosition.x)][(int)(players[currentPlayerIndex].gridPosition.y)], destTile, players.Where(x => x.gridPosition != players[currentPlayerIndex].gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles)
            {
                players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position + 1.5f * Vector3.up);
                //Debug.Log(players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].x + "," + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].z);
            }
            players[currentPlayerIndex].gridPosition = destTile.gridPosition;
        }
        else
        {
            Debug.Log("destination invalid");
        }
    }

    public void attackWithCurrentPlayer(Tile destTile)
    {
        if (destTile.visual.transform.GetComponentInChildren<Renderer>().materials[0].color == Color.white && !destTile.impassible)
        {
            Debug.Log("destination invalid");
            return;
        }

        Player target = null;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].gridPosition == destTile.gridPosition)
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
                removeHighlightTiles();
                players[currentPlayerIndex].attacking = false;

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

    public void highlightTileAt(Vector2 originLocation, Color highlightColor, int distance, bool ignorePlayers = true)
    {
        List<Tile> highlightTiles = new List<Tile>();

        if (ignorePlayers)
        {
            highlightTiles = TileHighlight.FindHighlight(map[(int)originLocation.x][(int)originLocation.y], distance);
        }
        else
        {
            highlightTiles = TileHighlight.FindHighlight(map[(int)originLocation.x][(int)originLocation.y], distance, players.Where(x => x.gridPosition != originLocation).Select(x => x.gridPosition).ToArray());
        }

        foreach (Tile t in highlightTiles)
        {
            t.visual.transform.GetComponentInChildren<Renderer>().materials[0].color = highlightColor;
        }
    }

    public void removeHighlightTiles()
    {
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                if (!map[i][j].impassible)
                {
                    map[i][j].visual.transform.GetComponentInChildren<Renderer>().materials[0].color = Color.white;
                }
            }
        }
    }

    void genetareMap()
    {
        loadMapFromXml();
        //Vector3 pos = Vector3.zero;
        //map = new List<List<Tile>>();
        ////map = new List<List<HexTile>>();

        ////for (int i = 0; i < mapHeight; i++)
        ////{
        ////    int offset = i >> 1;
        ////    List<HexTile> row = new List<HexTile>();
        ////    for (int j = -offset; j < mapWeight - offset; j++)
        ////    {
        ////        int Z = -i - j;
        ////        pos.x = ((float)(j - Z) / 2.0f);
        ////        pos.z = -i;

        ////        HexTile tile = ((GameObject)Instantiate(tilePrefab, cubeSize * pos, Quaternion.Euler(new Vector3()))).GetComponent<HexTile>();

        ////        tile.hex.q = j;
        ////        tile.hex.r = i;
        ////        row.Add(tile);
        ////    }
        ////    map.Add(row);
        ////}

        //for (int i = 0; i < mapWeight; i++)
        //{
        //    List<Tile> row = new List<Tile>();
        //    for (int j = 0; j < mapHeight; j++)
        //    {
        //        Tile tile = ((GameObject)Instantiate(tilePrefab, new Vector3(i - Mathf.Floor(mapWeight / 2), 0, -j + Mathf.Floor(mapHeight / 2)), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
        //        tile.gridPosition = new Vector2(i, j);
        //        row.Add(tile);
        //    }
        //    map.Add(row);
        //}
    }

    private void loadMapFromXml()
    {
        MapXmlContainer container = MapSaveLoad.Load("map.xml");
        mapSizeX = container.sizeX;
        mapSizeY = container.sizeY;

        for (int i = 0; i < mapTransform.transform.childCount; i++)
        {
            Destroy(mapTransform.transform.GetChild(i).gameObject);
        }

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

        for (int i = 0; i < mapSizeX; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = 0; j < mapSizeY; j++)
            {
                Tile tile = ((GameObject)Instantiate(PrefabHolder.instance.base_tile_prefab, new Vector3(i - Mathf.Floor(mapSizeX / 2), 0, -j + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
                tile.gridPosition = new Vector2(i, j);
                tile.transform.parent = mapTransform;
                tile.setType((TileType)container.tiles.Where(x => x.locX == i && x.locY == j).FirstOrDefault().id);
                row.Add(tile);
            }
            map.Add(row);
        }
    }


    void genetarePlayers()
    {
        UserPlayer player;

        player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(0, 0);
        player.playerName = "A";

        players.Add(player);

        player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3((mapSizeX - 1) - Mathf.Floor(mapSizeX / 2), 1.5f, -(mapSizeY - 1) + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(mapSizeX - 1, mapSizeY - 1);
        player.playerName = "B";

        players.Add(player);

        player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3((4-Mathf.Floor(mapSizeX / 2)), 1.5f, -4 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(4, 4);
        player.playerName = "C";

        players.Add(player);

        player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3((8 - Mathf.Floor(mapSizeX / 2)), 1.5f, -8 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(8, 8);
        player.playerName = "D";

        players.Add(player);

        AIPlayer aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(6 - Mathf.Floor(mapSizeX / 2), 1.5f, -4 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        aiplayer.gridPosition = new Vector2(6, 4);
        aiplayer.name = "Enemy1";

        players.Add(aiplayer);

        aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(8 - Mathf.Floor(mapSizeX / 2), 1.5f, -4 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        aiplayer.gridPosition = new Vector2(8, 4);
        aiplayer.name = "Enemy2";

        players.Add(aiplayer);

        aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(11 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        aiplayer.gridPosition = new Vector2(11, 0);
        aiplayer.name = "Enemy3";

        players.Add(aiplayer);

        aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(18 - Mathf.Floor(mapSizeY / 2), 1.5f, -8 + Mathf.Floor(mapSizeX / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        aiplayer.gridPosition = new Vector2(18, 8);
        aiplayer.name = "Enemy4";

        players.Add(aiplayer);

    }
}
