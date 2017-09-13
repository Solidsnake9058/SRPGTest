using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    //public GameObject tilePrefab;
    //public GameObject userPlayerPrefab;
    //public GameObject aiPlayerPrefab;
    Transform mapTransform;
    Transform playerTransform;

    public CanvasGroup blockUI;
    public Image menuImage;
    public CanvasGroup menu;
    public CanvasGroup endTurnConfirm;
    public Button stageMessage;
    public Text stageInfo;

    public int mapSizeX = 32;
    public int mapSizeY = 38;

    public float cubeSize = 1;

    public List<List<Tile>> map = new List<List<Tile>>();
    public List<List<HexTile>> mapHex = new List<List<HexTile>>();

    private List<PlayerRecord> saveUserPlayerRecords;
    private List<PlayerRecord> userPlayerRecords;
    private List<PlayerRecord> enemyPlayerRecords;
    public List<Player> players = new List<Player>();
    public List<Player> userPlayers = new List<Player>();
    public List<Player> enemyPlayers = new List<Player>();

    private GameElement gameElement;
    private List<CharacterTemplate> playerTypes;
    private List<CharacterTemplate> enemyTypes;

    private bool isPlayerTurn = true;
    private bool isWaitingAct = false;
    private bool isWaitingMsg = false;
    private bool isShowStage = true;
    private bool isSetCamera = false;

    private int turnCount = 1;

    public int currentPlayerIndex = 0;
    public int currentEnemyPlayerIndex = 0;

    private int playerIndex = -1;
    delegate void ButtonAction(int inputPlayerIndex);
    ButtonAction buttonAction;

    private string userPlayerNameFormat = "UserPlayer{0}";
    private string enemyPlayerNameFormat = "EnemyPlayer{0}";

    private string gameElementfilename = "ObjectJson.txt";

    public bool moving = false;
    public bool attacking = false;

    void Awake()
    {
        instance = this;
        mapTransform = transform.Find("Map");
        playerTransform = transform.Find("Players");
    }

    private void Start()
    {
        saveUserPlayerRecords = new List<PlayerRecord>();
        InitialStage();
        ShowStageInfo(false);
    }

    void Update()
    {
        if (!isWaitingMsg)
        {
            if (userPlayers.Where(x => x.hp > 0).Count() > 0 && enemyPlayers.Where(x => x.hp > 0).Count() > 0)
            {
                if (isPlayerTurn)
                {
                    if (!isWaitingAct)
                    {

                    }
                }
                else
                {
                    if (enemyPlayers[currentEnemyPlayerIndex].hp > 0)
                    {
                        StartCoroutine(RunEnemyTurn());
                        //enemyPlayers[currentEnemyPlayerIndex].TurnUpdate();
                    }
                    else
                    {
                        NextEnemyTurn();
                    }
                }
            }
        }
        //if (players[currentPlayerIndex].HP > 0)
        //{
        //    players[currentPlayerIndex].TurnUpdate();
        //}
        //else
        //{
        //    nextTurn();
        //}
    }

    private void InitialStage()
    {
        turnCount = 1;
        isPlayerTurn = true;
        isWaitingAct = false;
        isWaitingMsg = false;
        currentEnemyPlayerIndex = 0;
        playerIndex = -1;
        moving = false;
        attacking = false;

        LoadGameElements();
        GenetareMap();
        GenetarePlayers();
        SetStopWaiting();
        HideMenu();
        HideStageInfo();
        HideEndTurnConfirm();

        isShowStage = true;
    }

    public void SetStartWaiting()
    {
        blockUI.blocksRaycasts = blockUI.interactable = isWaitingAct = true;
    }

    public void SetStopWaiting()
    {
        blockUI.blocksRaycasts = blockUI.interactable = isWaitingAct = false;
    }

    public void ShowStageInfo(bool turnMsg = true)
    {
        stageInfo.text = string.Format("Stage {0}\n{1}",1,"Travel");
        if (turnMsg)
        {
            stageInfo.text = string.Format("Turn {0}\n{1} action", turnCount, isPlayerTurn ? "Ark's" : "Devil armys");
        }
        stageMessage.gameObject.SetActive(true);
        isWaitingMsg = true;
    }

    public void HideStageInfo()
    {
        stageMessage.gameObject.SetActive(false);
        isWaitingMsg = false;
        if (isShowStage)
        {
            isShowStage = false;
            ShowStageInfo();
        }
    }

    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }

    private void OnGUI()
    {
        //players[currentPlayerIndex].TurnOnGUI();
    }

    public void NextTurn()
    {
        //currentPlayerIndex++;
        //currentPlayerIndex = currentPlayerIndex % players.Count;
    }

    IEnumerator RunEnemyTurn()
    {
        if (isSetCamera)
        {
            Vector2 index = enemyPlayers[currentEnemyPlayerIndex].mapHexIndex;
            Vector3 pos = mapHex[(int)index.y][(int)index.x].transform.position;
            ScreenController.instance.SetCameraPos(pos);
            isSetCamera = false;
        }
        yield return new WaitForEndOfFrame();

        enemyPlayers[currentEnemyPlayerIndex].TurnUpdate();
    }

    public void NextEnemyTurn()
    {
        if (currentEnemyPlayerIndex == enemyPlayers.Count - 1)
        {
            turnCount++;
            isPlayerTurn = true;
            SetStopWaiting();
            foreach (Player p in userPlayers)
            {
                p.TurnActive();
            }
            ShowStageInfo();
        }
        else
        {
            isSetCamera = true;
        }
        currentEnemyPlayerIndex++;
        currentEnemyPlayerIndex = currentEnemyPlayerIndex % enemyPlayers.Count;
    }

    public void MoveCurrentPlayer(HexTile destTile)
    {
        if (destTile.visual.transform.GetComponentInChildren<Renderer>().materials[0].color !=Color.white && !destTile.impassible)
        {
            RemoveHighlightTiles();
            Player targetPlayer;
            //players[currentPlayerIndex].moving = false;
            if (isPlayerTurn)
            {
                targetPlayer = userPlayers[playerIndex];
                targetPlayer.SetOriginalPos();
            }
            else
            {
                targetPlayer = enemyPlayers[currentEnemyPlayerIndex];
            }
            foreach (HexTile t in HexTilePathFinder.FindPath(mapHex[(int)targetPlayer.mapHexIndex.y][(int)targetPlayer.mapHexIndex.x], destTile, userPlayers.Union(enemyPlayers).Where(x => x.gridPosition != targetPlayer.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles)
            {
                targetPlayer.positionQueue.Add(mapHex[(int)t.mapHexIndex.y][(int)t.mapHexIndex.x].transform.position + 1.5f * Vector3.up);
                //Debug.Log(players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].x + "," + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].z);
            }
            targetPlayer.gridPosition = destTile.gridPosition;
            SetStartWaiting();
        }
        else
        {
            Debug.Log("destination invalid");
        }
    }

    public void ShowConfirmMenu()
    {
        Player player = userPlayers[playerIndex];
        List<HexTile> highlightTiles = HexTileHighlight.FindHighlight(mapHex[(int)player.mapHexIndex.y][(int)player.mapHexIndex.x], player.attackRange, true);
        MenuType setType = MenuType.playerMoveCanAtkMenu;

        if (highlightTiles.Select(x => x.gridPosition).Intersect(enemyPlayers.Where(x => x.hp > 0).Select(y => y.gridPosition)).Count() == 0)
        {
            setType = MenuType.playerMoveCantAtkMenu;
        }
        ShowMenu();
        Vector3 pos = Camera.main.WorldToScreenPoint(player.gameObject.transform.position);
        //Debug.Log("(" + pos.x + "," + pos.y + "," + pos.z + ")");
        Vector2 newSize = TileMenu.instance.SetMenu(setType);
        float newX = (pos.x + newSize.x) + 10f >= Screen.width ? (pos.x - newSize.x) - 10f : pos.x;
        float newY = (pos.y - newSize.y) - 10f <= 0 ? (pos.y + newSize.y) + 10f : pos.y;

        menuImage.rectTransform.position = new Vector3(newX, newY, 0);
        SetStopWaiting();
    }

    public void AttackWithCurrentPlayer(HexTile destTile)
    {
        if (destTile.visual.transform.GetComponentInChildren<Renderer>().materials[0].color == Color.white && !destTile.impassible)
        {
            Debug.Log("destination invalid");
            return;
        }

        Player attacker = null;
        Player target = null;
        if (isPlayerTurn)
        {
            attacker = userPlayers[playerIndex];
            for (int i = 0; i < enemyPlayers.Count; i++)
            {
                if (enemyPlayers[i].gridPosition == destTile.gridPosition)
                {
                    target = enemyPlayers[i];
                    break;
                }
            }
        }
        else
        {
            attacker = enemyPlayers[currentEnemyPlayerIndex];
            for (int i = 0; i < userPlayers.Count; i++)
            {
                if (userPlayers[i].gridPosition == destTile.gridPosition)
                {
                    target = userPlayers[i];
                    break;
                }
            }
        }

        if (target != null)
        {
            if (attacker.gridPosition.x >= target.gridPosition.x - 1 && attacker.gridPosition.x <= target.gridPosition.x + 1 &&
                attacker.gridPosition.y >= target.gridPosition.y - 1 && attacker.gridPosition.y <= target.gridPosition.y + 1)
            {
                attacker.TurnEnd();
                RemoveHighlightTiles();
                attacking = false;

                bool hit = UnityEngine.Random.Range(0f, 1f) <= attacker.attackChance;
                if (hit)
                {
                    int amountOfDamage = (int)Mathf.Floor(attacker.damageBase + UnityEngine.Random.Range(0, attacker.damageRollSides));

                    target.hp -= amountOfDamage;

                    Debug.Log(attacker.playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage!");
                }
                else
                {
                    Debug.Log(attacker.playerName + " missed " + target.playerName + "!");
                }
            }
            else
            {
                Debug.Log("Target is not adjacent!");
            }
        }
    }

    public void HighlightTileAt(Vector2 originLocation, Color highlightColor, int distance, bool ignorePlayers = true)
    {
        List<HexTile> highlightTiles = new List<HexTile>();

        if (ignorePlayers)
        {
            highlightTiles = HexTileHighlight.FindHighlight(mapHex[(int)originLocation.y][(int)originLocation.x + (((int)originLocation.y) >> 1)], distance, true);
        }
        else
        {
            highlightTiles = HexTileHighlight.FindHighlight(mapHex[(int)originLocation.y][(int)originLocation.x + (((int)originLocation.y) >> 1)], distance, userPlayers.Union(enemyPlayers).Where(x => x.gridPosition != originLocation).Select(x => x.gridPosition).ToArray());
        }

        foreach (HexTile t in highlightTiles)
        {
            t.visual.transform.GetComponentInChildren<Renderer>().materials[0].color = highlightColor;
        }
    }

    public void RemoveHighlightTiles()
    {
        for (int i = 0; i < mapSizeY; i++)
        {
            for (int j = 0; j < mapSizeX - (i % 2); j++)
            {
                if (!mapHex[i][j].impassible)
                {
                    mapHex[i][j].visual.transform.GetComponentInChildren<Renderer>().materials[0].color = Color.white;
                }
            }
        }
    }

    void GenetareMap()
    {
        LoadMapFromXml();
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
            playerTypes.Sort((x, y) => { return x.id.CompareTo(y.id); });
            enemyTypes = gameElement.characters.Where(x => x.enemy).ToList();
            enemyTypes.Sort((x, y) => { return x.id.CompareTo(y.id); });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private void LoadMapFromXml()
    {
        MapXmlContainer container = ObjectSaveLoad.JsonLoad<MapXmlContainer>("test.txt");
        mapSizeX = container.sizeX;
        mapSizeY = container.sizeY;

        for (int i = 0; i < mapTransform.transform.childCount; i++)
        {
            Destroy(mapTransform.transform.GetChild(i).gameObject);
        }

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

        //for (int i = 0; i < mapSizeX; i++)
        //{
        //    List<Tile> row = new List<Tile>();
        //    for (int j = 0; j < mapSizeY; j++)
        //    {
        //        Tile tile = ((GameObject)Instantiate(PrefabHolder.instance.base_tile_prefab, new Vector3(i - Mathf.Floor(mapSizeX / 2), 0, -j + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
        //        tile.gridPosition = new Vector2(i, j);
        //        tile.transform.parent = mapTransform;
        //        tile.setType((TileType)container.tiles.Where(x => x.locX == i && x.locY == j).FirstOrDefault().id);
        //        row.Add(tile);
        //    }
        //    map.Add(row);
        //}
    }

    #region Button Action
    public void ClickButtonAction(Button clickButton)
    {
        buttonAction = (ButtonAction)Delegate.CreateDelegate(typeof(ButtonAction), this, clickButton.name);
        buttonAction(playerIndex);
        HideMenu();
    }


    private void Move(int inputPlayerIndex)
    {
        if (!moving)
        {
            RemoveHighlightTiles();
            moving = true;
            attacking = false;
            HighlightTileAt(userPlayers[inputPlayerIndex].gridPosition, Color.blue, (int)userPlayers[inputPlayerIndex].movementPerActionPoint, false);
        }
        else
        {
            moving = false;
            attacking = false;
            RemoveHighlightTiles();
        }
    }

    private void Action(int inputPlayerIndex)
    {
        if (!attacking)
        {
            RemoveHighlightTiles();
            attacking = true;
            HighlightTileAt(userPlayers[inputPlayerIndex].gridPosition, Color.red, userPlayers[inputPlayerIndex].attackRange);
        }
        else
        {
            attacking = false;
            RemoveHighlightTiles();
        }
    }

    private void Cancel(int inputPlayerIndex)
    {
        if (moving)
        {
            userPlayers[inputPlayerIndex].gridPosition = userPlayers[inputPlayerIndex].originalGridPosition;
            Vector3 oriPos = mapHex[(int)userPlayers[inputPlayerIndex].mapHexIndex.y][(int)userPlayers[inputPlayerIndex].mapHexIndex.x].transform.position;
            userPlayers[inputPlayerIndex].transform.position = new Vector3(oriPos.x, 1.5f, oriPos.z);
            moving = false;
            Move(inputPlayerIndex);
        }
        HideEndTurnConfirm();
    }

    private void Confirm(int playerIndex)
    {
        userPlayers[playerIndex].TurnEnd();
        moving = false;
        attacking = false;
        RemoveHighlightTiles();
    }

    private void ShowEndTurn(int playerIndex)
    {
        ShowEndTurnConfirm();
    }

    private void TurnEnd(int playerIndex)
    {
        isPlayerTurn = false;
        currentEnemyPlayerIndex = 0;
        foreach (Player p in enemyPlayers)
        {
            p.TurnActive();
        }
        SetStartWaiting();
        HideEndTurnConfirm();

        ShowStageInfo();
        isSetCamera = true;
    }

    #endregion

    public void CancelAction()
    {
        if (menu.blocksRaycasts)
        {
            HideMenu();
            Cancel(playerIndex);
        }
        else if (attacking && moving)
        {
            RemoveHighlightTiles();
            attacking = false;
            ShowConfirmMenu();
        }
        else if (moving || (attacking && !moving))
        {
            moving = false;
            playerIndex = -1;
            RemoveHighlightTiles();
        }
    }

    public void ShowMenu()
    {
        menu.alpha = 1;
        menu.interactable = true;
        menu.blocksRaycasts = true;
    }

    public void HideMenu()
    {
        menu.alpha = 0;
        menu.interactable = false;
        menu.blocksRaycasts = false;
    }

    public void ShowEndTurnConfirm()
    {
        HideMenu();
        endTurnConfirm.alpha = 1;
        endTurnConfirm.interactable = true;
        endTurnConfirm.blocksRaycasts = true;
    }

    public void HideEndTurnConfirm()
    {
        endTurnConfirm.alpha = 0;
        endTurnConfirm.interactable = false;
        endTurnConfirm.blocksRaycasts = false;
    }

    void GenetarePlayers()
    {
        for (int i = 0; i < userPlayerRecords.Count; i++)
        {
            UserPlayer player;
            int x = userPlayerRecords[i].locX + (userPlayerRecords[i].locY >> 1);
            int y = userPlayerRecords[i].locY;
            Vector3 tilePOs = mapHex[y][x].HexTilePos();
            CharacterTemplate playerData = playerTypes[userPlayerRecords[i].characterId];
            CharacterLevelTemplate playerLvData = playerTypes[userPlayerRecords[i].characterId].levelData[0];
            PlayerRecord record = saveUserPlayerRecords.Where(t => t.characterId == userPlayerRecords[i].characterId).FirstOrDefault();
            //player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
            player = ((GameObject)Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, new Vector3(tilePOs.x, 1.5f, tilePOs.z), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
            player.gameObject.name = string.Format(userPlayerNameFormat, i);
            player.transform.SetParent(playerTransform);

            player.gridPosition = new Vector2(userPlayerRecords[i].locX, userPlayerRecords[i].locY);

            player.playerName = playerData.name;
            player.race = playerData.race;
            player.movementPerActionPoint = playerData.move;

            if (userPlayerRecords[i].isNewPlayer || record == null)
            {
                player.level = (int)playerLvData.level;
                player.exp = (int)playerLvData.exp;
                player.hp = player.maxHP = (int)playerLvData.hp;
                player.atk = (int)playerLvData.atk;
                player.def = (int)playerLvData.def;
                player.wis = (int)playerLvData.wis;
                player.dex = (int)playerLvData.dex;
                player.mdef = (int)playerLvData.mdef;
                player.equipWeapon = playerLvData.equipWeapon;
            }
            else
            {
                player.level = (int)record.level;
                player.exp = (int)record.exp;
                player.hp = player.maxHP = (int)record.hp;
                player.atk = (int)record.atk;
                player.def = (int)record.def;
                player.wis = (int)record.wis;
                player.dex = (int)record.dex;
                player.mdef = (int)record.mdef;
                player.equipWeapon = record.equipWeapon;
            }

            player.playerIndex = i;

            userPlayers.Add(player);

        }

        for (int i = 0; i < enemyPlayerRecords.Count; i++)
        {
            AIPlayer player;
            int x = enemyPlayerRecords[i].locX + (enemyPlayerRecords[i].locY >> 1);
            int y = enemyPlayerRecords[i].locY;
            Vector3 tilePOs = mapHex[y][x].HexTilePos();
            CharacterTemplate playerData = enemyTypes[enemyPlayerRecords[i].characterId];
            CharacterLevelTemplate playerLvData = enemyTypes[enemyPlayerRecords[i].characterId].levelData[0];
            //player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
            player = ((GameObject)Instantiate(PlayerPrefabHolder.instance.enemyPlayer_prefab, new Vector3(tilePOs.x, 1.5f, tilePOs.z), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
            player.gameObject.name = string.Format(enemyPlayerNameFormat, i);
            player.transform.SetParent(playerTransform);

            player.gridPosition = new Vector2(enemyPlayerRecords[i].locX, enemyPlayerRecords[i].locY);

            player.playerName = playerData.name;
            player.race = playerData.race;
            player.movementPerActionPoint = playerData.move;

            player.level = (int)playerLvData.level;
            player.exp = (int)playerLvData.exp;
            player.hp = player.maxHP = (int)playerLvData.hp;
            player.atk = (int)playerLvData.atk;
            player.def = (int)playerLvData.def;
            player.wis = (int)playerLvData.wis;
            player.dex = (int)playerLvData.dex;
            player.mdef = (int)playerLvData.mdef;
            player.equipWeapon = playerLvData.equipWeapon;

            player.playerIndex = i;

            enemyPlayers.Add(player);

        }


        //r = mapSizeY - 1;
        //q = mapSizeX - 1 - (r >> 1) - ((mapSizeY - 1) % 2);
        //x = q + (r >> 1);
        //y = r;
        //HexTile temp = mapHex[y][x];
        //tilePOs = mapHex[y][x].HexTilePos();
        ////player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3((mapSizeX - 1) - Mathf.Floor(mapSizeX / 2), 1.5f, -(mapSizeY - 1) + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        ////player.gridPosition = new Vector2(mapSizeX - 1, mapSizeY - 1);
        //player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(tilePOs.x, 1.5f, tilePOs.z), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        //player.gridPosition = new Vector2(q, r);
        //player.playerName = "B";
        //player.playerIndex = 1;

        //userPlayers.Add(player);

        //q = 4;
        //r = 4;
        //x = q + (r >> 1);
        //y = r;
        //tilePOs = mapHex[y][x].HexTilePos();
        ////player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3((4-Mathf.Floor(mapSizeX / 2)), 1.5f, -4 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        ////player.gridPosition = new Vector2(4, 4);
        //player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(tilePOs.x, 1.5f, tilePOs.z), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        //player.gridPosition = new Vector2(q, r);
        //player.playerName = "C";
        //player.playerIndex = 2;

        //userPlayers.Add(player);

        //q = 8;
        //r = 8;
        //x = q + (r >> 1);
        //y = r;
        //tilePOs = mapHex[y][x].HexTilePos();
        ////player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3((8 - Mathf.Floor(mapSizeX / 2)), 1.5f, -8 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        ////player.gridPosition = new Vector2(8, 8);
        //player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(tilePOs.x, 1.5f, tilePOs.z), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        //player.gridPosition = new Vector2(q, r);
        //player.playerName = "D";
        //player.playerIndex = 3;

        //userPlayers.Add(player);

        //q = 6;
        //r = 4;
        //x = q + (r >> 1);
        //y = r;
        //tilePOs = mapHex[y][x].HexTilePos();
        ////AIPlayer aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(6 - Mathf.Floor(mapSizeX / 2), 1.5f, -4 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        ////aiplayer.gridPosition = new Vector2(6, 4);
        //AIPlayer aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(tilePOs.x, 1.5f, tilePOs.z), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        //aiplayer.gridPosition = new Vector2(q, r);
        //aiplayer.name = "Enemy1";
        //aiplayer.playerIndex = 0;

        //enemyPlayers.Add(aiplayer);

        //q = 8;
        //r = 4;
        //x = q + (r >> 1);
        //y = r;
        //tilePOs = mapHex[y][x].HexTilePos();
        ////aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(8 - Mathf.Floor(mapSizeX / 2), 1.5f, -4 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        ////aiplayer.gridPosition = new Vector2(8, 4);
        //aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(tilePOs.x, 1.5f, tilePOs.z), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        //aiplayer.gridPosition = new Vector2(q, r);
        //aiplayer.name = "Enemy2";
        //aiplayer.playerIndex = 1;

        //enemyPlayers.Add(aiplayer);

        //q = 11;
        //r = 0;
        //x = q + (r >> 1);
        //y = r;
        //tilePOs = mapHex[y][x].HexTilePos();
        ////aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(11 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        ////aiplayer.gridPosition = new Vector2(11, 0);
        //aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(tilePOs.x, 1.5f, tilePOs.z), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        //aiplayer.gridPosition = new Vector2(q, r);
        //aiplayer.name = "Enemy3";
        //aiplayer.playerIndex = 2;

        //enemyPlayers.Add(aiplayer);

        //q = 18;
        //r = 8;
        //x = q + (r >> 1);
        //y = r;
        //tilePOs = mapHex[y][x].HexTilePos();
        ////aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(18 - Mathf.Floor(mapSizeY / 2), 1.5f, -8 + Mathf.Floor(mapSizeX / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        ////aiplayer.gridPosition = new Vector2(18, 8);
        //aiplayer = ((GameObject)Instantiate(aiPlayerPrefab, new Vector3(tilePOs.x, 1.5f, tilePOs.z), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        //aiplayer.gridPosition = new Vector2(q, r);
        //aiplayer.name = "Enemy4";
        //aiplayer.playerIndex = 3;

        //enemyPlayers.Add(aiplayer);

    }
}
