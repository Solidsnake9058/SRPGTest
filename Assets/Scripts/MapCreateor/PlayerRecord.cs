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
    public uint currentHp { get; private set; }
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
    [JsonProperty]
    public bool isActable { get; private set; }

    //For create enemy
    [JsonProperty]
    public EnemyAIType aiType { get; private set; }
    [JsonProperty]
    public int searchRange { get; private set; }


    public PlayerRecord()
    {
    }

    public PlayerRecord Clone()
    {
        PlayerRecord playerRecord = new PlayerRecord();
        playerRecord.id = this.id;
        playerRecord.isEnemy = this.isEnemy;
        playerRecord.isNewPlayer = this.isNewPlayer;
        playerRecord.locX = this.locX;
        playerRecord.locY = this.locY;
        playerRecord.characterId = this.characterId;
        playerRecord.levelId = this.levelId;
        playerRecord.level = this.level;
        playerRecord.exp = this.exp;
        playerRecord.hp = this.hp;
        playerRecord.currentHp = this.currentHp;
        playerRecord.atk = this.atk;
        playerRecord.def = this.def;
        playerRecord.wis = this.wis;
        playerRecord.dex = this.dex;
        playerRecord.mdef = this.mdef;
        playerRecord.equipWeapon = this.equipWeapon;
        playerRecord.scenarioActorPivotType = this.scenarioActorPivotType;
        playerRecord.isActable = this.isActable;
        playerRecord.aiType = this.aiType;
        playerRecord.searchRange = this.searchRange;
        return playerRecord;
    }

    public void SetPlayerLocate(PlayerRecord playerRecord)
    {
        locX = playerRecord.locX;
        locY = playerRecord.locY;
    }

    public void SetPlayerHp(PlayerRecord playerRecord)
    {
        if (playerRecord != null)
        {
            hp = playerRecord.hp;
            currentHp = playerRecord.currentHp;
        }
        else
        {
            currentHp = hp;
        }
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

    public PlayerRecord(CharacterTemplate characterTemplate)
    {
        id = characterTemplate.id;
        isEnemy = false;
        isNewPlayer = true;
        locX = 0;
        locY = 0;
        characterId = characterTemplate.id;
        levelId = 0;
        level = characterTemplate.levelData[0].level;
        exp = characterTemplate.levelData[0].exp;
        hp = characterTemplate.levelData[0].hp;
        currentHp = characterTemplate.levelData[0].hp;
        atk = characterTemplate.levelData[0].atk;
        def = characterTemplate.levelData[0].def;
        wis = characterTemplate.levelData[0].wis;
        dex = characterTemplate.levelData[0].dex;
        mdef = characterTemplate.levelData[0].mdef;
        equipWeapon = characterTemplate.levelData[0].equipWeapon;
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

    public PlayerRecord(int id, bool isEnemy, bool isNewPlayer, int locX, int locY, int characterId, int levelId, uint level, uint exp, uint hp, uint currentHp, uint atk, uint def, uint wis, uint dex, uint mdef, int equipWeapon, EnemyAIType aiType, int searchRange, bool isActable = true)
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
        this.currentHp = currentHp;
        this.atk = atk;
        this.def = def;
        this.wis = wis;
        this.dex = dex;
        this.mdef = mdef;
        this.equipWeapon = equipWeapon;
        this.aiType = aiType;
        this.searchRange = searchRange;
        this.isActable = isActable;
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
