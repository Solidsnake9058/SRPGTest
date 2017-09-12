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

    public CanvasGroup tileGroup;
	public CanvasGroup playerGroup;
	public CanvasGroup enemyGroup;

    public MapSettingType settingSelection = MapSettingType.Tile;
    public TileType pallerSelection = TileType.Normal;

    public int playerIndex = 0;
    public int enemyIndex = 0;
	public int enemyAIIndex = 0;
    Transform mapTransform;

    [Header("Tile UI")]
	public Text tileTypeName;

	[Header("Player UI")]
	public Text playerTypeName;

	[Header("Enemy UI")]
	public Text enemyTypeName;
	public Text enemyLevelName;
	public Text enemyAIName;
    public EnemyAIType aiTypeSelection = EnemyAIType.Attacker;

	[Header("System UI")]
    public Text settingTypeName;
    public InputField fileName;

    private GameElement gameElement;
    private List<CharacterTemplate> playerTypes;
	private List<CharacterTemplate> enemyTypes;
    private List<CharacterLevelTemplate> enemyLevels;

    // Use this for initialization
    void Awake()
    {
        instance = this;
        mapTransform = transform.Find("Map");
        generateBlankMap(38, 32);
        ControlGroup();
        LoadGameElements();

        if (playerTypes.Count > 0)
        {
            playerTypeName.text = playerTypes[playerIndex].name;
        }
		if (enemyTypes.Count > 0)
		{
			enemyTypeName.text = enemyTypes[enemyIndex].name;
            SetEnemyLevels();
		}


	}

    // Update is called once per frame
    void Update()
    {

    }

	public void NextSettingType()
	{
		int temp = (int)settingSelection;
		temp++;
		temp = temp % Enum.GetNames(typeof(MapSettingType)).Length;
		settingSelection = (MapSettingType)temp;
		settingTypeName.text = settingSelection.ToString();
        ControlGroup();
	}

	public void LastSettingType()
	{
		int temp = (int)settingSelection;
		temp--;
		temp = (temp + Enum.GetNames(typeof(MapSettingType)).Length) % Enum.GetNames(typeof(MapSettingType)).Length;
		settingSelection = (MapSettingType)temp;
		settingTypeName.text = settingSelection.ToString();
        ControlGroup();
	}

    public void NextPlayerType()
    {
        int temp = playerIndex;
        temp++;
        temp = temp % playerTypes.Count;
        playerIndex = temp;
        if (playerTypes.Count > 0)
        {
            playerTypeName.text = playerTypes[playerIndex].name;
        }
    }

	public void LastPlayerType()
	{
		int temp = playerIndex;
		temp--;
		temp = (temp + playerTypes.Count) % playerTypes.Count;
		playerIndex =temp;
        if (playerTypes.Count > 0)
        {
            playerTypeName.text = playerTypes[playerIndex].name;
        }
	}


	private void ControlGroup()
    {
        DisableGroup(tileGroup);
        DisableGroup(playerGroup);
        DisableGroup(enemyGroup);
        switch (settingSelection)
        {
            case MapSettingType.Tile:
                EnableGroup(tileGroup);
                break;
            case MapSettingType.Player:
                EnableGroup(playerGroup);
				break;
            case MapSettingType.Enemy:
                EnableGroup(enemyGroup);
				break;
        }
    }

	public void DisableGroup(CanvasGroup group)
	{
		group.alpha = 0;
		group.interactable = false;
		group.blocksRaycasts = false;
	}

	public void EnableGroup(CanvasGroup group)
	{
		group.alpha = 1;
		group.interactable = true;
		group.blocksRaycasts = true;
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


	public void NextAIType()
	{
		int temp = (int)aiTypeSelection;
		temp++;
		temp = temp % Enum.GetNames(typeof(EnemyAIType)).Length;
		aiTypeSelection = (EnemyAIType)temp;
		enemyAIName.text = aiTypeSelection.ToString();
	}

	public void LastAIType()
	{
		int temp = (int)aiTypeSelection;
		temp--;
		temp = (temp + Enum.GetNames(typeof(EnemyAIType)).Length) % Enum.GetNames(typeof(EnemyAIType)).Length;
		aiTypeSelection = (EnemyAIType)temp;
		enemyAIName.text = aiTypeSelection.ToString();
	}

	public void NextEnemyType()
	{
		int temp = enemyIndex;
		temp++;
        temp = temp % enemyTypes.Count;
		enemyIndex = temp;
		if (enemyTypes.Count > 0)
		{
			enemyTypeName.text = enemyTypes[enemyIndex].name;
            SetEnemyLevels();
		}
	}

	public void LastEnemyType()
	{
		int temp = enemyIndex;
		temp--;
		temp = (temp + enemyTypes.Count) % enemyTypes.Count;
		enemyIndex = temp;
		if (enemyTypes.Count > 0)
		{
			enemyTypeName.text = enemyTypes[enemyIndex].name;
            SetEnemyLevels();
		}
	}

    private void SetEnemyLevels()
    {
        if (enemyTypes != null && enemyTypes.Count > 0)
        {
            enemyAIIndex = 0;
            enemyLevels = enemyTypes[enemyIndex].levelData;
            enemyLevelName.text = string.Format("Lv {0}", enemyLevels[enemyAIIndex].level);
        }
    }

	private void LoadGameElements()
    {
        if (!System.IO.File.Exists("Objects.xml"))
        {
            Debug.Log("File is not exist!");
            return;
        }
        try
        {
            ObjectXmlContainer container = ObjectSaveLoad.XmlLoad<ObjectXmlContainer>("Objects.xml");
            gameElement = ObjectSaveLoad.CreateGameElements(container);

            playerTypes = gameElement.characters.Where(x => !x.enemy).ToList();
            playerTypes.Sort((x,y)=>{ return x.id.CompareTo(y.id); });
            enemyTypes = gameElement.characters.Where(x => x.enemy).ToList();
            enemyTypes.Sort((x, y) => { return x.id.CompareTo(y.id); });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
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
