using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRecord
{
    [JsonProperty]
    public int id { get; private set; }

    [JsonProperty]
    public bool isEnemy { get; private set; }
    [JsonProperty]
    public bool isNewPlayer { get; private set; }
    [JsonProperty]
    public int locX { get; private set; }
    [JsonProperty]
    public int locY { get; private set; }

    [JsonProperty]
    public int characterId { get; private set; }
    [JsonProperty]
    public int levelId { get; private set; }

    //Read player data form save
    [JsonProperty]
    public uint level { get; private set; }
    [JsonProperty]
    public uint exp { get; private set; }
    [JsonProperty]
    public uint hp { get; private set; }
    [JsonProperty]
    public uint atk { get; private set; }
    [JsonProperty]
    public uint def { get; private set; }
    [JsonProperty]
    public uint wis { get; private set; }
    [JsonProperty]
    public uint dex { get; private set; }
    [JsonProperty]
    public uint mdef { get; private set; }
    [JsonProperty]
    public int equipWeapon { get; private set; }
    [JsonProperty]
    public ScenarioActorPivotType scenarioActorPivotType { get; private set; }

    //For create enemy
    [JsonProperty]
    public EnemyAIType aiType { get; private set; }
    [JsonProperty]
    public int searchRange { get; private set; }


    public PlayerRecord()
    {
    }

    public PlayerRecord(int id, bool isEnemy, bool isNewPlayer, int locX, int locY, int characterId, int levelId, EnemyAIType aiType, int searchRange)
    {
        this.id = id;
        this.isEnemy = isEnemy;
        this.isNewPlayer = isNewPlayer;
        this.locX = locX;
        this.locY = locY;
        this.characterId = characterId;
        this.levelId = levelId;
        this.aiType = aiType;
        this.searchRange = searchRange;
    }

    public PlayerRecord(int id, int locX, int locY, int characterId, ScenarioActorPivotType scenarioActorPivotType)
    {
        this.id = id;
        this.locX = locX;
        this.locY = locY;
        this.characterId = characterId;
        this.scenarioActorPivotType = scenarioActorPivotType;
        levelId = 0;
    }

    public PlayerRecord(int id, bool isEnemy, bool isNewPlayer, int locX, int locY, int characterId, int levelId, uint level, uint exp, uint hp, uint atk, uint def, uint wis, uint dex, uint mdef, int equipWeapon, EnemyAIType aiType, int searchRange)
    {
        this.id = id;
        this.isEnemy = isEnemy;
        this.isNewPlayer = isNewPlayer;
        this.locX = locX;
        this.locY = locY;
        this.characterId = characterId;
        this.levelId = levelId;
        this.level = level;
        this.exp = exp;
        this.hp = hp;
        this.atk = atk;
        this.def = def;
        this.wis = wis;
        this.dex = dex;
        this.mdef = mdef;
        this.equipWeapon = equipWeapon;
        this.aiType = aiType;
        this.searchRange = searchRange;
    }

    public PlayerRecord(uint hp, uint atk, uint def, uint wis, uint dex, uint mdef)
    {
        this.hp = hp;
        this.atk = atk;
        this.def = def;
        this.wis = wis;
        this.dex = dex;
        this.mdef = mdef;
    }
}
