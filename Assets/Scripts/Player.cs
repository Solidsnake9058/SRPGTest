﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    //movement animation
    public List<Vector3> positionQueue = new List<Vector3>();
    //

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
        if (HP <= 0)
        {
            transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            transform.GetComponent<Renderer>().material.color = Color.gray;
        }
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

    public void OnGUI()
    {
        //display HP
        Vector3 location = Camera.main.WorldToScreenPoint(transform.position);// + Vector3.up * 35;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;
        GUI.Label(new Rect(location.x, Screen.height - location.y, 30, 20), HP.ToString(), style);
        //Debug.Log(playerName + ":(" + location.x + "," + location.y + "," + location.z + ")");
    }
}
