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
    public float moveSpeed = 10f;
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

    //movement animation
    public List<Vector3> positionQueue = new List<Vector3>();
    //
    public Vector2 mapHexIndex
    {
        get {
            return new Vector2(gridPosition.x + (((int)gridPosition.y) >> 1), gridPosition.y);
        }
    }

    public HexTile.HexCoord hex
    {
        get
        {
            return new HexTile.HexCoord((int)gridPosition.x , (int)gridPosition.y);
        }
    }

    public int mapSizeX
    {
        get
        {
            return GameManager.instance.mapSizeX;
        }
    }

    public int mapSizeY
    {
        get
        {
            return GameManager.instance.mapSizeY;
        }
    }

    private void Awake()
    {
        moveDestination = transform.position;
        GameObject model = Instantiate(PlayerPrefabHolder.instance.playerModelPrefab01, transform.position, transform.rotation, visual);
        animator = model.GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {

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

    public virtual void TurnUpdate()
    {
        //if (actionPoint <=0)
        //{
        //    actionPoint = 2;
        //    moving = false;
        //    attacking = false;
        //    GameManager.instance.NextTurn();
        //}
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

    public void GetWeaponAttack(ref int derictAtk, ref int inderictAtk)
    {
        Weapon weapon = GameManager.instance.gameElement.weapons.Where(x => x.id == equipWeapon).FirstOrDefault();
        derictAtk = weapon.directAtk;
        inderictAtk = weapon.indirectAtk;
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
            range.AddRange(HexTile.GetCubeRingTile(gridPosition, 1, mapSizeX, mapSizeY));
        }
        if (GetIsCanAttack(false))
        {
            range.AddRange(HexTile.GetCubeRingTile(gridPosition, 2, mapSizeX, mapSizeY));
        }
        return range;
    }

    public virtual List<HexTile> GetAttackRangeWhitTarget(Vector2 targetGridPosition)
    {
        List<HexTile> range = new List<HexTile>();
        if (GetIsCanAttack(true))
        {
            range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 1, mapSizeX, mapSizeY));
        }
        if (GetIsCanAttack(false))
        {
            range.AddRange(HexTile.GetCubeRingTile(targetGridPosition, 2, mapSizeX, mapSizeY));
        }
        return range;
    }


    public virtual List<HexTile> GetHealRange()
    {
        return HexTile.GetCubeSpiralTile(gridPosition, 1, GameManager.instance.gameElement.races.Where(x => x.id == this.race).FirstOrDefault().healRange, mapSizeX, mapSizeY);
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
