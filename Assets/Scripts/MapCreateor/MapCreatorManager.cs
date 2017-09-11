using UnityEngine;
using UnityEngine.UI;
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
    public List<List<HexTile>> mapHex = new List<List<HexTile>>();

    public TileType pallerSelection = TileType.Normal;
    Transform mapTransform;
    public Text tileTypeName;
    public InputField fileName;
    // Use this for initialization
    void Awake()
    {
        instance = this;
        mapTransform = transform.Find("Map");
        generateBlankMap(38, 32);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NextType()
    {
        int temp = (int)pallerSelection;
        temp++;
        temp = temp % Enum.GetNames(typeof(TileType)).Length;
        pallerSelection = (TileType)temp;
        tileTypeName.text = pallerSelection.ToString();
    }

	public void LastType()
	{
		int temp = (int)pallerSelection;
		temp--;
        temp = (temp + Enum.GetNames(typeof(TileType)).Length) % Enum.GetNames(typeof(TileType)).Length;
		pallerSelection = (TileType)temp;
		tileTypeName.text = pallerSelection.ToString();
	}

    public void TrimFileName()
    {
        fileName.text = fileName.text.Trim();
    }

    public void generateBlankMapDefault()
    {
        generateBlankMap(38, 32);
    }

    private void generateBlankMap(int mSizeX, int mSizeY)
    {
        mapSizeX = mSizeX;
        mapSizeY = mSizeY;

        for (int i = 0; i < mapTransform.transform.childCount; i++)
        {
            Destroy(mapTransform.transform.GetChild(i).gameObject);
        }

        Vector3 pos = Vector3.zero;
        map = new List<List<Tile>>();
        mapHex = new List<List<HexTile>>();

        //Hexagons
        for (int i = 0; i < mapSizeY; i++)
        {
            int offset = i >> 1;
            List<HexTile> row = new List<HexTile>();
            for (int j = -offset; j < mapSizeX - offset; j++)
            {
                //int Z = -i - j;
                //pos.x = ((float)(j - Z) / 2.0f);
                //pos.z = -i;

                if (i % 2 == 1 && j == mapSizeX - offset - 1)
                {
                    continue;
                }
                HexTile tile = ((GameObject)Instantiate(PrefabHolder.instance.base_hex_tile_prefab, new Vector3(), Quaternion.Euler(new Vector3()))).GetComponent<HexTile>();
                tile.transform.parent = mapTransform;
                tile.setType(TileType.Normal);
                tile.hex.q = j;
                tile.hex.r = i;
                tile.mapSizeX = mapSizeX;
                tile.mapSizeY = mapSizeY;
                tile.gameObject.transform.localPosition = tile.HexTilePos();
                row.Add(tile);
            }
            mapHex.Add(row);
        }

        //Rectangle
        //for (int i = 0; i < mapSizeX; i++)
        //{
        //    List<Tile> row = new List<Tile>();
        //    for (int j = 0; j < mapSizeY; j++)
        //    {
        //        Tile tile = ((GameObject)Instantiate(PrefabHolder.instance.base_tile_prefab, new Vector3(i - Mathf.Floor(mapSizeX / 2), 0, -j + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
        //        tile.gridPosition = new Vector2(i, j);
        //        tile.transform.parent = mapTransform;
        //        tile.setType(TileType.Normal);
        //        row.Add(tile);
        //    }
        //    map.Add(row);
        //}
    }

    public void saveMapFromXml()
    {
        if (String.IsNullOrEmpty(fileName.text))
        {
            Debug.Log("File name cannot be empty!");
            return;
        }

        //MapSaveLoad.Save(MapSaveLoad.CreateMapContainer(map), fileName.text + ".xml");
        MapSaveLoad.Save(MapSaveLoad.CreateMapContainer(mapHex), fileName.text + ".xml");
    }

    public void loadMapFromXml()
    {
		if (String.IsNullOrEmpty(fileName.text))
		{
			Debug.Log("File name cannot be empty!");
			return;
		}
        if (!System.IO.File.Exists(fileName.text + ".xml"))
		{
            Debug.Log("File is not exist!");
            return;
		}
        MapXmlContainer container = MapSaveLoad.Load(fileName.text + ".xml");
        mapSizeX = container.sizeX;
        mapSizeY = container.sizeY;

        for (int i = 0; i < mapTransform.transform.childCount; i++)
        {
            Destroy(mapTransform.transform.GetChild(i).gameObject);
        }

        Vector3 pos = Vector3.zero;
        map = new List<List<Tile>>();
        mapHex = new List<List<HexTile>>();

        //Hexagons
        for (int i = 0; i < mapSizeY; i++)
        {
            int offset = i >> 1;
            List<HexTile> row = new List<HexTile>();
            for (int j = -offset; j < mapSizeX - offset; j++)
            {
                //int Z = -i - j;
                //pos.x = ((float)(j - Z) / 2.0f);
                //pos.z = -i;

                if (i % 2 == 1 && j == mapSizeX - offset - 1)
                {
                    continue;
                }
                HexTile tile = ((GameObject)Instantiate(PrefabHolder.instance.base_hex_tile_prefab, new Vector3(), Quaternion.Euler(new Vector3()))).GetComponent<HexTile>();
                tile.transform.parent = mapTransform;
                tile.hex.q = j;
                tile.hex.r = i;
                tile.mapSizeX = mapSizeX;
                tile.mapSizeY = mapSizeY;
                tile.gameObject.transform.localPosition = tile.HexTilePos();
                tile.setType((TileType)container.tiles.Where(x => x.locX == j && x.locY == i).FirstOrDefault().id);
                row.Add(tile);
            }
            mapHex.Add(row);
        }

        /*
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
        */
    }

    private void OnGUI()
    {
        /*
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
        */
    }

}
