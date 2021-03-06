﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : IGameUISystem
{
    [Header("UI Item")]
    [SerializeField]
    private Text m_PlayerName;
    [SerializeField]
    private Text m_PlayerClass;
    [SerializeField]
    private Text m_PlayerLevel;
    [SerializeField]
    private Text m_PlayerHP;
    [SerializeField]
    private Text m_PlayerMaxHP;
    [SerializeField]
    private Text m_PlayerExp;
    [SerializeField]
    private Text m_PlayerAtk;
    [SerializeField]
    private Text m_PlayerWeaponAtk;
    [SerializeField]
    private Text m_PlayerDef;
    [SerializeField]
    private Text m_PlayerWis;
    [SerializeField]
    private Text m_PlayerDex;
    [SerializeField]
    private Text m_PlayerMdef;
    [SerializeField]
    private Text m_PlayerEquip;
    [SerializeField]
    private Text m_PlayerEquipRange;

    [SerializeField]
    private Button m_ConfirmButton = default;

    public override void GameSetting()
    {
        m_ConfirmButton.onClick.AddListener(HideUI);
    }
    public void SetPlayerStatusUI(Player player, CharacterType race, Weapon weapon)
    {
        int directAtk;
        int indirectAtk;
        player.GetWeaponAttack(out directAtk, out indirectAtk);

        List<string> weaponRangeText = new List<string>();
        if (directAtk > 0)
        {
            weaponRangeText.Add("<color=orange>直接</color>");
        }
        if (indirectAtk > 0)
        {
            weaponRangeText.Add("<color=lime>間接</color>");
        }

        m_PlayerName.text = player.playerName;
        m_PlayerClass.text = race.name;
        m_PlayerLevel.text = player.level.ToString();
        m_PlayerHP.text = player.hp.ToString() + "/";
        m_PlayerMaxHP.text = player.maxHP.ToString();
        m_PlayerExp.text = player.exp.ToString();
        m_PlayerAtk.text = player.atk.ToString();
        m_PlayerWeaponAtk.text = $"<color=white>(</color><color=orange>{"+" + directAtk.ToString()}</color><color=white>/</color><color=lime>{"+" + indirectAtk.ToString()}</color><color=white>)</color>";
        m_PlayerWeaponAtk.text = m_PlayerWeaponAtk.text.Replace("+0", "✕");
        m_PlayerDef.text = player.def.ToString();
        m_PlayerWis.text = player.wis.ToString();
        m_PlayerDex.text = player.dex.ToString();
        m_PlayerMdef.text = player.mdef.ToString();
        m_PlayerEquip.text = weapon.name;
        m_PlayerEquipRange.text = string.Join("<color=white>/</color>", weaponRangeText.ToArray());
    }
}
