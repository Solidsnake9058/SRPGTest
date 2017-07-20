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
        transform.GetComponent<Renderer>().material.color = Color.blue;

        Debug.Log("Pos (" + gridPostion.x + "," + gridPostion.y + ")");
    }

    private void OnMouseExit()
    {
        transform.GetComponent<Renderer>().material.color = Color.white;

    }

    private void OnMouseDown()
    {
        GameManager.inatance.moveCurrentPlayer(this);
    }
}
