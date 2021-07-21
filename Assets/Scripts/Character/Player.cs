using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;

public class Player : MonoBehaviour, IPointerClickHandler
{
    private static readonly Vector3 m_HidePosition = new Vector3(0, -100, 0);

    public Vector2 gridPosition = Vector2.zero;
    public Vector2 originalGridPosition = Vector2.zero;

    public Transform visual;
    public Animator animator;
    public float moveSpeed { get { return GameManager.m_Instance.m_MoveSpeed; } }
    public int playerIndex;

    public float m_MovementPerActionPoint = 5;
    public int m_AttackRange = 1;

    public bool m_Moving = false;
    public bool m_Attacking = false;

    public bool m_IsActable = true;
    public bool m_IsMovable = true;
    public bool m_IsAttackable = true;

    public string m_PlayerName;

    public int m_Race = 0;
    public int m_Level = 1;
    public int m_MaxHP = 25;
    public int m_Hp = 25;
    public int m_Exp = 0;
    public int m_Atk = 10;
    public int m_Def = 5;
    public int m_Wis = 5;
    public int m_Dex = 8;
    public int m_MDef = 5;
    public int m_Gold = 0;

    //Enemy param
    public int m_EquipWeapon;
    public EnemyAIType m_EnemyAIType = EnemyAIType.Attacker;
    public int m_SearchRange = 5;

    private ScenarioActorPivotType m_PlayerPivot = ScenarioActorPivotType.Right;
    public bool m_IsEnemy { get { return _IsEnemy; } }
    protected bool _IsEnemy;

    public Vector3 HexTilePos()
    {
        return m_Hex.PositionSqr();
    }

    private Queue<Vector3> m_PositionQueue = new Queue<Vector3>();

    public HexTile.HexCoord m_Hex { get; private set; }
    protected HexTile.HexCoord m_HexTarget;

    public int m_MapSizeX
    {
        get
        {
            return GameManager.m_Instance.mapSizeX;
        }
    }

    public int m_MapSizeY
    {
        get
        {
            return GameManager.m_Instance.mapSizeY;
        }
    }

    public virtual void SetPlayerValue(PlayerRecord playerRecord, CharacterTemplate playerData)
    {
        //transform.position = new Vector3(tilePos.x, 0, tilePos.z);
        SetPosition(playerRecord);

        m_PlayerName = playerData.name;
        m_Race = playerData.race;
        m_MovementPerActionPoint = playerData.move;
        m_IsActable = playerRecord.isActable;
        m_Level = (int)playerRecord.level;
        m_Exp = (int)playerRecord.exp;
        m_MaxHP = (int)playerRecord.hp;
        m_Hp = (int)playerRecord.currentHp;
        m_Atk = (int)playerRecord.atk;
        m_Def = (int)playerRecord.def;
        m_Wis = (int)playerRecord.wis;
        m_Dex = (int)playerRecord.dex;
        m_MDef = (int)playerRecord.mdef;
        m_EquipWeapon = playerRecord.equipWeapon;
    }

    protected void SetPosition(PlayerRecord playerRecord)
    {
        gridPosition = new Vector2(playerRecord.locX, playerRecord.locY);
        m_Hex = new HexTile.HexCoord(playerRecord.locX, playerRecord.locY);
        transform.position = HexTilePos();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (SceneManager.GetActiveScene().name == "GameScene" && !GameManager.m_Instance.GetIsWaitingBattle() && m_Hp <= 0)
        {
            m_Hp = 0;
            m_IsActable = false;
            transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            transform.GetComponent<Renderer>().material.color = Color.gray;
        }
    }

    public void SetPlayerModel()
    {
        GameObject model = Instantiate(PlayerPrefabHolder.instance.playerModelPrefab01, visual);
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

        if ((m_MoveTimeCur < m_MoveTime && m_NextPosition.HasValue) || SetNextPosition())
        {
            m_MoveTimeCur += Time.deltaTime;
            transform.position = Vector3.Lerp(m_PositionCur, m_NextPosition.Value, m_MoveTimeCur / m_MoveTime);
            return;
        }
    }

    protected virtual void MoveToPointAction() { }

    public void SetHexTarget(HexTile.HexCoord hex)
    {
        m_HexTarget = hex;
    }

    protected float m_MoveTime = 1;
    protected float m_MoveTimeCur = 0;
    protected Vector3 m_PositionCur;
    protected Vector3? m_NextPosition = null;
    protected bool m_IsMoving = false;
    protected bool SetNextPosition()
    {
        if (m_PositionQueue.Count > 0)
        {
            m_PositionCur = m_NextPosition.HasValue ? m_NextPosition.Value : transform.position;
            m_NextPosition = m_PositionQueue.Dequeue();
            transform.LookAt(m_NextPosition.Value);
            float dis = Vector3.Distance(m_NextPosition.Value, transform.position);
            m_MoveTime = dis / moveSpeed;
            m_MoveTimeCur = 0;
            animator.SetBool("walk", m_IsMoving);
            m_IsMoving = true;
            return m_IsMoving;
        }
        m_NextPosition = null;
        animator.SetBool("walk", m_IsMoving);
        if (m_IsMoving)
        {
            m_IsMoving = false;
            MoveToPointAction();
        }
        return m_IsMoving;
    }

    public void SetPositionQueue(Vector3[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            m_PositionQueue.Enqueue(positions[i]);
        }
    }

    public virtual void SetOriginalPos()
    {

    }

    public virtual void TurnEnd()
    {
        m_IsActable = false;
        m_IsMovable = true;
        m_IsAttackable = true;
        m_Hex = m_HexTarget;
    }

    public void CancelMove()
    {
        transform.position = HexTilePos();
        m_HexTarget = m_Hex;
    }

    public virtual void TurnActive()
    {
        if (m_Hp != 0)
        {
            m_IsActable = true;
            m_IsMovable = true;
            m_IsAttackable = true;
        }
    }

    public void HidePlayer()
    {
        transform.position = HexTilePos() + m_HidePosition;
    }

    public void ShowPlayer()
    {
        transform.position = HexTilePos();
    }

    public void GetWeaponAttack(out int derictAtk, out int inderictAtk)
    {
        Weapon weapon = GameManager.m_Instance.gameElement.weapons.Where(x => x.id == m_EquipWeapon).FirstOrDefault();
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
        return GameManager.m_Instance.gameElement.races.Where(x => x.id == this.m_Race).FirstOrDefault().canHeal;
    }

    public bool GetIsCanFly()
    {
        return GameManager.m_Instance.gameElement.races.Where(x => x.id == this.m_Race).FirstOrDefault().canFly;
    }

    public virtual PlayerRecord LevelUp()
    {
        return new PlayerRecord();
    }

    public virtual PlayerRecord LevelUp(int levelMax)
    {
        int targetLevel = (m_Exp / 100) + 1;
        if (m_Level < levelMax && m_Hp > 0 && targetLevel > m_Level)
        {
            PlayerRecord pr = new PlayerRecord(targetLevel, StatusUpValue(targetLevel), 0, StatusUpValue(targetLevel), StatusUpValue(targetLevel), StatusUpValue(targetLevel), StatusUpValue(targetLevel), StatusUpValue(targetLevel));

            return pr;
        }
        return null;
    }

    public virtual void LevelUp(PlayerRecord playerRecord)
    {
        m_Level = (int)playerRecord.level;
        m_Hp = (int)playerRecord.hp;
        m_Atk = (int)playerRecord.atk;
        m_Def = (int)playerRecord.def;
        m_Wis = (int)playerRecord.wis;
        m_Dex = (int)playerRecord.dex;
        m_MDef = (int)playerRecord.mdef;
    }

    private int StatusUpValue(int upLevel)
    {
        int value = 0;
        for (int i = 0; i < upLevel; i++)
        {
            value += Random.Range(0, 2);
        }
        return value;
    }

    public PlayerRecord GetPlayerProp()
    {
        PlayerRecord pr = new PlayerRecord(m_MaxHP, m_Hp, m_Atk, m_Def, m_Wis, m_Dex, m_MDef);
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
            range.AddRange(HexTile.GetCubeRingTile(m_Hex, 1));
        }
        if (GetIsCanAttack(false))
        {
            range.AddRange(HexTile.GetCubeRingTile(m_Hex, 2));
        }
        return range;
    }

    //public virtual List<HexTile> GetAttackRangeWhitTarget(Vector2 targetGridPosition)
    //{
    //    List<HexTile> range = new List<HexTile>();
    //    if (GetIsCanAttack(true))
    //    {
    //        range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 1, m_MapSizeX, m_MapSizeY));
    //    }
    //    if (GetIsCanAttack(false))
    //    {
    //        range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 2, m_MapSizeX, m_MapSizeY));
    //    }
    //    return range;
    //}

    public virtual List<HexTile> GetAttackRangeWhitTarget(HexTile.HexCoord targetGridPosition)
    {
        List<HexTile> range = new List<HexTile>();
        if (GetIsCanAttack(true))
        {
            range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 1));
        }
        if (GetIsCanAttack(false))
        {
            range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 2));
        }
        return range;
    }


    public virtual List<HexTile> GetHealRange()
    {
        return HexTile.GetCubeSpiralTile(gridPosition, 1, GameManager.m_Instance.gameElement.races.Where(x => x.id == this.m_Race).FirstOrDefault().healRange, m_MapSizeX, m_MapSizeY);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            HexTile attTile = GameMidiator.m_Instance.m_StageMapManager.m_MapHex[(int)gridPosition.y][(int)gridPosition.x + (((int)gridPosition.y) >> 1)];
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
        m_PlayerPivot = scenarioActorPivotType;
        //switch (scenarioActorPivotType)
        //{
        //    case ScenarioActorPivotType.Right:
        //        transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        //        break;
        //    case ScenarioActorPivotType.UpRight:
        //        transform.rotation = Quaternion.Euler(new Vector3(0, 30, 0));
        //        break;
        //    case ScenarioActorPivotType.UpLeft:
        //        transform.rotation = Quaternion.Euler(new Vector3(0, 330, 0));
        //        break;
        //    case ScenarioActorPivotType.Left:
        //        transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
        //        break;
        //    case ScenarioActorPivotType.DownLeft:
        //        transform.rotation = Quaternion.Euler(new Vector3(0, 210, 0));
        //        break;
        //    case ScenarioActorPivotType.DownRight:
        //        transform.rotation = Quaternion.Euler(new Vector3(0, 150, 0));
        //        break;
        //}
    }

    public void SetPivot()
    {
        switch (m_PlayerPivot)
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
