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

    [SerializeField]
    public float _MoveSpeed = 10f;

    public float m_MoveSpeed { get { return _MoveSpeed; } }

    public GameObject playerUIPrefab;
    public GameObject gameSceneUI;

    public CanvasGroup blockUI;
    public CanvasGroup mapController;
    //public CanvasGroup menu;
    public CanvasGroup endTurnConfirm;
    //public CanvasGroup stageMessage;
    public CanvasGroup gameSetting;

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
    public bool m_IsPlayerTurn = true;
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

    private GameState m_GameState = GameState.Loading;
    private UnityAction m_Update = Utility.Nothing;

    public int m_TurnCount { get; private set; }

    private float waitingTime = 0;
    private float currentWaitingTime = 0;

    [HideInInspector]
    public int currentPlayerIndex = 0;
    private Player m_ControlPlayer = null;
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
    }

    private void Start()
    {
        //if (isLoadMap)
        {
            GameLoading();
        }
        loadIndex = -1;
        isLoadMap = false;
        PlayerPrefs.SetInt("loadIndex", -1);
        PlayerPrefs.SetInt("isLoadMap", 0);
    }

    private void ChangeState(GameState state)
    {
        m_GameState = state;
        switch (m_GameState)
        {
            case GameState.Loading:
                m_Update = null;
                break;
            case GameState.PlayerTurn:
                m_IsPlayerTurn = true;
                m_Update = UserTurnUpdate;
                break;
            case GameState.EnemyTurn:
                m_IsPlayerTurn = false;
                m_Update = EnemyTurnUpdate;
                break;
            case GameState.Battle:
                m_Update = null;
                break;
            case GameState.Scenarion:
                m_Update = ScenarionUpdate;
                break;
            case GameState.WaitAction:
                m_Update = WaitActionUpdate;
                break;
        }
    }

    void Update()
    {
        //if (SceneManager.GetActiveScene().name == "GameScene")
        //{
        //    if (currentWaitingTime >= waitingTime)
        //    {
        //        waitingTime = 0;
        //        currentWaitingTime = 0;
        //        if (isMoveCamera)
        //        {
        //            isMoveCamera = false;
        //            isWaitingActor = false;
        //            ScreenController.m_Instance.SetCameraPos(moveCaremaPos);
        //        }
        //        if (isToDark)
        //        {
        //            Color temp = darkFront.color;
        //            temp.a = 0;
        //            darkFront.color = temp;
        //        }
        //    }
        //    else
        //    {
        //        currentWaitingTime += Time.deltaTime;
        //        if (isMoveCamera)
        //        {
        //            if (Vector3.Distance(moveCaremaPos, ScreenController.m_Instance.cameraPos.position) > 0.1f)
        //            {
        //                Vector3 newPosition = ScreenController.m_Instance.cameraPos.position + (moveCaremaPos - ScreenController.m_Instance.cameraPos.position).normalized * cameraMoveSpeed * Time.deltaTime;
        //                ScreenController.m_Instance.SetCameraPos(new Vector3(newPosition.x, 0, newPosition.z));
        //            }
        //        }
        //        if (isToDark)
        //        {
        //            Color temp = darkFront.color;
        //            temp.a += (Time.deltaTime / waitingTime);
        //            darkFront.color = temp;
        //        }
        //        return;
        //    }

        //play mode
        //if (runningScenario == null && !isWin && !isLose)
        //{
        //    PlayMode();
        //}
        ////scenario mode
        //else if (!isWin && !isLose)
        //{
        //    ScenarioMode();
        //}
        //else
        //{
        //    EndMode();
        //}
        m_Update?.Invoke();
        //}
    }
    #region Game control

    private bool PlayMode()
    {
        if (!isWaitingMsg && !isWaitingBattle)
        {
            if (GameMidiator.m_Instance.m_StageManager.StageClearCheck())
            {
                //Win
                if (GameMidiator.m_Instance.m_ScenarionManager.SetClearScenarion())
                {
                    ChangeState(GameState.Scenarion);
                    return true;
                }
            }
        }
        return false;
    }

    private void UserTurnUpdate()
    {
        if (PlayMode())
        {
            return;
        }
    }

    private void EnemyTurnUpdate()
    {
        if (PlayMode())
        {
            return;
        }
        if (m_PlayerManager.CheckUserPlayerAlive() && m_PlayerManager.CheckEnemyPlayerAlive())
        {
            if (enemyPlayers[currentEnemyPlayerIndex].m_Hp > 0)
            {
                isSetCamera = true;
                StartCoroutine(RunEnemyTurn());
            }
            else
            {
                NextEnemyTurn();
            }
        }
    }

    private void ScenarionUpdate()
    {
        GameMidiator.m_Instance.m_ScenarionManager.SystemUpdate();
    }

    private void WaitActionUpdate()
    {
        if (!ScreenController.m_Instance.m_IsCameraMoving)
        {
            //TODO go to control mode
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
        m_Update = Utility.Nothing;
        SceneManager.LoadScene("GameEnd");
    }

    #endregion

    int loadIndex;
    bool isLoadMap = false;

    private void OnLevelWasLoaded(int level)
    {
        //gameSceneUI.SetActive(false);
        //if (level == 1)
        //{
        //    isLoadMap = false;
        //    try
        //    {
        //        loadIndex = PlayerPrefs.GetInt("loadIndex");
        //isLoadMap = PlayerPrefs.GetInt("isLoadMap") == 1;
        //    }
        //    catch
        //    {
        //        loadIndex = -1;
        //    }
        //    gameSceneUI.SetActive(true);
        //    isWaitingBattle = false;

        //    if (userPlayers != null && userPlayers.Values.Where(x => gameElement.races.Where(y => y.name == "ロード").FirstOrDefault().id == x.m_Race && x.m_Hp < 0).Count() > 0)
        //    {
        //        isLose = true;
        //        m_Update = EndMode;
        //        //Game Over
        //    }
        //}
    }

    private SaveDataStore m_SaveData;

    private void InitialStage()
    {
        m_IsPlayerTurn = true;
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

        GameMidiator.m_Instance.m_PlayerManager.ClearPlayer();

        LoadGameElements();
        SetStopWaiting();
        HideStageInfo();

        DisableGroup(endTurnConfirm);
        DisableGroup(gameSetting);
        isShowStage = true;
    }

    private void StartGame()
    {
        if (GameMidiator.m_Instance.m_StageManager.LoadStageData(m_SaveData))
        {
        }
    }

    private void GameLoading()
    {
        ScreenController.m_Instance.SetCameraPos(cameraPosition);
        ScreenController.m_Instance.SetCameraRot(cameraRotation);

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
        defeatedEnemyList = m_SaveData.m_DefeatedEnemyList;
        m_PlayerItems = m_SaveData.m_PlayerItems;
        m_PlayerWeapons = m_SaveData.m_PlayerWeapons;

        if (m_SaveData.m_IsEmpty)
        {
            m_SaveData.m_IsEmpty = false;
            m_SaveData.m_Gold = 1000;
            m_SaveData.m_StageIndex = 1;
            //Begin items
            for (int i = 0; i < gameElement.items.Count; i++)
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

        if (GameMidiator.m_Instance.m_ScenarionManager.SetOpenScenarion())
        {
            ChangeState(GameState.Scenarion);
        }
        else
        {
            GameMidiator.m_Instance.m_GameUIManager.ShowStageInfo();
            Debug.Log("No opening ");
            ChangeState(GameState.PlayerTurn);
        }
    }

    public void ScenationFin(bool isEnding)
    {
        if (isEnding)
        {
            //TODO next stage
        }
        else
        {
            ChangeState(m_IsPlayerTurn ? GameState.PlayerTurn : GameState.EnemyTurn);
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

    public void HideStageInfo()
    {
        //DisableGroup(stageMessage);
        isWaitingMsg = false;
        //if (isShowStage)
        //{
        //    isShowStage = false;
        //    ShowStageInfo();
        //}
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
            Vector3 pos = GameMidiator.m_Instance.m_PlayerManager.GetEnemyByList(currentEnemyPlayerIndex).HexTilePos();
            ScreenController.m_Instance.SetCameraPos(pos);
            isSetCamera = false;
            yield return null;
        }

        GameMidiator.m_Instance.m_PlayerManager.GetEnemyByList(currentEnemyPlayerIndex).TurnUpdate();
    }

    public void NextEnemyTurn()
    {
        if (currentEnemyPlayerIndex == enemyPlayers.Count - 1)
        {
            m_TurnCount++;
            m_IsPlayerTurn = true;
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
            Vector3 pos = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(userPlayers[0].m_Hex).HexTilePos(); ;
            ScreenController.m_Instance.SetCameraPos(pos);
            //isSetCamera = false;

            m_PlayerManager.RemoveDeadEnemy();
            GameMidiator.m_Instance.m_GameUIManager.ShowStageTurnInfo(m_TurnCount, m_IsPlayerTurn);
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
            Player targetPlayer = m_IsPlayerTurn ? userPlayers[playerIndex] : enemyPlayers[currentEnemyPlayerIndex];
            targetPlayer.SetPositionQueue(GameMidiator.m_Instance.m_StageMapManager.FindPath(targetPlayer.m_Hex, destTile.m_Hex, GameMidiator.m_Instance.m_PlayerManager.GetPlayerHexes(targetPlayer.m_Hex).ToArray()));
            targetPlayer.SetHexTarget(destTile.m_Hex);
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
        MenuType setType = MenuType.PlayerMoveCantAtkMenu;
        bool isShowAction = false;
        if (player.GetIsCanHeal())
        {
            isShowAction = GetUserTargetHex(player, true).Count > 0;
        }
        isShowAction |= GetUserTargetHex(player, false).Count > 0;

        if (isShowAction)
        {
            setType = MenuType.PlayerMoveCanAtkMenu;
        }
        GameMidiator.m_Instance.m_GameUIManager.SetMenu(setType, player.HexTilePos());
        SetStopWaiting();
    }

    public void ShowPlayerTileMenu(HexCoord hex, bool isShop)
    {
        m_ControlPlayer = GameMidiator.m_Instance.m_PlayerManager.GetPlayer(hex);
        MenuType setType = MenuType.PlayerDeadMenu;
        bool shopMenu = false;
        if (m_ControlPlayer != null)
        {
            SetPlayerStatusUI(m_ControlPlayer);
            if (!m_ControlPlayer.m_IsEnemy && m_ControlPlayer.m_Hp > 0)
            {
                bool isShowAction = false;
                shopMenu = isShop;
                switch (m_ControlPlayer.m_PlayerState)
                {
                    case PlayerState.Active:
                        isShowAction = CheckPlayerAction(m_ControlPlayer);
                        setType = isShowAction ? MenuType.PlayerShopMenu : MenuType.PlayerMoveShopMenu;
                        break;
                    case PlayerState.Move:
                        isShowAction = CheckPlayerAction(m_ControlPlayer);
                        setType = isShowAction ? MenuType.PlayerMoveCanAtkMenu : MenuType.PlayerMoveCantAtkMenu;
                        break;
                    case PlayerState.Wait:
                        setType = MenuType.PlayerStandShopMenu;
                        break;
                }
            }
        }
        else
        {
            setType = MenuType.TileMenu;
        }
        GameMidiator.m_Instance.m_GameUIManager.SetMenu(setType, shopMenu, hex.PositionSqr());
    }

    private bool CheckPlayerAction(Player player)
    {
        if (player.GetIsCanHeal())
        {
            List<HexCoord> hexTiles = player.GetHealRange();
            for (int i = 0; i < hexTiles.Count; i++)
            {
                Player healPlayer = GameMidiator.m_Instance.m_PlayerManager.GetPlayer(hexTiles[i]);
                if (healPlayer && !healPlayer.m_IsEnemy && healPlayer.m_Hp < healPlayer.m_MaxHP)
                {
                    return true;
                }
            }
        }
        List<HexCoord> hexAtkTiles = player.GetAttackRangeHex();
        for (int i = 0; i < hexAtkTiles.Count; i++)
        {
            Player atkPlayer = GameMidiator.m_Instance.m_PlayerManager.GetPlayer(hexAtkTiles[i]);
            if (atkPlayer && atkPlayer.m_IsEnemy && atkPlayer.m_Hp > 0)
            {
                return true;
            }
        }
        return false;
    }

    private List<HexCoord> GetUserTargetHex(Player player, bool isHeal)
    {
        List<HexCoord> hexes = new List<HexCoord>();
        List<HexCoord> result = new List<HexCoord>();
        hexes = isHeal ? player.GetHealRange() : player.GetAttackRangeHex();
        for (int i = 0; i < hexes.Count; i++)
        {
            Player targetPlayer = GameMidiator.m_Instance.m_PlayerManager.GetPlayer(hexes[i]);
            if (targetPlayer && ((isHeal && !targetPlayer.m_IsEnemy && targetPlayer.m_Hp < targetPlayer.m_MaxHP) || (!isHeal && targetPlayer.m_IsEnemy && targetPlayer.m_Hp > 0)))
            {
                result.Add(targetPlayer.m_Hex);
            }
        }
        return result;
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

    public void AttackWithCurrentPlayer(HexCoord hex)
    {
        AttackWithCurrentPlayer(GameMidiator.m_Instance.m_StageMapManager.GetMapTile(hex));
    }
    public void AttackWithCurrentPlayer(HexTile destTile)
    {
        if (!destTile.m_IsHighLight && !destTile.m_Impassible)
        {
            Debug.Log("destination invalid");
            return;
        }

        cameraPosition = ScreenController.m_Instance.mainCamera.transform.position;
        cameraRotation = ScreenController.m_Instance.mainCamera.transform.rotation.eulerAngles;

        battleData = null;

        Player attacker = null;
        Player target = null;
        bool isHeal = false;

        //set for battle scene
        string backGround = "";
        bool isPlayerAttack = m_IsPlayerTurn;
        bool isCounter = false;

        string attackerName;
        string attackerTileName = "";
        int attackerDefensRate = 0;
        int attackerMaxHP;
        int attackerHP;
        int damageByAttacker = 0;

        string targetName;
        string targetTileName = "";
        int targetDefensRate = 0;
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
        if (m_IsPlayerTurn)
        {
            attacker = GameMidiator.m_Instance.m_PlayerManager.GetPlayerByList(currentEnemyPlayerIndex); //userPlayers[playerIndex];
            attackerName = attacker.m_PlayerName;
            attackerMaxHP = attacker.m_MaxHP;
            attackerHP = attacker.m_Hp;
            playerExp = attacker.m_Exp;
            playerClass = gameElement.races.Where(x => x.id == attacker.m_Race).FirstOrDefault().name;
            //target = enemyPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
            target = GameMidiator.m_Instance.m_PlayerManager.GetPlayer(destTile.m_Hex);
            isHeal = !target.m_IsEnemy;
            //if (target == null)
            //{
            //    target = userPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
            //    isHeal = true;
            //}
            targetName = target.m_PlayerName;
            targetMaxHP = target.m_MaxHP;
            targetHP = target.m_Hp;
            SetTileName(destTile, out targetTileName, out targetDefensRate);
            //Scenarion temp = stageScenatios.Where(x => x.scenarionType == ScenarionType.Event && x.scenarionConditionType == ScenarionConditionType.BeforeBattle && (x.userPlayer == -1 || x.userPlayer == attacker.playerIndex) && (x.enemyPlayer == -1 || x.enemyPlayer == target.playerIndex)).FirstOrDefault();
            if (GameMidiator.m_Instance.m_ScenarionManager.SetBeforeScenarion(attacker.playerIndex, target.playerIndex, destTile.m_Hex))
            {
                ChangeState(GameState.Scenarion);
                return;
            }
            //if (temp != null)
            //{
            //    if (temp.isOnceEvent)
            //    {
            //        stageScenatios.Remove(temp);
            //    }
            //    temp.battleAfterEvent = destTile;
            //    runningScenario = temp;
            //    _Update = ScenarioMode;
            //}
        }
        else
        {
            attacker = GameMidiator.m_Instance.m_PlayerManager.GetEnemyByList(currentEnemyPlayerIndex); // enemyPlayers[currentEnemyPlayerIndex];
            targetName = attacker.m_PlayerName;
            targetMaxHP = attacker.m_MaxHP;
            targetHP = attacker.m_Hp;
            //target = userPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
            target = GameMidiator.m_Instance.m_PlayerManager.GetPlayer(destTile.m_Hex);
            isHeal = target.m_IsEnemy;
            //if (target == null)
            //{
            //    target = enemyPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
            //    isHeal = true;
            //}
            attackerName = target.m_PlayerName;
            attackerMaxHP = target.m_MaxHP;
            attackerHP = target.m_Hp;
            playerExp = target.m_Exp;
            playerClass = gameElement.races.Where(x => x.id == target.m_Race).FirstOrDefault().name;
            SetTileName(destTile, out attackerTileName, out attackerDefensRate);

            //Scenarion temp = stageScenatios.Where(x => x.scenarionType == ScenarionType.Event && x.scenarionConditionType == ScenarionConditionType.BeforeBattle && (x.enemyPlayer == -1 || x.enemyPlayer == attacker.playerIndex) && (x.userPlayer == -1 || x.userPlayer == target.playerIndex)).FirstOrDefault();
            if (GameMidiator.m_Instance.m_ScenarionManager.SetBeforeScenarion(target.playerIndex, attacker.playerIndex, destTile.m_Hex))
            {
                ChangeState(GameState.Scenarion);
                return;
            }
            //if (temp != null)
            //{
            //    if (temp.isOnceEvent)
            //    {
            //        stageScenatios.Remove(temp);
            //    }
            //    temp.battleAfterEvent = destTile;
            //    runningScenario = temp;
            //    _Update = ScenarioMode;
            //}
        }

        //if (runningScenario != null)
        //{
        //    return;
        //}
        if (target != null)
        {
            HexTile targetTile = GameMidiator.m_Instance.m_StageMapManager.GetMapTile(attacker.m_Hex);
            bool isDirectAtk = HexCoord.HexDistance(attacker.m_Hex, target.m_Hex) == 1;

            int directAtk = 0;
            int indirectAtk = 0;

            if (!m_IsPlayerTurn)
            {
                SetTileName(targetTile, out targetTileName, out targetDefensRate);
            }
            else
            {
                SetTileName(targetTile, out attackerTileName, out attackerDefensRate);
            }

            attacker.GetWeaponAttack(out directAtk, out indirectAtk);

            attacker.TurnEnd();
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
            attacking = false;
            moving = false;

            if (!isHeal)
            {
                //attack
                int amountOfDamage = Mathf.FloorToInt((attacker.m_Atk + (isDirectAtk ? directAtk : indirectAtk) - target.m_Def) * (1f - (destTile.m_DefenseRate / 100f)));
                target.m_Hp -= amountOfDamage;

                damageByTarget = (amountOfDamage + (target.m_Hp < 0 ? target.m_Hp : 0));
                //Target dead, user player get exp
                if (target.m_Hp <= 0)
                {
                    if (m_IsPlayerTurn)
                    {
                        damageByAttacker = (amountOfDamage + (target.m_Hp < 0 ? target.m_Hp : 0));
                        attacker.m_Exp += target.m_Exp;
                        getExp = target.m_Exp;
                        if (GameMidiator.m_Instance.m_ScenarionManager.SetAfterScenarion(target.playerIndex, target.m_Hex))
                        {
                            ChangeState(GameState.Scenarion);
                            return;
                        }
                    }

                    //Got item/weapon/gold by enemy
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5)
                    {
                        m_PlayerGold += target.m_Gold;
                        getItem = "<color=yellow>" + target.m_Gold + "</color>Gold";
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
                    }
                }
                else
                {
                    //Target not dead, user player get exp
                    if (m_IsPlayerTurn)
                    {
                        attacker.m_Exp += amountOfDamage;
                        damageByAttacker = (amountOfDamage + (target.m_Hp < 0 ? target.m_Hp : 0));
                        getExp = amountOfDamage;
                    }

                    //Counter
                    if (isDirectAtk && target.GetIsCanAttack(true) || !isDirectAtk && target.GetIsCanAttack(false))
                    {
                        isCounter = true;
                        target.GetWeaponAttack(out directAtk, out indirectAtk);

                        amountOfDamage = Mathf.FloorToInt((target.m_Atk + (isDirectAtk ? directAtk : indirectAtk) - attacker.m_Def) * (1f - (targetTile.m_DefenseRate / 100f)));
                        attacker.m_Hp -= amountOfDamage;

                        //Attacker dead, user player get exp
                        if (attacker.m_Hp <= 0)
                        {
                            if (!m_IsPlayerTurn)
                            {
                                damageByAttacker = (amountOfDamage + (attacker.m_Hp < 0 ? attacker.m_Hp : 0));
                                target.m_Exp += attacker.m_Exp;
                                getExp = attacker.m_Exp;
                                if (GameMidiator.m_Instance.m_ScenarionManager.SetAfterScenarion(attacker.playerIndex, attacker.m_Hex))
                                {
                                    ChangeState(GameState.Scenarion);
                                    return;
                                }
                            }

                            //Got item/weapon/gold by enemy
                            if (UnityEngine.Random.Range(0f, 1f) < 0.5)
                            {
                                m_PlayerGold += attacker.m_Gold;
                                getItem = "<color=yellow>" + attacker.m_Gold + "</color>Gold";
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
                            }
                        }
                        else
                        {
                            //Attacker not dead, user player get exp
                            if (!m_IsPlayerTurn)
                            {
                                target.m_Exp += amountOfDamage;
                                damageByAttacker = (amountOfDamage + (attacker.m_Hp < 0 ? attacker.m_Hp : 0));
                                getExp = amountOfDamage;
                            }
                        }
                    }
                }
            }
            else
            {
                //Heal player
                int amountOfHeal = Mathf.FloorToInt(attacker.m_Wis);
                amountOfHeal = amountOfHeal > target.m_MaxHP - target.m_Hp ? target.m_MaxHP - target.m_Hp : amountOfHeal;
                target.m_Hp += amountOfHeal;

                damageByAttacker = amountOfHeal;
                if (m_IsPlayerTurn)
                {
                    attacker.m_Exp += amountOfHeal;
                    getExp = amountOfHeal;
                }
            }

            //level up
            PlayerRecord lvUpProp = null;
            Player checkLevelUp = m_IsPlayerTurn ? attacker : target;
            lvUpProp = checkLevelUp.LevelUp(m_PlayerLevelMax);
            if (lvUpProp != null)
            {
                level = checkLevelUp.m_Level;
                playerData = checkLevelUp.GetPlayerProp();
                checkLevelUp.LevelUp(lvUpProp);
            }

            //send to battle scene
            isWaitingBattle = true;

            battleData = new BattleSendData(attackerName, targetName, backGround, attackerTileName, targetTileName, isPlayerAttack, isHeal, isCounter, isDirectAtk, attackerDefensRate, targetDefensRate, attackerMaxHP, attackerHP, damageByAttacker, targetMaxHP, targetHP, damageByTarget, getItem, playerExp, getExp, level, playerClass, playerData, lvUpProp);
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(battleData));
            //SceneManager.LoadScene("Battle");
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

        GameMidiator.m_Instance.m_GameUIManager.ShowGetItemInfo(gold, itemId, weaponId);
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
                Player player = userPlayers[playerIndex];
                player.m_Atk += selectItem.atk;
                player.m_Def += selectItem.def;
                player.m_Dex += selectItem.dex;
                player.m_Wis += selectItem.wis;
                player.m_MaxHP += selectItem.addHp;
                player.m_Hp = Mathf.Min(player.m_Hp + hp, player.m_MaxHP);

                if (selectItem.useCharType > 0)
                {
                    player.m_Race = selectItem.newCharType;
                    player.m_Level = 1;
                    player.m_Exp = 0;
                }
                GameMidiator.m_Instance.m_GameUIManager.ShowUseItemInfo(player.m_PlayerName, hp, selectItem.atk, selectItem.def, selectItem.dex, selectItem.wis, selectItem.addHp, selectItem.gold, (selectItem.useCharType > 0 ? gameElement.races.Where(x => x.id == selectItem.newCharType).FirstOrDefault().name : ""));
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

        GameMidiator.m_Instance.m_GameUIManager.ShowEquipWeaponInfo(selectedWeapon.name, selectedWeapon.name);
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
    }

    public void ClickButtonAction(string clickButton)
    {
        buttonAction = (ButtonAction)Delegate.CreateDelegate(typeof(ButtonAction), this, clickButton);
        buttonAction(playerIndex);
    }

    private void Move(int inputPlayerIndex)
    {
        if (!moving)
        {
            GameMidiator.m_Instance.m_StageMapManager.RemoveHighlightTiles();
            moving = true;
            attacking = false;
            Player player = GameMidiator.m_Instance.m_PlayerManager.GetPlayerByList(inputPlayerIndex);
            GameMidiator.m_Instance.m_StageMapManager.HighlightTileAt(player.m_Hex, player.m_MovementPerActionPoint, false);
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
            Player player = GameMidiator.m_Instance.m_PlayerManager.GetPlayerByList(inputPlayerIndex);
            GameMidiator.m_Instance.m_StageMapManager.HighlightTileAt(player.GetAttackRangeHex());
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
            userPlayers[inputPlayerIndex].CancelMove();
            moving = false;
            Move(inputPlayerIndex);
        }
        //DisableGroup(endTurnConfirm);
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
        m_IsPlayerTurn = false;
        currentEnemyPlayerIndex = 0;
        foreach (Player p in enemyPlayers.Values)
        {
            p.TurnActive();
            if (p.m_Hp > 0 && GameMidiator.m_Instance.m_StageMapManager.GetMapTile(p.m_Hex).m_TileType2D == TileType2D.Villa)
            {
                p.m_Hp += (p.m_MaxHP / 8);
                p.m_Hp = Mathf.Clamp(p.m_Hp, 0, p.m_MaxHP);
            }
        }
        m_PlayerManager.RemoveDeadEnemy();
        SetStartWaiting();
        DisableGroup(endTurnConfirm);
        GameMidiator.m_Instance.m_GameUIManager.ShowStageTurnInfo(m_TurnCount, m_IsPlayerTurn);
        //ShowStageInfo();
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
        if (GameMidiator.m_Instance.m_ScenarionManager.SetAfterShopScenarion())
        {
            ChangeState(GameState.Scenarion);
            return;
        }
    }

    private void UISettingConfirm(int playerIndex)
    {
        isShowTile = isShowTileLine.isOn;
        isShowPlayerUI = isShowPlayHP.isOn;

        GameMidiator.m_Instance.m_StageMapManager.SetTileLineIsShow(isShowTile);
        ScreenController.m_Instance.SetPlayerUIIsShow(isShowPlayerUI);

        DisableGroup(gameSetting);
    }


    #endregion

    public void CancelAction()
    {
        if (GameMidiator.m_Instance.m_GameUIManager.m_IsMenuShowing)
        {
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

    public void ShowEndTurnConfirm()
    {
        //DisableGroup(menu);
        EnableGroup(endTurnConfirm);
    }

}

public enum GameState
{
    Loading,
    PlayerTurn,
    EnemyTurn,
    Battle,
    Scenarion,
    WaitAction
}