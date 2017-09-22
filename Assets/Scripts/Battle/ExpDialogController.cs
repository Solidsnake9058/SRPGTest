using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpDialogController : MonoBehaviour
{
    public CanvasGroup group;
    public Image expBar;
    public Text expInfo;

    public bool startRun;
    public int exp = 0;
    public int getExp = 10;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (startRun && getExp > 0)
        {
            exp++;
            getExp--;
            SetExpBar();
        }
    }

    public void SetExp(int exp, int getExp, string playerName)
    {
        this.exp = exp;
        this.getExp = getExp;
        expInfo.text = string.Format("{0}は経験値{1}を得る", playerName, getExp.ToString());
        SetExpBar();
    }

    private void SetExpBar()
    {
        expBar.fillAmount = (exp % 100) / 100f;
    }

    public void Click()
    {
        if (getExp == 0)
        {
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
            BattleManager.instance.ShowDialog();
        }
        else
        {
            exp += getExp;
            getExp = 0;
            SetExpBar();
        }
    }

    public void ShowDialog()
    {
        group.alpha = 1;
        group.interactable = true;
        group.blocksRaycasts = true;
        startRun = true;
    }
}
