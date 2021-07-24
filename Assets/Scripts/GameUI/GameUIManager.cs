using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : IGameItem
{
    [SerializeField]
    private GameUI m_GameUI = default;

    [SerializeField]
    private DialogUI m_DialogUI = default;
    [SerializeField]
    private StatusUI m_StatusUI = default;
    [SerializeField]
    private UnitListUI m_UnitListUI = default;
    [SerializeField]
    private ItemUI m_ItemUI = default;
    [SerializeField]
    private WeaponUI m_WeaponUI = default;
    [SerializeField]
    private ShopUI m_ShopUI = default;

    public override void Initialize(GameManager gameManager)
    {
        m_GameUI.Initialize(gameManager);
        m_DialogUI.Initialize(gameManager);
        m_StatusUI.Initialize(gameManager);
        m_UnitListUI.Initialize(gameManager);
        m_ItemUI.Initialize(gameManager);
        m_WeaponUI.Initialize(gameManager);
        m_ShopUI.Initialize(gameManager);
    }
    public override void GameSetting()
    {
        m_GameUI.GameSetting();
        m_DialogUI.GameSetting();
        m_StatusUI.GameSetting();
        m_UnitListUI.GameSetting();
        m_ItemUI.GameSetting();
        m_WeaponUI.GameSetting();
        m_ShopUI.GameSetting();
    }

    #region Game UI
    public bool m_IsMenuShowing { get { return m_GameUI.m_IsMenuShowing; } }
    public bool m_IsEndConfirmShowing { get { return m_GameUI.m_IsEndConfirmShowing; } }
    public bool m_IsUIMenuShowing { get { return m_GameUI.m_IsEndConfirmShowing || m_GameUI.m_IsEndConfirmShowing; } }

    public Vector2 SetMenu(MenuType menuType, Vector3 worldPos)
    {
        return m_GameUI.SetMenu(menuType, worldPos);
    }

    public Vector2 SetMenu(MenuType menuType, bool isShop, Vector3 worldPos)
    {
        return m_GameUI.SetMenu(menuType, isShop, worldPos);
    }

    public void HideMenu()
    {
        m_GameUI.HideMenu();
    }

    //Msg
    public void ShowStageInfo()
    {
        m_GameUI.ShowStageInfo();
    }

    public void ShowStageTurnInfo(int turnCount, bool isPlayerTurn)
    {
        m_GameUI.ShowStageTrunInfo(turnCount, isPlayerTurn);
    }

    public void ShowUseItemInfo(string name, int hp, int atk, int def, int dex, int wis, int maxHP, int gold, string newCharType)
    {
        m_GameUI.ShowUseItemInfo(name, hp, atk, def, dex, wis, maxHP, gold, newCharType);
    }

    public void ShowGetItemInfo(int gold, int itemId, int weaponId)
    {
        m_GameUI.ShowGetItemInfo(gold, itemId, weaponId);
    }

    public void ShowEquipWeaponInfo(string originalWeapon, string newWeapon)
    {
        m_GameUI.ShowEquipWeaponInfo(originalWeapon, newWeapon);
    }
    #endregion

    public void SetPlayerStatusUI(Player player, CharacterType race, Weapon weapon)
    {
        m_StatusUI.SetPlayerStatusUI(player, race, weapon);
    }

    public void ShowStatusUI()
    {
        m_StatusUI.ShowUI();
    }

    public void ShowUnitListUI()
    {
        m_UnitListUI.ShowUI();
    }

    public void ShowItemUI()
    {
        m_ItemUI.ShowUI();
    }

    public void ShowWeaponUI()
    {
        m_WeaponUI.ShowUI();
    }

    public void ShowShopUI()
    {
        m_ShopUI.ShowUI();
    }

    public void SetDialog(string name, string content)
    {
        m_DialogUI.SetDialog(name, content);
    }
}
