using UnityEngine;
using System.Collections;

public class UserPlayer : Player
{
    public Vector2 originalMapHexIndex
    {
        get
        {
            return new Vector2(originalGridPosition.x + (((int)originalGridPosition.y) >> 1), originalGridPosition.y);
        }
    }
    // Use this for initialization
    void Start()
    {

    }

    public override void SetOriginalPos()
    {
        originalGridPosition = gridPosition;
        base.SetOriginalPos();
    }

    // Update is called once per frame
    public override void Update()
    {
        //if (GameManager.instance.players[GameManager.instance.currentPlayerIndex] == this)
        //{
        //    transform.GetComponent<Renderer>().material.color = Color.green;
        //}
        //else
        //{
        //    transform.GetComponent<Renderer>().material.color = Color.white;
        //}
        TurnUpdate();
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
                    GameManager.instance.ShowConfirmMenu();
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
                GameManager.instance.RemoveHighlightTiles();
                moving = true;
                attacking = false;
                GameManager.instance.HighlightTileAt(gridPosition, Color.blue, movementPerActionPoint, false);
            }
            else
            {
                moving = false;
                attacking = false;
                GameManager.instance.RemoveHighlightTiles();
            }
        }
        //attack button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 2, buttonWight, buttonHeight);

        if (GUI.Button(buttonRect, "Attack"))
        {
            if (!attacking)
            {
                GameManager.instance.RemoveHighlightTiles();
                moving = false;
                attacking = true;
                GameManager.instance.HighlightTileAt(gridPosition, Color.red, attackRange);
            }
            else
            {
                moving = false;
                attacking = false;
                GameManager.instance.RemoveHighlightTiles();
            }
        }
        //end turn button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 1, buttonWight, buttonHeight);

        if (GUI.Button(buttonRect,"End Turn"))
        {
            GameManager.instance.RemoveHighlightTiles();
            actionPoint = 2;
            moving = false;
            attacking = false;
            GameManager.instance.NextTurn();
        }

        base.TurnOnGUI();
    }
}
