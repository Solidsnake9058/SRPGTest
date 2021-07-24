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
    Right,
    UpRight,
    UpLeft,
    Left,
    DownLeft,
    DownRight,
    Max
}

public enum ScenarionSelectType
{
    Scenario,
    Action,
    Actor,
    Max
}