using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance { get; private set; }

    public float startWaitTime = 1f;
    public Transform actor1;
    public Transform actor2;

    public Transform attacker;
    public Transform target;

    public Text player1Name;
    public Text player1Tile;
    public Text player1DefRate;

    public Text player2Name;
    public Text player2Tile;
    public Text player2DefRate;

    public Text turnInfo;

    public CanvasGroup msgGroup;
    public CanvasGroup expGroup;
    public CanvasGroup levelGroup;

    public Image msgBack;
    public Text msgText;

    public bool isPlayerAttack;
    public bool isCounter;
    public bool isFirstAttack = true;
    public bool isEndBattle = true;
    public bool isEndAnimation;

    public float attackDistance = 0.5f;
    public bool isIndirectAttack;
    public bool isHeal;
    public Camera mainCamera;

    public float waitTime = 0;

    public string DefRateText = "防御率　{0}％";

    public string getItemText = "{0}得る";
    public string levelUpText = "{0}はレベル{1}アップする";
    public string turnInfoText = "{0}の攻撃";
    public string turnHealInfoText = "{0}のヒール";
    public string damageInfoText = "{0}に{1}ダメージ与える";
    public string healInfoText = "{0}の体力は{1}回復する";


    public int getItemWeight = 300;
    public int levelUpWeight = 400;

    BattleSendData battleData = new BattleSendData();

    private bool isShowItem;
    private bool isShowExp;
    private bool isShowLevel;
    private bool isShowLevelUp;
    public bool isTest;

    [Header("Level UI")]
    public Text playerName;
    public Text playerClass;
    public Text playerHP;
    public Text playerMaxHP;
    public Text playerAtk;
    public Text playerDef;
    public Text playerWis;
    public Text playerDex;
    public Text playerMDef;
    public Text playerMaxHPUp;
    public Text playerAtkUp;
    public Text playerDefUp;
    public Text playerWisUp;
    public Text playerDexUp;
    public Text playerMDefUp;

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        battleData = new BattleSendData("ランティア", "魔軍隊長", "", "平原", "村", false, false, true, false, 10, 50, 48, 48, 16, 36, 36, 16, "", 72, 16, 0, "ナイト", null, null);

        battleData.playerData = new PlayerRecord(44, 44, 14, 8, 7, 12, 7);
        battleData.lvUpData = new PlayerRecord(2, 0, 1, 0, 1, 2, 0);

        if (GameManager.m_Instance != null)
        {
            if (GameManager.m_Instance.battleData != null && GameManager.m_Instance.battleData != default(BattleSendData))
            {
                battleData = GameManager.m_Instance.battleData;
            }
        }

        actor1.GetComponent<BattleActorController>().SetHP(battleData.attackerHP, battleData.attackerMaxHP, battleData.damageByTarget, battleData.attacker);
        actor2.GetComponent<BattleActorController>().SetHP(battleData.targetHP, battleData.targetMaxHP, battleData.damageByAttacker, battleData.target);

        isPlayerAttack = battleData.isPlayerAttack;
        isCounter = battleData.isCounter;
        isIndirectAttack = !battleData.isDirect;
        isHeal = battleData.isHeal;

        player1Name.text = battleData.attacker;
        player1Tile.text = battleData.attackerTile;
        player1DefRate.text = string.Format(DefRateText, battleData.attackerDefensRate.ToString());
        player2Name.text = battleData.target;
        player2Tile.text = battleData.targetTile;
        player2DefRate.text = string.Format(DefRateText, battleData.targetDefensRate.ToString());

        isShowItem = !string.IsNullOrEmpty(battleData.getItem);
        isShowExp = battleData.getExp > 0;
        isShowLevel = isShowLevelUp = (battleData.lvUpData != null && battleData.lvUpData != default(PlayerRecord));

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isEndBattle && !isEndAnimation)
        {
            if (waitTime < startWaitTime)
            {
                waitTime += Time.deltaTime;
            }
            else
            {
                waitTime = 0;
                isEndBattle = false;
                ResetPlayer();
            }
        }
    }

    public void SendDamage()
    {
        target.GetComponent<BattleActorController>().GetDamage();
        turnInfo.text += "\r\n" + string.Format(isHeal ? healInfoText : damageInfoText, target.GetComponent<BattleActorController>().playerName, target.GetComponent<BattleActorController>().damage);
    }

    public void MoveCamera()
    {
        if (isHeal || isIndirectAttack)
        {
            mainCamera.GetComponent<BattleCameraCotroller>().isStartMove = true;
        }
    }

    public void EndBattle()
    {
        isEndBattle = true;
    }

    private void ResetPlayer()
    {
        mainCamera.GetComponent<BattleCameraCotroller>().isStartMove = false;

        if (isFirstAttack || (isCounter && !isHeal))
        {
            SetPlayer();
        }
        else
        {
            isEndAnimation = true;
            attacker.GetComponent<BattleActorController>().isEndAnimation = true;
            target.GetComponent<BattleActorController>().isEndAnimation = true;

            if (battleData.getExp > 0)
            {
                //show get exp dialog
                ShowDialog();
            }
            else
            {
                SceneManager.LoadScene("GameScene");
            }
        }
        if (isFirstAttack)
        {
            isFirstAttack = false;
            isPlayerAttack = !isPlayerAttack;
        }
        else
        {
            isCounter = false;
        }
        turnInfo.text = string.Format(isHeal ? turnHealInfoText : turnInfoText, attacker.GetComponent<BattleActorController>().playerName);
    }

    public void SetPlayer()
    {
        if (isPlayerAttack || isHeal)
        {
            attacker = actor1;
            target = actor2;
        }
        else
        {
            attacker = actor2;
            target = actor1;
        }
        attacker.GetComponent<BattleActorController>().ResetActor(true);
        target.GetComponent<BattleActorController>().ResetActor(false);
        mainCamera.GetComponent<BattleCameraCotroller>().SetFocusTarget(isPlayerAttack || isHeal);

    }

    public void ShowDialog()
    {
        if (isShowItem)
        {
            isShowItem = false;
            Vector2 newSize = new Vector2(getItemWeight, msgBack.rectTransform.sizeDelta.y);
            msgBack.rectTransform.sizeDelta = newSize;
            msgText.text = string.Format(getItemText, battleData.getItem);
            msgGroup.alpha = 1;
            msgGroup.interactable = true;
            msgGroup.blocksRaycasts = true;
            return;
        }
        if (isShowExp)
        {
            isShowExp = false;
            ExpDialogController expDialog = expGroup.GetComponent<ExpDialogController>();
            expDialog.SetExp(battleData.playerExp, battleData.getExp, battleData.attacker);
            expDialog.ShowDialog();
            return;
        }
        if (isShowLevel)
        {
            isShowLevel = false;
            Vector2 newSize = new Vector2(levelUpWeight, msgBack.rectTransform.sizeDelta.y);
            msgBack.rectTransform.sizeDelta = newSize;
            msgText.text = string.Format(levelUpText, battleData.attacker, battleData.level);
            msgGroup.alpha = 1;
            msgGroup.interactable = true;
            msgGroup.blocksRaycasts = true;
            return;
        }
        if (isShowLevelUp)
        {
            isShowLevelUp = false;
            playerName.text = battleData.attacker;
            playerClass.text = battleData.playerClass;
            playerHP.text = (battleData.attackerHP - battleData.damageByTarget).ToString() + "/";
            playerMaxHP.text = battleData.playerData.hp.ToString();
            playerAtk.text = battleData.playerData.atk.ToString();
            playerDef.text = battleData.playerData.def.ToString();
            playerWis.text = battleData.playerData.wis.ToString();
            playerDex.text = battleData.playerData.dex.ToString();
            playerMDef.text = battleData.playerData.mdef.ToString();
            playerMaxHPUp.text = battleData.lvUpData.hp > 0 ? "+" + battleData.lvUpData.hp.ToString() : "";
            playerAtkUp.text = battleData.lvUpData.atk > 0 ? "+" + battleData.lvUpData.atk.ToString() : "";
            playerDefUp.text = battleData.lvUpData.def > 0 ? "+" + battleData.lvUpData.def.ToString() : "";
            playerWisUp.text = battleData.lvUpData.wis > 0 ? "+" + battleData.lvUpData.wis.ToString() : "";
            playerDexUp.text = battleData.lvUpData.dex > 0 ? "+" + battleData.lvUpData.dex.ToString() : "";
            playerMDefUp.text = battleData.lvUpData.mdef > 0 ? "+" + battleData.lvUpData.mdef.ToString() : "";

            levelGroup.alpha = 1;
            levelGroup.interactable = true;
            levelGroup.blocksRaycasts = true;
            return;
        }
        //End Scene
        if (!isTest)
        {
            SceneManager.LoadScene("GameScene");
        }
    }

}
