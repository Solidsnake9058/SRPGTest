using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager inatance;

    public GameObject tilePrefab;
    public GameObject userPlayer;

    public int mapHeight = 11;
    public int mapWeight = 15;

    List<List<Tile>> map = new List<List<Tile>>();
    List<Player> players = new List<Player>();
    int currentPlayerIndex = 0;

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
        players[currentPlayerIndex].TurnUpdate();
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
        players[currentPlayerIndex].moveDestination = destTile.transform.position + 1.5f * Vector3.up;
    }

    void genetareMap()
    {
        map = new List<List<Tile>>();
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

        player = ((GameObject)Instantiate(userPlayer, new Vector3(0 - Mathf.Floor(mapWeight / 2), 1.5f, -0 + Mathf.Floor(mapHeight / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();

        players.Add(player);

        player = ((GameObject)Instantiate(userPlayer, new Vector3((mapWeight - 1) - Mathf.Floor(mapWeight / 2), 1.5f, -(mapHeight - 1) + Mathf.Floor(mapHeight / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();

        players.Add(player);

    }
}
