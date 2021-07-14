using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TileMenu : MonoBehaviour
{
    public static TileMenu instance;

    [SerializeField]
    private Image menuBase;

    [HideInInspector]
    public string[] tileMenu = { "ShowEndTurn", "Unit", "Save", "Load", "Setting", "EndGame" };
    [HideInInspector]
    public string[] playerMenu = { "Move", "Action", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] playerShopMenu = { "Move", "Action", "Shop", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] playerMoveMenu = { "Move", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] playerMoveShopMenu = { "Move", "Weapon", "Shop", "Item", "Status" };
    [HideInInspector]
    public string[] playerStandMenu = { "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] playerStandShopMenu = { "Shop", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] playerDeadMenu = { "Status" };
    [HideInInspector]
    public string[] playerMoveCanAtkMenu = { "Confirm", "Action", "Cancel" };
    [HideInInspector]
    public string[] playerMoveCantAtkMenu = { "Confirm", "Cancel" };

    private Dictionary<string, GameObject> m_ButtonDic = new Dictionary<string, GameObject>();
    private GameObject[] m_Buttons;
    private Vector2 m_ButtonBaseSize;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        m_Buttons = new GameObject[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            m_ButtonDic.Add(buttons[i].name, buttons[i].gameObject);
            m_Buttons[i] = buttons[i].gameObject;
        }
        m_ButtonBaseSize = m_Buttons[0].GetComponent<Image>().rectTransform.sizeDelta;
    }

    public Vector2 SetMenu(MenuType menuType)
    {
        string[] temp = (string[])GetType().GetField(menuType.ToString()).GetValue(this);
        //List<Button> buttons = GetComponentsInChildren<Button>(true).ToList();
        Vector2 newSize = new Vector2(m_ButtonBaseSize.x + 20, 20 + temp.Length * m_ButtonBaseSize.y);
		menuBase.rectTransform.sizeDelta = newSize;
		//int Count = 0;
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            m_Buttons[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < temp.Length; i++)
        {
            m_ButtonDic[temp[i]].SetActive(true);
        }
        //for (int i = 0; i < buttons.Count; i++)
        //{
        //    if (temp.Contains(buttons[i].name))
        //    {
        //        buttons[i].image.rectTransform.position = menuBase.rectTransform.position + new Vector3(newSize.x / 2, -(10 + 40 * Count), 0);
        //        Count++;
        //    }
        //    else
        //    {
        //        buttons[i].image.rectTransform.position = menuBase.rectTransform.position + new Vector3(Screen.width + 100, Screen.height + 100, 0);
        //    }
        //}

        return newSize;
    }

}
