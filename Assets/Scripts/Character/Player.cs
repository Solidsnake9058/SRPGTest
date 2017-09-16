using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, IPointerClickHandler
{
    public Vector2 gridPosition = Vector2.zero;
    public Vector2 originalGridPosition = Vector2.zero;

    public Vector3 moveDestination;
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

    public int race;
    public int level = 1;
    public int maxHP = 25;
    public int hp = 25;
    public int exp = 0;
    public int atk = 10;
    public int def = 5;
    public int wis = 5;
    public int dex = 8;
    public int mdef = 5;
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

    private void Awake()
    {
        moveDestination = transform.position;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (hp <= 0)
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

    public void GetWeaponAttack(ref int derictAtk, ref int inderictAtk)
    {
        Weapon weapon = GameManager.instance.gameElement.weapons[equipWeapon];
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

    public virtual void TurnOnGUI()
    {

    }

    public virtual void OnMouseDown()
    {

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

    public void OnGUI()
    {
        //display HP
        Vector3 location = Camera.main.WorldToScreenPoint(transform.position);// + Vector3.up * 35;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;
        GUI.Label(new Rect(location.x, Screen.height - location.y, 30, 20), hp.ToString(), style);
        //Debug.Log(playerName + ":(" + location.x + "," + location.y + "," + location.z + ")");
    }
}
