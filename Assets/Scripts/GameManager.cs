﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [SerializeField]
    private GameUIManager m_GameUIManager = default;

    #region parament

    //public GameObject tilePrefab;
    //public GameObject userPlayerPrefab;
    //public GameObject aiPlayerPrefab;
    Transform mapTransform;
    Transform playerTransform;
    Transform playerUITransform;
    Transform actorPlayerTransform;

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
    private List<PlayerRecord> saveEnemyPlayerRecords;
    private List<PlayerRecord> userPlayerRecords;
    private List<PlayerRecord> enemyPlayerRecords;
    //public List<Player> players = new List<Player>();
    public List<Player> userPlayers { get; private set; }
    public List<Player> enemyPlayers { get; private set; }
    public List<Player> actorPlayers { get; private set; }
    public List<Scenario> stageScenatios = new List<Scenario>();
    public Scenario runningScenario;
    public List<int> m_ShopItemList { get; private set; }
    public List<int> m_ShopWeaponList { get; private set; }
    public List<int> defeatedEnemyList;
    public List<int> removeScenaroList;

    public GameElement gameElement { get; private set; }
    private List<CharacterTemplate> playerTypes;
    private List<CharacterTemplate> enemyTypes;
    private List<StageClearCondition> stageClearConditions = new List<StageClearCondition>();

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
    private bool isMoveCamera = false;
    private bool isToDark = false;
    private bool isWin = false;
    private bool isLose = false;
    private Vector3 moveCaremaPos = new Vector3();
    public float cameraMoveSpeed = 1;
    [HideInInspector]
    public bool isSetCamera = false;

    private UnityAction _Update = Utility.Nothing;

    public int m_TurnCount { get; private set; }

    private float waitingTime = 0;
    private float currentWaitingTime = 0;

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

    private bool m_IsNewGame = true;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;

    [HideInInspector]
    public bool moving = false;
    [HideInInspector]
    public bool attacking = false;

    [SerializeField]
    private int m_PlayerGoldMax = 60000;
    public int _playerGold = 1000;
    public Dictionary<int, int> m_PlayerItems { get; private set; }
    public Dictionary<int, int> m_PlayerWeapons { get; private set; }

    public BattleSendData battleData = new BattleSendData();
    #endregion

    #region UI parament

    [Header("UI Setting UI")]
    public Toggle isShowTileLine;
    public Toggle isShowPlayHP;

    [Header("Dialog UI")]
    public CanvasGroup dialogGroup;
    public Text dialogName;
    public Text dialogText;

    public Image darkFront;
    #endregion

    public int m_PlayerGold
    {
        get
        {
            return _playerGold;
        }
        set { _playerGold = Mathf.Clamp(value, 0, m_PlayerGoldMax); }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        m_GameUIManager.Initialize(instance);
        mapTransform = transform.Find("Map");
        playerTransform = transform.Find("Players");
        playerUITransform = transform.Find("PlayerUIs");
        actorPlayerTransform = transform.Find("ActorPlayers");
    }

    private void Start()
    {
        //GameLoading();
        m_GameUIManager.GameSetting();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            if (currentWaitingTime >= waitingTime)
            {
                waitingTime = 0;
                currentWaitingTime = 0;
                if (isMoveCamera)
                {
                    isMoveCamera = false;
                    isWaitingActor = false;
                    ScreenController.instance.SetCameraPos(new Vector3(moveCaremaPos.x, 0, moveCaremaPos.z));
                }
                if (isToDark)
                {
                    Color temp = darkFront.color;
                    temp.a = 0;
                    darkFront.color = temp;
                }
            }
            else
            {
                currentWaitingTime += Time.deltaTime;
                if (isMoveCamera)
                {
                    if (Vector3.Distance(moveCaremaPos, ScreenController.instance.cameraPos.position) > 0.1f)
                    {
                        Vector3 newPosition = ScreenController.instance.cameraPos.position + (moveCaremaPos - ScreenController.instance.cameraPos.position).normalized * cameraMoveSpeed * Time.deltaTime;
                        ScreenController.instance.SetCameraPos(new Vector3(newPosition.x, 0, newPosition.z));
                    }
                }
                if (isToDark)
                {
                    Color temp = darkFront.color;
                    temp.a += (Time.deltaTime / waitingTime);
                    darkFront.color = temp;
                }
                return;
            }

            //play mode
            if (runningScenario == null && !isWin && !isLose)
            {
                PlayMode();
            }
            //scenario mode
            else if (!isWin && !isLose)
            {
                ScenarioMode();
            }
            else
            {
                EndMode();
            }
        }
    }
    #region Game control

    private void PlayMode()
    {
        if (!isWaitingMsg && !isWaitingBattle)
        {
            for (int i = 0; i < stageClearConditions.Count; i++)
            {
                switch (stageClearConditions[i].stageClearConditionType)
                {
                    case StageClearConditionType.EnemyDead:
                        int deadCount = 0;
                        int deadCondition = stageClearConditions[i].enemyDeadCount;
                        if (stageClearConditions[i].enemyDeadCount == 0 || stageClearConditions[i].enemyDeadCount > stageClearConditions[i].enemyDeadList.Count)
                        {
                            deadCondition = stageClearConditions[i].enemyDeadList.Count;
                        }
                        List<int> deatList = stageClearConditions[i].enemyDeadList;
                        for (int j = 0; j < deatList.Count; j++)
                        {
                            Player tempPlayer = enemyPlayers.Where(x => x.playerIndex == deatList[j]).FirstOrDefault();
                            if (tempPlayer == null || tempPlayer.hp <= 0)
                            {
                                deadCount++;
                            }
                        }
                        if (deadCount >= deadCondition)
                        {
                            isWin = true;
                            _Update = EndMode;
                        }
                        break;
                    case StageClearConditionType.SpecifyTile:
                        if (userPlayers.Where(x => x.hex == stageClearConditions[i].specifyTile).Count() > 0)
                        {
                            isWin = true;
                            _Update = EndMode;
                        }
                        break;
                }
                if (isWin)
                {
                    //Win
                    Scenario temp = stageScenatios.Where(x => x.scenarioType == ScenarioType.StageClear).FirstOrDefault();
                    if (temp != null)
                    {
                        runningScenario = temp;
                        _Update = ScenarioMode;
                    }
                    else
                    {
                        Debug.Log("No Clear Scenario");
                    }
                    break;
                }
            }

            if (!isPlayerTurn)
            {
                if (userPlayers.Where(x => x.hp > 0).Count() > 0 && enemyPlayers.Where(x => x.hp > 0).Count() > 0)
                {
                    if (enemyPlayers[currentEnemyPlayerIndex].hp > 0)
                    {
                        StartCoroutine(RunEnemyTurn());
                    }
                    else
                    {
                        NextEnemyTurn();
                    }
                }
            }
        }
    }

    private void ScenarioMode()
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
                        for (int i = 0; i < runningScenario.scenarioActions[runningScenario.scenarioActionStep].createActors.Count; i++)
                        {
                            PlayerRecord temp = runningScenario.scenarioActions[runningScenario.scenarioActionStep].createActors[i];
                            Vector3 pos = getMapTile(new HexTile.HexCoord(temp.locX, temp.locY)).HexTilePos();
                            ActorPlayer player = Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, pos, Quaternion.identity, actorPlayerTransform).GetComponent<ActorPlayer>();
                            player.gridPosition = new Vector2(temp.locX, temp.locY);
                            player.playerIndex = temp.id;
                            player.gameObject.name = temp.id.ToString();
                            player.SetPivot(temp.scenarioActorPivotType);
                            player.SetPlayerModel();
                            actorPlayers.Add(player);
                        }
                        break;
                    case ScenarioActionType.ControlActor:
                        Player tempPlayer = actorPlayers.Where(x => x.playerIndex == runningScenario.scenarioActions[runningScenario.scenarioActionStep].actorId).FirstOrDefault();
                        HexTile targetTile = getMapTile(runningScenario.scenarioActions[runningScenario.scenarioActionStep].targetMoveTile);
                        if (getMapTile(tempPlayer.mapHexIndex) != targetTile)
                        {
                            foreach (HexTile t in HexTilePathFinder.FindPath(getMapTile(tempPlayer.mapHexIndex), targetTile, true).listOfTiles)
                            {
                                tempPlayer.positionQueue.Add(getMapTile(t.mapHexIndex).transform.position + playerHeight * Vector3.up);
                            }
                            isWaitingActor = true;
                        }
                        tempPlayer.gridPosition = targetTile.gridPosition;

                        tempPlayer.playerPivot = runningScenario.scenarioActions[runningScenario.scenarioActionStep].actorPivot;
                        tempPlayer.SetPivot(tempPlayer.playerPivot);
                        break;
                    case ScenarioActionType.SetCamera:
                        Vector3 tilePos = getMapTile(runningScenario.scenarioActions[runningScenario.scenarioActionStep].setCameraPos).HexTilePos();
                        ScreenController.instance.SetCameraPos(new Vector3(tilePos.x, 0, tilePos.z));
                        break;
                    case ScenarioActionType.ControlCamera:
                        moveCaremaPos = getMapTile(runningScenario.scenarioActions[runningScenario.scenarioActionStep].targetMoveTile).HexTilePos();
                        isMoveCamera = true;
                        isWaitingActor = true;
                        break;
                    case ScenarioActionType.AddUserPlayer:
                        break;
                    case ScenarioActionType.AddEnemyPlayer:
                        break;
                }
                currentWaitingTime = 0;
                isToDark = runningScenario.scenarioActions[runningScenario.scenarioActionStep].isToDark;
                waitingTime = runningScenario.scenarioActions[runningScenario.scenarioActionStep].waitTime;
                runningScenario.scenarioActionStep++;
            }
            else
            {
                //Scenario is end
                SetStopWaiting();
                EnableGroup(mapController);
                if (runningScenario.scenarioType == ScenarioType.Openning)
                {
                    ShowPlayers();
                    HideActor();
                    ClearActorPlayer();
                    Player temp = userPlayers.Where(x => x.playerIndex == 0).FirstOrDefault();
                    ScreenController.instance.SetCameraPos(new Vector3(temp.transform.position.x, 0, temp.transform.position.z));
                    ShowStageInfo(false);
                }

                if (runningScenario.scenarioConditionType == ScenarioConditionType.BeforeBattle)
                {
                    AttackWithCurrentPlayer(runningScenario.battleAfterEvent);
                }
                if (runningScenario.isOnceEvent)
                {
                    removeScenaroList.Add(runningScenario.scenarioId);
                }
                runningScenario = null;
                _Update = PlayMode;
            }
        }
    }

    private void EndMode()
    {
        if (isWin)
        {
            PlayerPrefs.SetInt("endTpye", 0);
        }
        else
        {
            PlayerPrefs.SetInt("endTpye", 1);
        }
        _Update = Utility.Nothing;
        SceneManager.LoadScene("GameEnd");
    }

    #endregion

    private void OnLevelWasLoaded(int level)
    {
        gameSceneUI.SetActive(false);
        if (level == 1)
        {
            int loadIndex;
            bool isLoadMap = false;
            try
            {
                loadIndex = PlayerPrefs.GetInt("loadIndex");
                isLoadMap = PlayerPrefs.GetInt("isLoadMap") == 1;
            }
            catch
            {
                loadIndex = -1;
            }
            if (isLoadMap)
            {
                m_IsNewGame = true;
                if (loadIndex != -1)
                {
                    //Load game save
                    m_IsNewGame = false;
                }
                GameLoading();
            }
            loadIndex = -1;
            isLoadMap = false;
            PlayerPrefs.SetInt("loadIndex", -1);
            PlayerPrefs.SetInt("isLoadMap", 0);
            //ScreenController.instance.SetCameraPos(cameraPosition);
            //ScreenController.instance.SetCameraRot(cameraRotation);

            gameSceneUI.SetActive(true);
            isWaitingBattle = false;

            if (userPlayers.Where(x => gameElement.races.Where(y => y.name == "ロード").FirstOrDefault().id == x.race && x.hp < 0).Count() > 0)
            {
                isLose = true;
                _Update = EndMode;
                //Game Over
            }
        }
    }

    private void InitialStage()
    {
        m_TurnCount = 1;
        isPlayerTurn = true;
        isWaitingActor = false;
        isWaitingMsg = false;
        isWin = false;
        isLose = false;
        currentEnemyPlayerIndex = 0;
        playerIndex = -1;
        moving = false;
        attacking = false;
        Color temp = darkFront.color;
        temp.a = 0;
        darkFront.color = temp;

        ClearPlayer();
        ClearActorPlayer();

        LoadGameElements();
        SetStopWaiting();
        HideStageInfo();

        DisableGroup(menu);
        DisableGroup(endTurnConfirm);
        //DisableGroup(status);
        DisableGroup(gameSetting);
        //DisableGroup(itemGroup);
        //DisableGroup(weaponGroup);
        //DisableGroup(unitGroup);
        //DisableGroup(shopGroup);
        DisableGroup(dialogGroup);
        isShowStage = true;
    }

    private void StartGame()
    {
        GenetareMap();
        GenetarePlayers();
    }

    private void GameLoading()
    {
        ScreenController.instance.SetCameraPos(cameraPosition);
        ScreenController.instance.SetCameraRot(cameraRotation);

        isWaitingBattle = false;
        InitialStage();
        if (m_IsNewGame)
        {
            m_IsNewGame = false;
            saveUserPlayerRecords = new List<PlayerRecord>();
            saveEnemyPlayerRecords = new List<PlayerRecord>();
            defeatedEnemyList = new List<int>();
            m_PlayerItems = new Dictionary<int, int>();
            m_PlayerWeapons = new Dictionary<int, int>();
            //int herbId = gameElement.items.Where(x => x.name == "薬草").FirstOrDefault().id;
            //m_PlayerItems.Add(herbId, 4);
            for (int i = 0; i < m_PlayerItems.Count; i++)
            {
                m_PlayerItems.Add(gameElement.items[i].id, gameElement.items[i].initialCount);
            }
        }
        StartGame();
        Scenario opening = stageScenatios.Where(x => x.scenarioType == ScenarioType.Openning).FirstOrDefault();

        if (opening != null)
        {
            runningScenario = opening;
            _Update = ScenarioMode;
            SetStartWaiting();
            if (runningScenario.isOnceEvent)
            {
                stageScenatios.Remove(opening);
            }
        }
        else
        {
            ShowStageInfo(false);
            Debug.Log("No opening ");
            _Update = PlayMode;
        }
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
        stageInfo.text = string.Format("Stage {0}\n{1}", 1, "旅立ち");
        if (turnMsg)
        {
            stageInfo.text = string.Format("ターン {0}\n{1}の行動", m_TurnCount, isPlayerTurn ? "アークたち" : "魔軍");
        }

        Vector2 newSize = new Vector2(350, 150);
        msgBox.rectTransform.sizeDelta = newSize;

        EnableGroup(stageMessage);
        isWaitingMsg = true;
    }

    public void ShowGetItemInfo(int gold, int itemId, int weaponId)
    {
        List<string> msg = new List<string>();
        if (gold > 0)
        {
            msg.Add("<color=orange>" + gold + "</color>Gold");
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

    public void ShowUseItemInfo(string name, int hp, int atk, int def, int dex, int wis, int maxHP, int gold, string newCharType)
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

    private void ClearPlayer()
    {
        for (int i = 0; i < playerTransform.childCount; i++)
        {
            Destroy(playerTransform.GetChild(i).gameObject);
        }
        userPlayers = new List<Player>();
        enemyPlayers = new List<Player>();
    }

    private void ClearActorPlayer()
    {
        for (int i = 0; i < actorPlayerTransform.childCount; i++)
        {
            Destroy(actorPlayerTransform.GetChild(i).gameObject);
        }
        actorPlayers = new List<Player>();
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
        StopWaitActor();
    }

    public void StopWaitActor()
    {
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

        m_GameUIManager.SetPlayerStatusUI(player, race, weapon);
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
            Vector3 pos = getMapTile(index).transform.position;
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
            m_TurnCount++;
            isPlayerTurn = true;
            SetStopWaiting();
            foreach (Player p in userPlayers)
            {
                p.TurnActive();
                if (p.hp > 0 && getMapTile(p.mapHexIndex).type2D == TileType2D.Villa)
                {
                    p.hp += (p.maxHP / 8);
                    if (p.hp > p.maxHP)
                    {
                        p.hp = p.maxHP;
                    }
                }
            }
            Vector2 index = userPlayers[0].mapHexIndex;
            Vector3 pos = getMapTile(index).transform.position;
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
            Transform ui = playerUITransform.Find(p.name + "UI");
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
        if (destTile.visual.transform.GetComponentInChildren<Renderer>().materials[0].color != Color.white && !destTile.impassible)
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
            foreach (HexTile t in HexTilePathFinder.FindPath(getMapTile(targetPlayer.mapHexIndex), destTile, userPlayers.Union(enemyPlayers).Where(x => x.gridPosition != targetPlayer.gridPosition).Select(x => x.gridPosition).ToArray()).listOfTiles)
            {
                targetPlayer.positionQueue.Add(getMapTile(t.mapHexIndex).transform.position + playerHeight * Vector3.up);
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

    private void SetTileName(HexTile destTile, out string tileName, out int defRate)
    {
        tileName = "";
        defRate = (int)destTile.defenseRate;
        switch (destTile.type2D)
        {
            case TileType2D.Road:
                tileName = "道/橋";
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
            Scenario temp = stageScenatios.Where(x => x.scenarioType == ScenarioType.Event && x.scenarioConditionType == ScenarioConditionType.BeforeBattle && (x.userPlayer == -1 || x.userPlayer == attacker.playerIndex) && (x.enemyPlayer == -1 || x.enemyPlayer == target.playerIndex)).FirstOrDefault();
            if (temp != null)
            {
                if (temp.isOnceEvent)
                {
                    stageScenatios.Remove(temp);
                }
                temp.battleAfterEvent = destTile;
                runningScenario = temp;
                _Update = ScenarioMode;
            }
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

            Scenario temp = stageScenatios.Where(x => x.scenarioType == ScenarioType.Event && x.scenarioConditionType == ScenarioConditionType.BeforeBattle && (x.enemyPlayer == -1 || x.enemyPlayer == attacker.playerIndex) && (x.userPlayer == -1 || x.userPlayer == target.playerIndex)).FirstOrDefault();
            if (temp != null)
            {
                if (temp.isOnceEvent)
                {
                    stageScenatios.Remove(temp);
                }
                temp.battleAfterEvent = destTile;
                runningScenario = temp;
                _Update = ScenarioMode;
            }
        }

        if (runningScenario != null)
        {
            return;
        }
        if (target != null)
        {
            HexTile targetTile = getMapTile(attacker.mapHexIndex);
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

            attacker.GetWeaponAttack(out directAtk, out indirectAtk);

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

                //Debug.Log(attacker.playerName + (isDirectAtk ? " direct attack " : " indirect attack ") + target.playerName + " for " + (amountOfDamage + (target.hp < 0 ? target.hp : 0)) + " damage!");

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
                        Scenario temp = stageScenatios.Where(x => x.scenarioConditionType == ScenarioConditionType.AfterBattle && x.enemyPlayer == target.playerIndex).FirstOrDefault();
                        if (temp != null)
                        {
                            stageScenatios.Remove(temp);
                            runningScenario = temp;
                            _Update = ScenarioMode;
                        }
                        //Debug.Log(attacker.playerName + " get exp " + target.exp + "!");
                    }
                    //Debug.Log(attacker.playerName + " defeat " + target.playerName + "!");

                    //Got item/weapon/gold by enemy
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5)
                    {
                        m_PlayerGold += target.gold;
                        getItem = "<color=yellow>" + target.gold + "</color>Gold";
                        //Debug.Log("Got " + target.gold + " gold!");
                    }
                    else
                    {
                        if (m_PlayerWeapons.ContainsKey(target.equipWeapon))
                        {
                            m_PlayerWeapons[target.equipWeapon]++;
                        }
                        else
                        {
                            m_PlayerWeapons.Add(target.equipWeapon, 1);
                        }
                        getItem = gameElement.weapons.Where(x => x.id == target.equipWeapon).FirstOrDefault().name;
                        //Debug.Log("Got " + gameElement.weapons.Where(x => x.id == target.equipWeapon).FirstOrDefault().name + "!");
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
                        //Debug.Log(attacker.playerName + " get exp " + amountOfDamage + "!");
                    }

                    //Counter
                    if (isDirectAtk && target.GetIsCanAttack(true) || !isDirectAtk && target.GetIsCanAttack(false))
                    {
                        isCounter = true;
                        target.GetWeaponAttack(out directAtk, out indirectAtk);

                        amountOfDamage = Mathf.FloorToInt((target.atk + (isDirectAtk ? directAtk : indirectAtk) - attacker.def) * (1f - (targetTile.defenseRate / 100f)));
                        attacker.hp -= amountOfDamage;

                        //Debug.Log(target.playerName + " counter attack " + attacker.playerName + " for " + (amountOfDamage + (attacker.hp < 0 ? attacker.hp : 0)) + " damage!");
                        //Attacker dead, user player get exp
                        if (attacker.hp <= 0)
                        {
                            if (!isPlayerTurn)
                            {
                                attacker.gridPosition = new Vector2(-1, -1);
                                damageByAttacker = (amountOfDamage + (attacker.hp < 0 ? attacker.hp : 0));
                                target.exp += attacker.exp;
                                getExp = attacker.exp;
                                Scenario temp = stageScenatios.Where(x => x.scenarioConditionType == ScenarioConditionType.AfterBattle && x.enemyPlayer == attacker.playerIndex).FirstOrDefault();
                                if (temp != null)
                                {
                                    stageScenatios.Remove(temp);
                                    runningScenario = temp;
                                    _Update = ScenarioMode;
                                }
                                //Debug.Log(target.playerName + " get exp " + attacker.exp + "!");
                            }
                            //Debug.Log(target.playerName + " defeat " + attacker.playerName + "!");

                            //Got item/weapon/gold by enemy
                            if (UnityEngine.Random.Range(0f, 1f) < 0.5)
                            {
                                m_PlayerGold += attacker.gold;
                                getItem = "<color=yellow>" + attacker.gold + "</color>Gold";
                                //Debug.Log("Got " + attacker.gold + " gold!");
                            }
                            else
                            {
                                if (m_PlayerWeapons.ContainsKey(attacker.equipWeapon))
                                {
                                    m_PlayerWeapons[attacker.equipWeapon]++;
                                }
                                else
                                {
                                    m_PlayerWeapons.Add(attacker.equipWeapon, 1);
                                }
                                getItem = gameElement.weapons.Where(x => x.id == attacker.equipWeapon).FirstOrDefault().name;
                                //Debug.Log("Got " + gameElement.weapons.Where(x => x.id == attacker.equipWeapon).FirstOrDefault().name + "!");
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
                                //Debug.Log(target.playerName + " get exp " + amountOfDamage + "!");
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

                //Debug.Log(attacker.playerName + " heal " + target.playerName + " for " + amountOfHeal + " HP point!");

                if (isPlayerTurn)
                {
                    attacker.exp += amountOfHeal;
                    getExp = amountOfHeal;
                    //Debug.Log(attacker.playerName + " get exp " + amountOfHeal + "!");
                }
            }

            //level up
            PlayerRecord lvUpProp = null;
            if (isPlayerTurn && attacker.hp > 0 && (attacker.exp / 100) == attacker.level)
            {
                lvUpProp = attacker.LevelUp();
                level = attacker.level;
                playerData = attacker.GetPlayerProp();
                //Debug.Log(attacker.playerName + " is level up to " + attacker.level + "!");

            }
            else if (!isPlayerTurn && target.hp > 0 && (target.exp / 100) == target.level)
            {
                lvUpProp = target.LevelUp();
                level = target.level;
                playerData = target.GetPlayerProp();
                //Debug.Log(target.playerName + " is level up to " + target.level + "!");
            }

            //send to battle scene
            isWaitingBattle = true;

            battleData = new BattleSendData(attackerName, targetName, backGround, attackerTileName, targetTileName, isPlayerAttack, isHeal, isCounter, isDirectAtk, attackerDefensRate, targetDefensRate, attackerMaxHP, attackerHP, damageByAttacker, targetMaxHP, targetHP, damageByTarget, getItem, playerExp, getExp, level, playerClass, playerData, lvUpProp);
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(battleData));
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
            highlightTiles = HexTileHighlight.FindHighlight(getMapTile(new HexTile.HexCoord((int)originLocation.x, (int)originLocation.y)), distance, true);
        }
        else
        {
            highlightTiles = HexTileHighlight.FindHighlight(getMapTile(new HexTile.HexCoord((int)originLocation.x, (int)originLocation.y)), distance, userPlayers.Union(enemyPlayers)/*.Where(x => x.gridPosition != originLocation)*/.Select(x => x.gridPosition).ToArray());
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
        m_PlayerGold += gold;
        if (itemId >= 0)
        {
            if (m_PlayerItems.ContainsKey(itemId))
            {
                m_PlayerItems[itemId]++;
            }
            else
            {
                m_PlayerItems.Add(itemId, 1);
            }
        }

        if (weaponId >= 0)
        {
            if (m_PlayerWeapons.ContainsKey(weaponId))
            {
                m_PlayerWeapons[weaponId]++;
            }
            else
            {
                m_PlayerWeapons.Add(weaponId, 1);
            }
        }

        ShowGetItemInfo(gold, itemId, weaponId);
    }

    #region Item

    public Player GetSelectedPlayer()
    {
        if (playerIndex >= 0)
        {
            return userPlayers[playerIndex];
        }
        return null;
    }
    public Item GetItem(int id)
    {
        return gameElement.m_Items[id];
    }
    public Weapon GetWeapon(int id)
    {
        return gameElement.m_Weapons[id];
    }
    public CharacterType GetRace(int id)
    {
        return gameElement.m_Races[id];
    }
    public string[] GetEquipableRace(int id, out bool isEquipable)
    {
        isEquipable = false;
        List<string> raceNames = new List<string>();
        for (int i = 0; i < gameElement.races.Count; i++)
        {
            if (gameElement.races[i].equipWeapon.Equals(id))
            {
                raceNames.Add(gameElement.races[i].name);
                if (userPlayers[playerIndex].race.Equals(gameElement.races[i].id))
                {
                    isEquipable = true;
                }
            }
        }

        return raceNames.ToArray();
    }

    private void IncreaseItem(int id)
    {
        if (m_PlayerItems.ContainsKey(id))
        {
            m_PlayerItems[id]++;
        }
        else
        {
            m_PlayerItems.Add(id, 1);
        }
    }
    private bool DecreaseItem(int id)
    {
        if (m_PlayerItems.ContainsKey(id) && m_PlayerItems[id] > 0)
        {
            int newCount = m_PlayerItems[id] - 1;
            m_PlayerItems[id] = newCount;
            return true;
        }
        return false;
    }
    public bool SellItem(int id)
    {
        Item selectItem = gameElement.m_Items[id];
        if (selectItem != null && DecreaseItem(id))
        {
            m_PlayerGold += selectItem.price;
            return true;
        }
        return false;
    }
    public bool UseItem(int itemId)
    {
        Item selectItem = gameElement.m_Items[itemId];
        if (selectItem != null && !selectItem.m_IsKeyItem && DecreaseItem(itemId))
        {
            if (selectItem.m_FullCure)
            {
                for (int i = 0; i < userPlayers.Count; i++)
                {
                    userPlayers[i].hp = userPlayers[i].maxHP;
                }
            }
            else
            {
                int hp = selectItem.hp;
                userPlayers[playerIndex].atk += selectItem.atk;
                userPlayers[playerIndex].def += selectItem.def;
                userPlayers[playerIndex].dex += selectItem.dex;
                userPlayers[playerIndex].wis += selectItem.wis;
                userPlayers[playerIndex].maxHP += selectItem.addHp;
                userPlayers[playerIndex].hp = Mathf.Min(userPlayers[playerIndex].hp + hp, userPlayers[playerIndex].maxHP);
                m_PlayerGold += selectItem.gold;

                if (selectItem.useCharType > 0)
                {
                    userPlayers[playerIndex].race = selectItem.newCharType;
                    userPlayers[playerIndex].level = 1;
                    userPlayers[playerIndex].exp = 0;
                }
                ShowUseItemInfo(userPlayers[playerIndex].playerName, hp, selectItem.atk, selectItem.def, selectItem.dex, selectItem.wis, selectItem.addHp, selectItem.gold, (selectItem.useCharType > 0 ? gameElement.races.Where(x => x.id == selectItem.newCharType).FirstOrDefault().name : ""));
            }
            return true;
        }
        return false;
    }

    public bool EquipWeapon(int id)
    {
        Player player = GetSelectedPlayer();
        if (player == null || player.equipWeapon.Equals(id))
        {
            return false;
        }
        Weapon selectedWeapon = GetWeapon(id);
        Weapon equipedWeapon = GetWeapon(player.equipWeapon);

        player.equipWeapon = selectedWeapon.id;
        DecreaseWeapon(selectedWeapon.id);
        IncreaseWeapon(equipedWeapon.id);

        ShowEquipWeaponInfo(selectedWeapon.name, selectedWeapon.name);
        return true;
    }
    public bool SellWeapon(int id)
    {
        Weapon selectedWeapon = GetWeapon(id);
        if (selectedWeapon != null && DecreaseWeapon(id))
        {
            m_PlayerGold += selectedWeapon.price;
            return true;
        }
        return false;
    }
    private void IncreaseWeapon(int id)
    {
        if (m_PlayerWeapons.ContainsKey(id))
        {
            m_PlayerWeapons[id]++;
        }
        else
        {
            m_PlayerWeapons.Add(id, 1);
        }
    }
    private bool DecreaseWeapon(int id)
    {
        if (m_PlayerWeapons.ContainsKey(id) && m_PlayerWeapons[id] > 0)
        {
            int newCount = m_PlayerWeapons[id] - 1;
            m_PlayerWeapons[id] = newCount;
            return true;
        }
        return false;
    }

    public bool BuyItem(int id, bool isWeapon)
    {
        if (isWeapon)
        {
            Weapon selectWeapon = GetWeapon(id);
            if (selectWeapon != null)
            {
                IncreaseWeapon(selectWeapon.id);
                m_PlayerGold -= selectWeapon.price;
                return true;
            }

        }
        else
        {
            Item selectItem = GetItem(id);
            if (selectItem != null)
            {
                IncreaseItem(selectItem.id);
                m_PlayerGold -= selectItem.price;
                return true;
            }
        }
        return false;
    }
    #endregion

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
    }

    public TextAsset m_ObjectData;

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
            //gameElement = ObjectSaveLoad.JsonLoad<GameElement>(gameElementfilename);
            gameElement = ObjectSaveLoad.JsonDataLoad<GameElement>(m_ObjectData.text);

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

        MapXmlContainer container = ObjectSaveLoad.JsonLoad<MapXmlContainer>(Application.dataPath + "/Resources/test.txt");
        mapSizeX = container.sizeX;
        mapSizeY = container.sizeY;

        m_ShopItemList = container.shopItemList;
        m_ShopWeaponList = container.shopWeaponList;

        for (int i = 0; i < mapTransform.transform.childCount; i++)
        {
            Destroy(mapTransform.transform.GetChild(i).gameObject);
        }

        Vector3 pos = Vector3.zero;
        map = new List<List<Tile>>();
        mapHex = new List<List<HexTile>>();

        userPlayerRecords = container.userPlayerRecords;
        enemyPlayerRecords = container.enemyPlayerRecords;
        stageScenatios = container.scenarioList;
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

    public void ClickButtonAction(string clickButton)
    {
        buttonAction = (ButtonAction)Delegate.CreateDelegate(typeof(ButtonAction), this, clickButton);
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
            Vector3 oriPos = getMapTile(userPlayers[inputPlayerIndex].mapHexIndex).transform.position;
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
            if (p.hp > 0 && getMapTile(p.mapHexIndex).type2D == TileType2D.Villa)
            {
                p.hp += (p.maxHP / 8);
                if (p.hp > p.maxHP)
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
        m_GameUIManager.ShowStatusUI();
    }

    private void Item(int playerIndex)
    {
        m_GameUIManager.ShowItemUI();
    }

    private void Weapon(int playerIndex)
    {
        m_GameUIManager.ShowWeaponUI();
    }

    private void Unit(int playerIndex)
    {
        m_GameUIManager.ShowUnitListUI();
    }

    private void Setting(int playerIndex)
    {
        gameSetting.alpha = 1;
        gameSetting.interactable = true;
        gameSetting.blocksRaycasts = true;
    }

    private void Shop(int playerIndex)
    {
        m_GameUIManager.ShowShopUI();
    }

    public void SetShopDialog()
    {
        runningScenario = stageScenatios.Where(x => x.scenarioType == ScenarioType.Event && x.scenarioConditionType == ScenarioConditionType.AfterShop).FirstOrDefault();
        _Update = ScenarioMode;
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
        userPlayers = new List<Player>();
        enemyPlayers = new List<Player>();
        for (int i = 0; i < userPlayerRecords.Count; i++)
        {
            UserPlayer player;
            //int x = userPlayerRecords[i].locX + (userPlayerRecords[i].locY >> 1);
            //int y = userPlayerRecords[i].locY;
            Vector3 tilePos = getMapTile(new HexTile.HexCoord(userPlayerRecords[i].locX, userPlayerRecords[i].locY)).HexTilePos();
            CharacterTemplate playerData = playerTypes.Where(t => t.id == userPlayerRecords[i].characterId).FirstOrDefault();
            CharacterLevelTemplate playerLvData = playerData.levelData[0];
            PlayerRecord record = null;
            if (saveUserPlayerRecords != null)
            {
                record = saveUserPlayerRecords.Where(t => t.characterId == userPlayerRecords[i].characterId).FirstOrDefault();
            }
            //player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
            player = Instantiate(PlayerPrefabHolder.instance.userPlayer_prefab, new Vector3(tilePos.x, playerHeight, tilePos.z), Quaternion.Euler(new Vector3(0, 180, 0)), playerTransform).GetComponent<UserPlayer>();
            player.gameObject.name = string.Format(userPlayerNameFormat, i);
            //player.transform.SetParent(playerTransform);

            player.gridPosition = new Vector2(userPlayerRecords[i].locX, userPlayerRecords[i].locY);

            player.playerName = playerData.name;
            player.race = playerData.race;
            player.movementPerActionPoint = playerData.move;
            player.SetPlayerModel();
            PlayerUI playerUI = (Instantiate(playerUIPrefab, new Vector3(tilePos.x, 1, tilePos.z), playerUIPrefab.transform.rotation, playerUITransform)).GetComponent<PlayerUI>();
            playerUI.player = player;
            playerUI.gameObject.name = string.Format(userPlayerNameFormat + "UI", i);
            //playerUI.transform.SetParent(playerUITransform);

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
                tilePos = getMapTile(new HexTile.HexCoord(record.locX, record.locY)).HexTilePos();
                player.transform.position = new Vector3(tilePos.x, playerHeight, tilePos.z);
                player.gridPosition = new Vector2(record.locX, record.locY);
                player.isActable = record.isActable;
                player.level = (int)record.level;
                player.exp = (int)record.exp;
                player.maxHP = (int)record.hp;
                player.hp = (int)record.currentHp;
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
            if (defeatedEnemyList.Contains(i))
            {
                continue;
            }

            AIPlayer player;
            //int x = enemyPlayerRecords[i].locX + (enemyPlayerRecords[i].locY >> 1);
            //int y = enemyPlayerRecords[i].locY;
            Vector3 tilePos = getMapTile(new HexTile.HexCoord(enemyPlayerRecords[i].locX, enemyPlayerRecords[i].locY)).HexTilePos();
            CharacterTemplate playerData = enemyTypes.Where(t => t.id == enemyPlayerRecords[i].characterId).FirstOrDefault();
            CharacterLevelTemplate playerLvData = playerData.levelData.Where(t => t.id == enemyPlayerRecords[i].levelId).FirstOrDefault();
            PlayerRecord record = null;
            if (saveEnemyPlayerRecords != null)
            {
                record = saveEnemyPlayerRecords.Where(t => t.id == i).FirstOrDefault();
            }
            //player = ((GameObject)Instantiate(userPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
            player = ((GameObject)Instantiate(PlayerPrefabHolder.instance.enemyPlayer_prefab, new Vector3(tilePos.x, playerHeight, tilePos.z), Quaternion.Euler(new Vector3(0, 180, 0)), playerTransform)).GetComponent<AIPlayer>();
            player.gameObject.name = string.Format(enemyPlayerNameFormat, i);
            //player.transform.SetParent(playerTransform);

            player.gridPosition = new Vector2(enemyPlayerRecords[i].locX, enemyPlayerRecords[i].locY);

            player.playerName = playerData.name;
            player.race = playerData.race;
            player.movementPerActionPoint = playerData.move;

            player.level = (int)playerLvData.level;
            player.exp = (int)playerLvData.exp;
            player.hp = player.maxHP = (int)playerLvData.hp;
            player.gold = (int)playerLvData.gold;
            player.equipWeapon = playerLvData.equipWeapon;
            player.enemyAIType = enemyPlayerRecords[i].aiType;
            player.searchRange = enemyPlayerRecords[i].searchRange;
            PlayerUI playerUI = (Instantiate(playerUIPrefab, new Vector3(tilePos.x, 1, tilePos.z), playerUIPrefab.transform.rotation, playerUITransform)).GetComponent<PlayerUI>();
            playerUI.player = player;
            playerUI.gameObject.name = string.Format(enemyPlayerNameFormat + "UI", i);
            //playerUI.transform.SetParent(playerUITransform);

            if (record == null)
            {
                player.atk = (int)playerLvData.atk;
                player.def = (int)playerLvData.def;
                player.wis = (int)playerLvData.wis;
                player.dex = (int)playerLvData.dex;
                player.mdef = (int)playerLvData.mdef;
            }
            else
            {
                tilePos = getMapTile(new HexTile.HexCoord(record.locX, record.locY)).HexTilePos();
                player.transform.position = new Vector3(tilePos.x, playerHeight, tilePos.z);
                player.gridPosition = new Vector2(record.locX, record.locY);
                player.isActable = record.isActable;
                player.hp = (int)record.currentHp;
                player.atk = (int)record.atk;
                player.def = (int)record.def;
                player.wis = (int)record.wis;
                player.dex = (int)record.dex;
                player.mdef = (int)record.mdef;
            }
            player.playerIndex = i;
            player.SetPlayerModel();

            enemyPlayers.Add(player);
        }

        ScreenController.instance.SetPlayerUIs();
    }

    #region Map

    private HexTile getMapTile(HexTile.HexCoord hex)
    {
        return mapHex[hex.r][hex.q + (hex.r >> 1)];
    }

    private HexTile getMapTile(Vector2 hex, bool isIndex = true)
    {
        if (isIndex)
        {
            return mapHex[(int)hex.y][(int)hex.x];
        }
        return mapHex[(int)hex.y][(int)hex.x + (((int)hex.y) >> 1)];
    }

    #endregion
}
