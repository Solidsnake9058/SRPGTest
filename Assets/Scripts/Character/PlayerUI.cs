﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    public Player player;
    public Image playerHP;
    public Text playerHPText;
    public Text playerEnable;
    private float uiHeight = 0.3f;
    public float standradUIHeight = 0.3f;

    private void Awake()
    {
        Transform ui = transform.Find("Canvas").GetComponent<Canvas>().transform;

        playerHP = ui.Find("HP").GetComponent<Image>();
        playerHPText = ui.Find("HPText").GetComponent<Text>();
        playerEnable = ui.Find("IsEnable").GetComponent<Text>();
        uiHeight = standradUIHeight;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + new Vector3(0, uiHeight, 0);
        playerHP.fillAmount = player.hp / (float)player.maxHP;
        playerHPText.text = string.Format("{0}/{1}", player.hp, player.maxHP);
        playerEnable.enabled = !player.isActable;
    }

    public void SetShowUI()
    {
        uiHeight = standradUIHeight;
    }

    public void SetHideUI()
    {
        uiHeight = -100f;
    }

}