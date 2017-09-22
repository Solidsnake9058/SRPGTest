using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleActorController : MonoBehaviour
{

    public float startWaitTime = 1f;
    public float moveSpeed = 10f;
    public Transform target;
    public Transform startPos;
    public bool isAttacker;
    public Transform visual;
    public Animator animator;
    public float attackDistance = 0.5f;
    public bool isIndirectAttack;
    public bool isHeal;
    public bool isEndAnimation;
    public string playerName;

    public float waitTime = 0;

    public Image hpBack;
    public Image hpBar;
    public Text hpText;
    public int hpBarMax = 350;
    public int hp = 150;
    public int maxHp = 150;
    public int damage = 20;

    // Use this for initialization
    private void Awake()
    {

    }
    void Start()
    {
        GameObject model = Instantiate(PlayerPrefabHolder.instance.playerModelPrefab01, transform.position, transform.rotation, visual);
        animator = model.GetComponent<Animator>();
        SetHPBar();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isAttacker && !isEndAnimation)
        {

            if (waitTime < startWaitTime)
            {
                waitTime += Time.deltaTime;
            }
            else
            {
                if (Vector3.Distance(transform.position, target.position) > attackDistance)
                {
                    animator.SetBool("run", true);
                    transform.position += transform.forward * moveSpeed * Time.deltaTime;
                }
                else
                {
                    animator.SetTrigger("attack");
                }
            }
        }
        else
        {
            animator.SetBool("endDamage", false);
        }
    }

    public void ResetActor(bool attacker)
    {
        isAttacker = attacker;
        animator.SetBool("run", false);
        animator.SetBool("endDamage", true);
        transform.position = startPos.position;
        waitTime = 0;
    }

    public void GetDamage()
    {
        animator.SetTrigger("damage");
        hp -= damage;
        SetHPBar();
    }

    public void SetHP(int hp, int maxHp, int damage, string playerName)
    {
        this.playerName = playerName;
        this.hp = hp;
        this.maxHp = maxHp;
        this.damage = damage;
        SetHPBar();
    }

    public void SetHPBar()
    {
        Vector2 newSize = new Vector2(((float)maxHp / hpBarMax) * hpBarMax, hpBack.rectTransform.sizeDelta.y);

        hpBack.rectTransform.sizeDelta = newSize;
        hpBar.rectTransform.sizeDelta = newSize;
        hpBar.fillAmount = (float)hp / maxHp;
        hpText.text = string.Format("{0}/{1}", hp, maxHp);
    }
}
