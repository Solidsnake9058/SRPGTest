using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorPlayer : Player
{
    private void Start()
    {
        //SetPlayerModel();
    }

    // Update is called once per frame
    public override void Update()
    {
        TurnUpdate();
        base.Update();
    }

    public override void TurnEnd()
    {
        base.TurnEnd();
    }

    public override void TurnUpdate()
    {
        //highlight

        //if (positionQueue.Count > 0)
        //{
        //    transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
        //    transform.LookAt(positionQueue[0]);
        //    animator.SetBool("walk", true);
        //    if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f)
        //    {
        //        transform.position = positionQueue[0];
        //        positionQueue.RemoveAt(0);
        //        if (positionQueue.Count == 0)
        //        {
        //            GameManager.instance.StopWaitActor();
        //            SetPivot(playerPivot);
        //            animator.SetBool("walk", false);
        //        }
        //    }
        //}
        base.TurnUpdate();
        GameManager.instance.StopWaitActor();
        SetPivot(playerPivot);
    }
}
