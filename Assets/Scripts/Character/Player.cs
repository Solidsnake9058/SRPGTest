using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;

public class Player : MonoBehaviour, IPointerClickHandler
{
    public Vector2 gridPosition = Vector2.zero;
    public Vector2 originalGridPosition = Vector2.zero;

    public Vector3 moveDestination;

    public Transform visual;
    public Animator animator;
    public float moveSpeed { get { return GameManager.instance.m_MoveSpeed; } }
    public int playerIndex;

    public float movementPerActionPoint = 5;
    public int attackRange = 1;

    public bool moving = false;
    public bool attacking = false;

    public bool isActable = true;
    public bool isMovable = true;
    public bool isAttackable = true;

    public string playerName;

    public int race = 0;
    public int level = 1;
    public int maxHP = 25;
    public int hp = 25;
    public int exp = 0;
    public int atk = 10;
    public int def = 5;
    public int wis = 5;
    public int dex = 8;
    public int mdef = 5;
    public int gold = 0;
    public int equipWeapon;
    public EnemyAIType enemyAIType = EnemyAIType.Attacker;
    public int searchRange = 5;

    public float attackChance = 0.75f;
    public float defenseReduction = 0.15f;
    public int damageBase = 5;
    public float damageRollSides = 6;
    public ScenarioActorPivotType playerPivot = ScenarioActorPivotType.Right;
    public bool m_IsEnemy { get; private set; }

    //movement animation
    public Queue<Vector3> positionQueue = new Queue<Vector3>();
    //

    public HexTile.HexCoord hex
    {
        get
        {
            return new HexTile.HexCoord((int)gridPosition.x, (int)gridPosition.y);
        }
    }

    public HexTile.HexCoord m_Hex { get; private set; }

    public int m_MapSizeX
    {
        get
        {
            return GameManager.instance.mapSizeX;
        }
    }

    public int m_MapSizeY
    {
        get
        {
            return GameManager.instance.mapSizeY;
        }
    }

    private void Awake()
    {
        moveDestination = transform.position;
    }

    // Use this for initialization
    void Start()
    {
    }

    public void SetPlayerValue(PlayerRecord playerRecord, CharacterTemplate playerData, Vector3 tilePos)
    {
        transform.position = new Vector3(tilePos.x, 0, tilePos.z);
        gridPosition = new Vector2(playerRecord.locX, playerRecord.locY);
        m_Hex = new HexTile.HexCoord(playerRecord.locX, playerRecord.locY);

        playerName = playerData.name;
        race = playerData.race;
        movementPerActionPoint = playerData.move;
        isActable = playerRecord.isActable;
        level = (int)playerRecord.level;
        exp = (int)playerRecord.exp;
        maxHP = (int)playerRecord.hp;
        hp = (int)playerRecord.currentHp;
        atk = (int)playerRecord.atk;
        def = (int)playerRecord.def;
        wis = (int)playerRecord.wis;
        dex = (int)playerRecord.dex;
        mdef = (int)playerRecord.mdef;
        equipWeapon = playerRecord.equipWeapon;

        if (playerRecord.isEnemy)
        {
            CharacterLevelTemplate playerLvData = playerData.GetCharacterLevelData((int)playerRecord.level);
            level = (int)playerLvData.level;
            exp = (int)playerLvData.exp;
            hp = maxHP = (int)playerLvData.hp;
            gold = (int)playerLvData.gold;
            equipWeapon = playerLvData.equipWeapon;
            enemyAIType = playerRecord.aiType;
            searchRange = playerRecord.searchRange;
            m_IsEnemy = true;
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (SceneManager.GetActiveScene().name == "GameScene" && !GameManager.instance.GetIsWaitingBattle() && hp <= 0)
        {
            hp = 0;
            isActable = false;
            transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            transform.GetComponent<Renderer>().material.color = Color.gray;
        }
    }

    public void SetPlayerModel()
    {
        GameObject model = Instantiate(PlayerPrefabHolder.instance.playerModelPrefab01, transform.position, transform.rotation, visual);
        animator = model.GetComponent<Animator>();
    }

    public virtual void TurnUpdate()
    {
        //if (actionPoint <=0)
        //{
        //    actionPoint = 2;
        //    moving = false;
        //    attacking = false;
        //    GameManager.instance.NextTurn();
        //}

        if ((m_MoveTimeCur< m_MoveTime&& m_NextPosition.HasValue) || SetNextPosition())
        {
            m_MoveTimeCur += Time.deltaTime;
            transform.position = Vector3.Lerp(m_PositionCur, m_NextPosition.Value, m_MoveTimeCur / m_MoveTime);
            return;
        }
    }

    protected float m_MoveTime = 1;
    protected float m_MoveTimeCur = 0;
    protected Vector3 m_PositionCur;
    protected Vector3? m_NextPosition = null;
    protected bool SetNextPosition()
    {
        if (positionQueue.Count > 0)
        {
            m_PositionCur = m_NextPosition.Value;
            m_NextPosition = positionQueue.Dequeue();
            transform.LookAt(m_NextPosition.Value);
            float dis = Vector3.Distance(m_NextPosition.Value, transform.position);
            m_MoveTime = dis / moveSpeed;
            m_MoveTimeCur = 0;
            animator.SetBool("walk", true);
            return true;
        }
        m_NextPosition = null;
        animator.SetBool("walk", false);
        return false;
    }

    public void SetPositionQueue(List<Vector3> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            positionQueue.Enqueue(positions[i]);
        }
    }

    public virtual void SetOriginalPos()
    {

    }

    public virtual void TurnEnd()
    {
        isActable = false;
        isMovable = true;
        isAttackable = true;
    }

    public virtual void TurnActive()
    {
        if (hp != 0)
        {
            isActable = true;
            isMovable = true;
            isAttackable = true;
        }
    }

    public void HidePlayer()
    {
        transform.position = new Vector3(transform.position.x, -100, transform.position.z);
    }

    public void ShowPlayer()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    public void GetWeaponAttack(out int derictAtk, out int inderictAtk)
    {
        Weapon weapon = GameManager.instance.gameElement.weapons.Where(x => x.id == equipWeapon).FirstOrDefault();
        derictAtk = weapon.directAtk;
        inderictAtk = weapon.indirectAtk;
    }

    public bool GetIsCanAttack(bool isDirect)
    {
        int directAtk = 0;
        int indirectAtk = 0;

        GetWeaponAttack(out directAtk, out indirectAtk);

        if (isDirect)
        {
            return directAtk > 0;
        }
        return indirectAtk > 0;
    }

    public bool GetIsCanHeal()
    {
        return GameManager.instance.gameElement.races.Where(x => x.id == this.race).FirstOrDefault().canHeal;
    }

    public bool GetIsCanFly()
    {
        return GameManager.instance.gameElement.races.Where(x => x.id == this.race).FirstOrDefault().canFly;
    }

    public virtual PlayerRecord LevelUp()
    {
        return new PlayerRecord();
    }

    public PlayerRecord GetPlayerProp()
    {
        PlayerRecord pr = new PlayerRecord((uint)maxHP, (uint)atk, (uint)def, (uint)wis, (uint)dex, (uint)mdef);

        return pr;
    }

    public virtual void TurnOnGUI()
    {

    }

    public virtual void OnMouseDown()
    {

    }

    public virtual List<HexTile> GetAttackRange()
    {
        List<HexTile> range = new List<HexTile>();
        if (GetIsCanAttack(true))
        {
            range.AddRange(HexTile.GetCubeRingTile(gridPosition, 1, m_MapSizeX, m_MapSizeY));
        }
        if (GetIsCanAttack(false))
        {
            range.AddRange(HexTile.GetCubeRingTile(gridPosition, 2, m_MapSizeX, m_MapSizeY));
        }
        return range;
    }

    public virtual List<HexTile> GetAttackRangeWhitTarget(Vector2 targetGridPosition)
    {
        List<HexTile> range = new List<HexTile>();
        if (GetIsCanAttack(true))
        {
            range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 1, m_MapSizeX, m_MapSizeY));
        }
        if (GetIsCanAttack(false))
        {
            range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 2, m_MapSizeX, m_MapSizeY));
        }
        return range;
    }


    public virtual List<HexTile> GetHealRange()
    {
        return HexTile.GetCubeSpiralTile(gridPosition, 1, GameManager.instance.gameElement.races.Where(x => x.id == this.race).FirstOrDefault().healRange, m_MapSizeX, m_MapSizeY);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            HexTile attTile = GameManager.instance.mapHex[(int)gridPosition.y][(int)gridPosition.x + (((int)gridPosition.y) >> 1)];
            attTile.ClickEvent(eventData);
        }
        else if (SceneManager.GetActiveScene().name == "MapCreatorScene")
        {
            HexTile attTile = MapCreatorManager.instance.mapHex[(int)gridPosition.y][(int)gridPosition.x + (((int)gridPosition.y) >> 1)];
            attTile.ClickEvent(eventData);
        }
    }

    public void SetPivot(ScenarioActorPivotType scenarioActorPivotType)
    {
        switch (scenarioActorPivotType)
        {
            case ScenarioActorPivotType.Right:
                transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
            case ScenarioActorPivotType.UpRight:
                transform.rotation = Quaternion.Euler(new Vector3(0, 30, 0));
                break;
            case ScenarioActorPivotType.UpLeft:
                transform.rotation = Quaternion.Euler(new Vector3(0, 330, 0));
                break;
            case ScenarioActorPivotType.Left:
                transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
                break;
            case ScenarioActorPivotType.DownLeft:
                transform.rotation = Quaternion.Euler(new Vector3(0, 210, 0));
                break;
            case ScenarioActorPivotType.DownRight:
                transform.rotation = Quaternion.Euler(new Vector3(0, 150, 0));
                break;
        }
    }
}
