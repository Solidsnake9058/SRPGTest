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
    private bool isWin = false;
    private bool isLose = false;
    [HideInInspector]
    public bool isSetCamera = false;

    private GameState m_GameState = GameState.Loading;
    private UnityAction m_Update = Utility.Nothing;

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
    [SerializeField]
    private int m_PlayerHPMax = 150;

    public BattleSendData battleData = new BattleSendData();
    private SaveDataStore m_SaveData;

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
        GameLoading();
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


    private void InitialStage()
    {
        m_IsPlayerTurn = true;
        isWin = false;
        isLose = false;
        m_PlayerManager.ClearPlayer();
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

        InitialStage();

        //TODO get store index
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
        m_StageMapManager.SetTileLineIsShow(true);
        m_GameUIManager.m_ScreenControlUI.SetPlayerUIShow(true);

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
            m_GameUIManager.m_ScreenControlUI.SetPlayerUIShow(true);
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
        
        BattlePlayerData attackerData = GetBattlePlayerData(attacker, targetTile, targetDamage);
        BattlePlayerData targetData = GetBattlePlayerData(target, destTile, attackerDamage);

        //level up
        PlayerRecord lvUpProp = null;
        Player checkLevelUp = m_IsPlayerTurn ? attacker : target;
        lvUpProp = checkLevelUp.LevelUp(m_PlayerLevelMax, m_PlayerHPMax, m_PlayerStateMax);
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
                if (m_PlayerDataManager.m_PlayerWeapons.ContainsKey(target.m_EquipWeapon))
                {
                    m_PlayerDataManager.m_PlayerWeapons[target.m_EquipWeapon]++;
                }
                else
                {
                    m_PlayerDataManager.m_PlayerWeapons.Add(target.m_EquipWeapon, 1);
                }
                getItem = m_ElementManager.GetWeapon(target.m_EquipWeapon).name;
            }
        }
        battleData = new BattleSendData(attackerData, targetData, backGround, isHeal || !attacker.m_IsEnemy, isHeal, isDirectAtk, isCounter, getExp, getItem, playerClass, lvUpProp, playerData);
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
        data.m_GetDamage = causeDamage;
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
        m_GameUIManager.ShowStageTurnInfo();
        ChangeState(GameState.EnemyTurn);
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
        m_GameUIManager.ShowSetting();
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

    public void GameSettingConfirm(bool showTile, bool showHP)
    {
        SaveManager.SetIsShowTile(showTile);
        SaveManager.SetIsShowPlayerUI(showHP);
        SaveManager.Save();
        m_StageMapManager.SetTileLineIsShow(showTile);
        m_GameUIManager.m_ScreenControlUI.SetPlayerUIShow(showHP);
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