using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : IGameUISystem
{
    protected static GameUIManager m_GameUIManager { get { return GameMidiator.m_Instance.m_GameUIManager; } }
    protected static StageManager m_StageManager { get { return GameMidiator.m_Instance.m_StageManager; } }

    [Header("Tile menu")]
    private string[] m_TileMenu = { "ShowEndTurn", "Unit", "Save", "Load", "Setting", "EndGame" };
    private string[] m_PlayerShopMenu = { "Move", "Action", "Shop", "Weapon", "Item", "Status" };
    private string[] m_PlayerStandShopMenu = { "Shop", "Weapon", "Item", "Status" };
    private string[] m_PlayerDeadMenu = { "Status" };
    private string[] m_PlayerMoveCanAtkMenu = { "Confirm", "Action", "Cancel" };

    /*
     TileMenu,
    PlayerShopMenu,
    PlayerStandShopMenu,
    PlayerDeadMenu,
    PlayerMoveCanAtkMenu,
     */

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

    [Header("Confirm")]
    [SerializeField]
    private GameObject m_ConfireBase = default;
    [SerializeField]
    private Button m_ButtonCOnfirmYes = default;
    [SerializeField]
    private Button m_ButtonCOnfirmNo = default;

    [SerializeField]
    private UIBlock m_UIBlock = default;
    private Image m_UIBlockImage = default;

    private Camera m_MainCamera
    {
        get
        {
            if (_MainCamera == null)
            {
                _MainCamera = Camera.main;
            }
            return _MainCamera;
        }
    }
    private Camera _MainCamera;


    public bool m_IsMsgShowing { get; private set; }
    public bool m_IsStageInfoShowing { get; private set; }

    public bool m_IsMenuShowing { get; private set; }
    public bool m_IsEndConfirmShowing { get; private set; }
    private int m_TileMask;

    public override void GameSetting()
    {
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            m_ButtonDic.Add(m_Buttons[i].name, m_Buttons[i].gameObject);
            string btnName = m_Buttons[i].name;
            m_Buttons[i].onClick.AddListener(() => MenuButtonClick(btnName));
        }
        //m_ButtonBaseSize = m_Buttons[0].image.rectTransform.sizeDelta;
        m_ContentSizeFitter = m_MenuBoard.GetComponent<ContentSizeFitter>();
        m_MenuTypeDic.Add(MenuType.TileMenu, m_TileMenu);
        m_MenuTypeDic.Add(MenuType.PlayerShopMenu, m_PlayerShopMenu);
        m_MenuTypeDic.Add(MenuType.PlayerStandShopMenu, m_PlayerStandShopMenu);
        m_MenuTypeDic.Add(MenuType.PlayerDeadMenu, m_PlayerDeadMenu);
        m_MenuTypeDic.Add(MenuType.PlayerMoveCanAtkMenu, m_PlayerMoveCanAtkMenu);

        //for (MenuType i = MenuType.START + 1; i < MenuType.MAX; i++)
        //{
        //    MenuType menuType = i;
        //    string[] temp = (string[])GetType().GetField("m_" + i.ToString()).GetValue(this);
        //    if (temp != null)
        //    {
        //        m_MenuTypeDic.Add(menuType, temp);
        //    }
        //}
        m_UIBlockImage = m_UIBlock.GetComponent<Image>();
        //m_UIBlock.onClick.AddListener(UIBlockClick);
        m_UIBlock.SetPointEvent(UIBlockClick);
        UIBlockSwitch(false);
        SetMsgShow(false);

        m_TileMask = LayerMask.GetMask("Tile");
        m_ButtonCOnfirmYes.onClick.AddListener(ConfirmYesClick);
        m_ButtonCOnfirmNo.onClick.AddListener(ConfirmNoClick);
    }

    private void UIBlockClick(PointerEventData pointerEventData)
    {
        if (m_IsMsgShowing)
        {
            SetMsgShow(false);
            if (m_IsStageInfoShowing)
            {
                m_IsStageInfoShowing = false;
                ShowStageTrunInfo();
            }
            //TODO call hide msg action
        }
        else
        {
            if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                GameManager.m_Instance.Cancel();
                UIBlockSwitch(false);
                HideMenu();
            }
            else if (pointerEventData.button == PointerEventData.InputButton.Left)
            {
                Vector3 clickPos = Input.mousePosition;
                Vector3 worldPos = m_MainCamera.ScreenToWorldPoint(clickPos);

                RaycastHit raycastHit;
                Ray ray = new Ray(worldPos, m_MainCamera.transform.forward);
                if (Physics.Raycast(ray, out raycastHit, 100, m_TileMask))
                {
                    HexTile hexTile = raycastHit.transform.GetComponent<HexTile>();
                    PointerEventData eventData = new PointerEventData(EventSystem.current);
                    hexTile.ClickEventGame(eventData);
                }
            }
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

    public void SetMenu(MenuType menuType, bool isAction, bool isShop, Vector3 worldPos)
    {
        Vector3 pos = m_MainCamera.WorldToScreenPoint(worldPos);
        if (m_MenuTypeDic.ContainsKey(menuType))
        {
            m_IsMenuShowing = true;
            string[] list = m_MenuTypeDic[menuType];

            for (int i = 0; i < m_Buttons.Length; i++)
            {
                m_Buttons[i].transform.SetParent(m_MenuButtonTemp);
            }
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Equals("Shop") && !isShop)
                {
                    continue;
                }
                if (list[i].Equals("Action") && !isAction)
                {
                    continue;
                }
                m_ButtonDic[list[i]].transform.SetParent(m_MenuBoard.transform);
            }
            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            StartCoroutine(SetMenuPos(pos));
            UIBlockSwitch(true);
        }
    }

    private IEnumerator SetMenuPos(Vector2 pos)
    {
        yield return null;
        Vector2 newSize = m_MenuBoard.rectTransform.sizeDelta;
        Vector2 refRes = m_GameUIManager.CanvasRefRes;
        pos.x *= m_GameUIManager.UITransRate.x;
        pos.y *= m_GameUIManager.UITransRate.y;
        float newX = Mathf.Clamp(pos.x + newSize.x, 0, refRes.x) - newSize.x;
        float newY = Mathf.Clamp(pos.y - newSize.y, 0, refRes.y) + newSize.y;
        m_MenuBoard.rectTransform.anchoredPosition = new Vector2(newX, newY);
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
        m_IsMsgShowing = isShow;
        if (isShow)
        {
            UIBlockSwitch(isShow);
        }
        else
        {
            m_MsgBox.SetActive(isShow);
            m_PlayerMsgBox.SetActive(isShow);
        }
    }
    public void ShowStageInfo()
    {
        m_IsStageInfoShowing = true;
        m_StageInfoText.text = string.Format("Stage {0}\n{1}", 1, m_StageManager.StageTitle);
        m_MsgBox.SetActive(true);
        SetMsgShow();
    }

    public void ShowStageTrunInfo()
    {
        m_StageInfoText.text = string.Format("ターン {0}\n{1}の行動", GameManager.m_Instance.m_TurnCount, GameManager.m_Instance.m_IsPlayerTurn ? "アークたち" : "魔軍");
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
        m_PlayerMsgBox.SetActive(true);
        SetMsgShow();
    }

    public void ShowEquipWeaponInfo(string originalWeapon, string newWeapon)
    {
        m_PlayerInfoText.text = string.Format("{0}を外して\n{1}を装備します", string.Format("<color=yellow>{0}</color>", originalWeapon), string.Format("<color=yellow>{0}</color>", newWeapon));
        m_PlayerMsgBox.SetActive(true);
        SetMsgShow();
    }
    #endregion

    #region Confirm
    public void SetConfirmShow(bool isShow)
    {
        m_ConfireBase.SetActive(isShow);
    }

    private void ConfirmYesClick()
    {
        GameManager.m_Instance.TurnEnd();
    }

    private void ConfirmNoClick()
    {
        SetConfirmShow(false);
    }
    #endregion
}
