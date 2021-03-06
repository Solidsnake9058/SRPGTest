﻿using System.Collections;
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
    AddUserPlayer,
    AddEnemyPlayer,
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

public enum ScenarioSelectType
{
    Scenario,
    Action,
    Actor,
    Max
}