using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCameraCotroller : MonoBehaviour {

    public Transform actor1;
    public Transform actor2;

    public Transform target;
    public Transform targetDamage;

    public bool isIndirectAttack;
    public bool isHeal;
    public bool isStartMove;

    public float moveSpeed = 1f;

    // Use this for initialization
    //void Start () {
    //    SetFocusTarget(true);
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    if (!BattleManager.instance.isIndirectAttack && !BattleManager.instance.isHeal)
    //    {
    //        if (target != null)
    //        {
    //            transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
    //        }
    //    }
    //    else
    //    {
    //        if (isStartMove)
    //        {
    //            if ((transform.localPosition.x * targetDamage.localPosition.x < 0) || Mathf.Abs(transform.localPosition.x) <= Mathf.Abs(targetDamage.localPosition.x))
    //            {
    //                transform.localPosition += (new Vector3(targetDamage.localPosition.x, transform.localPosition.y, transform.localPosition.z) - transform.localPosition).normalized * moveSpeed * Time.deltaTime;
    //            }
    //            else
    //            {
    //                BattleManager.instance.SendDamage();
    //                isStartMove = false;
    //            }
    //        }
            
    //    }
    //}

    public void SetFocusTarget(bool isUserPlayer)
    {
        if (isUserPlayer)
        {
            target = actor1;
            targetDamage = actor2;
        }
        else
        {
            target = actor2;
            targetDamage = actor1;
        }

        transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
    }

}
