using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager m_Instance { get; private set; }

    [Header("Sub system")]
    [SerializeField]
    private PlayerManager m_PlayerManager = default;
    [SerializeField]
    private GameUIManager m_GameUIManager = default;

    #region parament

    [Header("Parament")]
    Transform actorPlayerTransform;

    [SerializeField]
    public float _MoveSpeed = 10f;

    public float m_MoveSpeed { get { return _MoveSpeed; } }

    public GameObject playerUIPrefab;
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

    public Dictionary<int, Player> userPlayers { get { return m_PlayerManager.m_DicUserPlayers; } }
    public Dictionary<int, Player> enemyPlayers { get { return m_PlayerManager.m_DicEnemyPlayers; } }
    public List<Player> actorPlayers { get; private set; }
    public List<Scenarion> stageScenatios = new List<Scenarion>();
    public Scenarion runningScenario;
    public List<int> m_ShopItemList { get; private set; }
    public List<int> m_ShopWeaponList { get; private set; }
    public List<int> defeatedEnemyList;
    public List<int> removeScenaroList;

    public GameElement gameElement { get; private set; }
    public List<CharacterTemplate> playerTypes { get; private set; }
    private Dictionary<int, CharacterTemplate> m_DicPlayerTypes;
    public List<CharacterTemplate> enemyTypes { get; private set; }
    private Dictionary<int, CharacterTemplate> m_DicEnemyTypes;
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

    public Vector3 cameraPosition;
    public Vector3 cameraRotation;

    [HideInInspector]
    public bool moving = false;
    [HideInInspector]
    public bool attacking = false;

    [SerializeField]
    private int m_PlayerGoldMax = 60000;
    public int _playerGold = 1000;

    [SerializeField]
    private int m_PlayerLevelMax = 20;
    [SerializeField]
    private int m_PlayerStateMax = 45;

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
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        m_GameUIManager.Initialize(m_Instance);
        m_PlayerManager.Initialize(m_Instance);
        actorPlayerTransform = transform.Find("ActorPlayers");
    }

    private void Start()
    {
        //GameLoading();
        //m_StageMapManager.GameSetting();
        //m_StageManager.GameSetting();
        //m_PlayerManager.GameSetting();

        //m_GameUIManager.GameSetting();

        if (isLoadMap)
        {
            //m_IsNewGame = true;
            //if (loadIndex != -1)
            //{
            //    //Load game save
            //    m_IsNewGame = false;
            //}
            GameLoading();
        }
        loadIndex = -1;
        isLoadMap = false;
        PlayerPrefs.SetInt("loadIndex", -1);
        PlayerPrefs.SetInt("isLoadMap", 0);
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
                    ScreenController.instance.SetCameraPos(moveCaremaPos);
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
            if (GameMidiator.m_Instance.m_StageManager.StageClearCheck())
            {
                //Win
                Scenarion temp = stageScenatios.Where(x => x.scenarionType == ScenarioType.StageClear).FirstOrDefault();
                if (temp != null)
                {
                    runningScenario = temp;
                    _Update = ScenarioMode;
                }
                else
                {
                    Debug.Log("No Clear Scenario");
                }
            }

            //for (int i = 0; i < stageClearConditions.Count; i++)
            //{
            //    switch (stageClearConditions[i].stageClearConditionType)
            //    {
            //        case StageClearConditionType.EnemyDead:
            //            int deadCount = 0;
            //            int deadCondition = stageClearConditions[i].enemyDeadCount;
            //            if (stageClearConditions[i].enemyDeadCount == 0 || stageClearConditions[i].enemyDeadCount > stageClearConditions[i].enemyDeadList.Count)
            //            {
            //                deadCondition = stageClearConditions[i].enemyDeadList.Count;
            //            }
            //            List<int> deatList = stageClearConditions[i].enemyDeadList;
            //            for (int j = 0; j < deatList.Count; j++)
            //            {
            //                Player tempPlayer = enemyPlayers[deatList[j]] ?? null;
            //                if (tempPlayer == null || tempPlayer.hp <= 0)
            //                {
            //                    deadCount++;
            //                }
            //            }
            //            if (deadCount >= deadCondition)
            //            {
            //                isWin = true;
            //                _Update = EndMode;
            //            }
            //            break;
            //        case StageClearConditionType.SpecifyTile:
            //            if (userPlayers.Values.Where(x => x.m_Hex == stageClearConditions[i].specifyTile).Count() > 0)
            //            {
            //                isWin = true;
            //                _Update = EndMode;
            //            }
            //            break;
            //    }
            //    if (isWin)
            //    {
            //        //Win
            //        Scenario temp = stageScenatios.Where(x => x.scenarioType == ScenarioType.StageClear).FirstOrDefault();
            //        if (temp != null)
            //        {
            //            runningScenario = temp;
            //            _Update = ScenarioMode;
            //        }
            //        else
            //        {
            //            Debug.Log("No Clear Scenario");
            //        }
            //        break;
            //    }
            //}

            if (!isPlayerTurn)
            {
                if (m_PlayerManager.CheckUserPlayerAlive() && m_PlayerManager.CheckEnemyPlayerAlive())
                {
                    if (enemyPlayers[currentEnemyPlayerIndex].m_Hp > 0)
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
            if (runningScenario.scenarionActionStep < runningScenario.scenarionActions.Count)
            {
                //play action
                if (runningScenario.scenarionType == ScenarioType.Openning)
                {
                    //Hide all players
                    m_PlayerManager.HidePlayers();
                }

                switch (runningScenario.scenarionActions[runningScenario.scenarionActionStep].scenarioActionType)
                {
                    case ScenarioActionType.Dialog:
                        //Show dialog
                        EnableGroup(dialogGroup);
                        dialogName.text = runningScenario.scenarionActions[runningScenario.scenarionActionStep].dialogName;
                        dialogText.text = runningScenario.scenarionActions[runningScenario.scenarionActionStep].dialogText;
                        isWaitingActor = true;
                        break;
                    case ScenarioActionType.CreateActor:
                        for (int i = 0; i < runningScenario.scenarionActions[runningScenario.scenarionActionStep].createActors.Count; i++)
                        {
                            PlayerRecord temp = runningScenario.scenarionActions[runningScenario.scenarionActionStep].createActors[i];
                            Vector3 pos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(temp.locX, temp.locY).HexTilePos();
                            ActorPlayer player = Instantiate(PlayerPrefabHolder.instance.m_ActorPlayerPrefab, pos, Quaternion.identity, actorPlayerTransform).GetComponent<ActorPlayer>();
                            //player.gridPosition = new Vector2(temp.locX, temp.locY);
                            player.playerIndex = temp.id;
                            player.gameObject.name = temp.id.ToString();
                            player.SetPlayerValue(temp, null);
                            player.SetPivot(temp.scenarioActorPivotType);
                            player.SetPlayerModel();
                            actorPlayers.Add(player);
                        }
                        break;
                    case ScenarioActionType.ControlActor:
                        Player tempPlayer = actorPlayers.Where(x => x.playerIndex == runningScenario.scenarionActions[runningScenario.scenarionActionStep].actorId).FirstOrDefault();
                        HexTile targetTile = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(runningScenario.scenarionActions[runningScenario.scenarionActionStep].targetMoveTile);
                        if (GameMidiator.m_Instance.m_StageMapManager.GetMapTile(tempPlayer.m_Hex) != targetTile)
                        {
                            List<HexTile> path = HexTilePathFinder.FindPath(GameMidiator.m_Instance.m_StageMapManager.GetMapTile(tempPlayer.m_Hex), targetTile, true).listOfTiles;
                            tempPlayer.SetPositionQueue(GameMidiator.m_Instance.m_StageMapManager.GetPathPosition(path));
                            isWaitingActor = true;
                        }
                        tempPlayer.SetHexTarget(targetTile.m_Hex);
                        //tempPlayer.m_PlayerPivot = runningScenario.scenarionActions[runningScenario.scenarionActionStep].actorPivot;
                        tempPlayer.SetPivot(runningScenario.scenarionActions[runningScenario.scenarionActionStep].actorPivot);
                        tempPlayer.SetPivot();
                        break;
                    case ScenarioActionType.SetCamera:
                        Vector3 tilePos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(runningScenario.scenarionActions[runningScenario.scenarionActionStep].setCameraPos).HexTilePos();
                        ScreenController.instance.SetCameraPos(new Vector3(tilePos.x, 0, tilePos.z));
                        break;
                    case ScenarioActionType.ControlCamera:
                        moveCaremaPos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(runningScenario.scenarionActions[runningScenario.scenarionActionStep].targetMoveTile).HexTilePos();
                        isMoveCamera = true;
                        isWaitingActor = true;
                        break;
                    case ScenarioActionType.AddUserPlayer:
                        break;
                    case ScenarioActionType.AddEnemyPlayer:
                        break;
                }
                currentWaitingTime = 0;
                isToDark = runningScenario.scenarionActions[runningScenario.scenarionActionStep].isToDark;
                waitingTime = runningScenario.scenarionActions[runningScenario.scenarionActionStep].waitTime;
                runningScenario.scenarionActionStep++;
            }
            else
            {
                //Scenario is end
                SetStopWaiting();
                EnableGroup(mapController);
                if (runningScenario.scenarionType == ScenarioType.Openning)
                {
                    m_PlayerManager.ShowPlayers();
                    HideActor();
                    ClearActorPlayer();
                    Player temp = userPlayers.Values.Where(x => x.playerIndex == 0).FirstOrDefault();
                    ScreenController.instance.SetCameraPos(new Vector3(temp.transform.position.x, 0, temp.transform.position.z));
                    ShowStageInfo(false);
                }

                if (runningScenario.scenarionConditionType == ScenarioConditionType.BeforeBattle)
                {
                    AttackWithCurrentPlayer(runningScenario.battleAfterEvent);
                }
                if (runningScenario.isOnceEvent)
                {
                    removeScenaroList.Add(runningScenario.scenarionId);
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

    int loadIndex;
    bool isLoadMap = false;

    private void OnLevelWasLoaded(int level)
    {
        gameSceneUI.SetActive(false);
        if (level == 1)
        {
            isLoadMap = false;
            try
            {
                loadIndex = PlayerPrefs.GetInt("loadIndex");
                isLoadMap = PlayerPrefs.GetInt("isLoadMap") == 1;
            }
            catch
            {
                loadIndex = -1;
            }
            //if (isLoadMap)
            //{
            //    m_IsNewGame = true;
            //    if (loadIndex != -1)
            //    {
            //        //Load game save
            //        m_IsNewGame = false;
            //    }
            //    GameLoading();
            //}
            //loadIndex = -1;
            //isLoadMap = false;
            //PlayerPrefs.SetInt("loadIndex", -1);
            //PlayerPrefs.SetInt("isLoadMap", 0);
            //ScreenController.instance.SetCameraPos(cameraPosition);
            //ScreenController.instance.SetCameraRot(cameraRotation);

            gameSceneUI.SetActive(true);
            isWaitingBattle = false;

            if (userPlayers != null && userPlayers.Values.Where(x => gameElement.races.Where(y => y.name == "ロード").FirstOrDefault().id == x.m_Race && x.m_Hp < 0).Count() > 0)
            {
                isLose = true;
                _Update = EndMode;
                //Game Over
            }
        }
    }

    private SaveDataStore m_SaveData;

    private void InitialStage()
    {
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

        m_PlayerManager.ClearPlayer();
        ClearActorPlayer();

        LoadGameElements();
        SetStopWaiting();
        HideStageInfo();

        DisableGroup(menu);
        DisableGroup(endTurnConfirm);
        DisableGroup(gameSetting);
        DisableGroup(dialogGroup);
        isShowStage = true;
    }

    private void StartGame()
    {
        //int stageIndex = m_SaveData.m_StageIndex;
        if (GameMidiator.m_Instance.m_StageManager.LoadStageData(m_SaveData))
        {
            //mapSizeX = m_StageManager.m_Container.sizeX;
            //mapSizeY = m_StageManager.m_Container.sizeY;
            //GameMidiator.m_Instance.m_StageMapManager.CreateStageMap(GameMidiator.m_Instance.m_StageManager.m_Container);

            //userPlayerRecords = GameMidiator.m_Instance.m_StageManager.GetUserPlayerRecords(m_SaveData.m_UserPlayerRecords, m_SaveData.m_StagePlayerRecords);
            //enemyPlayerRecords = GameMidiator.m_Instance.m_StageManager.GetEnemyPlayerRecords(m_SaveData.m_StageEnemyRecords);
            //stageScenatios = GameMidiator.m_Instance.m_StageManager.m_Container.scenarioList;

            //GameMidiator.m_Instance.m_PlayerManager.GenetarePlayers(userPlayerRecords);
            //GameMidiator.m_Instance.m_PlayerManager.GenetarePlayers(enemyPlayerRecords);
        }
    }

    private void GameLoading()
    {
        ScreenController.instance.SetCameraPos(cameraPosition);
        ScreenController.instance.SetCameraRot(cameraRotation);

        isWaitingBattle = false;
        InitialStage();

        int storeIndex = -1;
        bool isLoad = false;
        if (isLoad)
        {
            m_SaveData = SaveManager.GetSaveDataStore(storeIndex);
        }
        else
        {
            m_SaveData = SaveManager.GetSaveDataStore();
        }

        m_TurnCount = m_SaveData.m_TurnCount;
        //saveUserPlayerRecords = m_SaveData.m_StagePlayerRecords;
        //saveEnemyPlayerRecords = m_SaveData.m_StageEnemyRecords;
        defeatedEnemyList = m_SaveData.m_DefeatedEnemyList;
        m_PlayerItems = m_SaveData.m_PlayerItems;
        m_PlayerWeapons = m_SaveData.m_PlayerWeapons;

        if (m_SaveData.m_IsEmpty)
        {
            m_SaveData.m_IsEmpty = false;
            m_SaveData.m_Gold = 1000;
            //Begin items
            for (int i = 0; i < m_PlayerItems.Count; i++)
            {
                m_PlayerItems.Add(gameElement.items[i].id, gameElement.items[i].initialCount);
            }
            //Begin characters value
            for (int i = 0; i < gameElement.characters.Count; i++)
            {
                if (!gameElement.characters[i].enemy)
                {
                    m_SaveData.m_UserPlayerRecords.Add(gameElement.characters[i].id, new PlayerRecord(gameElement.characters[i]));
                }
            }
        }
        StartGame();
        Scenarion opening = stageScenatios.Where(x => x.scenarionType == ScenarioType.Openning).FirstOrDefault();

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
        CharacterType race = gameElement.races[player.m_Race];
        Weapon weapon = gameElement.weapons[player.m_EquipWeapon];

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
            //Vector2 index = enemyPlayers[currentEnemyPlayerIndex].mapHexIndex;
            Vector3 pos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(enemyPlayers[currentEnemyPlayerIndex].m_Hex).transform.position;
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
            foreach (Player p in userPlayers.Values)
            {
                p.TurnActive();
                if (p.m_Hp > 0 && GameMidiator.m_Instance.m_StageMapManager.GetMapTile(p.m_Hex).m_TileType2D == TileType2D.Villa)
                {
                    p.m_Hp += (p.m_MaxHP / 8);
                    if (p.m_Hp > p.m_MaxHP)
                    {
                        p.m_Hp = p.m_MaxHP;
                    }
                }
            }
            //Vector2 index = userPlayers[0].mapHexIndex;
            Vector3 pos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(userPlayers[0].m_Hex).HexTilePos(); ;
            ScreenController.instance.SetCameraPos(pos);
            //isSetCamera = false;

            m_PlayerManager.RemoveDeadEnemy();
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
        if (destTile.m_IsHighLight && !destTile.m_Impassible)
        {
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
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
            //List<Vector3> positions = new List<Vector3>();
            //List<HexTile> path = HexTilePathFinder.FindPath(GameMidiator.m_Instance.m_StageMapManager.GetMapTile(targetPlayer.m_Hex), destTile, GameMidiator.m_Instance.m_PlayerManager.GetPlayerHexes(targetPlayer.m_Hex).ToArray()).listOfTiles;
            //foreach (HexTile t in HexTilePathFinder.FindPath(m_StageMapManager.GetMapTile(targetPlayer.m_Hex), destTile, m_PlayerManager.GetPlayerHexes(targetPlayer.m_Hex).ToArray()).listOfTiles)
            //{
            //    positions.Add(m_StageMapManager.GetMapTile(t.m_Hex).transform.position + playerHeight * Vector3.up);
            //    //targetPlayer.positionQueue.Add(m_StageMapManager.GetMapTile(t.m_Hex).transform.position + playerHeight * Vector3.up);
            //    //Debug.Log(players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].x + "," + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].z);
            //}
            //targetPlayer.SetPositionQueue(GameMidiator.m_Instance.m_StageMapManager.GetPathPosition(path));

            targetPlayer.SetPositionQueue(GameMidiator.m_Instance.m_StageMapManager.FindPath(targetPlayer.m_Hex, destTile.m_Hex, GameMidiator.m_Instance.m_PlayerManager.GetPlayerHexes(targetPlayer.m_Hex).ToArray()));
            targetPlayer.SetHexTarget(destTile.m_Hex);
            //targetPlayer.gridPosition = destTile.m_GridPosition;
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
            isShowAction = player.GetHealRange().Where(x => m_Instance.userPlayers.Values.Where(y => y.m_Hp < y.m_MaxHP && y.m_Hex == x.m_Hex).Count() > 0).Count() > 0;
        }
        isShowAction = isShowAction || player.GetAttackRange().Where(x => m_Instance.enemyPlayers.Values.Where(y => y.m_Hp > 0 && y.m_Hex == x.m_Hex).Count() > 0).Count() > 0;
        if (isShowAction)
        {
            setType = MenuType.playerMoveCanAtkMenu;
        }
        ShowMenu();
        Vector3 pos = Camera.main.WorldToScreenPoint(player.HexTilePos());
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
        defRate = (int)destTile.m_DefenseRate;
        switch (destTile.m_TileType2D)
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
        if (!destTile.m_IsHighLight && !destTile.m_Impassible)
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
            attackerName = attacker.m_PlayerName;
            attackerMaxHP = attacker.m_MaxHP;
            attackerHP = attacker.m_Hp;
            playerExp = attacker.m_Exp;
            playerClass = gameElement.races.Where(x => x.id == attacker.m_Race).FirstOrDefault().name;
            target = enemyPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
            if (target == null)
            {
                target = userPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
                isHeal = true;
            }
            targetName = target.m_PlayerName;
            targetMaxHP = target.m_MaxHP;
            targetHP = target.m_Hp;
            SetTileName(destTile, out targetTileName, out targetDefensRate);
            Scenarion temp = stageScenatios.Where(x => x.scenarionType == ScenarioType.Event && x.scenarionConditionType == ScenarioConditionType.BeforeBattle && (x.userPlayer == -1 || x.userPlayer == attacker.playerIndex) && (x.enemyPlayer == -1 || x.enemyPlayer == target.playerIndex)).FirstOrDefault();
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
            targetName = attacker.m_PlayerName;
            targetMaxHP = attacker.m_MaxHP;
            targetHP = attacker.m_Hp;
            target = userPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
            if (target == null)
            {
                target = enemyPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
                isHeal = true;
            }
            attackerName = target.m_PlayerName;
            attackerMaxHP = target.m_MaxHP;
            attackerHP = target.m_Hp;
            playerExp = target.m_Exp;
            playerClass = gameElement.races.Where(x => x.id == target.m_Race).FirstOrDefault().name;
            SetTileName(destTile, out attackerTileName, out attackerDefensRate);

            Scenarion temp = stageScenatios.Where(x => x.scenarionType == ScenarioType.Event && x.scenarionConditionType == ScenarioConditionType.BeforeBattle && (x.enemyPlayer == -1 || x.enemyPlayer == attacker.playerIndex) && (x.userPlayer == -1 || x.userPlayer == target.playerIndex)).FirstOrDefault();
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
            HexTile targetTile = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(attacker.m_Hex);
            bool isDirectAtk = HexTile.Distance(attacker.m_Hex, target.m_Hex) == 1;

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
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
            attacking = false;
            moving = false;

            if (!isHeal)
            {
                //attack
                int amountOfDamage = Mathf.FloorToInt((attacker.m_Atk + (isDirectAtk ? directAtk : indirectAtk) - target.m_Def) * (1f - (destTile.m_DefenseRate / 100f)));
                target.m_Hp -= amountOfDamage;

                //Debug.Log(attacker.playerName + (isDirectAtk ? " direct attack " : " indirect attack ") + target.playerName + " for " + (amountOfDamage + (target.hp < 0 ? target.hp : 0)) + " damage!");

                damageByTarget = (amountOfDamage + (target.m_Hp < 0 ? target.m_Hp : 0));
                //Target dead, user player get exp
                if (target.m_Hp <= 0)
                {
                    if (isPlayerTurn)
                    {
                        target.gridPosition = new Vector2(-1, -1);
                        damageByAttacker = (amountOfDamage + (target.m_Hp < 0 ? target.m_Hp : 0));
                        attacker.m_Exp += target.m_Exp;
                        getExp = target.m_Exp;
                        Scenarion temp = stageScenatios.Where(x => x.scenarionConditionType == ScenarioConditionType.AfterBattle && x.enemyPlayer == target.playerIndex).FirstOrDefault();
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
                        m_PlayerGold += target.m_Gold;
                        getItem = "<color=yellow>" + target.m_Gold + "</color>Gold";
                        //Debug.Log("Got " + target.gold + " gold!");
                    }
                    else
                    {
                        if (m_PlayerWeapons.ContainsKey(target.m_EquipWeapon))
                        {
                            m_PlayerWeapons[target.m_EquipWeapon]++;
                        }
                        else
                        {
                            m_PlayerWeapons.Add(target.m_EquipWeapon, 1);
                        }
                        getItem = gameElement.weapons.Where(x => x.id == target.m_EquipWeapon).FirstOrDefault().name;
                        //Debug.Log("Got " + gameElement.weapons.Where(x => x.id == target.equipWeapon).FirstOrDefault().name + "!");
                    }
                }
                else
                {
                    //Target not dead, user player get exp
                    if (isPlayerTurn)
                    {
                        attacker.m_Exp += amountOfDamage;
                        damageByAttacker = (amountOfDamage + (target.m_Hp < 0 ? target.m_Hp : 0));
                        getExp = amountOfDamage;
                        //Debug.Log(attacker.playerName + " get exp " + amountOfDamage + "!");
                    }

                    //Counter
                    if (isDirectAtk && target.GetIsCanAttack(true) || !isDirectAtk && target.GetIsCanAttack(false))
                    {
                        isCounter = true;
                        target.GetWeaponAttack(out directAtk, out indirectAtk);

                        amountOfDamage = Mathf.FloorToInt((target.m_Atk + (isDirectAtk ? directAtk : indirectAtk) - attacker.m_Def) * (1f - (targetTile.m_DefenseRate / 100f)));
                        attacker.m_Hp -= amountOfDamage;

                        //Debug.Log(target.playerName + " counter attack " + attacker.playerName + " for " + (amountOfDamage + (attacker.hp < 0 ? attacker.hp : 0)) + " damage!");
                        //Attacker dead, user player get exp
                        if (attacker.m_Hp <= 0)
                        {
                            if (!isPlayerTurn)
                            {
                                attacker.gridPosition = new Vector2(-1, -1);
                                damageByAttacker = (amountOfDamage + (attacker.m_Hp < 0 ? attacker.m_Hp : 0));
                                target.m_Exp += attacker.m_Exp;
                                getExp = attacker.m_Exp;
                                Scenarion temp = stageScenatios.Where(x => x.scenarionConditionType == ScenarioConditionType.AfterBattle && x.enemyPlayer == attacker.playerIndex).FirstOrDefault();
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
                                m_PlayerGold += attacker.m_Gold;
                                getItem = "<color=yellow>" + attacker.m_Gold + "</color>Gold";
                                //Debug.Log("Got " + attacker.gold + " gold!");
                            }
                            else
                            {
                                if (m_PlayerWeapons.ContainsKey(attacker.m_EquipWeapon))
                                {
                                    m_PlayerWeapons[attacker.m_EquipWeapon]++;
                                }
                                else
                                {
                                    m_PlayerWeapons.Add(attacker.m_EquipWeapon, 1);
                                }
                                getItem = gameElement.weapons.Where(x => x.id == attacker.m_EquipWeapon).FirstOrDefault().name;
                                //Debug.Log("Got " + gameElement.weapons.Where(x => x.id == attacker.equipWeapon).FirstOrDefault().name + "!");
                            }
                        }
                        else
                        {
                            //Attacker not dead, user player get exp
                            if (!isPlayerTurn)
                            {
                                target.m_Exp += amountOfDamage;
                                damageByAttacker = (amountOfDamage + (attacker.m_Hp < 0 ? attacker.m_Hp : 0));
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
                int amountOfHeal = Mathf.FloorToInt(attacker.m_Wis);
                amountOfHeal = amountOfHeal > target.m_MaxHP - target.m_Hp ? target.m_MaxHP - target.m_Hp : amountOfHeal;
                target.m_Hp += amountOfHeal;

                damageByAttacker = amountOfHeal;

                //Debug.Log(attacker.playerName + " heal " + target.playerName + " for " + amountOfHeal + " HP point!");

                if (isPlayerTurn)
                {
                    attacker.m_Exp += amountOfHeal;
                    getExp = amountOfHeal;
                    //Debug.Log(attacker.playerName + " get exp " + amountOfHeal + "!");
                }
            }

            //level up
            PlayerRecord lvUpProp = null;
            Player checkLevelUp = isPlayerTurn ? attacker : target;
            lvUpProp = checkLevelUp.LevelUp(m_PlayerLevelMax);
            if (lvUpProp != null)
            {
                level = checkLevelUp.m_Level;
                playerData = checkLevelUp.GetPlayerProp();
                checkLevelUp.LevelUp(lvUpProp);
            }
            //if (checkLevelUp.m_Level<m_PlayerLevelMax && checkLevelUp.m_Hp > 0 && (checkLevelUp.m_Exp / 100) >= checkLevelUp.m_Level)
            //{
            //    lvUpProp = checkLevelUp.LevelUp();
            //}

            //if (isPlayerTurn && attacker.m_Hp > 0 && (attacker.m_Exp / 100) == attacker.m_Level)
            //{
            //    lvUpProp = attacker.LevelUp();
            //    level = attacker.m_Level;
            //    playerData = attacker.GetPlayerProp();
            //    //Debug.Log(attacker.playerName + " is level up to " + attacker.level + "!");

            //}
            //else if (!isPlayerTurn && target.m_Hp > 0 && (target.m_Exp / 100) == target.m_Level)
            //{
            //    lvUpProp = target.LevelUp();
            //    level = target.m_Level;
            //    playerData = target.GetPlayerProp();
            //    //Debug.Log(target.playerName + " is level up to " + target.level + "!");
            //}

            //send to battle scene
            isWaitingBattle = true;

            battleData = new BattleSendData(attackerName, targetName, backGround, attackerTileName, targetTileName, isPlayerAttack, isHeal, isCounter, isDirectAtk, attackerDefensRate, targetDefensRate, attackerMaxHP, attackerHP, damageByAttacker, targetMaxHP, targetHP, damageByTarget, getItem, playerExp, getExp, level, playerClass, playerData, lvUpProp);
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(battleData));
            //SceneManager.LoadScene("Battle");

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
            highlightTiles = HexTileHighlight.FindHighlight(GameMidiator.m_Instance.m_StageMapManager.GetMapTile(originLocation.x, originLocation.y), distance, true);
        }
        else
        {
            highlightTiles = HexTileHighlight.FindHighlight(GameMidiator.m_Instance.m_StageMapManager.GetMapTile(originLocation.x, originLocation.y), distance, userPlayers.Values.Union(enemyPlayers.Values)/*.Where(x => x.gridPosition != originLocation)*/.Select(x => x.gridPosition).ToArray());
        }

        foreach (HexTile t in highlightTiles)
        {
            //t.m_Visual.transform.GetComponentInChildren<Renderer>().materials[0].color = highlightColor;
            t.SetHightLight(true, false);
        }
    }

    public void HighlightTileAt(Vector2 originLocation, Color highlightColor, List<HexTile> highlightTiles)
    {
        foreach (HexTile t in highlightTiles)
        {
            //t.m_Visual.transform.GetComponentInChildren<Renderer>().materials[0].color = highlightColor;
            t.SetHightLight(true, false);
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
                if (userPlayers[playerIndex].m_Race.Equals(gameElement.races[i].id))
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
                    userPlayers[i].m_Hp = userPlayers[i].m_MaxHP;
                }
            }
            else
            {
                int hp = selectItem.hp;
                userPlayers[playerIndex].m_Atk += selectItem.atk;
                userPlayers[playerIndex].m_Def += selectItem.def;
                userPlayers[playerIndex].m_Dex += selectItem.dex;
                userPlayers[playerIndex].m_Wis += selectItem.wis;
                userPlayers[playerIndex].m_MaxHP += selectItem.addHp;
                userPlayers[playerIndex].m_Hp = Mathf.Min(userPlayers[playerIndex].m_Hp + hp, userPlayers[playerIndex].m_MaxHP);
                m_PlayerGold += selectItem.gold;

                if (selectItem.useCharType > 0)
                {
                    userPlayers[playerIndex].m_Race = selectItem.newCharType;
                    userPlayers[playerIndex].m_Level = 1;
                    userPlayers[playerIndex].m_Exp = 0;
                }
                ShowUseItemInfo(userPlayers[playerIndex].m_PlayerName, hp, selectItem.atk, selectItem.def, selectItem.dex, selectItem.wis, selectItem.addHp, selectItem.gold, (selectItem.useCharType > 0 ? gameElement.races.Where(x => x.id == selectItem.newCharType).FirstOrDefault().name : ""));
            }
            return true;
        }
        return false;
    }

    public bool EquipWeapon(int id)
    {
        Player player = GetSelectedPlayer();
        if (player == null || player.m_EquipWeapon.Equals(id))
        {
            return false;
        }
        Weapon selectedWeapon = GetWeapon(id);
        Weapon equipedWeapon = GetWeapon(player.m_EquipWeapon);

        player.m_EquipWeapon = selectedWeapon.id;
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

    public TextAsset m_ObjectData;

    public CharacterTemplate GetPlayerType(int id, bool isEnemy)
    {
        if (isEnemy)
        {
            return m_DicEnemyTypes[id];
        }
        else
        {
            return m_DicPlayerTypes[id];
        }
    }

    private void LoadGameElements()
    {
        if (gameElement != null)
        {
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

            m_DicPlayerTypes = new Dictionary<int, CharacterTemplate>();
            m_DicEnemyTypes = new Dictionary<int, CharacterTemplate>();
            for (int i = 0; i < gameElement.characters.Count; i++)
            {
                if (gameElement.characters[i].enemy)
                {
                    m_DicEnemyTypes.Add(gameElement.characters[i].id, gameElement.characters[i]);
                }
                else
                {
                    m_DicPlayerTypes.Add(gameElement.characters[i].id, gameElement.characters[i]);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
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
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
            moving = true;
            attacking = false;
            HighlightTileAt(userPlayers[inputPlayerIndex].gridPosition, moveTileColor, (int)userPlayers[inputPlayerIndex].m_MovementPerActionPoint, false);
        }
        else
        {
            moving = false;
            attacking = false;
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
        }
    }

    private void Action(int inputPlayerIndex)
    {
        if (!attacking)
        {
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
            attacking = true;
            HighlightTileAt(userPlayers[inputPlayerIndex].gridPosition, attackTileColor, userPlayers[inputPlayerIndex].GetAttackRange());
        }
        else
        {
            attacking = false;
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
        }
    }

    private void Cancel(int inputPlayerIndex)
    {
        if (moving)
        {
            //userPlayers[inputPlayerIndex].gridPosition = userPlayers[inputPlayerIndex].originalGridPosition;
            //Vector3 oriPos = m_StageMapManager.GetMapTile(userPlayers[inputPlayerIndex].m_Hex).transform.position;
            //userPlayers[inputPlayerIndex].transform.position = new Vector3(oriPos.x, playerHeight, oriPos.z);
            userPlayers[inputPlayerIndex].CancelMove();
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
        GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
    }

    private void ShowEndTurn(int playerIndex)
    {
        ShowEndTurnConfirm();
    }

    private void TurnEnd(int playerIndex)
    {
        isPlayerTurn = false;
        currentEnemyPlayerIndex = 0;
        foreach (Player p in enemyPlayers.Values)
        {
            p.TurnActive();
            if (p.m_Hp > 0 && GameMidiator.m_Instance.m_StageMapManager.GetMapTile(p.m_Hex).m_TileType2D == TileType2D.Villa)
            {
                p.m_Hp += (p.m_MaxHP / 8);
                if (p.m_Hp > p.m_MaxHP)
                {
                    p.m_Hp = p.m_MaxHP;
                }
            }
        }
        m_PlayerManager.RemoveDeadEnemy();
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
        runningScenario = stageScenatios.Where(x => x.scenarionType == ScenarioType.Event && x.scenarionConditionType == ScenarioConditionType.AfterShop).FirstOrDefault();
        _Update = ScenarioMode;
    }

    private void UISettingConfirm(int playerIndex)
    {
        isShowTile = isShowTileLine.isOn;
        isShowPlayerUI = isShowPlayHP.isOn;

        GameMidiator.m_Instance.m_StageMapManager.SetTileLineIsShow(isShowTile);
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
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
            attacking = false;
            ShowConfirmMenu();
        }
        else if (moving || (attacking && !moving))
        {
            moving = false;
            attacking = false;
            playerIndex = -1;
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
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

}

public enum GameState
{
    Loading,
    PlayerTrun,
    EnemyTrun,
    Battle,
    ScenarioMode,
    WaitAction
}