using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class MapCreatorManager : MonoBehaviour
{
    public static MapCreatorManager instance;

    public int mapSizeX;
    public int mapSizeY;

    public List<List<Tile>> map = new List<List<Tile>>();

    public TileType pallerSelection = TileType.Normal;
    Transform mapTransform;

    // Use this for initialization
    void Awake()
    {
        instance = this;
        mapTransform = transform.Find("Map");
        genetareBlankMap(32, 38);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void genetareBlankMap(int mSizeX, int mSizeY)
    {
        mapSizeX = mSizeX;
        mapSizeY = mSizeY;

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
                tile.setType(TileType.Normal);
                row.Add(tile);
            }
            map.Add(row);
        }
    }

    private void saveMapFromXml()
    {
        MapSaveLoad.Save(MapSaveLoad.CreateMapContainer(map), "map.xml");
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

    private void OnGUI()
    {
        Rect rect = new Rect(10, Screen.height - 80, 100, 60);

        if (GUI.Button(rect,"Normal"))
        {
            pallerSelection = TileType.Normal;
        }
        rect = new Rect(10 + (100 + 10) * 1, Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Difficult"))
        {
            pallerSelection = TileType.Difficult;
        }
        rect = new Rect(10 + (100 + 10) * 2, Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Very Difficult"))
        {
            pallerSelection = TileType.VeryDifficult;
        }
        rect = new Rect(10 + (100 + 10) * 3, Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Impassible"))
        {
            pallerSelection = TileType.Impassible;
        }

        //IO
        rect = new Rect(Screen.width - (10 + (100 + 10) * 2), Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Clear Map"))
        {
            genetareBlankMap(mapSizeX, mapSizeY);
        }

        rect = new Rect(Screen.width -(10+ (100 + 10) * 2), Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Load Map"))
        {
            loadMapFromXml();
        }

        rect = new Rect(Screen.width - (10 + (100 + 10) * 1), Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Save Map"))
        {
            saveMapFromXml();
        }
    }

}
