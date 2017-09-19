using Newtonsoft.Json;
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

    [JsonProperty]
    public int id { get; private set; }
    [JsonProperty]
    public ItemType itemType { get; private set; }
    [JsonProperty]
    public string name { get; private set; }
    [JsonProperty]
    public int hp { get; private set; }
    [JsonProperty]
    public int atk { get; private set; }
    [JsonProperty]
    public int def { get; private set; }
    [JsonProperty]
    public int wis { get; private set; }
    [JsonProperty]
    public int dex { get; private set; }
    [JsonProperty]
    public int addHp { get; private set; }
    [JsonProperty]
    public int gold { get; private set; }
    [JsonProperty]
    public int price { get; private set; }
    [JsonProperty]
    public int useCharType { get; private set; }
    [JsonProperty]
    public int newCharType { get; private set; }
    [JsonProperty]
    public bool sell { get; private set; }
    [JsonProperty]
    public string notice { get; private set; }

    public Item() { }

    public Item(int id, ItemType itemType, string name, int hp, int atk, int def, int wis, int dex, int addHp, int gold, int price, int useCharType, int newCharType, bool sell, string notice)
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
        this.notice = notice;
    }

}

public enum ItemType
{
    cure,
    resurge,
    special,
}
