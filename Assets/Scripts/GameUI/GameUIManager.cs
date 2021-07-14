using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : IGameItem
{
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
        m_StatusUI.Initialize(gameManager);
        m_UnitListUI.Initialize(gameManager);
        m_ItemUI.Initialize(gameManager);
        m_WeaponUI.Initialize(gameManager);
        m_ShopUI.Initialize(gameManager);
    }
    public override void GameSetting()
    {
        m_StatusUI.GameSetting();
        m_UnitListUI.GameSetting();
        m_ItemUI.GameSetting();
        m_WeaponUI.GameSetting();
        m_ShopUI.GameSetting();
    }

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
}
