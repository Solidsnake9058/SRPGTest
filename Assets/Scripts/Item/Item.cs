using System.Collections;
using System.Collections.Generic;

public class Item
{
    //herb,    elfTesr,    pumpkinSeed,    pills,
    //whisky,    elixir,
    //powerFruit,    guardStone,    loreBook,    luckyBoots,    paperCharm,    commonMushroom,
    //holyStaff,    powerAx,    knightRance,    bardHarp,    elfBow,    magicianStick,
    //redFeather,
    //loveDream,
    //dokatiKey,

    public int id { get; private set; }
    public ItemType itemType { get; private set; }
    public string name { get; private set; }
    public int hp { get; private set; }
    public int atk { get; private set; }
    public int def { get; private set; }
    public int wis { get; private set; }
    public int dex { get; private set; }
    public int addHp { get; private set; }
    public int gold { get; private set; }
    public int price { get; private set; }
    public int useCharType { get; private set; }
    public int newCharType { get; private set; }
    public bool sell { get; private set; }

    public Item() { }

    public Item(int id, ItemType itemType, string name, int hp, int atk, int def, int wis, int dex, int addHp, int gold, int price, int useCharType, int newCharType, bool sell)
    {
        this.id = id;
        this.itemType = itemType;
        this.name = name;
        this.hp = hp;
        this.atk = atk;
        this.def = def;
        this.wis = wis;
        this.dex = dex;
        this.addHp = addHp;
        this.gold = gold;
        this.price = price;
        this.useCharType = useCharType;
        this.newCharType = newCharType;
        this.sell = sell;
    }

}

public enum ItemType
{
    cure,
    resurge,
    special,
}
