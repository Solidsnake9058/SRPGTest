using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : IGameUISystem
{
    [Header("Tile menu")]
    [HideInInspector]
    public string[] m_TileMenu = { "ShowEndTurn", "Unit", "Save", "Load", "Setting", "EndGame" };
    [HideInInspector]
    public string[] m_PlayerMenu = { "Move", "Action", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] m_PlayerShopMenu = { "Move", "Action", "Shop", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] m_PlayerMoveMenu = { "Move", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] m_PlayerMoveShopMenu = { "Move", "Weapon", "Shop", "Item", "Status" };
    [HideInInspector]
    public string[] m_PlayerStandMenu = { "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] m_PlayerStandShopMenu = { "Shop", "Weapon", "Item", "Status" };
    [HideInInspector]
    public string[] m_PlayerDeadMenu = { "Status" };
    [HideInInspector]
    public string[] m_PlayerMoveCanAtkMenu = { "Confirm", "Action", "Cancel" };
    [HideInInspector]
    public string[] m_PlayerMoveCantAtkMenu = { "Confirm", "Cancel" };

    [SerializeField]
    private Image m_MenuBoard;
    private ContentSizeFitter m_ContentSizeFitter;
    [SerializeField]
    private Transform m_MenuButtonTemp = default;

    [SerializeField]
    private Button[] m_Buttons;
    private Dictionary<string, GameObject> m_ButtonDic = new Dictionary<string, GameObject>();
    private Dictionary<MenuType, string[]> m_MenuTypeDic = new Dictionary<MenuType, string[]>();

    [Header("System message")]
    [SerializeField]
    private Text m_StageInfoText;
    [SerializeField]
    private GameObject m_MsgBox;
    [SerializeField]
    private Text m_PlayerInfoText;
    [SerializeField]
    private GameObject m_PlayerMsgBox;

    [SerializeField]
    private Button m_UIBlock = default;
    private Image m_UIBlockImage = default;

    public bool m_IsMsgShowing { get; private set; }
    public bool m_IsMenuShowing { get; private set; }
    public bool m_IsEndConfirmShowing { get; private set; }

    public override void GameSetting()
    {
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            m_ButtonDic.Add(m_Buttons[i].name, m_Buttons[i].gameObject);
            m_Buttons[i].onClick.AddListener(() => MenuButtonClick(m_Buttons[i].name.ToString()));
        }
        //m_ButtonBaseSize = m_Buttons[0].image.rectTransform.sizeDelta;
        m_ContentSizeFitter = m_MenuBoard.GetComponent<ContentSizeFitter>();
        for (MenuType i = MenuType.START + 1; i < MenuType.MAX; i++)
        {
            MenuType menuType = i;
            string[] temp = (string[])GetType().GetField("m_" + i.ToString()).GetValue(this);
            if (temp != null)
            {
                m_MenuTypeDic.Add(menuType, temp);
            }
        }
        m_UIBlockImage = m_UIBlock.GetComponent<Image>();
        m_UIBlock.onClick.AddListener(UIBlockClick);
        UIBlockSwitch(false);
        SetMsgShow(false);
    }

    private void UIBlockClick()
    {
        if (m_IsMsgShowing)
        {
            SetMsgShow(false);
            //TODO call hide msg action
        }
        else
        {
            GameManager.m_Instance.CancelAction();
        }
    }

    private void UIBlockSwitch(bool isBlock)
    {
        //m_UIBlock.enabled = isBlock;
        m_UIBlockImage.raycastTarget = isBlock;
    }

    #region Menu Button
    private void MenuButtonClick(string buttonName)
    {
        GameManager.m_Instance.ClickButtonAction(buttonName);
        UIBlockSwitch(false);
        HideMenu();
    }

    public Vector2 SetMenu(MenuType menuType, Vector3 worldPos)
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(worldPos);
        if (m_MenuTypeDic.ContainsKey(menuType))
        {
            m_IsMenuShowing = true;
            string[] list = m_MenuTypeDic[menuType];

            for (int i = 0; i < list.Length; i++)
            {
                m_Buttons[i].transform.SetParent(m_MenuBoard.transform);
            }
            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Vector2 newSize = m_MenuBoard.rectTransform.sizeDelta;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            float newX = Mathf.Clamp(pos.x + newSize.x, 0, screenSize.x) - newSize.x;
            float newY = Mathf.Clamp(pos.y + newSize.y, 0, screenSize.y) - newSize.y;
            m_MenuBoard.rectTransform.anchoredPosition = new Vector2(newX, newY);
            UIBlockSwitch(true);
        }
        return m_MenuBoard.rectTransform.sizeDelta;
    }

    public Vector2 SetMenu(MenuType menuType, bool isShop, Vector3 worldPos)
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(worldPos);
        if (m_MenuTypeDic.ContainsKey(menuType))
        {
            m_IsMenuShowing = true;
            string[] list = m_MenuTypeDic[menuType];

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Equals("Shop") && !isShop)
                {
                    continue;
                }
                m_ButtonDic[list[i]].transform.SetParent(m_MenuBoard.transform);
            }
            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Vector2 newSize = m_MenuBoard.rectTransform.sizeDelta;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            float newX = Mathf.Clamp(pos.x + newSize.x, 0, screenSize.x) - newSize.x;
            float newY = Mathf.Clamp(pos.y + newSize.y, 0, screenSize.y) - newSize.y;
            m_MenuBoard.rectTransform.anchoredPosition = new Vector2(newX, newY);
            UIBlockSwitch(true);
        }
        return m_MenuBoard.rectTransform.sizeDelta;
    }

    public void HideMenu()
    {
        m_IsMenuShowing = false;
        m_MenuBoard.rectTransform.anchoredPosition = new Vector2(-1000, -1000);
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            m_Buttons[i].transform.SetParent(m_MenuButtonTemp);
        }
    }
    #endregion

    #region Message
    private void SetMsgShow(bool isShow = true)
    {
        if (isShow)
        {
            m_IsMsgShowing = true;
            UIBlockSwitch(true);
        }
        else
        {
            m_IsMsgShowing = false;
            m_MsgBox.SetActive(false);
            m_PlayerMsgBox.SetActive(false);
        }
    }
    public void ShowStageInfo()
    {
        m_StageInfoText.text = string.Format("Stage {0}\n{1}", 1, GameMidiator.m_Instance.m_StageManager.StageTitle);
        m_MsgBox.SetActive(true);
        SetMsgShow();
    }

    public void ShowStageTrunInfo(int turnCount, bool isPlayerTurn)
    {
        m_StageInfoText.text = string.Format("ターン {0}\n{1}の行動", turnCount, isPlayerTurn ? "アークたち" : "魔軍");
        m_MsgBox.SetActive(true);
        SetMsgShow();
    }

    public void ShowUseItemInfo(string name, int hp, int atk, int def, int dex, int wis, int maxHP, int gold, string newCharType)
    {
        List<string> msg = new List<string>();
        if (gold > 0)
        {
            msg.Add(string.Format("<color=orange>{0}</color>得る！", gold));
        }
        if (hp > 0)
        {
            msg.Add(string.Format("の体力は<color=lime>{0}</color>回復する！", hp));
        }
        if (atk > 0)
        {
            msg.Add(string.Format("の攻撃力は<color=lime>{0}</color>アップする！", atk));
        }
        if (def > 0)
        {
            msg.Add(string.Format("の防御力は<color=lime>{0}</color>アップする！", def));
        }
        if (dex > 0)
        {
            msg.Add(string.Format("の敏捷さは<color=lime>{0}</color>アップする！", dex));
        }
        if (wis > 0)
        {
            msg.Add(string.Format("の知力は<color=lime>{0}</color>アップする！", wis));
        }
        if (maxHP > 0)
        {
            msg.Add(string.Format("の体力は<color=lime>{0}</color>アップする！", maxHP));
        }

        if (!string.IsNullOrEmpty(newCharType))
        {
            msg.Add("のクラスは{0}なる！" + newCharType);
        }
        m_StageInfoText.text = string.Format("{0}\n{1}", name, string.Join("\n", msg));
        m_MsgBox.SetActive(true);
        SetMsgShow();
    }

    public void ShowGetItemInfo(int gold, int itemId, int weaponId)
    {
        //TODO item/weapon name
        List<string> msg = new List<string>();
        if (gold > 0)
        {
            msg.Add("<color=orange>" + gold + "</color>Gold");
        }
        if (itemId > 0)
        {
            //msg.Add(gameElement.items.Where(x => x.id == itemId).FirstOrDefault().name);
        }
        if (weaponId > 0)
        {
            //msg.Add(gameElement.weapons.Where(x => x.id == weaponId).FirstOrDefault().name);
        }
        m_PlayerInfoText.text = string.Format("宝箱を見つけた！\n宝箱の中から {0}をみつけた！", string.Join("\n", msg));
        SetMsgShow();
    }

    public void ShowEquipWeaponInfo(string originalWeapon, string newWeapon)
    {
        m_PlayerInfoText.text = string.Format("{0}を外して\n{1}を装備します", string.Format("<color=yellow>{0}</color>", originalWeapon), string.Format("<color=yellow>{0}</color>", newWeapon));
        m_MsgBox.SetActive(true);
        SetMsgShow();
    }
    #endregion
}
