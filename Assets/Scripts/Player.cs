using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public Vector2 gridPosition = Vector2.zero;

    public Vector3 moveDestination;
    public float moveSpeed = 10f;

    public int movementPerActionPoint = 5;
    public int attackRange = 1;

    public bool moving = false;
    public bool attacking = false;

    public string playerName;
    public int HP = 25;

    public float attackChance = 0.75f;
    public float defenseReduction = 0.15f;
    public int damageBase = 5;
    public float damageRollSides = 6;

    public int actionPoint = 2;

    private void Awake()
    {
        moveDestination = transform.position;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void TurnUpdate()
    {
        if (actionPoint <=0)
        {
            actionPoint = 2;
            moving = false;
            attacking = false;
            GameManager.inatance.nextTurn();
        }
    }

    public virtual void TurnOnGUI()
    {

    }
}
