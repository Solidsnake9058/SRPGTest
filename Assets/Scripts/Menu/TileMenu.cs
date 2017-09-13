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
    public string[] playerMenu = { "Move", "Action", "Weapon", "Item", "Switch", "Status" };
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
        Vector2 newSize = new Vector2(180, 20 + temp.Length * 40);
        menuBase.rectTransform.sizeDelta = newSize;
        List<Button> buttons = GetComponentsInChildren<Button>(true).ToList();
        int Count = 0;
        for (int i = 0; i < buttons.Count; i++)
        {
            if (temp.Contains(buttons[i].name))
            {
                buttons[i].image.rectTransform.position = menuBase.rectTransform.position + new Vector3(90, -(10 + 40 * Count), 0);
                Count++;
            }
            else
            {
                buttons[i].image.rectTransform.position = menuBase.rectTransform.position + new Vector3(Screen.width + 100, Screen.height + 100, 0);
            }
        }

        return newSize;
    }

}
