using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;

public class Player : IGameItem, IPointerClickHandler
{
    protected static ElementManager m_ElementManager { get { return GameMidiator.m_Instance.m_ElementManager; } }
    protected static StageMapManager m_StageMapManager { get { return GameMidiator.m_Instance.m_StageMapManager; } }
    protected static PlayerManager m_PlayerManager { get { return GameMidiator.m_Instance.m_PlayerManager; } }


    private static readonly Vector3 m_HidePosition = new Vector3(0, -100, 0);
    public Transform visual;
    protected Animator m_Animator;

    protected PlayerUI m_PlayerUI;
    public float moveSpeed { get { return GameManager.m_Instance.m_MoveSpeed; } }
    public int playerIndex;

    public float m_MovementPerActionPoint = 5;
    public int m_AttackRange = 1;

    public bool m_Moving = false;
    public bool m_Attacking = false;

    public bool m_IsMovable = true;
    public bool m_IsAttackable = true;
    public bool IsAlive { get { return m_Hp > 0; } }
    public bool IsTurnEnd { get { return m_PlayerState.Equals(PlayerState.Wait); } }

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

    private ScenarionActorPivotType m_PlayerPivot = ScenarionActorPivotType.Right;
    public bool m_IsEnemy { get { return _IsEnemy; } }
    protected bool _IsEnemy;

    public PlayerState m_PlayerState { get { return _PlayerState; } }
    protected PlayerState _PlayerState;

    public Vector3 HexTilePos()
    {
        return m_Hex.PositionSqr();
    }
    public Vector3 TargetHexTilePos()
    {
        return m_HexTarget.PositionSqr();
    }

    private Queue<Vector3> m_PositionQueue = new Queue<Vector3>();

    public HexCoord m_Hex { get; private set; }
    protected HexCoord m_HexTarget;

    public virtual void SetPlayerValue(PlayerRecord playerRecord, CharacterTemplate playerData)
    {
        SetPosition(playerRecord);

        m_PlayerName = playerData.name;
        m_Race = playerData.race;
        m_MovementPerActionPoint = playerData.move;
        _PlayerState = playerRecord.isActable ? PlayerState.Active : PlayerState.Wait;
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

    public void SetPlayerUI(PlayerUI playerUI)
    {
        m_PlayerUI = playerUI;
        m_PlayerUI.SetPosition(HexTilePos());
    }

    protected void SetPosition(PlayerRecord playerRecord)
    {
        m_Hex = new HexCoord(playerRecord.locX, playerRecord.locY);
        transform.position = HexTilePos();
    }

    protected virtual void SetPlayerDead()
    {
        m_Hp = 0;
        transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        transform.GetComponent<Renderer>().material.color = Color.gray;
    }

    public void SetPlayerModel()
    {
        GameObject model = Instantiate(PlayerPrefabHolder.instance.playerModelPrefab01, visual);
        m_Animator = model.GetComponent<Animator>();
    }

    public virtual bool TurnUpdate()
    {
        if (m_PlayerState.Equals(PlayerState.Wait))
        {
            return false;
        }
        if ((m_MoveTimeCur < m_MoveTime && m_NextPosition.HasValue) || SetNextPosition())
        {
            m_MoveTimeCur += Time.deltaTime;
            transform.position = Vector3.Lerp(m_PositionCur, m_NextPosition.Value, m_MoveTimeCur / m_MoveTime);
            m_PlayerUI?.SetPosition(transform.position);
        }
        return true;
    }

    protected virtual void MoveToPointAction() { }

    public void SetHexTarget(HexCoord hex)
    {
        m_HexTarget = hex;
    }

    protected float m_MoveTime = 1;
    protected float m_MoveTimeCur = 0;
    protected Vector3 m_PositionCur;
    protected Vector3? m_NextPosition = null;
    protected bool m_IsMoving = false;
    protected bool m_IsMoved = false;

    public bool SetMoveCheck()
    {
        if (m_PlayerState.Equals(PlayerState.Active))
        {
            _PlayerState = PlayerState.Move;
            return true;
        }
        return false;
    }

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
            m_IsMoving = true;
            m_Animator.SetBool("walk", m_IsMoving);
            return m_IsMoving;
        }
        m_NextPosition = null;
        if (m_IsMoving)
        {
            m_IsMoving = false;
            m_IsMoved = true;
            _PlayerState = PlayerState.MoveEnd;
            MoveToPointAction();
        }
        m_Animator.SetBool("walk", m_IsMoving);
        return m_IsMoving;
    }

    public void SetPositionQueue(Vector3[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            m_PositionQueue.Enqueue(positions[i]);
        }
        _PlayerState = PlayerState.Move;
    }

    public virtual void TurnEnd()
    {
        m_IsMovable = true;
        m_IsAttackable = true;
        m_Hex = m_HexTarget;
        _PlayerState = PlayerState.Wait;
        m_PlayerUI?.RefreshUI(m_Hp, m_MaxHP, m_PlayerState);
    }

    public void CancelMoveRange()
    {
        m_IsMoved = false;
        _PlayerState = PlayerState.Active;
    }
    public void CancelMove()
    {
        transform.position = HexTilePos();
        m_PlayerUI.SetPosition(HexTilePos());
        m_HexTarget = m_Hex;
        _PlayerState = PlayerState.Move;
    }
    public void CancelActionRange()
    {
        _PlayerState = m_IsMoved ? PlayerState.MoveEnd : PlayerState.Active;
        if (m_IsMoved)
        {
            MoveToPointAction();
        }
    }

    public virtual void TurnActive()
    {
        if (m_Hp > 0)
        {
            m_IsMovable = true;
            m_IsAttackable = true;
            m_IsMoved = false;
            _PlayerState = PlayerState.Active;
            m_PlayerUI.RefreshUI(m_Hp, m_MaxHP, m_PlayerState);
        }
    }

    public void HidePlayer()
    {
        transform.position = HexTilePos() + m_HidePosition;
        m_PlayerUI.SetHideUI();
    }

    public void ShowPlayer()
    {
        transform.position = HexTilePos();
        m_PlayerUI.SetShowUI();
    }

    public void GetWeaponAttack(ref int derictAtk, ref int inderictAtk)
    {
        m_ElementManager.GetWeaponAttack(m_EquipWeapon, ref derictAtk, ref inderictAtk);
    }

    public bool GetIsCanAttack(bool isDirect)
    {
        int directAtk = 0;
        int indirectAtk = 0;

        GetWeaponAttack(ref directAtk, ref indirectAtk);

        if (isDirect)
        {
            return directAtk > 0;
        }
        return indirectAtk > 0;
    }

    public bool GetIsCanHeal()
    {
        return m_ElementManager.GetRace(m_Race).canHeal;
    }

    public bool GetIsCanFly()
    {
        return m_ElementManager.GetRace(m_Race).canFly;
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

    public virtual List<HexCoord> GetAttackRangeHex()
    {
        List<HexCoord> range = new List<HexCoord>();
        if (GetIsCanAttack(true))
        {
            range.AddRange(HexCoord.HexRing(m_Hex, 1));
        }
        if (GetIsCanAttack(false))
        {
            range.AddRange(HexCoord.HexRing(m_Hex, 2));
        }
        return range;
    }

    public virtual List<HexCoord> GetAttackRangeWithTarget(HexCoord targetGridPosition, List<HexCoord> excludeHex = null)
    {
        List<HexCoord> range = new List<HexCoord>();
        if (GetIsCanAttack(true))
        {
            range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 1));
        }
        if (GetIsCanAttack(false))
        {
            range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 2));
        }
        if (excludeHex != null)
        {
            for (int i = 0; i < range.Count; i++)
            {
                if (excludeHex.Contains(range[i]))
                {
                    range.Remove(range[i]);
                }
            }
        }

        return range;
    }

    public virtual List<HexCoord> GetHealRange()
    {
        int healRange = m_ElementManager.GetRace(m_Race).healRange;
        return HexCoord.HexSpiral(m_Hex, healRange);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        HexTile attTile = m_StageMapManager.GetMapTile(m_Hex);
        attTile.ClickEvent(eventData);
    }

    public void SetPivot(ScenarionActorPivotType scenarioActorPivotType)
    {
        m_PlayerPivot = scenarioActorPivotType;
    }

    public void SetPivot()
    {
        transform.rotation = Quaternion.Euler(0, (int)m_PlayerPivot, 0);

        //switch (m_PlayerPivot)
        //{
        //    case ScenarionActorPivotType.Right:
        //        transform.rotation = Quaternion.Euler(0, 90, 0);
        //        break;
        //    case ScenarionActorPivotType.UpRight:
        //        transform.rotation = Quaternion.Euler(0, 30, 0);
        //        break;
        //    case ScenarionActorPivotType.UpLeft:
        //        transform.rotation = Quaternion.Euler(0, 330, 0);
        //        break;
        //    case ScenarionActorPivotType.Left:
        //        transform.rotation = Quaternion.Euler(0, 270, 0);
        //        break;
        //    case ScenarionActorPivotType.DownLeft:
        //        transform.rotation = Quaternion.Euler(0, 210, 0);
        //        break;
        //    case ScenarionActorPivotType.DownRight:
        //        transform.rotation = Quaternion.Euler(0, 150, 0);
        //        break;
        //}
    }
}

public enum PlayerState
{
    Active,
    Move,
    MoveEnd,
    Action,
    Wait
}