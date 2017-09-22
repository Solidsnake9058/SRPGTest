using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public override void TurnEnd()
    {
        HexTile tile = GameManager.instance.mapHex[(int)mapHexIndex.y][(int)mapHexIndex.x];
        if (tile.isHaveChest && !tile.isChestOpened)
        {
            tile.OpenChest();
        }
        base.TurnEnd();
    }

    public override void TurnUpdate()
    {
        //highlight

        if (positionQueue.Count > 0)
        {
            transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
            transform.LookAt(positionQueue[0]);
            animator.SetBool("walk", true);
            if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f)
            {
                transform.position = positionQueue[0];
                positionQueue.RemoveAt(0);
                if (positionQueue.Count == 0)
                {
                    //actionPoint--;
                    animator.SetBool("walk", false);
                    //transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    GameManager.instance.ShowConfirmMenu();
                }
            }
        }

        base.TurnUpdate();
    }

    public override PlayerRecord LevelUp()
    {
        //hp atk def dex wis mdef
        if (level >= 20)
        {
            return new PlayerRecord();
        }

        List<LevelProp> lvprops = new List<LevelProp>();
        for (int i = 0; i < (int)LevelProp.max; i++)
        {
            lvprops.Add((LevelProp)i);
        }
        lvprops = Shuffle(lvprops);
        int upCount = Random.Range(1, 4);

        level++;
        int addValue = 0;
        for (int i = 0; i < upCount; i++)
        {
            addValue = Random.Range(1, 3);
            switch (lvprops[i])
            {
                case LevelProp.hp:
                    maxHP += addValue;
                    break;
                case LevelProp.atk:
                    if (atk + addValue > 45)
                    {
                        addValue = 45 - atk;
                    }
                    atk += addValue;
                    break;
                case LevelProp.def:
                    if (atk + addValue > 45)
                    {
                        addValue = 45 - def;
                    }
                    def += addValue;
                    break;
                case LevelProp.dex:
                    if (atk + addValue > 45)
                    {
                        addValue = 45 - dex;
                    }
                    dex += addValue;
                    break;
                case LevelProp.wis:
                    if (atk + addValue > 45)
                    {
                        addValue = 45 - wis;
                    }
                    wis += addValue;
                    break;
                case LevelProp.mdef:
                    if (atk + addValue > 45)
                    {
                        addValue = 45 - mdef;
                    }
                    mdef += addValue;
                    break;
            }
            if (addValue == 0 && upCount <= (int)LevelProp.max)
            {
                upCount++;
            }
        }

        return new PlayerRecord((uint)hp, (uint)atk, (uint)def, (uint)wis, (uint)dex, (uint)mdef);
    }

    public override void TurnOnGUI()
    {
        //float buttonHeight = 50;
        //float buttonWight = 150;

        //Rect buttonRect = new Rect(0, Screen.height - buttonHeight * 3, buttonWight, buttonHeight);

        ////move button
        //if (GUI.Button(buttonRect, "Move"))
        //{
        //    if (!moving)
        //    {
        //        GameManager.instance.RemoveHighlightTiles();
        //        moving = true;
        //        attacking = false;
        //        GameManager.instance.HighlightTileAt(gridPosition, Color.blue, (int)movementPerActionPoint, false);
        //    }
        //    else
        //    {
        //        moving = false;
        //        attacking = false;
        //        GameManager.instance.RemoveHighlightTiles();
        //    }
        //}
        ////attack button
        //buttonRect = new Rect(0, Screen.height - buttonHeight * 2, buttonWight, buttonHeight);

        //if (GUI.Button(buttonRect, "Attack"))
        //{
        //    if (!attacking)
        //    {
        //        GameManager.instance.RemoveHighlightTiles();
        //        moving = false;
        //        attacking = true;
        //        GameManager.instance.HighlightTileAt(gridPosition, Color.red, attackRange);
        //    }
        //    else
        //    {
        //        moving = false;
        //        attacking = false;
        //        GameManager.instance.RemoveHighlightTiles();
        //    }
        //}
        ////end turn button
        //buttonRect = new Rect(0, Screen.height - buttonHeight * 1, buttonWight, buttonHeight);

        //if (GUI.Button(buttonRect,"End Turn"))
        //{
        //    GameManager.instance.RemoveHighlightTiles();
        //    actionPoint = 2;
        //    moving = false;
        //    attacking = false;
        //    GameManager.instance.NextTurn();
        //}

        //base.TurnOnGUI();
    }
}
