using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

public class CharacterType
{
    //lord,
    //fighter,    knight,
    //wizard,
    //magician,    sorcerer,
    //bishop,
    //cleric,    priest,
    //elf,
    //hiElf,    elementMaster,
    //berserke,
    //dwarf,    spartan,
    //valkyrie,
    //siren,    bird,
    [JsonProperty]
    public int id { get; private set; }
    [JsonProperty]
    public string name { get; private set; }
    [JsonProperty]
    public List<int> equipWeapon { get; private set; }
    [JsonProperty]
    public bool canFly { get; private set; }
    [JsonProperty]
    public bool canHeal { get; private set; }
    [JsonProperty]
    public int healRange { get; private set; }

    public CharacterType() { }

    public CharacterType(int id, string name, List<int> equipWeapon, bool canFly, bool canHeal, int healRange)
    {
        this.id = id;
        this.name = name;
        this.equipWeapon = equipWeapon;
        this.canFly = canFly;
        this.canHeal = canHeal;
        this.healRange = healRange;
    }
}
