using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCameraCotroller : MonoBehaviour {

    public Transform actor1;
    public Transform actor2;

    public Transform target;

    public bool isIndirectAttack;
    public bool isHeal;

    // Use this for initialization
    void Start () {
        SetFocusTarget(true);

    }

    // Update is called once per frame
    void Update()
    {
        if (!isIndirectAttack && !isHeal)
        {
            if (target != null)
            {
                transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
            }
        }
    }

    public void SetFocusTarget(bool isUserPlayer)
    {
        if (isUserPlayer)
        {
            target = actor1;
        }
        else
        {
            target = actor2;
        }

        transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
    }

}
