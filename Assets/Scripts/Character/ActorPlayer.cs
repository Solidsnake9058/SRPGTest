using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorPlayer : Player
{
    public override void SetPlayerValue(PlayerRecord playerRecord, CharacterTemplate playerData)
    {
        SetPosition(playerRecord);
    }
    public override void Update()
    {
        TurnUpdate();
        base.Update();
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
    }

    protected override void MoveToPointAction()
    {
        TurnEnd();
        GameManager.m_Instance.StopWaitActor();
        SetPivot();
    }
}
