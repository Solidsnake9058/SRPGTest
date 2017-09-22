using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    public Player player;
    public Image playerHP;
    public Text playerHPText;
    public Text playerEnable;
    private float uiHeight = 1f;
    public float standradUIHeight = 1f;

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
        if (!GameManager.instance.GetIsWaitingBattle())
        {
            transform.position = player.transform.position + new Vector3(0, uiHeight, 0);
            playerHP.fillAmount = player.hp / (float)player.maxHP;
            playerHPText.text = string.Format("{0}/{1}", player.hp, player.maxHP);
            playerEnable.enabled = !player.isActable;
            playerEnable.text = player.hp > 0 ? "<color=lightblue>E</color>" : "<color=red>D</color>";
        }
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
