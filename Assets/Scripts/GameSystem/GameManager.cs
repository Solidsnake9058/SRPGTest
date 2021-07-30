using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager m_Instance { get; private set; }

    protected static ElementManager m_ElementManager { get { return GameMidiator.m_Instance.m_ElementManager; } }
    protected static StageMapManager m_StageMapManager { get { return GameMidiator.m_Instance.m_StageMapManager; } }
    protected static StageManager m_StageManager { get { return GameMidiator.m_Instance.m_StageManager; } }
    protected static PlayerDataManager m_PlayerDataManager { get { return GameMidiator.m_Instance.m_PlayerDataManager; } }
    protected static ScenarionManager m_ScenarionManager { get { return GameMidiator.m_Instance.m_ScenarionManager; } }
    protected static PlayerManager m_PlayerManager { get { return GameMidiator.m_Instance.m_PlayerManager; } }

    protected static GameUIManager m_GameUIManager { get { return GameMidiator.m_Instance.m_GameUIManager; } }

    #region parament

    [SerializeField]
    public float _MoveSpeed = 10f;

    public float m_MoveSpeed { get { return _MoveSpeed; } }

    public CanvasGroup blockUI;
    public CanvasGroup gameSetting;

    public int mapSizeX = 32;
    public int mapSizeY = 38;

    public bool isShowTile = true;
    public bool isShowPlayerUI = true;

    [SerializeField]
    private Color _MoveTileColor;
    public Color m_MoveTileColor { get { return _MoveTileColor; } }
    [SerializeField]
    private Color _AttackTileColor;
    public Color m_AttackTileColor { get { return _AttackTileColor; } }

    #endregion

    #region game parament
    [HideInInspector]
    public bool m_IsPlayerTurn = true;
    public int m_TurnCount { get; private set; }
    //private bool isWaitingActor = false;
    private bool isWaitingBattle = false;
    private bool isWaitingMsg = false;
    private bool isMoveCamera = false;
    private bool isWin = false;
    private bool isLose = false;
    [HideInInspector]
    public bool isSetCamera = false;

    private GameState m_GameState = GameState.Loading;
    private UnityAction m_Update = Utility.Nothing;

    //private float waitingTime = 0;
    //private float currentWaitingTime = 0;

    private Player m_ControlPlayer = null;
    [HideInInspector]
    public int m_CurrentEnemyIndex = 0;

    private int playerIndex = -1;
    delegate void ButtonAction();
    ButtonAction buttonAction;

    public Vector3 cameraPosition;
    public Vector3 cameraRotation;

    [SerializeField]
    private int m_PlayerGoldInit = 1000;

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

    #endregion

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
                m_GameUIManager.m_ScreenControlUI.ShowUI();
                m_Update = UserTurnUpdate;
                break;
            case GameState.EnemyTurn:
                m_IsPlayerTurn = false;
                m_GameUIManager.m_ScreenControlUI.ShowUI();
                m_Update = EnemyTurnUpdate;
                break;
            case GameState.Battle:
                m_Update = null;
                m_GameUIManager.m_ScreenControlUI.HideUI();
                break;
            case GameState.Scenarion:
                m_Update = ScenarionUpdate;
                m_GameUIManager.m_ScreenControlUI.HideUI();
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
        m_Update?.Invoke();
    }
    #region Game control

    private bool PlayMode()
    {
        m_GameUIManager.SystemUpdate();
        if (m_StageManager.StageClearCheck())
        {
            //Win
            if (m_ScenarionManager.SetClearScenarion())
            {
                ChangeState(GameState.Scenarion);
                return true;
            }
        }
        return false;
    }

    private void UserTurnUpdate()
    {
        m_GameUIManager.SystemUpdate();
        if (PlayMode())
        {
            return;
        }
        if (m_ControlPlayer != null)
        {
            if (m_ControlPlayer.IsTurnEnd)
            {
                m_ControlPlayer = null;
                return;
            }
            m_ControlPlayer.SystemUpdate();
        }
    }

    private void EnemyTurnUpdate()
    {
        m_GameUIManager.SystemUpdate();
        if (PlayMode())
        {
            return;
        }
        if (m_PlayerManager.CheckUserPlayerAlive() && m_PlayerManager.CheckEnemyPlayerAlive())
        {
            if (m_ControlPlayer.m_Hp > 0)
            {
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
        m_GameUIManager.SystemUpdate();
        m_ScenarionManager.SystemUpdate();
    }

    private void WaitActionUpdate()
    {
        m_GameUIManager.SystemUpdate();
        if (!m_GameUIManager.m_ScreenControlUI.m_IsCameraMoving)
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
        //isWaitingActor = false;
        isWin = false;
        isLose = false;
        //Color temp = darkFront.color;
        //temp.a = 0;
        //darkFront.color = temp;
        m_PlayerManager.ClearPlayer();
        SetStopWaiting();
        DisableGroup(gameSetting);
    }

    private void StartGame()
    {
        if (m_StageManager.LoadStageData(m_SaveData))
        {
            m_PlayerManager.TurnActive(true);
        }
    }

    private void GameLoading()
    {
        m_GameUIManager.m_ScreenControlUI.SetCameraPos(cameraPosition);
        m_GameUIManager.m_ScreenControlUI.SetCameraRot(cameraRotation);

        isWaitingBattle = false;
        InitialStage();

        int storeIndex = -1;
        bool isLoad = false;
        m_SaveData = isLoad ? SaveManager.GetSaveDataStore(storeIndex) : SaveManager.GetSaveDataStore();

        m_TurnCount = m_SaveData.m_TurnCount;

        if (m_SaveData.m_IsEmpty)
        {
            m_SaveData.m_IsEmpty = false;
            m_SaveData.m_Gold = m_PlayerGoldInit;
            m_SaveData.m_StageIndex = 1;
            m_SaveData.m_PlayerItems = m_ElementManager.GetInitItems();
            m_SaveData.m_UserPlayerRecords = m_ElementManager.GetInitPlayerValue();
        }
        m_PlayerDataManager.SetPlayerItemData(m_SaveData);
        StartGame();

        if (m_ScenarionManager.SetOpenScenarion())
        {
            ChangeState(GameState.Scenarion);
        }
        else
        {
            m_GameUIManager.ShowStageInfo();
            Debug.Log("No opening ");
            ScenationFin(true, false);
        }
    }

    public void ScenationFin(bool isOpening, bool isEnding)
    {
        if (isOpening)
        {
            m_PlayerManager.ShowPlayers();
            Player temp = m_PlayerManager.GetPlayerByID(0);
            m_GameUIManager.m_ScreenControlUI.SetCameraPos(temp.HexTilePos());
            m_GameUIManager.ShowStageInfo();
        }
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
        CharacterType race = m_ElementManager.GetRace(player.m_Race);
        Weapon weapon = m_ElementManager.GetWeapon(player.m_EquipWeapon);
        m_GameUIManager.SetPlayerStatusUI(player, race, weapon);
    }

    private IEnumerator RunEnemyTurn()
    {
        if (isSetCamera)
        {
            Vector3 pos = m_ControlPlayer.HexTilePos();
            m_GameUIManager.m_ScreenControlUI.SetCameraPos(pos);
            isSetCamera = false;
            yield return null;
        }
        m_ControlPlayer.TurnUpdate();
    }

    public void NextEnemyTurn()
    {
        m_ControlPlayer = null;
        int enemyCount = m_PlayerManager.EnemyPlayerCount;
        while (m_ControlPlayer == null && m_CurrentEnemyIndex < enemyCount)
        {
            m_ControlPlayer = m_PlayerManager.GetEnemyByList(m_CurrentEnemyIndex);
            if (!m_ControlPlayer.IsAlive || !m_ControlPlayer.IsTurnEnd)
            {
                m_ControlPlayer = null;
            }
            m_CurrentEnemyIndex++;
        }

        if (m_ControlPlayer == null)
        {
            m_TurnCount++;
            List<Player> players = m_PlayerManager.GetUserPlayers(null, false, false);
            for (int i = 0; i < players.Count; i++)
            {
                players[i].TurnActive();
                if (m_StageMapManager.GetMapTile(players[i].m_Hex).m_TileType2D == TileType2D.Villa)
                {
                    players[i].m_Hp = Mathf.Min(players[i].m_Hp + (players[i].m_MaxHP / 8), players[i].m_MaxHP);
                }
            }
            Vector3 pos = m_StageMapManager.GetMapTile(m_PlayerManager.GetPlayerByList(0).m_Hex).HexTilePos();
            m_GameUIManager.m_ScreenControlUI.SetCameraPos(pos);

            m_PlayerManager.RemoveDeadEnemy();
            m_GameUIManager.ShowStageTurnInfo();
            ChangeState(GameState.PlayerTurn);
            m_CurrentEnemyIndex = 0;
        }
        else
        {
            isSetCamera = true;
        }
    }

    public void MoveCurrentPlayer(HexTile destTile)
    {
        if (destTile.m_IsHighLight && !destTile.m_Impassible && m_ControlPlayer != null)
        {
            m_StageMapManager.RemoveHighlightTiles();
            m_ControlPlayer.SetPositionQueue(m_StageMapManager.FindPath(m_ControlPlayer.m_Hex, destTile.m_Hex, m_PlayerManager.GetPlayerHexes(m_ControlPlayer.m_Hex).ToArray()));
            m_ControlPlayer.SetHexTarget(destTile.m_Hex);
        }
        else
        {
            Debug.Log("destination invalid");
        }
    }

    public void ShowConfirmMenu()
    {
        bool isShowAction = m_PlayerManager.CheckPlayerAction(m_ControlPlayer);
        m_GameUIManager.SetMenu(MenuType.PlayerMoveCanAtkMenu, isShowAction, false, m_ControlPlayer.TargetHexTilePos());
    }

    public void ShowPlayerTileMenu(HexCoord hex, bool isShop)
    {
        m_ControlPlayer = m_PlayerManager.GetPlayer(hex);
        MenuType setType = MenuType.PlayerDeadMenu;
        bool shopMenu = false;
        bool isShowAction = true;
        if (m_ControlPlayer != null)
        {
            SetPlayerStatusUI(m_ControlPlayer);
            if (!m_ControlPlayer.m_IsEnemy && m_ControlPlayer.m_Hp > 0)
            {
                shopMenu = isShop;
                switch (m_ControlPlayer.m_PlayerState)
                {
                    case PlayerState.Active:
                        setType = MenuType.PlayerShopMenu;
                        break;
                    case PlayerState.Move:
                    case PlayerState.MoveEnd:
                        setType = MenuType.PlayerMoveCanAtkMenu;
                        break;
                    case PlayerState.Wait:
                        setType = MenuType.PlayerStandShopMenu;
                        isShowAction = false;
                        break;
                }
            }
            isShowAction = isShowAction && m_PlayerManager.CheckPlayerAction(m_ControlPlayer);
        }
        else
        {
            setType = MenuType.TileMenu;
        }
        m_GameUIManager.SetMenu(setType, isShowAction, shopMenu, hex.PositionSqr());
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

    private string SetTileName(HexTile destTile)
    {
        switch (destTile.m_TileType2D)
        {
            case TileType2D.Road:
                return "道/橋";
            case TileType2D.Plain:
                return "平原";
            case TileType2D.Wasteland:
                return "荒地";
            case TileType2D.Villa:
                return "村";
            case TileType2D.Forest:
                return "森";
        }
        return "";
    }

    public void AttackWithCurrentPlayer(HexCoord hex)
    {
        AttackWithCurrentPlayer(GetSelectedPlayer(), m_StageMapManager.GetMapTile(hex));
    }

    public void AttackWithCurrentPlayer(HexTile destTile)
    {
        if (!destTile.m_IsHighLight && !destTile.m_Impassible)
        {
            Debug.Log("destination invalid");
            return;
        }

        //TODO reduce process

        //cameraPosition = ScreenControlUI.m_Instance.m_MainCameraTrans.transform.position;
        //cameraRotation = ScreenControlUI.m_Instance.m_MainCameraTrans.transform.rotation.eulerAngles;

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
            attacker = m_PlayerManager.GetPlayerByList(m_CurrentEnemyIndex); //userPlayers[playerIndex];
            attackerName = attacker.m_PlayerName;
            attackerMaxHP = attacker.m_MaxHP;
            attackerHP = attacker.m_Hp;
            playerExp = attacker.m_Exp;
            playerClass = m_ElementManager.GetRace(attacker.m_Race).name;
            //target = enemyPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
            target = m_PlayerManager.GetPlayer(destTile.m_Hex);
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
            if (m_ScenarionManager.SetBeforeScenarion(attacker.playerIndex, target.playerIndex, destTile.m_Hex))
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
            attacker = m_PlayerManager.GetEnemyByList(m_CurrentEnemyIndex); // enemyPlayers[currentEnemyPlayerIndex];
            targetName = attacker.m_PlayerName;
            targetMaxHP = attacker.m_MaxHP;
            targetHP = attacker.m_Hp;
            //target = userPlayers.Values.Where(x => x.gridPosition == destTile.m_GridPosition).FirstOrDefault();
            target = m_PlayerManager.GetPlayer(destTile.m_Hex);
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
            playerClass = m_ElementManager.GetRace(target.m_Race).name;
            SetTileName(destTile, out attackerTileName, out attackerDefensRate);

            //Scenarion temp = stageScenatios.Where(x => x.scenarionType == ScenarionType.Event && x.scenarionConditionType == ScenarionConditionType.BeforeBattle && (x.enemyPlayer == -1 || x.enemyPlayer == attacker.playerIndex) && (x.userPlayer == -1 || x.userPlayer == target.playerIndex)).FirstOrDefault();
            if (m_ScenarionManager.SetBeforeScenarion(target.playerIndex, attacker.playerIndex, destTile.m_Hex))
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
            HexTile targetTile = m_StageMapManager.GetMapTile(attacker.m_Hex);
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

            attacker.GetWeaponAttack(ref directAtk, ref indirectAtk);

            attacker.TurnEnd();
            m_StageMapManager.RemoveHighlightTiles();

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
                        if (m_ScenarionManager.SetAfterScenarion(target.playerIndex, target.m_Hex))
                        {
                            ChangeState(GameState.Scenarion);
                            return;
                        }
                    }

                    //Got item/weapon/gold by enemy
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5)
                    {
                        m_PlayerDataManager.m_PlayerGold += target.m_Gold;
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
                        getItem = m_ElementManager.GetWeapon(target.m_EquipWeapon).name;
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
                        target.GetWeaponAttack(ref directAtk, ref indirectAtk);

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
                                if (m_ScenarionManager.SetAfterScenarion(attacker.playerIndex, attacker.m_Hex))
                                {
                                    ChangeState(GameState.Scenarion);
                                    return;
                                }
                            }

                            //Got item/weapon/gold by enemy
                            if (UnityEngine.Random.Range(0f, 1f) < 0.5)
                            {
                                m_PlayerDataManager.m_PlayerGold += attacker.m_Gold;
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
                                getItem = m_ElementManager.GetWeapon(attacker.m_EquipWeapon).name;
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

    public void AttackWithCurrentPlayer(Player player, HexTile destTile, bool checkScenarion = true)
    {
        if (!destTile.m_IsHighLight && !destTile.m_Impassible)
        {
            Debug.Log("destination invalid");
            return;
        }

        Player attacker = player;
        Player target = m_PlayerManager.GetPlayer(destTile.m_Hex);

        if (checkScenarion && m_ScenarionManager.SetBeforeScenarion(attacker, target, destTile.m_Hex))
        {
            ChangeState(GameState.Scenarion);
            return;
        }

        string backGround = "";
        string getItem = "";
        bool isHeal = attacker.m_IsEnemy && target.m_IsEnemy;
        int getExp = 0;
        bool isDirectAtk = HexCoord.HexDistance(attacker.m_Hex, target.m_Hex) == 1;
        bool isCounter = target.GetIsCanAttack(isDirectAtk);
        int attackerDamage = 0;
        int targetDamage = 0;
        int level;
        int raceID = attacker.m_IsEnemy ? target.m_Race : attacker.m_Race;
        string playerClass = m_ElementManager.GetRace(raceID).name;
        HexTile targetTile = m_StageMapManager.GetMapTile(attacker.m_Hex);
        PlayerRecord playerData = new PlayerRecord();

        isCounter &= GetBattleResult(attacker, target, destTile, isDirectAtk, isHeal, out attackerDamage, ref getExp);
        if (isCounter)
        {
            GetBattleResult(target, attacker, targetTile, isDirectAtk, isHeal, out targetDamage, ref getExp);
        }
        if (!isHeal)
        {
            int enemyIndex = attacker.m_IsEnemy ? attacker.playerIndex : target.playerIndex;
            HexCoord hex = attacker.m_IsEnemy ? attacker.m_Hex : target.m_Hex;
            SetBattleScenarionChecker(enemyIndex, hex);
        }
        attacker.TurnEnd();
        m_StageMapManager.RemoveHighlightTiles();

        BattlePlayerData attackerData = GetBattlePlayerData(attacker, targetTile, attackerDamage);
        BattlePlayerData targetData = GetBattlePlayerData(target, destTile, targetDamage);

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

        Player enemy = attacker.m_IsEnemy ? attacker : target;
        if (!isHeal && !enemy.IsAlive)
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.5)
            {
                m_PlayerDataManager.m_PlayerGold += target.m_Gold;
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
                getItem = m_ElementManager.GetWeapon(target.m_EquipWeapon).name;
            }
        }
        battleData = new BattleSendData(attackerData, targetData, backGround, !attacker.m_IsEnemy, isHeal, isDirectAtk, isCounter, getExp, getItem, playerClass, lvUpProp, playerData);
        //TODO set battle data to demo
        ////send to battle scene
        //isWaitingBattle = true;

        //battleData = new BattleSendData(attackerName, targetName, backGround, attackerTileName, targetTileName, isPlayerAttack, isHeal, isCounter, isDirectAtk, attackerDefensRate, targetDefensRate, attackerMaxHP, attackerHP, damageByAttacker, targetMaxHP, targetHP, damageByTarget, getItem, playerExp, getExp, level, playerClass, playerData, lvUpProp);
        //Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(battleData));
        ////SceneManager.LoadScene("Battle");
    }

    private BattlePlayerData GetBattlePlayerData(Player player, HexTile tile, int causeDamage)
    {
        BattlePlayerData data = new BattlePlayerData();
        data.m_PlayerName = player.m_PlayerName;
        data.m_MaxHP = player.m_MaxHP;
        data.m_HP = player.m_Hp;
        data.m_IsEnemy = player.m_IsEnemy;
        data.m_TileName = SetTileName(tile);
        data.m_DefensRate = tile.m_DefenseRate;
        data.m_CauseDamage = causeDamage;
        return data;
    }

    private bool GetBattleResult(Player attacker, Player target, HexTile hexTile, bool isDirectAtk, bool isHeal, out int damege, ref int getExp)
    {
        if (isHeal)
        {
            damege = attacker.m_Wis;
            damege = -Mathf.Min(damege, target.m_MaxHP - target.m_Hp);
            if (!attacker.m_IsEnemy)
            {
                getExp = -damege;
                attacker.m_Exp += getExp;
            }
        }
        else
        {
            int wpAtk = m_ElementManager.GetWeaponAttack(attacker.m_EquipWeapon, isDirectAtk);
            damege = Mathf.FloorToInt((attacker.m_Atk + wpAtk - target.m_Def) * (1f - (hexTile.m_DefenseRate / 100f)));
            damege = Mathf.Min(damege, target.m_Hp);
            if (!attacker.m_IsEnemy && damege >= target.m_Hp)
            {
                getExp = target.m_Exp;
                attacker.m_Exp += getExp;
            }
        }
        target.m_Hp -= damege;

        return !isHeal && target.IsAlive;
    }


    //TODO check when battle end
    private int m_BattleScenarionEnemyIndex;
    private HexCoord m_BattleScenarionHex;
    private void SetBattleScenarionChecker(int enemyIndex, HexCoord hex)
    {
        m_BattleScenarionEnemyIndex = enemyIndex;
        m_BattleScenarionHex = hex;
    }

    public Player GetSelectedPlayer()
    {
        return m_ControlPlayer;
    }

    #region Button Action
    public void ClickButtonAction(string clickButton)
    {
        buttonAction = (ButtonAction)Delegate.CreateDelegate(typeof(ButtonAction), this, clickButton);
        buttonAction();
    }

    private void Move()
    {
        m_StageMapManager.RemoveHighlightTiles();
        if (m_ControlPlayer != null && m_ControlPlayer.SetMoveCheck())
        {
            m_StageMapManager.HighlightTileAt(m_ControlPlayer.m_Hex, m_ControlPlayer.m_MovementPerActionPoint, false);
        }
    }

    private void Action()
    {
        m_StageMapManager.RemoveHighlightTiles();
        m_StageMapManager.HighlightTileAt(m_ControlPlayer.GetAttackRangeHex());
    }

    public void Cancel()
    {
        if (m_ControlPlayer != null)
        {
            PlayerState playerState = m_ControlPlayer.m_PlayerState;
            m_StageMapManager.RemoveHighlightTiles();
            switch (playerState)
            {
                case PlayerState.Move:
                    HexTile hexTile = m_StageMapManager.GetMapTile(m_ControlPlayer.m_Hex);
                    m_ControlPlayer.CancelMoveRange();
                    ShowPlayerTileMenu(hexTile.m_Hex, hexTile.m_IsShop);
                    break;
                case PlayerState.MoveEnd:
                    m_ControlPlayer.CancelMove();
                    Move();
                    break;
                case PlayerState.Action:
                    m_ControlPlayer.CancelActionRange();
                    break;
            }
        }
    }

    private void Confirm()
    {
        m_ControlPlayer.TurnEnd();
        m_StageMapManager.RemoveHighlightTiles();
    }

    private void ShowEndTurn()
    {
        m_GameUIManager.SetConfirmShow(true);
    }

    public void TurnEnd()
    {
        m_IsPlayerTurn = false;
        m_CurrentEnemyIndex = 0;
        List<Player> players = m_PlayerManager.GetEnemyPlayers(null, false, true);
        for (int i = 0; i < players.Count; i++)
        {
            players[i].TurnActive();
            if (players[i].m_Hp > 0 && m_StageMapManager.GetMapTile(players[i].m_Hex).m_TileType2D == TileType2D.Villa)
            {
                players[i].m_Hp += (players[i].m_MaxHP / 8);
                players[i].m_Hp = Mathf.Clamp(players[i].m_Hp, 0, players[i].m_MaxHP);
            }
        }
        m_PlayerManager.RemoveDeadEnemy();
        SetStartWaiting();
        m_GameUIManager.ShowStageTurnInfo();
        ChangeState(GameState.EnemyTurn);
        //ShowStageInfo();
        //isSetCamera = true;
    }

    private void Status()
    {
        m_GameUIManager.HideMenu();
        m_GameUIManager.ShowStatusUI();
    }

    private void Item()
    {
        m_GameUIManager.HideMenu();
        m_GameUIManager.ShowItemUI();
    }

    private void Weapon()
    {
        m_GameUIManager.HideMenu();
        m_GameUIManager.ShowWeaponUI();
    }

    private void Unit()
    {
        m_GameUIManager.HideMenu();
        m_GameUIManager.ShowUnitListUI();
    }

    private void Setting()
    {
        m_GameUIManager.HideMenu();
        gameSetting.alpha = 1;
        gameSetting.interactable = true;
        gameSetting.blocksRaycasts = true;
    }

    private void Shop()
    {
        m_GameUIManager.HideMenu();
        m_GameUIManager.ShowShopUI();
    }

    #endregion
    public void SetShopDialog()
    {
        if (m_ScenarionManager.SetAfterShopScenarion())
        {
            ChangeState(GameState.Scenarion);
            return;
        }
    }

    private void UISettingConfirm()
    {
        isShowTile = isShowTileLine.isOn;
        isShowPlayerUI = isShowPlayHP.isOn;

        m_StageMapManager.SetTileLineIsShow(isShowTile);
        m_GameUIManager.m_ScreenControlUI.SetPlayerUIShow(isShowPlayerUI);

        DisableGroup(gameSetting);
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