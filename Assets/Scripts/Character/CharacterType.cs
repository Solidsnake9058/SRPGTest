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
    public int id { get; private set; }
    public string name { get; private set; }
    public List<int> equipWeapon { get; private set; }
    public bool canFly { get; private set; }
    public bool canHeal { get; private set; }

    public CharacterType() { }

    public CharacterType(int id, string name, List<int> equipWeapon, bool canFly, bool canHeal)
    {
        this.id = id;
        this.name = name;
        this.equipWeapon = equipWeapon;
        this.canFly = canFly;
        this.canHeal = canHeal;
    }
}
