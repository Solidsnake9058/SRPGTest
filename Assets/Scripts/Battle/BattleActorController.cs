using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleActorController : MonoBehaviour
{
    [SerializeField]
    private bool m_IsPlayer = true;
    private BattleAnimationController m_BattleAnimation;
    private bool m_IsHeal;
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
    public bool isAttacked;
    public bool isEndAnimation;
    public string m_PlayerName;
    public string m_TileName;
    public float m_DefRate = 0;

    public float waitTime = 0;

    public Image hpBack;
    public Image hpBar;
    public Text hpText;
    public int hpBarMax = 350;
    public int m_Hp = 150;
    public int m_MaxHp = 150;
    public int m_Damage = 20;

    public void SetActorInfo(BattlePlayerData battlePlayerData)
    {
        m_PlayerName = battlePlayerData.m_PlayerName;
        m_TileName = battlePlayerData.m_TileName;
        m_DefRate = battlePlayerData.m_DefensRate;
        m_MaxHp = battlePlayerData.m_MaxHP;
        m_Hp = battlePlayerData.m_HP;
        m_Damage = battlePlayerData.m_GetDamage;

        Vector2 newSize = new Vector2((float)m_MaxHp / hpBarMax * hpBarMax, hpBack.rectTransform.sizeDelta.y);

        hpBack.rectTransform.sizeDelta = newSize;
        hpBar.rectTransform.sizeDelta = newSize;
        SetHPBar();
    }

    void Start()
    {
        m_BattleAnimation = animator.GetComponent<BattleAnimationController>();
        //GameObject model = Instantiate(PlayerPrefabHolder.instance.playerModelPrefab01, transform.position, transform.rotation, visual);
        //animator = model.GetComponent<Animator>();
        //SetHPBar();
    }

    // Update is called once per frame
    //void FixedUpdate()
    //{
    //    //if (isAttacker && !isEndAnimation)
    //    //{

    //    //    if (waitTime < startWaitTime)
    //    //    {
    //    //        waitTime += Time.deltaTime;
    //    //    }
    //    //    else
    //    //    {
    //    //        if (!BattleManager.instance.isHeal && !BattleManager.instance.isIndirectAttack)
    //    //        {
    //    //            if (Vector3.Distance(transform.position, target.position) > attackDistance)
    //    //            {
    //    //                animator.SetBool("run", true);
    //    //                transform.position += transform.forward * moveSpeed * Time.deltaTime;
    //    //            }
    //    //            else
    //    //            {
    //    //                if (!isAttacked)
    //    //                {
    //    //                    animator.SetTrigger("attack");
    //    //                    isAttacked = true;
    //    //                }
    //    //            }
    //    //        }
    //    //        else
    //    //        {
    //    //            if (!isAttacked)
    //    //            {
    //    //                if (BattleManager.instance.isHeal)
    //    //                {
    //    //                    animator.SetBool("heal", true);
    //    //                }
    //    //                else
    //    //                {
    //    //                    animator.SetTrigger("attack");
    //    //                    //Debug.Log("attack");
    //    //                }
    //    //                isAttacked = true;
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    animator.SetBool("endDamage", false);
    //    //}
    //}

    public void SetIdle()
    {
        animator.SetTrigger("Reset");
    }
    public void SetRun()
    {
        animator.SetBool("run", true);
    }

    public void SetAttack()
    {
        animator.SetTrigger("attack");
    }

    public void SetDamage()
    {
        animator.SetTrigger("damage");
    }

    public void ResetActor(bool attacker)
    {
        isAttacker = attacker;
        isAttacked = !attacker;
        animator.SetBool("run", false);
        animator.SetBool("endDamage", true);
        transform.position = startPos.position;
        waitTime = 0;
    }

    public void GetDamage()
    {
        //if (m_IsHeal)
        //{
        //    hp += damage;
        //    //animator.SetTrigger("cure");
        //}
        //else
        //{
            m_Hp -= m_Damage;
            if (m_Hp <= 0)
            {
                animator.SetTrigger("dead");
            }
            else
            {
                animator.SetTrigger("damage");
            }
        //}
        SetHPBar();
    }

    public void SetHP(int hp, int maxHp, int damage, string playerName)
    {
        this.m_PlayerName = playerName;
        this.m_Hp = hp;
        this.m_MaxHp = maxHp;
        this.m_Damage = damage;
        SetHPBar();
    }

    public void SetHPBar()
    {
        
        hpBar.fillAmount = (float)m_Hp / m_MaxHp;
        hpText.text = string.Format("{0}/{1}", m_Hp, m_MaxHp);
    }


}
