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
    public TileType2D pallerSelection2D = TileType2D.Plain;
    public int spriteIndex = 0;
    public int spritesMax = 0;

    public int playerIndex = 0;
    public int enemyIndex = 0;
	public int enemyAIIndex = 0;
    public int enemyLevelIndex = 0;

    public float playerScale = 0.7f;

    Transform mapTransform;
    Transform playerTransform;

    [Header("Tile UI")]
	public Text tileTypeName;
    public Image tileSprite;

	[Header("Player UI")]
	public Text playerTypeName;
    public Toggle isNewPlayer;

	[Header("Enemy UI")]
	public Text enemyTypeName;
	public Text enemyLevelName;
	public Text enemyAIName;
    public EnemyAIType aiTypeSelection = EnemyAIType.Attacker;
    public InputField searchRange;

    [Header("System UI")]
    public Text settingTypeName;
    public InputField fileName;
    public InputField inputMapSizeX;
    public InputField inputMapSizeY;

    private GameElement gameElement;
    private List<CharacterTemplate> playerTypes;
	private List<CharacterTemplate> enemyTypes;
    private List<CharacterLevelTemplate> enemyLevels;
    private List<PlayerRecord> userPlayerRecords;
    private List<PlayerRecord> enemyPlayerRecords;

    private string userPlayerNameFormat = "UserPlayer{0}";
    private string enemyPlayerNameFormat = "EnemyPlayer{0}";

    private string gameElementfilename = "ObjectJson.txt";

    // Use this for initialization
    void Awake()
    {
        instance = this;
        mapTransform = transform.Find("Map");
        playerTransform = transform.Find("Players");
	}

    private void Start()
    {
        generateBlankMap(Convert.ToInt32(inputMapSizeX.text), Convert.ToInt32(inputMapSizeY.text));
        ControlGroup();
        LoadGameElements();
        ResetTileType();

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

    #region UI Control
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

    private void ResetTileType()
    {
        spriteIndex = 0;
        GetSpriteSize();
        SetSprite();
        tileTypeName.text = pallerSelection2D.ToString();
    }

    public void NextType()
    {
        int temp = (int)pallerSelection2D;
        temp++;
        temp = temp % Enum.GetNames(typeof(TileType2D)).Length;
        pallerSelection2D = (TileType2D)temp;
        ResetTileType();
    }

	public void LastType()
	{
		int temp = (int)pallerSelection2D;
		temp--;
        temp = (temp + Enum.GetNames(typeof(TileType2D)).Length) % Enum.GetNames(typeof(TileType2D)).Length;
        pallerSelection2D = (TileType2D)temp;
        spriteIndex = 0;
        GetSpriteSize();
        SetSprite();
        tileTypeName.text = pallerSelection2D.ToString();
        ResetTileType();
    }

    public void NextSprite()
    {
        int temp = spriteIndex;
        temp++;
        temp = temp % spritesMax;
        spriteIndex = temp;
        SetSprite();
    }

    public void LastSprite()
    {
        int temp = spriteIndex;
        temp--;
        temp = (temp + spritesMax) % spritesMax;
        spriteIndex = temp;
        SetSprite();
    }


    private void GetSpriteSize()
    {
        switch (pallerSelection2D)
        {
            case TileType2D.Impassible:
                spritesMax = TilePrefabHolder.instance.tile_Impassible_prefab.GetComponent<SpriteMetarial>().spriteCount;
                break;
            case TileType2D.Road:
                spritesMax = TilePrefabHolder.instance.tile_Road_prefab.GetComponent<SpriteMetarial>().spriteCount;
                break;
            case TileType2D.Plain:
                spritesMax = TilePrefabHolder.instance.tile_Plain_prefab.GetComponent<SpriteMetarial>().spriteCount;
                break;
            case TileType2D.Wasteland:
                spritesMax = TilePrefabHolder.instance.tile_Wasteland_prefab.GetComponent<SpriteMetarial>().spriteCount;
                break;
            case TileType2D.Villa:
                spritesMax = TilePrefabHolder.instance.tile_Villa_prefab.GetComponent<SpriteMetarial>().spriteCount;
                break;
            case TileType2D.Forest:
                spritesMax = TilePrefabHolder.instance.tile_Forest_prefab.GetComponent<SpriteMetarial>().spriteCount;
                break;
        }
    }

    private void SetSprite()
    {
        List<Sprite> temp = new List<Sprite>();
        switch (pallerSelection2D)
        {
            case TileType2D.Impassible:
                temp = TilePrefabHolder.instance.tile_Impassible_prefab.GetComponent<SpriteMetarial>().sprites;
                break;
            case TileType2D.Road:
                temp = TilePrefabHolder.instance.tile_Road_prefab.GetComponent<SpriteMetarial>().sprites;
                break;
            case TileType2D.Plain:
                temp = TilePrefabHolder.instance.tile_Plain_prefab.GetComponent<SpriteMetarial>().sprites;
                break;
            case TileType2D.Wasteland:
                temp = TilePrefabHolder.instance.tile_Wasteland_prefab.GetComponent<SpriteMetarial>().sprites;
                break;
            case TileType2D.Villa:
                temp = TilePrefabHolder.instance.tile_Villa_prefab.GetComponent<SpriteMetarial>().sprites;
                break;
            case TileType2D.Forest:
                temp = TilePrefabHolder.instance.tile_Forest_prefab.GetComponent<SpriteMetarial>().sprites;
                break;
        }

        if (temp.Count > 0)
        {
            tileSprite.sprite = temp[spriteIndex];
        }
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

    public void NextLevelType()
    {
        int temp = enemyLevelIndex;
        temp++;
        temp = temp % enemyLevels.Count;
        enemyLevelIndex = temp;
        if (enemyLevels.Count > 0)
        {
            enemyLevelName.text = string.Format("Lv {0} HP {1}", enemyLevels[enemyLevelIndex].level, enemyLevels[enemyLevelIndex].hp);
        }
    }

    public void LastLevelType()
    {
        int temp = enemyLevelIndex;
        temp--;
        temp = (temp + enemyTypes.Count) % enemyTypes.Count;
        enemyLevelIndex = temp;
        if (enemyLevels.Count > 0)
        {
            enemyLevelName.text = string.Format("Lv {0} HP {1}", enemyLevels[enemyLevelIndex].level, enemyLevels[enemyLevelIndex].hp);
        }
    }

    #endregion

    public void SetPlayer(Vector2 gridPosion, Vector3 pos,bool isDelete = false)
    {
        if (isDelete)
        {
            PlayerRecord prtemp = userPlayerRecords.Where(x => x.locX == (int)gridPosion.x && x.locY == (int)gridPosion.y).FirstOrDefault();
            if (prtemp != null)
            {
                Transform temp = playerTransform.Find(string.Format(userPlayerNameFormat, prtemp.id));
                Destroy(temp.gameObject);
                userPlayerRecords.Remove(prtemp);
            }
        }
        else
        {
            if (userPlayerRecords.Intersect(enemyPlayerRecords).Count() == 0 || userPlayerRecords.Intersect(enemyPlayerRecords).Where(x => x.locX == (int)gridPosion.x && x.locY == (int)gridPosion.y).Count() == 0)
            {
                if (userPlayerRecords.Where(x => x.characterId == playerIndex).Count() > 0)
                {
                    PlayerRecord prtemp = userPlayerRecords.Where(x => x.characterId == playerIndex).FirstOrDefault();
                    Transform temp = playerTransform.Find(string.Format(userPlayerNameFormat, prtemp.id));
                    Destroy(temp.gameObject);
                    userPlayerRecords.Remove(prtemp);
                }

                int id = userPlayerRecords.Count > 0 ? userPlayerRecords.Max(x => x.id) + 1 : 0;
                userPlayerRecords.Add(new PlayerRecord(id, false, isNewPlayer.isOn, (int)gridPosion.x, (int)gridPosion.y, playerIndex, 0, aiTypeSelection, 0));

                GameObject newPlayer = Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, new Vector3(pos.x, playerScale, pos.z), Quaternion.Euler(new Vector3()));
                newPlayer.name = string.Format(userPlayerNameFormat, id);
                newPlayer.transform.SetParent(playerTransform);
                newPlayer.GetComponent<UserPlayer>().gridPosition = gridPosion;
            }
        }
    }

    public void SetEnemyPlayer(Vector2 gridPosion, Vector3 pos, bool isDelete = false)
    {
        if (isDelete)
        {
            PlayerRecord prtemp = enemyPlayerRecords.Where(x => x.locX == (int)gridPosion.x && x.locY == (int)gridPosion.y).FirstOrDefault();
            if (prtemp != null)
            {
                Transform temp = playerTransform.Find(string.Format(enemyPlayerNameFormat, prtemp.id));
                Destroy(temp.gameObject);
                enemyPlayerRecords.Remove(prtemp);
            }
        }
        else
        {
            if (userPlayerRecords.Intersect(enemyPlayerRecords).Count() == 0 || userPlayerRecords.Intersect(enemyPlayerRecords).Where(x => x.locX == (int)gridPosion.x && x.locY == (int)gridPosion.y).Count() == 0)
            {
                int intSearchRange = string.IsNullOrEmpty(searchRange.text) ? 0 : Convert.ToInt32(searchRange.text);
                if (aiTypeSelection == EnemyAIType.Defanser && intSearchRange <= 0)
                {
                    Debug.Log("Search range invalid");
                    return;
                }

                int id = enemyPlayerRecords.Count > 0 ? enemyPlayerRecords.Max(x => x.id) + 1 : 0;
                enemyPlayerRecords.Add(new PlayerRecord(id, false, true, (int)gridPosion.x, (int)gridPosion.y, enemyIndex, enemyLevelIndex, aiTypeSelection, intSearchRange));

                GameObject newPlayer = Instantiate(PlayerPrefabHolder.instance.enemyPlayer_prefab, new Vector3(pos.x, playerScale, pos.z), Quaternion.Euler(new Vector3()));
                newPlayer.name = string.Format(enemyPlayerNameFormat, id);
                newPlayer.transform.SetParent(playerTransform);
                newPlayer.GetComponent<AIPlayer>().gridPosition = gridPosion;
            }
        }
    }


    private void SetEnemyLevels()
    {
        if (enemyTypes != null && enemyTypes.Count > 0)
        {
            enemyLevelIndex = 0;
            enemyLevels = enemyTypes[enemyIndex].levelData;
            enemyLevelName.text = string.Format("Lv {0} HP {1}", enemyLevels[enemyLevelIndex].level, enemyLevels[enemyLevelIndex].hp);
        }
    }

	private void LoadGameElements()
    {
        if (!System.IO.File.Exists(gameElementfilename))
        {
            Debug.Log("File is not exist!");
            return;
        }
        try
        {
            gameElement = ObjectSaveLoad.JsonLoad<GameElement>(gameElementfilename);

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
        if (Convert.ToInt32(inputMapSizeX.text) <= 0 || Convert.ToInt32(inputMapSizeY.text) <= 0)
        {
            Debug.LogError("Map size invalid");
        }
        generateBlankMap(Convert.ToInt32(inputMapSizeX.text), Convert.ToInt32(inputMapSizeY.text));
    }

    private void generateBlankMap(int mSizeX, int mSizeY)
    {
        Vector3 connerPointA = Vector3.zero;
        Vector3 connerPointB = Vector3.zero;
        Vector3 connerPointC = Vector3.zero;
        Vector3 connerPointD = Vector3.zero;

        mapSizeX = mSizeX;
        mapSizeY = mSizeY;

        for (int i = 0; i < mapTransform.transform.childCount; i++)
        {
            Destroy(mapTransform.transform.GetChild(i).gameObject);
        }

        Vector3 pos = Vector3.zero;
        map = new List<List<Tile>>();
        mapHex = new List<List<HexTile>>();


        userPlayerRecords = new List<PlayerRecord>();
        enemyPlayerRecords = new List<PlayerRecord>();

        //Hexagons
        for (int i = 0; i < mapSizeY; i++)
        {
            int offset = i >> 1;
            List<HexTile> row = new List<HexTile>();
            for (int j = -offset; j < mapSizeX - offset; j++)
            {
                if (i % 2 == 1 && j == mapSizeX - offset - 1)
                {
                    continue;
                }
                HexTile tile = ((GameObject)Instantiate(PrefabHolder.instance.base_hex_tile_prefab, new Vector3(), Quaternion.Euler(new Vector3()))).GetComponent<HexTile>();
                tile.TileInitializer(mapTransform, TileType.Normal, TileType2D.Plain, 0, j, i, mapSizeX, mapSizeY);
                row.Add(tile);
                if (i == 0)
                {
                    if (j == 0)
                    {
                        connerPointA = tile.HexTilePos();
                    }
                    else if (j == mapSizeX - offset - 1)
                    {
                        connerPointB = tile.HexTilePos();
                    }
                }
            }
            mapHex.Add(row);
        }
        connerPointD = new Vector3(0, 0, -mapSizeY + 1);
        connerPointC = new Vector3(connerPointB.x, 0, connerPointD.z);

        ScreenController.instance.SetLimitPoint(connerPointA, connerPointB, connerPointC, connerPointD);

        ScreenController.instance.SetCameraPos(new Vector3((float)mapSizeX / 2, 0, -(float)mapSizeY / 2));

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
        //MapSaveLoad.Save(MapSaveLoad.CreateMapContainer(mapHex), fileName.text + ".xml");

        ObjectSaveLoad.JsonSave(MapSaveLoad.CreateMapContainer(mapHex,userPlayerRecords, enemyPlayerRecords), fileName.text + ".txt");
    }

    public void loadMapFromXml()
    {
		if (String.IsNullOrEmpty(fileName.text))
		{
			Debug.Log("File name cannot be empty!");
			return;
		}
        if (!System.IO.File.Exists(fileName.text + ".txt"))
		{
            Debug.Log("File is not exist!");
            return;
		}

        Vector3 connerPointA = Vector3.zero;
        Vector3 connerPointB = Vector3.zero;
        Vector3 connerPointC = Vector3.zero;
        Vector3 connerPointD = Vector3.zero;

        MapXmlContainer container = ObjectSaveLoad.JsonLoad<MapXmlContainer>(fileName.text + ".txt");
        mapSizeX = container.sizeX;
        mapSizeY = container.sizeY;

        inputMapSizeX.text = mapSizeX.ToString();
        inputMapSizeY.text = mapSizeY.ToString();

        for (int i = 0; i < mapTransform.transform.childCount; i++)
        {
            Destroy(mapTransform.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < playerTransform.transform.childCount; i++)
        {
            Destroy(playerTransform.transform.GetChild(i).gameObject);
        }

        ScreenController.instance.SetCameraPos(new Vector3((float)mapSizeX /2, 0, -(float)mapSizeY / 2));

        Vector3 pos = Vector3.zero;
        map = new List<List<Tile>>();
        mapHex = new List<List<HexTile>>();

        userPlayerRecords = container.userPlayerRecords;
        enemyPlayerRecords = container.enemyPlayerRecords;

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
                tile.TileInitializer(mapTransform, (TileType)container.tiles.Where(x => x.locX == j && x.locY == i).FirstOrDefault().id, (TileType2D)container.tiles.Where(x => x.locX == j && x.locY == i).FirstOrDefault().id, container.tiles.Where(x => x.locX == j && x.locY == i).FirstOrDefault().spritIndex, j, i, mapSizeX, mapSizeY);
                row.Add(tile);
                if (i == 0)
                {
                    if (j == 0)
                    {
                        connerPointA = tile.HexTilePos();
                    }
                    else if (j == mapSizeX - offset - 1)
                    {
                        connerPointB = tile.HexTilePos();
                    }
                }
            }
            mapHex.Add(row);
        }
        connerPointD = new Vector3(0, 0, -mapSizeY + 1);
        connerPointC = new Vector3(connerPointB.x, 0, connerPointD.z);

        ScreenController.instance.SetLimitPoint(connerPointA, connerPointB, connerPointC, connerPointD);

        LoadPlayers();
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

    private void LoadPlayers()
    {
        for (int i = 0; i < userPlayerRecords.Count; i++)
        {
            int id = userPlayerRecords[i].id;

            Vector3 pos = mapHex[userPlayerRecords[i].locY][userPlayerRecords[i].locX + (userPlayerRecords[i].locY >> 1)].HexTilePos();
            GameObject newPlayer = Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, new Vector3(pos.x, playerScale, pos.z), Quaternion.Euler(new Vector3()));
            newPlayer.name = string.Format(userPlayerNameFormat, id);
            newPlayer.transform.SetParent(playerTransform);
            newPlayer.GetComponent<UserPlayer>().gridPosition = new Vector2(userPlayerRecords[i].locX, userPlayerRecords[i].locY);
        }

        for (int i = 0; i < enemyPlayerRecords.Count; i++)
        {
            int id = enemyPlayerRecords[i].id;

            Vector3 pos = mapHex[enemyPlayerRecords[i].locY][enemyPlayerRecords[i].locX + (enemyPlayerRecords[i].locY >> 1)].HexTilePos();
            GameObject newPlayer = Instantiate(PlayerPrefabHolder.instance.enemyPlayer_prefab, new Vector3(pos.x, playerScale, pos.z), Quaternion.Euler(new Vector3()));
            newPlayer.name = string.Format(enemyPlayerNameFormat, id);
            newPlayer.transform.SetParent(playerTransform);
            newPlayer.GetComponent<AIPlayer>().gridPosition = new Vector2(enemyPlayerRecords[i].locX, enemyPlayerRecords[i].locY);
        }

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
