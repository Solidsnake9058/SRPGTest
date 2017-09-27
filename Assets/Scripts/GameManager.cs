﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    #region parament

    //public GameObject tilePrefab;
    //public GameObject userPlayerPrefab;
    //public GameObject aiPlayerPrefab;
    Transform mapTransform;
    Transform playerTransform;
    Transform playerUITransform;

    public GameObject playerUIPrefab;
    public GameObject itemUIPrefab;
    public GameObject weaponUIPrefab;
    public GameObject characterUIPrefab;
    public GameObject shopUIPrefab;
    public GameObject gameSceneUI;

    public CanvasGroup blockUI;
    public Image menuImage;
    public CanvasGroup mapController;
    public CanvasGroup menu;
    public CanvasGroup endTurnConfirm;
    public CanvasGroup stageMessage;
    public CanvasGroup status;
    public CanvasGroup shopGroup;
    public CanvasGroup itemGroup;
    public CanvasGroup weaponGroup;
    public CanvasGroup unitGroup;
    public CanvasGroup gameSetting;
    public Text stageInfo;
    public Image msgBox;

    public int mapSizeX = 32;
    public int mapSizeY = 38;

    public float cubeSize = 1;
    public float playerHeight = 0f;

    public bool isShowTile = true;
    public bool isShowPlayerUI = true;

    public List<List<Tile>> map = new List<List<Tile>>();
    public List<List<HexTile>> mapHex = new List<List<HexTile>>();

    private List<PlayerRecord> saveUserPlayerRecords;
    private List<PlayerRecord> userPlayerRecords;
    private List<PlayerRecord> enemyPlayerRecords;
    //public List<Player> players = new List<Player>();
    public List<Player> userPlayers = new List<Player>();
    public List<Player> enemyPlayers = new List<Player>();
    public List<Player> actorPlayers = new List<Player>();
    public List<Scenario> stageScenatios = new List<Scenario>();
    public Scenario runningScenario;
    public List<int> shopItemList;
    public List<int> shopWeaponList;

    public GameElement gameElement { get; private set; }
    private List<CharacterTemplate> playerTypes;
    private List<CharacterTemplate> enemyTypes;

    public Color moveTileColor;
    public Color attackTileColor;

    #endregion

    #region game parament
    [HideInInspector]
    public bool isPlayerTurn = true;
    private bool isWaitingActor = false;
    private bool isWaitingBattle = false;
    private bool isWaitingMsg = false;
    private bool isShowStage = false;
    [HideInInspector]
    public bool isSetCamera = false;

    private int turnCount = 1;

    [HideInInspector]
    public int currentPlayerIndex = 0;
    [HideInInspector]
    public int currentEnemyPlayerIndex = 0;

    private int playerIndex = -1;
    delegate void ButtonAction(int inputPlayerIndex);
    ButtonAction buttonAction;

    private string userPlayerNameFormat = "UserPlayer{0}";
    private string enemyPlayerNameFormat = "EnemyPlayer{0}";

    private string gameElementfilename = "ObjectJson.txt";

    private bool isStartGame = true;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;

    [HideInInspector]
    public bool moving = false;
    [HideInInspector]
    public bool attacking = false;

    public int _playerGold = 1000;
    public Dictionary<int, int> playerItems;
    public Dictionary<int, int> playerWeapons;

    public BattleSendData battleData = new BattleSendData();
    #endregion

    #region UI parament

    [Header("StatusUI")]
    public Text playerName;
    public Text playerClass;
    public Text playerLevel;
    public Text playerHP;
    public Text playerMaxHP;
    public Text playerExp;
    public Text playerAtk;
    public Text playerWeaponAtk;
    public Text playerDef;
    public Text playerWis;
    public Text playerDex;
    public Text playerMdef;
    public Text playerEquip;
    public Text playerEquipRange;

    [Header("UI Setting UI")]
    public Toggle isShowTileLine;
    public Toggle isShowPlayHP;

    [Header("Item UI")]
    public int itemSelectedId = -1;
    public Text itemGold;
    public Text itemNotice;
    public RectTransform itemList;
    public Button buttonUseItem;
    public Button buttonSellItem;

    [Header("Weapon UI")]
    public int weaponSelectedId = -1;
    public Text weaponNotice;
    public RectTransform weaponList;
    public Button buttonEquipWeapon;
    public Button buttonSellWeapon;

    [Header("Unit UI")]
    public RectTransform userPanel;
    public RectTransform enemyPanel;
    public RectTransform userPlayerList;
    public RectTransform enemyPlayerList;
    public Text turnText;

    [Header("Shop UI")]
    public string shopSelectedId = "";
    public Text shopGold;
    public Text shopNotice;
    public RectTransform shopList;
    public Button buttonBuyItem;

    [Header("Dialog UI")]
    public CanvasGroup dialogGroup;
    public Text dialogName;
    public Text dialogText;
    #endregion

    public int playerGold
    {
        get
        {
            if (_playerGold > 600000)
            {
                return 600000;
            }
            else
            {
                return _playerGold;
            }
        }
        set { _playerGold = value; }
    }

    void Awake()
    {
        if (instance==null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        mapTransform = transform.Find("Map");
        playerTransform = transform.Find("Players");
        playerUITransform = transform.Find("PlayerUIs");
    }

    private void Start()
    {
        ScreenController.instance.SetCameraPos(cameraPosition);
        ScreenController.instance.SetCameraRot(cameraRotation);

        isWaitingBattle = false;
        InitialStage();
        if (isStartGame)
        {
            isStartGame = false;
            saveUserPlayerRecords = new List<PlayerRecord>();
            playerItems = new Dictionary<int, int>();
            playerWeapons = new Dictionary<int, int>();
            StartGame();
            Scenario opening = stageScenatios.Where(x => x.scenarioType == ScenarioType.Openning).FirstOrDefault();

            //test
            //List<ScenarioAction> testDialog = new List<ScenarioAction>();
            //testDialog.Add(new ScenarioAction(0, "Name1", "Test1"));
            //testDialog.Add(new ScenarioAction(1, "Name2", "Test2"));
            //opening = new Scenario(0, ScenarioType.Openning, ScenarioConditionType.None, null, null, true, testDialog, null);
            if (opening != null)
            {
                runningScenario = opening;
                if (runningScenario.isOnceEvent)
                {
                    stageScenatios.Remove(opening);
                }
            }
            else
            {
                ShowStageInfo(false);
                Debug.Log("No opening ");
            }
        }
    }

    void Update()
    {
        //play mode
        if (runningScenario == null)
        {
            if (!isWaitingMsg && !isWaitingBattle)
            {
                if (userPlayers.Where(x => x.hp > 0).Count() > 0 && enemyPlayers.Where(x => x.hp > 0).Count() > 0)
                {
                    if (isPlayerTurn)
                    {
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
                else if (enemyPlayers.Count == 0 || enemyPlayers.Where(x => x.hp > 0).Count() == 0)
                {
                    //Win
                }
                else if (userPlayers.Where(x => gameElement.races.Where(y => y.name == "ロード").FirstOrDefault().id == x.race && x.hp < 0).Count() > 0)
                {
                    //Game Over
                }
            }
        }
        //scenario mode
        else
        {
            DisableGroup(mapController);
            if (!isWaitingActor)
            {
                //run scenario
                if (runningScenario.scenarioActionStep < runningScenario.scenarioActions.Count)
                {
                    //play action
                    if (runningScenario.scenarioType == ScenarioType.Openning)
                    {
                        //Hide all players
                        HidePlayers();
                    }
                    switch (runningScenario.scenarioActions[runningScenario.scenarioActionStep].scenarioActionType)
                    {
                        case ScenarioActionType.Dialog:
                            //Show dialog
                            EnableGroup(dialogGroup);
                            dialogName.text = runningScenario.scenarioActions[runningScenario.scenarioActionStep].dialogName;
                            dialogText.text = runningScenario.scenarioActions[runningScenario.scenarioActionStep].dialogText;
                            isWaitingActor = true;
                            break;
                        case ScenarioActionType.CreateActor:
                            //Create actors
                            break;
                        case ScenarioActionType.ControlActor:
                            //Control actors
                            isWaitingActor = true;
                            break;
                    }
                    runningScenario.scenarioActionStep++;
                }
                else
                {
                    //Scenario is end
                    EnableGroup(mapController);
                    if (runningScenario.scenarioType == ScenarioType.Openning)
                    {
                        ShowPlayers();
                        ShowStageInfo(false);
                    }
                    runningScenario = null;
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

    private void OnLevelWasLoaded(int level)
    {
        gameSceneUI.SetActive(false);
        if (level == 0)
        {
            //ScreenController.instance.SetCameraPos(cameraPosition);
            //ScreenController.instance.SetCameraRot(cameraRotation);

            gameSceneUI.SetActive(true);
            isWaitingBattle = false;
        }
    }

    private void InitialStage()
    {
        turnCount = 1;
        isPlayerTurn = true;
        isWaitingActor = false;
        isWaitingMsg = false;
        currentEnemyPlayerIndex = 0;
        playerIndex = -1;
        moving = false;
        attacking = false;

        LoadGameElements();
        SetStopWaiting();
        HideStageInfo();

        DisableGroup(menu);
        DisableGroup(endTurnConfirm);
        DisableGroup(status);
        DisableGroup(gameSetting);
        DisableGroup(itemGroup);
        DisableGroup(weaponGroup);
        DisableGroup(unitGroup);
        DisableGroup(shopGroup);
        DisableGroup(dialogGroup);
        isShowStage = true;
    }

    private void StartGame()
    {
        GenetareMap();
        GenetarePlayers();
        int herbId = gameElement.items.Where(x => x.name == "薬草").FirstOrDefault().id;
        playerItems.Add(herbId, 4);

    }

    public bool GetIsWaitingBattle()
    {
        return isWaitingBattle;
    }

    public void SetStartWaiting()
    {
        blockUI.blocksRaycasts = blockUI.interactable = true;
    }

    public void SetStopWaiting()
    {
        blockUI.blocksRaycasts = blockUI.interactable = false;
    }

    public void ShowStageInfo(bool turnMsg = true)
    {
        stageInfo.text = string.Format("Stage {0}\n{1}",1,"旅立ち");
        if (turnMsg)
        {
            stageInfo.text = string.Format("ターン {0}\n{1}の行動", turnCount, isPlayerTurn ? "アークたち" : "魔軍");
        }

        Vector2 newSize = new Vector2(350, 150);
        msgBox.rectTransform.sizeDelta = newSize;

        EnableGroup(stageMessage);
        isWaitingMsg = true;
    }

    public void ShowGetItemInfo(int gold, int itemId, int weaponId)
    {
        List<string> msg = new List<string>();
        if (gold>0)
        {
            msg.Add("<color=orange>"+gold + "</color>Gold");
        }
        if (itemId > 0)
        {
            msg.Add(gameElement.items.Where(x => x.id == itemId).FirstOrDefault().name);
        }
        if (weaponId > 0)
        {
            msg.Add(gameElement.weapons.Where(x => x.id == weaponId).FirstOrDefault().name);
        }
        stageInfo.text = string.Format("宝箱を見つけた！\n宝箱の中から {0}をみつけた！", string.Join("\n", msg.ToArray()));

        Vector2 newSize = new Vector2(600, 150);
        msgBox.rectTransform.sizeDelta = newSize;

        EnableGroup(stageMessage);
        isWaitingMsg = true;
    }

    public void ShowUseItemInfo(string name, int hp, int atk, int def, int dex, int wis, int maxHP, int gold,string newCharType)
    {
        Vector2 newSize = new Vector2(350, 150);
        msgBox.rectTransform.sizeDelta = newSize;

        List<string> msg = new List<string>();
        if (gold > 0)
        {
            msg.Add(string.Format("<color=orange>{0}</color>得る！", gold));
        }
        if (hp > 0)
        {
            msg.Add(string.Format("の体力は<color=lime>{0}</color>回復する！", hp));
        }
        if (atk > 0)
        {
            msg.Add(string.Format("の攻撃力は<color=lime>{0}</color>アップする！", atk));
        }
        if (def > 0)
        {
            msg.Add(string.Format("の防御力は<color=lime>{0}</color>アップする！", def));
        }
        if (dex > 0)
        {
            msg.Add(string.Format("の敏捷さは<color=lime>{0}</color>アップする！", dex));
        }
        if (wis > 0)
        {
            msg.Add(string.Format("の知力は<color=lime>{0}</color>アップする！", wis));
        }
        if (maxHP > 0)
        {
            msg.Add(string.Format("の体力は<color=lime>{0}</color>アップする！", maxHP));
        }

        if (!string.IsNullOrEmpty(newCharType))
        {
            msg.Add("のクラスは{0}なる！" + newCharType);
        }
        stageInfo.text = string.Format("{0}\n{1}", name, string.Join("\n", msg.ToArray()));

        EnableGroup(stageMessage);
        isWaitingMsg = true;
    }

    public void ShowEquipWeaponInfo(string originalWeapon, string newWeapon)
    {
        Vector2 newSize = new Vector2(600, 150);
        msgBox.rectTransform.sizeDelta = newSize;

        stageInfo.text = string.Format("{0}を外して\n{1}を装備します", string.Format("<color=yellow>{0}</color>", originalWeapon), string.Format("<color=yellow>{0}</color>", newWeapon));

        EnableGroup(stageMessage);
        isWaitingMsg = true;
    }

    private void HidePlayers()
    {
        foreach (Player p in userPlayers.Union(enemyPlayers))
        {
            if (p.hp <= 0)
            {
                continue;
            }
            p.HidePlayer();
        }
    }

    private void ShowPlayers()
    {
        foreach (Player p in userPlayers.Union(enemyPlayers))
        {
            if (p.hp <= 0)
            {
                continue;
            }
            p.ShowPlayer();
        }
    }

    private void HideActor()
    {
        foreach (Player p in actorPlayers)
        {
            p.HidePlayer();
        }
    }

    private void ShowActor()
    {
        foreach (Player p in actorPlayers)
        {
            p.ShowPlayer();
        }
    }


    public void HideStageInfo()
    {
        DisableGroup(stageMessage);
        isWaitingMsg = false;
        if (isShowStage)
        {
            isShowStage = false;
            ShowStageInfo();
        }
    }

    public void ConfirmDialog()
    {
        DisableGroup(dialogGroup);
        isWaitingActor = false;
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

    public void SetPlayerStatusUI(Player player)
    {
        CharacterType race = gameElement.races[player.race];
        Weapon weapon = gameElement.weapons[player.equipWeapon];
        int directAtk = 0;
        int indirectAtk = 0;
        player.GetWeaponAttack(ref directAtk, ref indirectAtk);

        List<string> weaponRangeText = new List<string>();
        if (directAtk > 0)
        {
            weaponRangeText.Add(string.Format("<color=orange>{0}</color>", "直接"));
        }
        if (indirectAtk > 0)
        {
            weaponRangeText.Add(string.Format("<color=lime>{0}</color>", "間接"));
        }

        playerName.text = player.playerName;
        playerClass.text = race.name;
        playerLevel.text = player.level.ToString();
        playerHP.text = player.hp.ToString() + "/";
        playerMaxHP.text = player.maxHP.ToString();
        playerExp.text = player.exp.ToString();
        playerAtk.text = player.atk.ToString();
        playerWeaponAtk.text = string.Format("<color=white>(</color><color=orange>{0}</color><color=white>/</color><color=lime>{1}</color><color=white>)</color>", directAtk > 0 ? "+" + directAtk.ToString() : "✕", indirectAtk > 0 ? "+" + indirectAtk.ToString() : "✕");
        playerDef.text = player.def.ToString();
        playerWis.text = player.wis.ToString();
        playerDex.text = player.dex.ToString();
        playerMdef.text = player.mdef.ToString();
        playerEquip.text = weapon.name;
        playerEquipRange.text = string.Join("<color=white>/</color>", weaponRangeText.ToArray());
    }

    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
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
                if (p.hp > 0 && mapHex[(int)p.mapHexIndex.y][(int)p.mapHexIndex.x].type2D == TileType2D.Villa)
                {
                    p.hp += (p.maxHP / 8);
                    if (p.hp > p.maxHP)
                    {
                        p.hp = p.maxHP;
                    }
                }
            }
            Vector2 index = userPlayers[0].mapHexIndex;
            Vector3 pos = mapHex[(int)index.y][(int)index.x].transform.position;
            ScreenController.instance.SetCameraPos(pos);
            //isSetCamera = false;

            RemoveDeadEnemy();
            ShowStageInfo();
        }
        else
        {
            isSetCamera = true;
        }
        currentEnemyPlayerIndex++;
        currentEnemyPlayerIndex = currentEnemyPlayerIndex % enemyPlayers.Count;
    }

    private void RemoveDeadEnemy()
    {
        List<Player> temp = enemyPlayers.Where(x => x.hp <= 0).ToList();
        foreach (var p in temp)
        {
            enemyPlayers.Remove(p);
            Transform ui = playerUITransform.Find(p.name+"UI");
            ScreenController.instance.RemoveUI(p.name + "UI");
            Destroy(ui.gameObject);
            Destroy(p.gameObject);
        }

    }

    private void SetTileLineIsShow()
    {
        for (int i = 0; i < mapHex.Count; i++)
        {
            for (int j = 0; j < mapHex[i].Count; j++)
            {
                if (isShowTile)
                {
                    mapHex[i][j].SetShowUI();
                }
                else
                {
                    mapHex[i][j].SetHideUI();
                }
            }
        }
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
                targetPlayer.positionQueue.Add(mapHex[(int)t.mapHexIndex.y][(int)t.mapHexIndex.x].transform.position + playerHeight * Vector3.up);
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
        MenuType setType = MenuType.playerMoveCantAtkMenu;
        bool isShowAction = false;
        if (player.GetIsCanHeal())
        {
            isShowAction = player.GetHealRange().Where(x => GameManager.instance.userPlayers.Where(y => y.hp < y.maxHP && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0;
        }
        isShowAction = isShowAction || player.GetAttackRange().Where(x => GameManager.instance.enemyPlayers.Where(y => y.hp > 0 && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0;
        if (isShowAction)
        {
            setType = MenuType.playerMoveCanAtkMenu;
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

    private void SetTileName(HexTile destTile,out string tileName,out int defRate)
    {
        tileName = "";
        defRate = (int)destTile.defenseRate;
        switch (destTile.type2D)
        {
            case TileType2D.Road:
                tileName = "道";
                break;
            case TileType2D.Plain:
                tileName = "平原";
                break;
            case TileType2D.Wasteland:
                tileName = "荒地";
                break;
            case TileType2D.Villa:
                tileName = "村";
                break;
            case TileType2D.Forest:
                tileName = "森";
                break;
        }
    }

    public void AttackWithCurrentPlayer(HexTile destTile)
    {
        if (destTile.visual.transform.GetComponentInChildren<Renderer>().materials[0].color == Color.white && !destTile.impassible)
        {
            Debug.Log("destination invalid");
            return;
        }

        cameraPosition = ScreenController.instance.mainCamera.transform.position;
        cameraRotation = ScreenController.instance.mainCamera.transform.rotation.eulerAngles;

        battleData = null;

        Player attacker = null;
        Player target = null;
        bool isHeal = false;

        //set for battle scene
        string attackerName;
        string targetName;
        string backGround = "";
        string attackerTileName = "";
        string targetTileName = "";
        bool isPlayerAttack = isPlayerTurn;
        bool isCounter = false;
        int attackerDefensRate = 0;
        int targetDefensRate = 0;

        int attackerMaxHP;
        int attackerHP;
        int damageByAttacker = 0;
        int targetMaxHP;
        int targetHP;
        int damageByTarget = 0;

        string getItem = "";
        int playerExp = 0;
        int getExp = 0;
        int level = 0;
        string playerClass;
        PlayerRecord playerData = null;

        //Get attacker and target
        if (isPlayerTurn)
        {
            attacker = userPlayers[playerIndex];
            attackerName = attacker.playerName;
            attackerMaxHP = attacker.maxHP;
            attackerHP = attacker.hp;
            playerExp = attacker.exp;
            playerClass = gameElement.races.Where(x => x.id == attacker.race).FirstOrDefault().name;
            target = enemyPlayers.Where(x => x.gridPosition == destTile.gridPosition).FirstOrDefault();
            if (target == null)
            {
                target = userPlayers.Where(x => x.gridPosition == destTile.gridPosition).FirstOrDefault();
                isHeal = true;
            }
            targetName = target.playerName;
            targetMaxHP = target.maxHP;
            targetHP = target.hp;
            SetTileName(destTile, out targetTileName, out targetDefensRate);
        }
        else
        {
            attacker = enemyPlayers[currentEnemyPlayerIndex];
            targetName = attacker.playerName;
            targetMaxHP = attacker.maxHP;
            targetHP = attacker.hp;
            target = userPlayers.Where(x => x.gridPosition == destTile.gridPosition).FirstOrDefault();
            if (target == null)
            {
                target = enemyPlayers.Where(x => x.gridPosition == destTile.gridPosition).FirstOrDefault();
                isHeal = true;
            }
            attackerName = target.playerName;
            attackerMaxHP = target.maxHP;
            attackerHP = target.hp;
            playerExp = target.exp;
            playerClass = gameElement.races.Where(x => x.id == target.race).FirstOrDefault().name;
            SetTileName(destTile, out attackerTileName, out attackerDefensRate);
        }

        if (target != null)
        {
            HexTile targetTile = mapHex[(int)attacker.mapHexIndex.y][(int)attacker.mapHexIndex.x];
            bool isDirectAtk = HexTile.Distance(attacker.hex, target.hex) == 1;

            int directAtk = 0;
            int indirectAtk = 0;

            if (!isPlayerTurn)
            {
                SetTileName(targetTile, out targetTileName, out targetDefensRate);
            }
            else
            {
                SetTileName(targetTile, out attackerTileName, out attackerDefensRate);
            }

            attacker.GetWeaponAttack(ref directAtk, ref indirectAtk);

            //if (attacker.gridPosition.x >= target.gridPosition.x - 1 && attacker.gridPosition.x <= target.gridPosition.x + 1 &&
            //    attacker.gridPosition.y >= target.gridPosition.y - 1 && attacker.gridPosition.y <= target.gridPosition.y + 1)
            attacker.TurnEnd();
            RemoveHighlightTiles();
            attacking = false;
            moving = false;

            if (!isHeal)
            {
                //attack
                int amountOfDamage = Mathf.FloorToInt((attacker.atk + (isDirectAtk ? directAtk : indirectAtk) - target.def) * (1f - (destTile.defenseRate / 100f)));
                target.hp -= amountOfDamage;

                Debug.Log(attacker.playerName + (isDirectAtk ? " direct attack " : " indirect attack ") + target.playerName + " for " + (amountOfDamage + (target.hp < 0 ? target.hp : 0)) + " damage!");

                damageByTarget = (amountOfDamage + (target.hp < 0 ? target.hp : 0));
                //Target dead, user player get exp
                if (target.hp <= 0)
                {
                    if (isPlayerTurn)
                    {
                        target.gridPosition = new Vector2(-1, -1);
                        damageByAttacker = (amountOfDamage + (target.hp < 0 ? target.hp : 0));
                        attacker.exp += target.exp;
                        getExp = target.exp;
                        Debug.Log(attacker.playerName + " get exp " + target.exp + "!");
                    }
                    Debug.Log(attacker.playerName + " defeat " + target.playerName + "!");

                    //Got item/weapon/gold by enemy
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5)
                    {
                        playerGold += target.gold;
                        getItem = "<color=yellow>" + target.gold + "</color>Gold";
                        Debug.Log("Got " + target.gold + " gold!");
                    }
                    else
                    {
                        if (playerWeapons.ContainsKey(target.equipWeapon))
                        {
                            playerWeapons[target.equipWeapon]++;
                        }
                        else
                        {
                            playerWeapons.Add(target.equipWeapon, 1);
                        }
                        getItem = gameElement.weapons.Where(x => x.id == target.equipWeapon).FirstOrDefault().name;
                        Debug.Log("Got " + gameElement.weapons.Where(x => x.id == target.equipWeapon).FirstOrDefault().name + "!");
                    }
                }
                else
                {
                    //Target not dead, user player get exp
                    if (isPlayerTurn)
                    {
                        attacker.exp += amountOfDamage;
                        damageByAttacker = (amountOfDamage + (target.hp < 0 ? target.hp : 0));
                        getExp = amountOfDamage;
                        Debug.Log(attacker.playerName + " get exp " + amountOfDamage + "!");
                    }

                    //Counter
                    if (isDirectAtk && target.GetIsCanAttack(true) || !isDirectAtk && target.GetIsCanAttack(false))
                    {
                        isCounter = true;
                        target.GetWeaponAttack(ref directAtk, ref indirectAtk);

                        amountOfDamage = Mathf.FloorToInt((target.atk + (isDirectAtk ? directAtk : indirectAtk) - attacker.def) * (1f - (targetTile.defenseRate / 100f)));
                        attacker.hp -= amountOfDamage;

                        Debug.Log(target.playerName + " counter attack " + attacker.playerName + " for " + (amountOfDamage + (attacker.hp < 0 ? attacker.hp : 0)) + " damage!");
                        //Attacker dead, user player get exp
                        if (attacker.hp <= 0)
                        {
                            if (!isPlayerTurn)
                            {
                                attacker.gridPosition = new Vector2(-1, -1);
                                damageByAttacker = (amountOfDamage + (attacker.hp < 0 ? attacker.hp : 0));
                                target.exp += attacker.exp;
                                getExp = attacker.exp;
                                Debug.Log(target.playerName + " get exp " + attacker.exp + "!");
                            }
                            Debug.Log(target.playerName + " defeat " + attacker.playerName + "!");

                            //Got item/weapon/gold by enemy
                            if (UnityEngine.Random.Range(0f, 1f) < 0.5)
                            {
                                playerGold += attacker.gold;
                                getItem = "<color=yellow>" + attacker.gold + "</color>Gold";
                                Debug.Log("Got " + attacker.gold + " gold!");
                            }
                            else
                            {
                                if (playerWeapons.ContainsKey(attacker.equipWeapon))
                                {
                                    playerWeapons[attacker.equipWeapon]++;
                                }
                                else
                                {
                                    playerWeapons.Add(attacker.equipWeapon, 1);
                                }
                                getItem = gameElement.weapons.Where(x => x.id == attacker.equipWeapon).FirstOrDefault().name;
                                Debug.Log("Got " + gameElement.weapons.Where(x => x.id == attacker.equipWeapon).FirstOrDefault().name + "!");
                            }
                        }
                        else
                        {
                            //Attacker not dead, user player get exp
                            if (!isPlayerTurn)
                            {
                                target.exp += amountOfDamage;
                                damageByAttacker = (amountOfDamage + (attacker.hp < 0 ? attacker.hp : 0));
                                getExp = amountOfDamage;
                                Debug.Log(target.playerName + " get exp " + amountOfDamage + "!");
                            }
                        }
                    }
                }

                //bool hit = UnityEngine.Random.Range(0f, 1f) <= attacker.attackChance;
                //if (hit)
                //{
                //    int amountOfDamage = (int)Mathf.Floor(attacker.damageBase + UnityEngine.Random.Range(0, attacker.damageRollSides));

                //    target.hp -= amountOfDamage;

                //    Debug.Log(attacker.playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage!");
                //}
                //else
                //{
                //    Debug.Log(attacker.playerName + " missed " + target.playerName + "!");
                //}
            }
            else
            {
                //Heal player
                int amountOfHeal = Mathf.FloorToInt(attacker.wis);
                amountOfHeal = amountOfHeal > target.maxHP - target.hp ? target.maxHP - target.hp : amountOfHeal;
                target.hp += amountOfHeal;

                damageByAttacker = amountOfHeal;

                Debug.Log(attacker.playerName + " heal " + target.playerName + " for " + amountOfHeal + " HP point!");

                if (isPlayerTurn)
                {
                    attacker.exp += amountOfHeal;
                    getExp = amountOfHeal;
                    Debug.Log(attacker.playerName + " get exp " + amountOfHeal + "!");
                }
            }

            //level up
            PlayerRecord lvUpProp = null;
            if (isPlayerTurn && attacker.hp > 0 && (attacker.exp / 100) == attacker.level)
            {
                lvUpProp = attacker.LevelUp();
                level = attacker.level;
                playerData = attacker.GetPlayerProp();
                Debug.Log(attacker.playerName + " is level up to " + attacker.level + "!");

            }
            else if (!isPlayerTurn && target.hp > 0 && (target.exp / 100) == target.level)
            {
                lvUpProp = target.LevelUp();
                level = target.level;
                playerData = target.GetPlayerProp();
                Debug.Log(target.playerName + " is level up to " + target.level + "!");
            }

            //send to battle scene
            isWaitingBattle = true;

            //heal is not finish
            if (isHeal)
            {
                //return;
            }
            battleData = new BattleSendData(attackerName, targetName, backGround, attackerTileName, targetTileName, isPlayerAttack, isHeal, isCounter, isDirectAtk, attackerDefensRate, targetDefensRate, attackerMaxHP, attackerHP, damageByAttacker, targetMaxHP, targetHP, damageByTarget, getItem, playerExp, getExp, level, playerClass, playerData, lvUpProp);
            SceneManager.LoadScene("Battle");

            //else
            //{
            //    Debug.Log("Target is not adjacent!");
            //}
        }
    }

    public void HighlightTileAt(Vector2 originLocation, Color highlightColor, float distance, bool ignorePlayers = true)
    {
        List<HexTile> highlightTiles = new List<HexTile>();
        if (ignorePlayers)
        {
            highlightTiles = HexTileHighlight.FindHighlight(mapHex[(int)originLocation.y][(int)originLocation.x + (((int)originLocation.y) >> 1)], distance, true);
        }
        else
        {
            highlightTiles = HexTileHighlight.FindHighlight(mapHex[(int)originLocation.y][(int)originLocation.x + (((int)originLocation.y) >> 1)], distance, userPlayers.Union(enemyPlayers)/*.Where(x => x.gridPosition != originLocation)*/.Select(x => x.gridPosition).ToArray());
        }

        foreach (HexTile t in highlightTiles)
        {
            t.visual.transform.GetComponentInChildren<Renderer>().materials[0].color = highlightColor;
        }
    }

    public void HighlightTileAt(Vector2 originLocation, Color highlightColor, List<HexTile> highlightTiles)
    {
        foreach (HexTile t in highlightTiles)
        {
            t.visual.transform.GetComponentInChildren<Renderer>().materials[0].color = highlightColor;
        }
    }

    public void GetChest(int gold, int itemId, int weaponId)
    {
        playerGold += gold;
        if (itemId >= 0)
        {
            if (playerItems.ContainsKey(itemId))
            {
                playerItems[itemId]++;
            }
            else
            {
                playerItems.Add(itemId, 1);
            }
        }

        if (weaponId >= 0)
        {
            if (playerWeapons.ContainsKey(weaponId))
            {
                playerWeapons[weaponId]++;
            }
            else
            {
                playerWeapons.Add(weaponId, 1);
            }
        }

        ShowGetItemInfo(gold, itemId, weaponId);
    }


    public void SetItem()
    {
        itemGold.text = string.Format("所持金 <color=yellow>{0}</color> Gold", playerGold);

        buttonUseItem.enabled = false;
        buttonSellItem.enabled = false;

        Item selectItem = itemSelectedId >= 0 ? gameElement.items.Where(x => x.id==itemSelectedId).FirstOrDefault() : null;
        if (selectItem != null)
        {
            itemNotice.text = string.Format("<color=yellow>{0}</color>\r\n{1}", selectItem.name, selectItem.notice);
            if (playerIndex >= 0)
            {
                switch (selectItem.itemType)
                {
                    case ItemType.cure:
                        if (userPlayers[playerIndex].hp > 0 && userPlayers[playerIndex].hp < userPlayers[playerIndex].maxHP)
                        {
                            buttonUseItem.enabled = true;
                        }
                        break;
                    case ItemType.resurge:
                        if (userPlayers[playerIndex].hp <= 0)
                        {
                            buttonUseItem.enabled = true;
                        }
                        break;
                    case ItemType.special:
                        if (userPlayers[playerIndex].hp > 0)
                        {
                            if (selectItem.useCharType == -1 || (selectItem.useCharType == userPlayers[playerIndex].race && userPlayers[playerIndex].level >= 10))
                            {
                                buttonUseItem.enabled = true;
                            }
                        }
                        break;
                }
            }
            if (selectItem.price > 0)
            {
                buttonSellItem.enabled = true;
            }
        }
        else
        {
            itemNotice.text = "";
        }

        ClearItemList();
        foreach (var item in playerItems)
        {
            if (item.Value > 0)
            {
                Item setItem = gameElement.items.Where(x => x.id == item.Key).FirstOrDefault();
                GameObject newObject = Instantiate(itemUIPrefab, Vector3.zero, Quaternion.Euler(new Vector3()));
                string typeName = "";
                newObject.GetComponent<Button>().name = item.Key.ToString();
                if (item.Key == itemSelectedId)
                {
                    Color newColor = newObject.GetComponent<Image>().color;
                    newColor.a = 0.5f;
                    newObject.GetComponent<Image>().color = newColor;
                }
                switch (setItem.itemType)
                {
                    case ItemType.cure:
                        typeName = "回復";
                        break;
                    case ItemType.resurge:
                        typeName = "復活";
                        break;
                    case ItemType.special:
                        typeName = "特殊";
                        break;
                }
                newObject.GetComponent<ItemSelection>().SetItemInfo(setItem.name, typeName, item.Value);
                newObject.transform.SetParent(itemList);
            }
        }
    }

    public void SellItem()
    {
        Item selectItem = itemSelectedId >= 0 ? gameElement.items.Where(x => x.id == itemSelectedId).FirstOrDefault() : null;
        playerItems[itemSelectedId]--;
        playerGold += selectItem.price;

        itemSelectedId = -1;
        SetItem();
    }

    public void UseItem()
    {
        Item selectItem = itemSelectedId >= 0 ? gameElement.items.Where(x => x.id == itemSelectedId).FirstOrDefault() : null;
        playerItems[itemSelectedId]--;

        switch (selectItem.name)
        {
            case "愛の夢":
                for (int i = 0; i < userPlayers.Count; i++)
                {
                    userPlayers[i].hp = userPlayers[i].maxHP;
                }
                TurnEnd(0);
                break;
            case "ドカティの鍵":

                break;
            default:
                int hp = selectItem.hp;
                userPlayers[playerIndex].atk += selectItem.atk;
                userPlayers[playerIndex].def += selectItem.def;
                userPlayers[playerIndex].dex += selectItem.dex;
                userPlayers[playerIndex].wis += selectItem.wis;
                userPlayers[playerIndex].maxHP += selectItem.addHp;
                if (userPlayers[playerIndex].hp + hp > userPlayers[playerIndex].maxHP)
                {
                    hp = userPlayers[playerIndex].maxHP - userPlayers[playerIndex].hp;
                }
                userPlayers[playerIndex].hp += hp;

                playerGold += selectItem.gold;
                if (userPlayers[playerIndex].hp > userPlayers[playerIndex].maxHP)
                {
                    userPlayers[playerIndex].hp = userPlayers[playerIndex].maxHP;
                }

                if (selectItem.useCharType > 0)
                {
                    userPlayers[playerIndex].race = selectItem.newCharType;
                    userPlayers[playerIndex].level = 1;
                    userPlayers[playerIndex].exp = 0;
                }

                itemSelectedId = -1;

                itemGroup.alpha = 0;
                itemGroup.interactable = false;
                itemGroup.blocksRaycasts = false;

                ShowUseItemInfo(userPlayers[playerIndex].playerName, hp, selectItem.atk, selectItem.def, selectItem.dex, selectItem.wis, selectItem.addHp, selectItem.gold, (selectItem.useCharType > 0 ? gameElement.races.Where(x => x.id == selectItem.newCharType).FirstOrDefault().name : ""));

                break;
        }
    }

    private void ClearItemList()
    {
        for (int i = 0; i < itemList.transform.childCount; i++)
        {
            Destroy(itemList.transform.GetChild(i).gameObject);
        }
    }

    public void SetWeapon()
    {
        buttonEquipWeapon.enabled = false;
        buttonSellWeapon.enabled = false;

        Weapon selectWeapon = weaponSelectedId >= 0 ? gameElement.weapons.Where(x => x.id == weaponSelectedId).FirstOrDefault() : null;
        if (selectWeapon != null)
        {
            List<string> equipableRace = gameElement.races.Where(x => x.equipWeapon.Contains(weaponSelectedId)).Select(y => y.name).ToList();
            weaponNotice.text = string.Format("<color=yellow>{0}</color>\r\n{1}\r\n使えるクラスは、<color=yellow>{2}</color>です。", selectWeapon.name, selectWeapon.notice, (string.Join("</color><color=white>、</color><color=yellow>", equipableRace.ToArray())));
            if (playerIndex >= 0)
            {
                if (gameElement.races.Where(x => x.id == userPlayers[playerIndex].race && x.equipWeapon.Contains(weaponSelectedId)).Count() > 0)
                {
                    buttonEquipWeapon.enabled = true;
                }
            }
            if (selectWeapon.price > 0)
            {
                buttonSellWeapon.enabled = true;
            }
        }
        else
        {
            weaponNotice.text = "";
        }

        ClearWeaponList();
        foreach (var weapon in playerWeapons)
        {
            if (weapon.Value > 0)
            {
                Weapon selWeapon = gameElement.weapons.Where(x => x.id == weapon.Key).FirstOrDefault();
                GameObject newObject = Instantiate(weaponUIPrefab, Vector3.zero, Quaternion.Euler(new Vector3()));
                newObject.GetComponent<Button>().name = weapon.Key.ToString();
                if (weapon.Key == weaponSelectedId)
                {
                    Color newColor = newObject.GetComponent<Image>().color;
                    newColor.a = 0.5f;
                    newObject.GetComponent<Image>().color = newColor;
                }
                List<string> weaponRangeText = new List<string>();
                if (selWeapon.directAtk > 0)
                {
                    weaponRangeText.Add(string.Format("<color=orange>{0}</color>", "直接"));
                }
                if (selWeapon.indirectAtk > 0)
                {
                    weaponRangeText.Add(string.Format("<color=lime>{0}</color>", "間接"));
                }

                newObject.GetComponent<ItemSelection>().SetItemInfo(selWeapon.name, string.Join("<color=white>/</color>", weaponRangeText.ToArray()), weapon.Value);
                newObject.transform.SetParent(weaponList);
            }
        }
    }

    public void SellWeapon()
    {
        Weapon selectWeapon = weaponSelectedId >= 0 ? gameElement.weapons.Where(x => x.id == weaponSelectedId).FirstOrDefault() : null;
        playerWeapons[weaponSelectedId]--;
        playerGold += selectWeapon.price;

        itemSelectedId = -1;
        SetWeapon();
    }

    public void EquipWeapon()
    {
        Weapon selectWeapon = weaponSelectedId >= 0 ? gameElement.weapons.Where(x => x.id == weaponSelectedId).FirstOrDefault() : null;
        playerWeapons[weaponSelectedId]--;
        if (userPlayers[playerIndex].equipWeapon == weaponSelectedId)
        {
            return;
        }

        string originWeaponName = gameElement.weapons.Where(x => x.id == userPlayers[playerIndex].equipWeapon).FirstOrDefault().name;
        if (playerWeapons.ContainsKey(userPlayers[playerIndex].equipWeapon))
        {
            playerWeapons[userPlayers[playerIndex].equipWeapon]++;
        }
        else
        {
            playerWeapons.Add(userPlayers[playerIndex].equipWeapon, 1);
        }
        userPlayers[playerIndex].equipWeapon = selectWeapon.id;
        weaponSelectedId = -1;

        weaponGroup.alpha = 0;
        weaponGroup.interactable = false;
        weaponGroup.blocksRaycasts = false;

        ShowEquipWeaponInfo(originWeaponName, selectWeapon.name);
    }

    private void ClearWeaponList()
    {
        for (int i = 0; i < weaponList.transform.childCount; i++)
        {
            Destroy(weaponList.transform.GetChild(i).gameObject);
        }
    }

    public void SetShop()
    {
        shopGold.text = string.Format("所持金 <color=yellow>{0}</color> Gold", playerGold);
        buttonBuyItem.enabled = false;

        int price = 999999999;

        if (shopSelectedId != "")
        {
            string[] id = shopSelectedId.Split(':');
            if (id[0] == "Weapon")
            {
                Weapon selectWeapon = gameElement.weapons.Where(x => x.id.ToString() == id[1]).FirstOrDefault();
                price = selectWeapon.price;
                shopNotice.text = selectWeapon.notice;
            }
            else
            {
                Item selectItem = gameElement.items.Where(x => x.id.ToString() == id[1]).FirstOrDefault();
                price = selectItem.price;
                shopNotice.text = selectItem.notice;
            }
        }

        if (price <= playerGold)
        {
            buttonBuyItem.enabled = true;
        }

        ClearShopList();

        for (int i = 0; i < shopItemList.Count; i++)
        {
            Item selItem = gameElement.items.Where(x => x.id == shopItemList[i]).FirstOrDefault();
            GameObject newObject = Instantiate(shopUIPrefab, Vector3.zero, Quaternion.Euler(new Vector3()), shopList);
            string itemTypeName = "";
            switch (selItem.itemType)
            {
                case ItemType.cure:
                    itemTypeName = string.Format("<color=red>{0}</color>", "回復");
                    break;
                case ItemType.resurge:
                    itemTypeName = string.Format("<color=red>{0}</color>", "復活");
                    break;
                case ItemType.special:
                    itemTypeName = string.Format("<color=lightblue>{0}</color>", "特殊");
                    break;
            }
            newObject.GetComponent<Button>().name = string.Format("Item:{0}", shopItemList[i].ToString());
            newObject.GetComponent<ItemSelection>().SetItemInfo(selItem.name, itemTypeName, selItem.price);
            if (shopSelectedId == newObject.GetComponent<Button>().name)
            {
                Color newColor = newObject.GetComponent<Image>().color;
                newColor.a = 0.5f;
                newObject.GetComponent<Image>().color = newColor;
            }
        }

        for (int i = 0; i < shopWeaponList.Count; i++)
        {
            Weapon selWeapon = gameElement.weapons.Where(x => x.id == shopWeaponList[i]).FirstOrDefault();
            GameObject newObject = Instantiate(shopUIPrefab, Vector3.zero, Quaternion.Euler(new Vector3()), shopList);
            List<string> weaponRangeText = new List<string>();
            if (selWeapon.directAtk > 0)
            {
                weaponRangeText.Add(string.Format("<color=orange>{0}</color>", "直接"));
            }
            if (selWeapon.indirectAtk > 0)
            {
                weaponRangeText.Add(string.Format("<color=lime>{0}</color>", "間接"));
            }

            newObject.GetComponent<Button>().name = string.Format("Weapon:{0}", shopWeaponList[i].ToString());
            newObject.GetComponent<ItemSelection>().SetItemInfo(selWeapon.name, string.Join("<color=white>/</color>", weaponRangeText.ToArray()), selWeapon.price);
            if (shopSelectedId == newObject.GetComponent<Button>().name)
            {
                Color newColor = newObject.GetComponent<Image>().color;
                newColor.a = 0.5f;
                newObject.GetComponent<Image>().color = newColor;
            }
        }
    }

    private void ClearShopList()
    {
        for (int i = 0; i < shopList.transform.childCount; i++)
        {
            Destroy(shopList.transform.GetChild(i).gameObject);
        }
    }

    public void BuyItem()
    {
        if (shopSelectedId != "")
        {
            string[] id = shopSelectedId.Split(':');
            if (id[0] == "Weapon")
            {
                Weapon selectWeapon = gameElement.weapons.Where(x => x.id.ToString() == id[1]).FirstOrDefault();
                playerGold -= selectWeapon.price;
                if (playerWeapons.ContainsKey(selectWeapon.id))
                {
                    playerWeapons[selectWeapon.id]++;
                }
                else
                {
                    playerWeapons.Add(selectWeapon.id, 1);
                }
            }
            else
            {
                Item selectItem = gameElement.items.Where(x => x.id.ToString() == id[1]).FirstOrDefault();
                playerGold -= selectItem.price;
                if (playerItems.ContainsKey(selectItem.id))
                {
                    playerItems[selectItem.id]++;
                }
                else
                {
                    playerItems.Add(selectItem.id, 1);
                }
            }
            SetShop();
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
        if (gameElement != null)
        {
            return;
        }
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
        Vector3 connerPointA = Vector3.zero;
        Vector3 connerPointB = Vector3.zero;
        Vector3 connerPointC = Vector3.zero;
        Vector3 connerPointD = Vector3.zero;

        MapXmlContainer container = ObjectSaveLoad.JsonLoad<MapXmlContainer>("test.txt");
        mapSizeX = container.sizeX;
        mapSizeY = container.sizeY;

        shopItemList = container.shopItemList;
        shopWeaponList = container.shopWeaponList;

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
                TileXml temp = container.tiles.Where(x => x.locX == j && x.locY == i).FirstOrDefault();
                tile.TileInitializer(mapTransform, (TileType)temp.id, (TileType2D)temp.id, temp.spritIndex, 0, j, i, mapSizeX, mapSizeY, temp.gold, temp.itemId, temp.weaponId, temp.isShop);
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
        DisableGroup(menu);
    }


    private void Move(int inputPlayerIndex)
    {
        if (!moving)
        {
            RemoveHighlightTiles();
            moving = true;
            attacking = false;
            HighlightTileAt(userPlayers[inputPlayerIndex].gridPosition, moveTileColor, (int)userPlayers[inputPlayerIndex].movementPerActionPoint, false);
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
            HighlightTileAt(userPlayers[inputPlayerIndex].gridPosition, attackTileColor, userPlayers[inputPlayerIndex].GetAttackRange());
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
            userPlayers[inputPlayerIndex].transform.position = new Vector3(oriPos.x, playerHeight, oriPos.z);
            moving = false;
            Move(inputPlayerIndex);
        }
        DisableGroup(endTurnConfirm);
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
            if (p.hp > 0 && mapHex[(int)p.mapHexIndex.y][(int)p.mapHexIndex.x].type2D == TileType2D.Villa)
            {
                p.hp += (p.maxHP / 8);
                if (p.hp> p.maxHP)
                {
                    p.hp = p.maxHP;
                }
            }
        }
        RemoveDeadEnemy();
        SetStartWaiting();
        DisableGroup(endTurnConfirm);

        ShowStageInfo();
        isSetCamera = true;
    }

    private void Status(int playerIndex)
    {
        EnableGroup(status);
    }

    private void Item(int playerIndex)
    {
        SetItem();
        itemGroup.alpha = 1;
        itemGroup.interactable = true;
        itemGroup.blocksRaycasts = true;
    }

    private void Weapon(int playerIndex)
    {
        SetWeapon();
        weaponGroup.alpha = 1;
        weaponGroup.interactable = true;
        weaponGroup.blocksRaycasts = true;
    }

    private void Unit(int playerIndex)
    {
        unitGroup.alpha = 1;
        unitGroup.blocksRaycasts = true;
        unitGroup.interactable = true;

        turnText.text = string.Format("ターン{0}", turnCount);
        for (int i = 0; i < userPlayerList.transform.childCount; i++)
        {
            Destroy(userPlayerList.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < enemyPlayerList.transform.childCount; i++)
        {
            Destroy(enemyPlayerList.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < userPlayers.Count; i++)
        {
            CharacterSelection item = Instantiate(characterUIPrefab, new Vector3(0, 0, 0), Quaternion.identity, userPlayerList).GetComponent<CharacterSelection>();
            item.SetText(userPlayers[i].playerName, gameElement.races.Where(x => x.id == userPlayers[i].race).FirstOrDefault().name, userPlayers[i].level, userPlayers[i].hp, userPlayers[i].maxHP, userPlayers[i].isActable, userPlayers[i].transform.position);
        }

        for (int i = 0; i < enemyPlayers.Count; i++)
        {
            if (enemyPlayers[i].hp <= 0)
            {
                continue;
            }
            CharacterSelection item = Instantiate(characterUIPrefab, new Vector3(0, 0, 0), Quaternion.identity, enemyPlayerList).GetComponent<CharacterSelection>();
            item.SetText(enemyPlayers[i].playerName, gameElement.races.Where(x => x.id == enemyPlayers[i].race).FirstOrDefault().name, enemyPlayers[i].level, enemyPlayers[i].hp, enemyPlayers[i].maxHP, enemyPlayers[i].isActable, enemyPlayers[i].transform.position);
        }

        userPanel.SetAsLastSibling();
        userPanel.Find("Panel").gameObject.SetActive(true);
        enemyPanel.Find("Panel").gameObject.SetActive(false);
    }

    private void Setting(int playerIndex)
    {
        gameSetting.alpha = 1;
        gameSetting.interactable = true;
        gameSetting.blocksRaycasts = true;
    }

    private void Shop(int playerIndex)
    {
        shopSelectedId = "";
        SetShop();
        EnableGroup(shopGroup);
    }

    private void UISettingConfirm(int playerIndex)
    {
        isShowTile = isShowTileLine.isOn;
        isShowPlayerUI = isShowPlayHP.isOn;

        SetTileLineIsShow();
        ScreenController.instance.SetPlayerUIIsShow(isShowPlayerUI);

        DisableGroup(gameSetting);
    }


    #endregion

    public void CancelAction()
    {
        if (menu.blocksRaycasts)
        {
            DisableGroup(menu);
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
            attacking = false;
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

    public void ShowEndTurnConfirm()
    {
        DisableGroup(menu);
        EnableGroup(endTurnConfirm);
    }

    void GenetarePlayers()
    {
        for (int i = 0; i < userPlayerRecords.Count; i++)
        {
            UserPlayer player;
            int x = userPlayerRecords[i].locX + (userPlayerRecords[i].locY >> 1);
            int y = userPlayerRecords[i].locY;
            Vector3 tilePOs = mapHex[y][x].HexTilePos();
            CharacterTemplate playerData = playerTypes.Where(t=>t.id== userPlayerRecords[i].characterId).FirstOrDefault();
            CharacterLevelTemplate playerLvData = playerData.levelData[0];
            PlayerRecord record = saveUserPlayerRecords.Where(t => t.characterId == userPlayerRecords[i].characterId).FirstOrDefault();
            //player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
            player = Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, new Vector3(tilePOs.x, playerHeight, tilePOs.z), Quaternion.Euler(new Vector3(0, 180, 0))).GetComponent<UserPlayer>();
            player.gameObject.name = string.Format(userPlayerNameFormat, i);
            player.transform.SetParent(playerTransform);

            player.gridPosition = new Vector2(userPlayerRecords[i].locX, userPlayerRecords[i].locY);

            player.playerName = playerData.name;
            player.race = playerData.race;
            player.movementPerActionPoint = playerData.move;
            PlayerUI playerUI = (Instantiate(playerUIPrefab, new Vector3(tilePOs.x, 1, tilePOs.z), playerUIPrefab.transform.rotation)).GetComponent<PlayerUI>();
            playerUI.player = player;
            playerUI.gameObject.name = string.Format(userPlayerNameFormat + "UI", i);
            playerUI.transform.SetParent(playerUITransform);

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
            if (i == 0)
            {
                ScreenController.instance.SetCameraPos(new Vector3(tilePOs.x, 0, tilePOs.z));
            }
        }

        for (int i = 0; i < enemyPlayerRecords.Count; i++)
        {
            AIPlayer player;
            int x = enemyPlayerRecords[i].locX + (enemyPlayerRecords[i].locY >> 1);
            int y = enemyPlayerRecords[i].locY;
            Vector3 tilePOs = mapHex[y][x].HexTilePos();
            CharacterTemplate playerData = enemyTypes.Where(t=>t.id== enemyPlayerRecords[i].characterId).FirstOrDefault();
            CharacterLevelTemplate playerLvData = playerData.levelData.Where(t => t.id == enemyPlayerRecords[i].levelId).FirstOrDefault();
            //player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
            player = ((GameObject)Instantiate(PlayerPrefabHolder.instance.enemyPlayer_prefab, new Vector3(tilePOs.x, playerHeight, tilePOs.z), Quaternion.Euler(new Vector3(0,180,0)))).GetComponent<AIPlayer>();
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
            player.gold = (int)playerLvData.gold;
            player.equipWeapon = playerLvData.equipWeapon;
            player.enemyAIType = enemyPlayerRecords[i].aiType;
            player.searchRange = enemyPlayerRecords[i].searchRange;
            player.playerIndex = i;
            PlayerUI playerUI = (Instantiate(playerUIPrefab, new Vector3(tilePOs.x, 1, tilePOs.z), playerUIPrefab.transform.rotation)).GetComponent<PlayerUI>();
            playerUI.player = player;
            playerUI.gameObject.name = string.Format(enemyPlayerNameFormat + "UI", i);
            playerUI.transform.SetParent(playerUITransform);

            enemyPlayers.Add(player);
        }

        ScreenController.instance.SetPlayerUIs();
    }
}
