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
    public string[] playerMenu = { "Move", "Action", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] playerMoveMenu = { "Move", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] playerStandMenu = { "Weapon", "Item", "Status" };
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
        Vector2 newSize = new Vector2(buttons[0].GetComponent<Image>().rectTransform.sizeDelta.x + 20, 20 + temp.Length * 40);
		menuBase.rectTransform.sizeDelta = newSize;
		int Count = 0;
        for (int i = 0; i < buttons.Count; i++)
        {
            if (temp.Contains(buttons[i].name))
            {
                buttons[i].image.rectTransform.position = menuBase.rectTransform.position + new Vector3(newSize.x / 2, -(10 + 40 * Count), 0);
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
