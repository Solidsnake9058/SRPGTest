using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorPlayer : Player
{
    public override void SetPlayerValue(PlayerRecord playerRecord, CharacterTemplate playerData)
    {
        SetPosition(playerRecord);
    }
    public override void SystemUpdate()
    {
        TurnUpdate();
    }

    protected override void MoveToPointAction()
    {
        TurnEnd();
        _PlayerState = PlayerState.Wait;
        SetPivot();
    }
}
