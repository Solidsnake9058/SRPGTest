using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

public class CharacterTemplate
{
    [JsonProperty]
    public int id { get; private set; }
    [JsonProperty]
    public string name { get; private set; }
    [JsonProperty]
    public int race { get; private set; }
    [JsonProperty]
    public float move { get; private set; }
    [JsonProperty]
    public bool enemy { get; private set; }
    [JsonProperty]
    public List<CharacterLevelTemplate> levelData { get; private set; }

    public CharacterLevelTemplate GetCharacterLevelData(int level)
    {
        for (int i = 0; i < levelData.Count; i++)
        {
            if (levelData[i].Equals(level))
            {
                return levelData[i];
            }
        }
        return levelData[0];
    }

    public CharacterTemplate() { }

    public CharacterTemplate(int id, string name, int race, float move, bool enemy, List<CharacterLevelTemplate> levelData)
    {
        this.id = id;
        this.name = name;
        this.race = race;
        this.move = move;
        this.enemy = enemy;
        this.levelData = levelData;
    }
}

public class CharacterLevelTemplate
{
    [JsonProperty]
    public int id { get; private set; }
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
    public uint gold { get; private set; }
    [JsonProperty]
    public int equipWeapon { get; private set; }

    public CharacterLevelTemplate() { }

    public CharacterLevelTemplate(int id, uint level, uint exp, uint hp, uint atk, uint def, uint wis, uint dex, uint mdef, uint gold, int equipWeapon)
    {
        this.id = id;
        this.level = level;
        this.exp = exp;
        this.hp = hp;
        this.atk = atk;
        this.def = def;
        this.wis = wis;
        this.dex = dex;
        this.mdef = mdef;
        this.gold = gold;
        this.equipWeapon = equipWeapon;
    }
}
