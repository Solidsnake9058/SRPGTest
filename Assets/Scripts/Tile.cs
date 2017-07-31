using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{

    public Vector2 gridPostion = Vector2.zero;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseEnter()
    {
        if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex].moving)
        {
            transform.GetComponent<Renderer>().material.color = Color.blue;
        }
        else if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex].attacking)
        {
            transform.GetComponent<Renderer>().material.color = Color.red;
        }

        //Debug.Log("Pos (" + gridPostion.x + "," + gridPostion.y + ")");
    }

    private void OnMouseExit()
    {
        transform.GetComponent<Renderer>().material.color = Color.white;

    }

    private void OnMouseDown()
    {
        if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex].moving)
        {
            GameManager.inatance.moveCurrentPlayer(this);
        }
        else if (GameManager.inatance.players[GameManager.inatance.currentPlayerIndex].attacking)
        {
            GameManager.inatance.attackWithCurrentPlayer(this);
        }

    }
}
