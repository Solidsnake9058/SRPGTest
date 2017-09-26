using System.Collections;
using System.Collections.Generic;

public enum ScenarioType
{
    Openning,
    Event,
    StageClear,
    Max
}

public enum ScenarioConditionType
{
    None,
    AfterShop,
    BeforeBattle,
    AfterBattle,
    Max
}

public enum ScenarioActionType
{
    Dialog,
    CreateActor,
    ControlActor,
    SetCamera,
    ControlCamera,
    Max
}

public enum ScenarioActorPivotType
{
    Right,
    UpRight,
    UpLeft,
    Left,
    DownLeft,
    DownRight,
    Max
}