using System.Collections;
using System.Collections.Generic;

public class Weapon
{
    //shortSword,    longSword,    broadSword,    busterSword,    greatSword,    holySword,
    //lance,    silverRance,    medusaKiller,    holyRance,    dragonRance,
    //mace,    warHanmer,    heavyMace,    flail,    morningStar,
    //handAx,    broadAx,    paulAx,    battleAx,    tomahawk,
    //fireRod,    iceRod,    lightning,    burningRod,    flameBird,
    //shortBow,    longBow,    silverBOw,    silvanBow,    repeater,
    //rapier,    estoc,    sylpheed,    laFlare,    titaniaSword,    gungnir,
    //spear,    javelin,    trident,    thunderSpear,    odinSpear,
    //harp,    lute,    lira
    public int id { get; private set; }
    public string name { get; private set; }
    public int directAtk { get; private set; }
    public int indirectAtk { get; private set; }
    public int directWis { get; private set; }
    public int indirectWis { get; private set; }
    public int price { get; private set; }
    public bool sell { get; private set; }
    public bool atkTwice { get; private set; }
    public int atkPoint { get; private set; }

    public Weapon() { }

    public Weapon(int id, string name, int directAtk, int indirectAtk, int directWis, int indirectWis, int price, bool sell, bool atkTwice)
    {
        this.id = id;
        this.name = name;
        this.directAtk = directAtk;
        this.indirectAtk = indirectAtk;
        this.directWis = directWis;
        this.indirectWis = indirectWis;
        this.price = price;
        this.sell = sell;
        this.atkTwice = atkTwice;
        this.atkPoint = atkTwice ? 2 : 1;
    }
}
