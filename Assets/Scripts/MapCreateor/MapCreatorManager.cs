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

    public CanvasGroup stageGroup;
    public CanvasGroup tileGroup;
	public CanvasGroup playerGroup;
	public CanvasGroup enemyGroup;

    public MapSettingType settingSelection = MapSettingType.Tile;
    public TileType pallerSelection = TileType.Normal;
    public TileType2D pallerSelection2D = TileType2D.Plain;
    public bool isScenarioMode = false;
    public bool isGetPos = false;
    public int spriteIndex = 0;
    public int spritesMax = 0;
    public int spriteChestIndex = 0;
    public int spritesChestMax = 0;

    public int playerIndex = 0;
    public int enemyIndex = 0;

    public int enemyAIIndex = 0;
    public int enemyLevelIndex = 0;

    public float playerHeight = 0;

    public Transform pointer;

    Transform mapTransform;
    Transform playerTransform;
    Transform actorPlayerTransform;

    [Header("Main UI")]
    public Text changeModeText;

    [Header("Tile UI")]
	public Text tileTypeName;
    public Image tileSprite;
    public Image tileChestSprite;
    public Toggle isHaveChest;
    public InputField chestGoldInput;
    public Dropdown chestItemInput;
    public Dropdown chestWeaponInput;
    public int chestItem = -1;
    public int chestWeapon =-1;
    public CanvasGroup chestGroup;
    public CanvasGroup shopGroup;
    public Toggle isShop;
    public Dropdown shopItemInput;
    public Dropdown shopWeaponInput;
    public List<int> shopItemList;
    public List<int> shopWeaponList;
    public GameObject shopSelectionPrefab;
    public RectTransform shopList;

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

    [Header("Scenario UI")]
    public CanvasGroup scenarioGroup;
    public CanvasGroup createScenarioGroup;
    public CanvasGroup getPositionGroup;

    public CanvasGroup createActorGroup;
    public CanvasGroup controlActorGroup;
    public CanvasGroup controlCameraGroup;
    public CanvasGroup dialogGroup;

    public int selectedScenarioId = -1;
    public RectTransform scenarioItemList;
    public GameObject creatorScenarioSelectorPrefab;
    public Dropdown scenarioType;
    public Dropdown scenarioConditionType;
    public Dropdown userPlayer;
    public Dropdown enemyPlayer;
    public Toggle isOnceEvent;
    //action
    public Dropdown actionType;
	public InputField waitTime;
	public Toggle isToDark;
    public RectTransform scenarioActionItemList;
    public GameObject creatorActionSelectorPrefab;
    //create actor
    public int selectedActionId = -1;
    public Dropdown characterTemplate;
    public Dropdown createActorPivot;
    public InputField createActorX;
    public InputField createActorY;
    public RectTransform createActorItemList;
    public GameObject creatorActorSelectorPrefab;
    //control actor
    public Dropdown selectedActor;
    public Dropdown selectedActorPivot;
    public InputField controlActorX;
    public InputField controlActorY;
    //control camera
    public InputField controlCameraX;
    public InputField controlCameraY;
    //dialog
    public InputField dialogName;
    public InputField dialogContect;
    //get pos
    public InputField getPosX;
    public InputField getPosY;

    private GameElement gameElement;
    private List<CharacterTemplate> playerTypes;
	private List<CharacterTemplate> enemyTypes;
    private List<CharacterLevelTemplate> enemyLevels;
    private List<PlayerRecord> userPlayerRecords;
    private List<PlayerRecord> enemyPlayerRecords;
    private List<Player> stagePlayer;
    private List<Player> actorPlayer;
    private List<PlayerRecord> scenarioActorPlayerRecords;
    private List<Scenario> scenarios;
    private List<ScenarioAction> scenarioActions;
    private Dictionary<int, string> dicItem;
    private Dictionary<int, string> dicWeapon;

    private string userPlayerNameFormat = "UserPlayer{0}";
    private string enemyPlayerNameFormat = "EnemyPlayer{0}";

    private string gameElementfilename = "ObjectJson.txt";

    // Use this for initialization
    void Awake()
    {
        instance = this;
        mapTransform = transform.Find("Map");
        playerTransform = transform.Find("Players");
        actorPlayerTransform = transform.Find("ActorPlayers");
    }

    private void Start()
    {
        generateBlankMap(Convert.ToInt32(inputMapSizeX.text), Convert.ToInt32(inputMapSizeY.text));
        isScenarioMode = false;
        EnableGroup(stageGroup);
        DisableGroup(scenarioGroup);
        ControlGroup();
        LoadGameElements();
        InitialScenarioUI();
        ResetTileType();
        ResetPointer();

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

    public void ResetPointer()
    {
        pointer.position = new Vector3(0, -100, 0);
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


	private void ControlGroup(bool hideAll = false)
    {
        DisableGroup(tileGroup);
        DisableGroup(playerGroup);
        DisableGroup(enemyGroup);
        if (hideAll)
        {
            return;
        }
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

        DisableGroup(chestGroup);
        DisableGroup(shopGroup);
        switch (pallerSelection2D)
        {
            case TileType2D.Plain:
                EnableGroup(chestGroup);
                break;
            case TileType2D.Villa:
                EnableGroup(shopGroup);
                break;
        }
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

    public void NextChestSprite()
    {
        int temp = spriteChestIndex;
        temp++;
        temp = temp % spritesChestMax;
        spriteChestIndex = temp;
        SetSprite();
    }

    public void LastChestSprite()
    {
        int temp = spriteChestIndex;
        temp--;
        temp = (temp + spritesChestMax) % spritesChestMax;
        spriteChestIndex = temp;
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
                spritesChestMax = TilePrefabHolder.instance.tile_Plain_prefab.GetComponent<SpriteMetarial>().chestSprites.Count;

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
        List<Sprite> chestTemp = new List<Sprite>();
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
                chestTemp = TilePrefabHolder.instance.tile_Plain_prefab.GetComponent<SpriteMetarial>().chestSprites;
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
        if (chestTemp.Count > 0)
        {
            tileChestSprite.sprite = chestTemp[spriteChestIndex];
        }
        else
        {
            tileChestSprite.sprite = null;
        }

    }

    public void SetChestValue()
    {
        chestItem = dicItem.Where(x => x.Value == chestItemInput.options[chestItemInput.value].text).FirstOrDefault().Key;
        chestWeapon = dicWeapon.Where(x => x.Value == chestWeaponInput.options[chestWeaponInput.value].text).FirstOrDefault().Key;
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

    public void ChangeMode()
    {
        isScenarioMode = !isScenarioMode;
        ResetPointer();
        if (isScenarioMode)
        {
            changeModeText.text = "Stage Mode";
            SetUserPlayer();
            SetEnemyPlayer();
            DisableGroup(stageGroup);
            ControlGroup(true);
            NewScenario();
            playerTransform.position = new Vector3(0, -100, 0);
            actorPlayerTransform.position = new Vector3(0, 0, 0);
        }
        else
        {
            changeModeText.text = "Scenario Mode";
            EnableGroup(stageGroup);
            DisableGroup(scenarioGroup);
            ControlGroup();
            playerTransform.position = new Vector3(0, 0, 0);
            actorPlayerTransform.position = new Vector3(0, -100, 0);
        }
    }
    #endregion

    #region Stage
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
                //playerId=
                if (userPlayerRecords.Where(x => x.characterId == playerTypes[playerIndex].id).Count() > 0)
                {
                    PlayerRecord prtemp = userPlayerRecords.Where(x => x.characterId == playerTypes[playerIndex].id).FirstOrDefault();
                    Transform temp = playerTransform.Find(string.Format(userPlayerNameFormat, prtemp.id));
                    Destroy(temp.gameObject);
                    userPlayerRecords.Remove(prtemp);
                }

                int id = userPlayerRecords.Count > 0 ? userPlayerRecords.Max(x => x.id) + 1 : 0;
                userPlayerRecords.Add(new PlayerRecord(id, false, isNewPlayer.isOn, (int)gridPosion.x, (int)gridPosion.y, playerTypes[playerIndex].id, 0, aiTypeSelection, 0));

                GameObject newPlayer = Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, new Vector3(pos.x, playerHeight, pos.z), Quaternion.Euler(new Vector3(0, 180, 0)), isScenarioMode ? actorPlayerTransform : playerTransform);
                newPlayer.name = string.Format(userPlayerNameFormat, id);
                newPlayer.GetComponent<UserPlayer>().gridPosition = gridPosion;
                newPlayer.GetComponent<UserPlayer>().SetPlayerModel();
                if (isScenarioMode) 
                {
                    actorPlayer.Add(newPlayer.GetComponent<UserPlayer>());
                }
                else
                {
                    stagePlayer.Add(newPlayer.GetComponent<UserPlayer>());
                }
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
                enemyPlayerRecords.Add(new PlayerRecord(id, false, true, (int)gridPosion.x, (int)gridPosion.y, enemyIndex, enemyTypes[enemyLevelIndex].id, aiTypeSelection, intSearchRange));

                GameObject newPlayer = Instantiate(PlayerPrefabHolder.instance.enemyPlayer_prefab, new Vector3(pos.x, playerHeight, pos.z), Quaternion.Euler(new Vector3(0, 180, 0)));
                newPlayer.name = string.Format(enemyPlayerNameFormat, id);
                newPlayer.transform.SetParent(playerTransform);
                newPlayer.GetComponent<AIPlayer>().gridPosition = gridPosion;
                newPlayer.GetComponent<AIPlayer>().SetPlayerModel();
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
            SetItem();
            SetWeapon();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private void SetItem()
    {
        dicItem = new Dictionary<int, string>();

        dicItem.Add(-1, "None");

        chestItemInput.options.Clear();
        shopItemInput.options.Clear();
        chestItemInput.options.Add(new Dropdown.OptionData() { text = "None" });
        shopItemInput.options.Add(new Dropdown.OptionData() { text = "None" });

        foreach (Item item in gameElement.items)
        {
            dicItem.Add(item.id, item.name);
            chestItemInput.options.Add(new Dropdown.OptionData() { text = item.name });
            if (!item.sell && item.price > 0)
            {
                shopItemInput.options.Add(new Dropdown.OptionData() { text = item.name });
            }
        }
        chestItemInput.RefreshShownValue();
        shopItemInput.RefreshShownValue();
    }

    private void SetWeapon()
    {
        dicWeapon = new Dictionary<int, string>();

        dicWeapon.Add(-1, "None");

        chestWeaponInput.options.Clear();
        shopWeaponInput.options.Clear();
        chestWeaponInput.options.Add(new Dropdown.OptionData() { text = "None" });
        shopWeaponInput.options.Add(new Dropdown.OptionData() { text = "None" });
        foreach (Weapon weapon in gameElement.weapons)
        {
            dicWeapon.Add(weapon.id, weapon.name);
            chestWeaponInput.options.Add(new Dropdown.OptionData() { text = weapon.name });
            if (!weapon.sell && weapon.price > 0)
            {
                shopWeaponInput.options.Add(new Dropdown.OptionData() { text = weapon.name });
            }
        }
        chestWeaponInput.RefreshShownValue();
        shopWeaponInput.RefreshShownValue();
    }

    public void AddShopItem(bool isWeapon)
    {
        if (isWeapon)
        {
            int id = dicWeapon.Where(x => x.Value == shopWeaponInput.options[shopWeaponInput.value].text).FirstOrDefault().Key;
            if (!shopWeaponList.Contains(id))
            {
                shopWeaponList.Add(id);
            }
        }
        else
        {
            int id = dicItem.Where(x => x.Value == shopItemInput.options[shopItemInput.value].text).FirstOrDefault().Key;
            if (!shopItemList.Contains(id))
            {
                shopItemList.Add(id);
            }
        }
        SetShopList();
    }

    public void RemoveShopItem(bool isWeapon,string id)
    {
        if (isWeapon)
        {
            if (shopWeaponList.Contains(Convert.ToInt32(id)))
            {
                shopWeaponList.Remove(shopWeaponList.Where(x => x.ToString() == id).FirstOrDefault());
            }
        }
        else
        {
            if (shopItemList.Contains(Convert.ToInt32(id)))
            {
                shopItemList.Remove(shopItemList.Where(x => x.ToString() == id).FirstOrDefault());
            }
        }
        SetShopList();
    }

    private void SetShopList()
    {
        for (int i = 0; i < shopList.transform.childCount; i++)
        {
            Destroy(shopList.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < shopItemList.Count; i++)
        {
            GameObject item = Instantiate(shopSelectionPrefab, new Vector3(), Quaternion.identity, shopList);
            item.GetComponent<ShopEditSelect>().SetName(gameElement.items.Where(x => x.id == shopItemList[i]).FirstOrDefault().name, false, shopItemList[i]);
        }

        for (int i = 0; i < shopWeaponList.Count; i++)
        {
            GameObject item = Instantiate(shopSelectionPrefab, new Vector3(), Quaternion.identity, shopList);
            item.GetComponent<ShopEditSelect>().SetName(gameElement.weapons.Where(x => x.id == shopWeaponList[i]).FirstOrDefault().name, false, shopWeaponList[i]);
        }
    }
    #endregion

    #region Scenario
    public void NewScenario()
    {
        InitialScenarioUI();
        ClearActor();
        EnableGroup(scenarioGroup);
        scenarioActions = new List<ScenarioAction>();
        actorPlayer = new List<Player>();
    }

    private void InitialScenarioUI()
    {
        SetScenarioType();
        SetScenarioConditionType();
        SetActionType();
        SetActorPivotType();
        SetCreateActorPlayer();
        EnableGroup(createScenarioGroup);
        DisableGroup(createActorGroup);
        DisableGroup(controlActorGroup);
        DisableGroup(controlCameraGroup);
        DisableGroup(dialogGroup);
        DisableGroup(getPositionGroup);
        actionType.value = 0;
        actionType.RefreshShownValue(); ChangeActionType();
        scenarioType.interactable = true;
        scenarioType.value = 0;
        scenarioType.RefreshShownValue();
        scenarioConditionType.interactable = true;
        scenarioConditionType.value = 0;
        scenarioConditionType.RefreshShownValue();
    }

    private void SetScenarioType()
    {
        scenarioType.options.Clear();

        for (int i = 0; i < (int)ScenarioType.Max; i++)
        {
            scenarioType.options.Add(new Dropdown.OptionData() { text = Enum.GetName(typeof(ScenarioType), (ScenarioType)i) });
        }
        scenarioType.RefreshShownValue();
    }

    private void SetScenarioConditionType()
    {
        scenarioConditionType.options.Clear();

        for (int i = 0; i < (int)ScenarioConditionType.Max; i++)
        {
            scenarioConditionType.options.Add(new Dropdown.OptionData() { text = Enum.GetName(typeof(ScenarioConditionType), (ScenarioConditionType)i) });
        }
        scenarioConditionType.RefreshShownValue();
    }

    private void SetUserPlayer()
    {
        userPlayer.options.Clear();
        userPlayer.options.Add(new Dropdown.OptionData() { text = "-1,Any" });

        for (int i = 0; i < userPlayerRecords.Count; i++)
        {
            userPlayer.options.Add(new Dropdown.OptionData() { text = string.Format("{0},{1}", userPlayerRecords[i].id, gameElement.characters.Where(x => !x.enemy && x.id == userPlayerRecords[i].characterId).FirstOrDefault().name) });
        }
        userPlayer.RefreshShownValue();
    }

    private void SetEnemyPlayer()
    {
        enemyPlayer.options.Clear();
        enemyPlayer.options.Add(new Dropdown.OptionData() { text = "-1,Any" });

        for (int i = 0; i < enemyPlayerRecords.Count; i++)
        {
            enemyPlayer.options.Add(new Dropdown.OptionData() { text = string.Format("{0},{1}", enemyPlayerRecords[i].id, gameElement.characters.Where(x => x.enemy && x.id == enemyPlayerRecords[i].characterId).FirstOrDefault().name) });
        }
        enemyPlayer.RefreshShownValue();
    }

    private void SetActionType()
    {
        actionType.options.Clear();

        for (int i = 0; i < (int)ScenarioActionType.Max; i++)
        {
            actionType.options.Add(new Dropdown.OptionData() { text = Enum.GetName(typeof(ScenarioActionType), (ScenarioActionType)i) });
        }
        actionType.RefreshShownValue();
    }

    private void SetActorPivotType()
    {
        createActorPivot.options.Clear();
        selectedActorPivot.options.Clear();

        for (int i = 0; i < (int)ScenarioActorPivotType.Max; i++)
        {
            createActorPivot.options.Add(new Dropdown.OptionData() { text = Enum.GetName(typeof(ScenarioActorPivotType), (ScenarioActorPivotType)i) });
            selectedActorPivot.options.Add(new Dropdown.OptionData() { text = Enum.GetName(typeof(ScenarioActorPivotType), (ScenarioActorPivotType)i) });
        }
        createActorPivot.RefreshShownValue();
        selectedActorPivot.RefreshShownValue();
    }

    private void SetCreateActorPlayer()
    {
        characterTemplate.options.Clear();

        List<CharacterTemplate> temp = gameElement.characters.OrderBy(x => x.enemy ? 1 : 0).ThenBy(x => x.id).ToList();
        for (int i = 0; i < temp.Count; i++)
        {
            characterTemplate.options.Add(new Dropdown.OptionData() { text = string.Format("{0},{1},{2}", temp[i].enemy ? "E" : "F", temp[i].name, temp[i].id) });
        }
        characterTemplate.RefreshShownValue();
    }

    public void ChangeActionType()
    {
        DisableGroup(createActorGroup);
        DisableGroup(controlActorGroup);
        DisableGroup(controlCameraGroup);
        DisableGroup(dialogGroup);
        ScenarioActionType temp = (ScenarioActionType)Enum.Parse(typeof(ScenarioActionType), actionType.options[actionType.value].text);
        switch (temp)
        {
            case ScenarioActionType.Dialog:
                EnableGroup(dialogGroup);
                break;
            case ScenarioActionType.CreateActor:
                EnableGroup(createActorGroup);
                scenarioActorPlayerRecords = new List<PlayerRecord>();
                ResetCreateActor();
                break;
            case ScenarioActionType.ControlActor:
                EnableGroup(controlActorGroup);
                SetActorSelector();
                break;
            case ScenarioActionType.SetCamera:
            case ScenarioActionType.ControlCamera:
                EnableGroup(controlCameraGroup);
                break;
        }
    }

    public void SetGetPos()
    {
        DisableGroup(createScenarioGroup);
        EnableGroup(getPositionGroup);
        isGetPos = true;

        getPosX.text = "";
        getPosY.text = "";
    }

    public void GetPos()
    {
        ResetPointer();
        DisableGroup(getPositionGroup);
        EnableGroup(createScenarioGroup);
        isGetPos = false;

        ScenarioActionType temp = (ScenarioActionType)Enum.Parse(typeof(ScenarioActionType), actionType.options[actionType.value].text);
        switch (temp)
        {
            case ScenarioActionType.CreateActor:
                createActorX.text = getPosX.text;
                createActorY.text = getPosY.text;
                break;
            case ScenarioActionType.ControlActor:
                controlActorX.text = getPosX.text;
                controlActorY.text = getPosY.text;
                break;
            case ScenarioActionType.SetCamera:
            case ScenarioActionType.ControlCamera:
                controlCameraX.text = getPosX.text;
                controlCameraY.text = getPosY.text;
                break;
        }
    }

    //Scenario
    public void ResetScenario()
    {
        selectedScenarioId = -1;
		actionType.value = 0;
		actionType.RefreshShownValue(); ChangeActionType();
		scenarioType.interactable = true;
		scenarioType.value = 0;
		scenarioType.RefreshShownValue();
		scenarioConditionType.interactable = true;
		scenarioConditionType.value = 0;
		scenarioConditionType.RefreshShownValue();
		userPlayer.value = 0;
        enemyPlayer.value = 0;
        isOnceEvent.isOn = true;
        scenarioActions = new List<ScenarioAction>();
        ClearActionList();
        ClearActor();
        ResetAction();
    }

    private void ClearScenarioList()
    {
        for (int i = 0; i < scenarioItemList.transform.childCount; i++)
        {
            Destroy(scenarioItemList.transform.GetChild(i).gameObject);
        }
    }

    public void RemoveScenarioList(string id)
    {
        Scenario temp = scenarios.Where(x => x.scenarioId.ToString() == id).FirstOrDefault();

        for (int i = 0; i < temp.scenarioActions.Count; i++)
        {
            List<Player> players = (temp.scenarioActions[i].createActors != null && temp.scenarioActions[i].createActors.Count > 0) ? actorPlayer.Where(x => temp.scenarioActions[i].createActors.Where(y => y.id == x.playerIndex).Count() > 0).ToList() : null;
            if (players != null)
            {
                for (int j = 0; j < players.Count; j++)
                {
                    actorPlayer.Remove(players[j]);
                    Destroy(players[j].gameObject);
                }
            }

        }

        scenarios.Remove(temp);
        ReloadScenarioList();
        ResetScenario();
    }

    private void ReloadScenarioList()
    {
        ClearScenarioList();
        for (int i = 0; i < scenarios.Count; i++)
        {
            CreaterActorSelector temp = Instantiate(creatorScenarioSelectorPrefab, new Vector3(), Quaternion.identity, scenarioItemList).GetComponent<CreaterActorSelector>();
            temp.SetActionName(scenarios[i].scenarioId, scenarios[i].scenarioType);
        }
    }

    public void AddScenario()
    {
        int scenarioId = selectedScenarioId == -1? scenarios.Count > 0 ? scenarios.OrderBy(x => -x.scenarioId).FirstOrDefault().scenarioId + 1 : 0: selectedScenarioId;
        ScenarioType scenarioTypeValue = (ScenarioType)Enum.Parse(typeof(ScenarioType), scenarioType.options[scenarioType.value].text);
        ScenarioConditionType scenarioConditionTypeValue = (ScenarioConditionType)Enum.Parse(typeof(ScenarioConditionType), scenarioConditionType.options[scenarioConditionType.value].text);
        string userPlayerId = userPlayer.options[userPlayer.value].text.Split(',')[0];
        string enemyPlayerId = enemyPlayer.options[enemyPlayer.value].text.Split(',')[0];

        if (selectedScenarioId == -1 && (scenarioTypeValue == ScenarioType.Openning || scenarioTypeValue == ScenarioType.StageClear))
        {
            if (scenarios.Count > 0 && scenarios.Where(x => x.scenarioType == scenarioTypeValue).Count() > 0)
            {
                Debug.Log("Allow only Opening/StageClear scenario");
                return;
            }
        }
        Scenario scenario = new Scenario(scenarioId, scenarioTypeValue, scenarioConditionTypeValue, isOnceEvent.isOn, scenarioActions);
        if (scenarioConditionTypeValue == ScenarioConditionType.BeforeBattle || scenarioConditionTypeValue == ScenarioConditionType.AfterBattle)
        {
            scenario = new Scenario(scenarioId, scenarioTypeValue, scenarioConditionTypeValue, Convert.ToInt32(userPlayerId), Convert.ToInt32(enemyPlayerId), isOnceEvent.isOn, scenarioActions);
        }

        if (selectedScenarioId == -1)
        {
            scenarios.Add(scenario);
        }
        else
        {
            //update
            scenario.scenarioId = selectedScenarioId;
            List<Scenario> tempScenarios = scenarios.Where(x => x.scenarioId != selectedScenarioId).ToList();
            tempScenarios.Add(scenario);
            scenarios.Clear();
            scenarios.AddRange(tempScenarios);
        }

        ReloadScenarioList();
        ResetScenario();
    }

    public void LoadScenario(string id)
    {
        Scenario scenario = scenarios.Where(x => x.scenarioId.ToString() == id).FirstOrDefault();
        if (scenario == null)
        {
            return;
        }
        ResetScenario();
        selectedScenarioId = Convert.ToInt32(id);
        scenarioType.interactable = false;
        scenarioType.value = scenarioType.options.FindIndex(x => x.text == Enum.GetName(typeof(ScenarioType), scenario.scenarioType));
        scenarioType.RefreshShownValue();

        scenarioConditionType.interactable = false;
        scenarioConditionType.value = scenarioConditionType.options.FindIndex(x => x.text == Enum.GetName(typeof(ScenarioConditionType), scenario.scenarioConditionType));
        scenarioConditionType.RefreshShownValue();

        if (selectedScenarioId != -1)
        {
            Scenario temp = scenarios.Where(x => x.scenarioId == selectedScenarioId).FirstOrDefault();
            if (temp != null)
            {
                for (int i = 0; i < temp.scenarioActions.Count; i++)
                {
                    if (temp.scenarioActions[i].scenarioActionType == ScenarioActionType.CreateActor)
                    {
                        for (int j = 0; j < temp.scenarioActions[i].createActors.Count; j++)
                        {
                            Vector3 pos = mapHex[Convert.ToInt32(temp.scenarioActions[i].createActors[j].locY)][Convert.ToInt32(temp.scenarioActions[i].createActors[j].locX) + (Convert.ToInt32(temp.scenarioActions[i].createActors[j].locY) >> 1)].HexTilePos();
                            Player player = Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, pos, Quaternion.identity, actorPlayerTransform).GetComponent<Player>();
                            player.gridPosition = new Vector2(Convert.ToInt32(temp.scenarioActions[i].createActors[j].locX), Convert.ToInt32(temp.scenarioActions[i].createActors[j].locY));
                            player.playerIndex = temp.scenarioActions[i].createActors[j].id;
                            player.gameObject.name = temp.scenarioActions[i].createActors[j].id.ToString();
                            player.SetPivot(temp.scenarioActions[i].createActors[j].scenarioActorPivotType);
                            player.SetPlayerModel();
                            actorPlayer.Add(player);
                        }
                    }
                }
            }
        }

        scenarioActions = scenario.scenarioActions;
        ReloadActionList();
        SetActorPos();
    }

    //Action   scenarioActionItemList
    public void ResetAction()
    {
        selectedActionId = -1;
        actionType.interactable = true;
        actionType.value = 0;
        actionType.RefreshShownValue();
        scenarioActorPlayerRecords = new List<PlayerRecord>();
        waitTime.text = "";
        isToDark.isOn = false;
        ResetCreateActor();
        ResetControlActor();
        ResetControlCamera();
        ResetDialog();

        SetActorPos();
    }

    private void ClearActionList()
    {
        for (int i = 0; i < scenarioActionItemList.transform.childCount; i++)
        {
            Destroy(scenarioActionItemList.transform.GetChild(i).gameObject);
        }
    }

    public void RemoveActionList(string id)
    {
        ScenarioAction temp = scenarioActions.Where(x => x.scenarioActionId.ToString() == id).FirstOrDefault();

        if (temp.scenarioActionType== ScenarioActionType.CreateActor)
        {
            //when create actor using
            if (temp.createActors.Select(x => x.id).Intersect(scenarioActions.Where(x => x.scenarioActionType == ScenarioActionType.ControlActor).Select(x => x.actorId)).Count() > 0)
            {
                Debug.Log("create actor is using");
                return;
            }
        }
        List<Player> players = (temp.createActors != null && temp.createActors.Count > 0) ? actorPlayer.Where(x => temp.createActors.Where(y => y.id == x.playerIndex).Count() > 0).ToList() : null;
        if (players != null)
        {
            for (int i = 0; i < players.Count; i++)
            {
                actorPlayer.Remove(players[i]);
                Destroy(players[i].gameObject);
            }
        }
        scenarioActions.Remove(temp);
        ResetActionID();
        ReloadActionList();
        ResetAction();
    }

    private void ReloadActionList()
    {
        ClearActionList();
        for (int i = 0; i < scenarioActions.Count; i++)
        {
            CreaterActorSelector temp = Instantiate(creatorActionSelectorPrefab, new Vector3(), Quaternion.identity, scenarioActionItemList).GetComponent<CreaterActorSelector>();
            temp.SetActionName(scenarioActions[i].scenarioActionId, scenarioActions[i].scenarioActionType);
        }
    }

    private void ResetActionID()
    {
        scenarioActions = scenarioActions.OrderBy(x => x.scenarioActionId).ToList();
        for (int i = 0; i < scenarioActions.Count; i++)
        {
            scenarioActions[i].scenarioActionId = i;
        }
    }

    public void AddAction()
    {
        int actionId = scenarioActions.Count > 0 ? scenarioActions.OrderBy(x => -x.scenarioActionId).FirstOrDefault().scenarioActionId + 1 : 0;
        ScenarioActionType temp = (ScenarioActionType)Enum.Parse(typeof(ScenarioActionType), actionType.options[actionType.value].text);
        float afterWaitTime = string.IsNullOrEmpty(waitTime.text) ? 0 : (float)Convert.ToDouble(waitTime.text);
        ScenarioAction action = new ScenarioAction();
        switch (temp)
        {
            case ScenarioActionType.Dialog:
                action = new ScenarioAction(actionId, dialogName.text, dialogContect.text);
                break;
            case ScenarioActionType.CreateActor:
                if (scenarioActorPlayerRecords.Count == 0)
                {
                    return;
                }
                action = new ScenarioAction(actionId, scenarioActorPlayerRecords, afterWaitTime, isToDark.isOn);
                break;
            case ScenarioActionType.ControlActor:
                ScenarioActorPivotType tempPivot = (ScenarioActorPivotType)Enum.Parse(typeof(ScenarioActorPivotType), selectedActorPivot.options[selectedActorPivot.value].text);
                string[] tempActorId = selectedActor.options[selectedActor.value].text.Split(',');
                action = new ScenarioAction(actionId, Convert.ToInt32(tempActorId[0]), new HexTile.HexCoord(Convert.ToInt32(controlActorX.text), Convert.ToInt32(controlActorY.text)), tempPivot, afterWaitTime, isToDark.isOn);
                break;
            case ScenarioActionType.SetCamera:
            case ScenarioActionType.ControlCamera:
                action = new ScenarioAction(actionId, (temp == ScenarioActionType.ControlCamera), new HexTile.HexCoord(Convert.ToInt32(controlCameraX.text), Convert.ToInt32(controlCameraY.text)), afterWaitTime, isToDark.isOn);
                break;
            case ScenarioActionType.AddUserPlayer:
                break;
            case ScenarioActionType.AddEnemyPlayer:
                break;
        }
        if (selectedActionId == -1)
        {
            scenarioActions.Add(action);
        }
        else
        {
            //update
            action.scenarioActionId = selectedActionId;
            List<ScenarioAction> tempActions = scenarioActions.Where(x => x.scenarioActionId != selectedActionId).ToList();
            tempActions.Add(action);
            scenarioActions.Clear();
            scenarioActions.AddRange(tempActions);
        }
        ResetAction();
        ReloadActionList();
        ResetCreateActor();
    }

    public void LoadAction(string id)
    {
        ScenarioAction action = scenarioActions.Where(x => x.scenarioActionId.ToString() == id).FirstOrDefault();
        if (action == null)
        {
            return;
        }
        ResetAction();
        selectedActionId = Convert.ToInt32(id);
        actionType.interactable = false;
        actionType.value = actionType.options.FindIndex(x => x.text == Enum.GetName(typeof(ScenarioActionType), action.scenarioActionType));
        actionType.RefreshShownValue();
        SetActorPos();
        waitTime.text = action.waitTime.ToString();
        isToDark.isOn = action.isToDark;
        switch (action.scenarioActionType)
        {
            case ScenarioActionType.Dialog:
                dialogName.text = action.dialogName;
                dialogContect.text = action.dialogText;
                break;
            case ScenarioActionType.CreateActor:
                scenarioActorPlayerRecords = action.createActors;
                ReloadCreateActorList();
                break;
            case ScenarioActionType.ControlActor:
                SetActorSelector();
                selectedActor.value = selectedActor.options.FindIndex(x => x.text.Split(',')[0] == action.actorId.ToString());
                selectedActorPivot.value = selectedActorPivot.options.FindIndex(x => x.text== Enum.GetName(typeof(ScenarioActorPivotType), action.actorPivot));
                controlActorX.text = action.targetMoveTile.q.ToString();
                controlActorY.text = action.targetMoveTile.r.ToString();
                break;
            case ScenarioActionType.SetCamera:
				controlCameraX.text = action.setCameraPos.q.ToString();
				controlCameraY.text = action.setCameraPos.r.ToString();
				break;
			case ScenarioActionType.ControlCamera:
                controlCameraX.text = action.targetMoveTile.q.ToString();
                controlCameraY.text = action.targetMoveTile.r.ToString();
                break;
            case ScenarioActionType.AddUserPlayer:
                break;
            case ScenarioActionType.AddEnemyPlayer:
                break;
        }
    }

    //Create Actor
    private void ResetCreateActor()
    {
        characterTemplate.value = 0;
        createActorPivot.value = 0;
        createActorX.text = "";
        createActorY.text = "";
        ClearCreateActorList();
    }

    private void ClearCreateActorList()
    {
        for (int i = 0; i < createActorItemList.transform.childCount; i++)
        {
            Destroy(createActorItemList.transform.GetChild(i).gameObject);
        }

    }

    private void ReloadCreateActorList()
    {
        ClearCreateActorList();
        for (int i = 0; i < scenarioActorPlayerRecords.Count; i++)
        {
            CreaterActorSelector temp = Instantiate(creatorActorSelectorPrefab, new Vector3(), Quaternion.identity, createActorItemList).GetComponent<CreaterActorSelector>();
            temp.SetActorName(scenarioActorPlayerRecords[i].id, gameElement.characters.Where(x => x.id == scenarioActorPlayerRecords[i].characterId).FirstOrDefault().name, scenarioActorPlayerRecords[i].locX, scenarioActorPlayerRecords[i].locY, scenarioActorPlayerRecords[i].scenarioActorPivotType);
        }
    }

    public void RemoveCreateActorList(string id)
    {
        scenarioActorPlayerRecords.Remove(scenarioActorPlayerRecords.Where(x => x.id.ToString() == id).FirstOrDefault());
        Player temp = actorPlayer.Where(x => x.playerIndex.ToString() == id).FirstOrDefault();
        if (temp!=null)
        {
            actorPlayer.Remove(temp);
            Destroy(temp.gameObject);
        }
        ReloadCreateActorList();
    }

    public void AddActor()
    {
        string[] id = characterTemplate.options[characterTemplate.value].text.Split(',');
        CharacterTemplate temp = gameElement.characters.Where(x => x.id.ToString() == id[2]).FirstOrDefault();
        ScenarioActorPivotType tempPivot = (ScenarioActorPivotType)Enum.Parse(typeof(ScenarioActorPivotType), createActorPivot.options[createActorPivot.value].text);

        int val = 0;
        if (!Int32.TryParse(createActorX.text, out val) || !Int32.TryParse(createActorY.text, out val))
        {
            return;
        }
        if (scenarioActorPlayerRecords.Where(x => x.locX.ToString() == createActorX.text && x.locY.ToString() == createActorY.text).Count() > 0)
        {
            return;
        }

        if (temp != null)
        {
            int actorId = actorPlayer.Count > 0 ? actorPlayer.OrderBy(x => -x.playerIndex).FirstOrDefault().playerIndex + 1 : 0;
            scenarioActorPlayerRecords.Add(new PlayerRecord(actorId, Convert.ToInt32(createActorX.text), Convert.ToInt32(createActorY.text), temp.id, tempPivot));
            Vector3 pos = mapHex[Convert.ToInt32(createActorY.text)][Convert.ToInt32(createActorX.text) + (Convert.ToInt32(createActorY.text) >> 1)].HexTilePos();
            Player player = Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, pos, Quaternion.identity, actorPlayerTransform).GetComponent<Player>();
            player.gridPosition = new Vector2(Convert.ToInt32(createActorX.text), Convert.ToInt32(createActorY.text));
            player.playerIndex = actorId;
            player.gameObject.name = actorId.ToString();
            player.SetPivot(tempPivot);
            player.SetPlayerModel();
            actorPlayer.Add(player);
        }
        ResetCreateActor();
        ReloadCreateActorList();
    }

    private void ClearActor()
    {
        for (int i = 0; i < actorPlayer.Count; i++)
        {
            Destroy(actorPlayer[i].gameObject);
            actorPlayer.Remove(actorPlayer[i]);
        }
    }

    //Control actor
    private void SetActorSelector()
    {
        selectedActor.options.Clear();
        for (int i = 0; i < (selectedActionId == -1 ? scenarioActions.Count : selectedActionId); i++)
        {
            List<PlayerRecord> actors = scenarioActions[i].createActors;
            if (actors == null || actors.Count == 0)
            {
                continue;
            }
            for (int j = 0; j < actors.Count; j++)
            {
                selectedActor.options.Add(new Dropdown.OptionData() { text = string.Format("{0},{1}", actors[j].id, gameElement.characters.Where(x => x.id == actors[j].characterId).FirstOrDefault().name) });
            }
        }
        selectedActor.value = 0;
        selectedActor.RefreshShownValue();
    }

    private void ResetControlActor()
    {
        selectedActor.value = 0;
        selectedActorPivot.value = 0;
        controlActorX.text = "";
        controlActorY.text = "";
    }

    private void SetActorPos()
    {
        for (int i = 0; i < actorPlayer.Count; i++)
        {
            actorPlayer[i].HidePlayer();
        }

        for (int i = 0; i < (selectedActionId == -1 ? scenarioActions.Count : selectedActionId + 1); i++)
        {
            if (scenarioActions[i].scenarioActionType == ScenarioActionType.CreateActor)
            {
                for (int j = 0; j < scenarioActions[i].createActors.Count; j++)
                {
                    Vector3 pos = mapHex[Convert.ToInt32(scenarioActions[i].createActors[j].locY)][Convert.ToInt32(scenarioActions[i].createActors[j].locX) + (Convert.ToInt32(scenarioActions[i].createActors[j].locY) >> 1)].HexTilePos();
                    Player player = actorPlayer.Where(x => x.playerIndex == scenarioActions[i].createActors[j].id).FirstOrDefault();
                    player.ShowPlayer();
                    player.gridPosition = new Vector2(Convert.ToInt32(scenarioActions[i].createActors[j].locX), Convert.ToInt32(scenarioActions[i].createActors[j].locY));
                    player.transform.position = pos;
                    player.SetPivot(scenarioActions[i].createActors[j].scenarioActorPivotType);
                }
            }
            else if (scenarioActions[i].scenarioActionType == ScenarioActionType.ControlActor)
            {
                Vector3 pos = mapHex[Convert.ToInt32(scenarioActions[i].targetMoveTile.r)][Convert.ToInt32(scenarioActions[i].targetMoveTile.q) + (Convert.ToInt32(scenarioActions[i].targetMoveTile.r) >> 1)].HexTilePos();
                Player player = actorPlayer.Where(x => x.playerIndex == scenarioActions[i].actorId).FirstOrDefault();
                player.ShowPlayer();
                player.gridPosition = new Vector2(Convert.ToInt32(scenarioActions[i].targetMoveTile.q), Convert.ToInt32(scenarioActions[i].targetMoveTile.r));
                player.transform.position = pos;
                player.SetPivot(scenarioActions[i].actorPivot);
            }
        }
    }

    //Control/move camera
    private void ResetControlCamera()
    {
        controlCameraX.text = "";
        controlCameraY.text = "";
    }

    //Dialog
    private void ResetDialog()
    {
        dialogName.text = "";
        dialogContect.text = "";
    }

    #endregion

    #region IO
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

        shopItemList = new List<int>();
        shopWeaponList = new List<int>();

        scenarios = new List<Scenario>();
        actorPlayer = new List<Player>();

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
                tile.TileInitializer(mapTransform, TileType.Normal, TileType2D.Plain, 0, 0, j, i, mapSizeX, mapSizeY, 0, -1, -1, false);
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

        ReloadScenarioList();
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

        ObjectSaveLoad.JsonSave(MapSaveLoad.CreateMapContainer(mapHex, userPlayerRecords, enemyPlayerRecords, shopItemList, shopWeaponList, scenarios), fileName.text + ".txt");
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

        shopItemList = container.shopItemList;
        shopWeaponList = container.shopWeaponList;
        SetShopList();

        scenarios = container.scenarioList;
        actorPlayer = new List<Player>();

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
                TileXml temp = container.tiles.Where(x => x.locX == j && x.locY == i).FirstOrDefault();
                tile.TileInitializer(mapTransform, (TileType)temp.id, (TileType2D)temp.id, temp.spritIndex, temp.spritChestIndex, j, i, mapSizeX, mapSizeY, temp.gold, temp.itemId, temp.weaponId, temp.isShop);
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

        isScenarioMode = false;
        InitialScenarioUI();
        EnableGroup(stageGroup);
        DisableGroup(scenarioGroup);
        ControlGroup();
        ReloadScenarioList();

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
            GameObject newPlayer = Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, new Vector3(pos.x, playerHeight, pos.z), Quaternion.Euler(new Vector3(0, 180, 0)));
            newPlayer.name = string.Format(userPlayerNameFormat, id);
            newPlayer.transform.SetParent(playerTransform);
            newPlayer.GetComponent<UserPlayer>().gridPosition = new Vector2(userPlayerRecords[i].locX, userPlayerRecords[i].locY);
            newPlayer.GetComponent<UserPlayer>().SetPlayerModel();
        }

        for (int i = 0; i < enemyPlayerRecords.Count; i++)
        {
            int id = enemyPlayerRecords[i].id;

            Vector3 pos = mapHex[enemyPlayerRecords[i].locY][enemyPlayerRecords[i].locX + (enemyPlayerRecords[i].locY >> 1)].HexTilePos();
            GameObject newPlayer = Instantiate(PlayerPrefabHolder.instance.enemyPlayer_prefab, new Vector3(pos.x, playerHeight, pos.z), Quaternion.Euler(new Vector3(0, 180, 0)));
            newPlayer.name = string.Format(enemyPlayerNameFormat, id);
            newPlayer.transform.SetParent(playerTransform);
            newPlayer.GetComponent<AIPlayer>().gridPosition = new Vector2(enemyPlayerRecords[i].locX, enemyPlayerRecords[i].locY);
            newPlayer.GetComponent<AIPlayer>().SetPlayerModel();
        }

        ClearActor();
    }
    #endregion

}
