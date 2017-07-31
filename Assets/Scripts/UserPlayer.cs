using UnityEngine;
using System.Collections;

public class UserPlayer : Player
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex] == this)
        {
            transform.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            transform.GetComponent<Renderer>().material.color = Color.white;
        }

        if (HP <= 0)
        {
            transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            transform.GetComponent<Renderer>().material.color = Color.gray;
        }
    }

    public override void TurnUpdate()
    {
        if (Vector3.Distance(moveDestination, transform.position) > 0.1f)
        {
            transform.position += (moveDestination - transform.position).normalized * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(moveDestination, transform.position) <= 0.1f)
            {
                transform.position = moveDestination;
                actionPoint--;
            }
        }

        base.TurnUpdate();
    }

    public override void TurnOnGUI()
    {
        float buttonHeight = 50;
        float buttonWight = 150;

        Rect buttonRect = new Rect(0, Screen.height - buttonHeight * 3, buttonWight, buttonHeight);

        //move button
        if (GUI.Button(buttonRect, "Move"))
        {
            if (!moving)
            {
                moving = true;
                attacking = false;
            }
            else
            {
                moving = false;
                attacking = false;
            }
        }
        //attack button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 2, buttonWight, buttonHeight);

        if (GUI.Button(buttonRect, "Attack"))
        {
            if (!attacking)
            {
                moving = false;
                attacking = true;
            }
            else
            {
                moving = false;
                attacking = false;
            }
        }
        //end turn button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 1, buttonWight, buttonHeight);

        if (GUI.Button(buttonRect,"End Turn"))
        {
            actionPoint = 2;
            moving = false;
            attacking = false;
            GameManager.inatance.nextTurn();
        }

        base.TurnOnGUI();
    }
}
