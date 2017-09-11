using System.Collections;
using System.Collections.Generic;

public class CharacterTemplate
{
    public int id { get; private set; }
    public string name { get; private set; }
    public string race { get; private set; }
    public uint move { get; private set; }
    public List<CharacterLevelTemplate> levelData { get; private set; }

    public CharacterTemplate() { }

    public CharacterTemplate(int id, string name, string race, uint move, List<CharacterLevelTemplate> levelData)
    {
        this.name = name;
        this.race = race;
        this.move = move;
        this.levelData = levelData;
    }
}

public class CharacterLevelTemplate
{
    public int id { get; private set; }
    public uint level { get; private set; }
    public uint exp { get; private set; }
    public uint hp { get; private set; }
    public uint atk { get; private set; }
    public uint def { get; private set; }
    public uint wis { get; private set; }
    public uint dex { get; private set; }
    public uint mdef { get; private set; }
    public int equipWeapon { get; private set; }

    public CharacterLevelTemplate() { }

    public CharacterLevelTemplate(int id, uint level, uint exp, uint hp, uint atk, uint def, uint wis, uint dex, uint mdef, int equipWeapon)
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
        this.equipWeapon = equipWeapon;
    }
}
