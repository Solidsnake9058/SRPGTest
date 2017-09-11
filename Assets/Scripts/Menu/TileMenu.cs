using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TileMenu : MonoBehaviour
{
    public static TileMenu instance;

    public Image menuBase;

    [HideInInspector]
    public string[] tileMenu = { "ShowEndTurn", "Unit", "Save", "Load", "Setting", "EndGame" };
    [HideInInspector]
    public string[] playerMenu = { "Move", "Action", "Weapon", "Item", "ItemSwitch", "Status" };
    [HideInInspector]
    public string[] playerStandMenu = { "Weapon", "Item", "ItemSwitch", "Status" };
    [HideInInspector]
    public string[] playerDeadMenu = { "Status" };
    [HideInInspector]
    public string[] playerMoveCanAtkMenu = { "Confirm", "Action", "Cancel" };
    [HideInInspector]
    public string[] playerMoveCantAtkMenu = { "Confirm", "Cancel" };

    private void Awake()
    {
        instance = this;
    }

    public Vector2 SetMenu(MenuType menuType)
    {
        string[] temp = (string[])GetType().GetField(menuType.ToString()).GetValue(this);
        List<Button> buttons = GetComponentsInChildren<Button>(true).ToList();
        for (int i = 0; i < buttons.Count; i++)
        {
            if (temp.Contains(buttons[i].name))
            {
                buttons[i].gameObject.SetActive(true);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }

        List<Button> enableButtons = buttons.Where(x => x.gameObject.activeSelf == true).ToList();
        Vector2 newSize = new Vector2(180, 20 + enableButtons.Count * 40);
        menuBase.rectTransform.sizeDelta = newSize;
        for (int i = 0; i < enableButtons.Count; i++)
        {
            enableButtons[i].image.rectTransform.position = menuBase.rectTransform.position + new Vector3(90, -(10 + 40 * i), 0);
        }

        return newSize;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
