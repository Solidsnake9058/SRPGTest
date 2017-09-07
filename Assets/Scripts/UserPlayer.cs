using UnityEngine;
using System.Collections;

public class UserPlayer : Player
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    public override void Update()
    {
        if (GameManager.instance.players[GameManager.instance.currentPlayerIndex] == this)
        {
            transform.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            transform.GetComponent<Renderer>().material.color = Color.white;
        }

        base.Update();
    }

    public override void TurnUpdate()
    {
        //highlight

        if (positionQueue.Count > 0)
        {
            transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f)
            {
                transform.position = positionQueue[0];
                positionQueue.RemoveAt(0);
                if (positionQueue.Count == 0)
                {
                    actionPoint--;
                }
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
                GameManager.instance.removeHighlightTiles();
                moving = true;
                attacking = false;
                GameManager.instance.highlightTileAt(gridPosition, Color.blue, movementPerActionPoint, false);
            }
            else
            {
                moving = false;
                attacking = false;
                GameManager.instance.removeHighlightTiles();
            }
        }
        //attack button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 2, buttonWight, buttonHeight);

        if (GUI.Button(buttonRect, "Attack"))
        {
            if (!attacking)
            {
                GameManager.instance.removeHighlightTiles();
                moving = false;
                attacking = true;
                GameManager.instance.highlightTileAt(gridPosition, Color.red, attackRange);
            }
            else
            {
                moving = false;
                attacking = false;
                GameManager.instance.removeHighlightTiles();
            }
        }
        //end turn button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 1, buttonWight, buttonHeight);

        if (GUI.Button(buttonRect,"End Turn"))
        {
            GameManager.instance.removeHighlightTiles();
            actionPoint = 2;
            moving = false;
            attacking = false;
            GameManager.instance.nextTurn();
        }

        base.TurnOnGUI();
    }
}
