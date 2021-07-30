using System.Collections;
using System.Collections.Generic;

public enum ScenarionType
{
    Openning,
    Event,
    StageClear,
    Max
}

public enum ScenarionConditionType
{
    None,
    AfterShop,
    BeforeBattle,
    AfterBattle,
    Max
}

public enum ScenarionActionType
{
    Dialog,
    CreateActor,
    ControlActor,
    SetCamera,
    ControlCamera,
    AddUserPlayer,
    AddEnemyPlayer,
    Max
}

public enum ScenarionActorPivotType
{
    Right = 90,
    UpRight = 30,
    UpLeft = 330,
    Left = 270,
    DownLeft = 210,
    DownRight = 150,
}

public enum ScenarionSelectType
{
    Scenario,
    Action,
    Actor,
    Max
}