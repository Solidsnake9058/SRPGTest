using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class ItemXml
{
    [XmlAttribute("itemid")]
    public int id;

    [XmlAttribute("itemType")]
    public int itemType;

    [XmlAttribute("itemname")]
    public string name;

    [XmlAttribute("itemhp")]
    public int hp;

    [XmlAttribute("itematk")]
    public int atk;

    [XmlAttribute("itemdef")]
    public int def;

    [XmlAttribute("itemwis")]
    public int wis;

    [XmlAttribute("itemdex")]
    public int dex;

    [XmlAttribute("itemaddHp")]
    public int addHp;

    [XmlAttribute("itemgold")]
    public int gold;

    [XmlAttribute("itemprice")]
    public int price;

    [XmlAttribute("itemuseCharType")]
    public int useCharType;

    [XmlAttribute("itemnewCharType")]
    public int newCharType;

    [XmlAttribute("itemsell")]
    public bool sell;
}

public class WeaponXml
{
    [XmlAttribute("weaponid")]
    public int id;

    [XmlAttribute("weaponname")]
    public string name;

    [XmlAttribute("weapondirectAtk")]
    public int directAtk;

    [XmlAttribute("weaponindirectAtk")]
    public int indirectAtk;

    [XmlAttribute("weapondirectWis")]
    public int directWis;

    [XmlAttribute("weaponindirectWis")]
    public int indirectWis;

    [XmlAttribute("weaponprice")]
    public int price;

    [XmlAttribute("weaponsell")]
    public bool sell;

    [XmlAttribute("weaponatkTwice")]
    public bool atkTwice;

    [XmlAttribute("weaponatkPoint")]
    public int atkPoint;
}

public class CharacterTypeXml
{
    [XmlAttribute("charTypeid")]
    public int id;

    [XmlAttribute("charTypename")]
    public string name;

    [XmlArray("charTypeEquipWeapons")]
    [XmlArrayItem("int")]
    public List<int> equipWeapons;

    [XmlAttribute("charTypecanFly")]
    public bool canFly;

    [XmlAttribute("charTypecanHeal")]
    public bool canHeal;
}

public class CharacterLevelTemplateXml
{
    [XmlAttribute("charlvid")]
    public int id;

    [XmlAttribute("charlvlevel")]
    public uint level;

    [XmlAttribute("charlvexp")]
    public uint exp;

    [XmlAttribute("charlvhp")]
    public uint hp;

    [XmlAttribute("charlvatk")]
    public uint atk;

    [XmlAttribute("charlvdef")]
    public uint def;

    [XmlAttribute("charlvwis")]
    public uint wis;

    [XmlAttribute("charlvdex")]
    public uint dex;

    [XmlAttribute("charlvmdef")]
    public uint mdef;

    [XmlAttribute("charlvequipWeapon")]
    public int equipWeapon;
}

public class CharacterTemplateXml
{
    [XmlAttribute("charTempid")]
    public int id;

    [XmlAttribute("charTempname")]
    public string name;

    [XmlAttribute("charTemprace")]
    public int race;

    [XmlAttribute("charTempmove")]
    public uint move;

    [XmlAttribute("charTempEnemy")]
    public bool enemy;

    [XmlArray("charTempLevelDatas")]
    [XmlArrayItem("CharacterLevelTemplateXml")]
    public List<CharacterLevelTemplateXml> levelDatas = new List<CharacterLevelTemplateXml>();
}

[XmlRoot("ObjectCollection")]
public class ObjectXmlContainer
{
    [XmlArray("Items")]
    [XmlArrayItem("ItemXml")]
    public List<ItemXml> items = new List<ItemXml>();

    [XmlArray("Weapons")]
    [XmlArrayItem("WeaponXml")]
    public List<WeaponXml> weapons = new List<WeaponXml>();

    [XmlArray("CharacterTypes")]
    [XmlArrayItem("CharacterTypeXml")]
    public List<CharacterTypeXml> charTyps = new List<CharacterTypeXml>();

    [XmlArray("CharacterTemplates")]
    [XmlArrayItem("CharacterTemplateXml")]
    public List<CharacterTemplateXml> charTemplates = new List<CharacterTemplateXml>();
}

public class ObjectSaveLoad
{
    public static ObjectXmlContainer CreateObjectContainer(List<Item> items, List<Weapon> weapons, List<CharacterType> charTypes, List<CharacterTemplate> charTemplates)
    {
        List<ItemXml> itemXmls = new List<ItemXml>();
        List<WeaponXml> weaponXmls = new List<WeaponXml>();
        List<CharacterTypeXml> characterTypeXmls = new List<CharacterTypeXml>();
        List<CharacterTemplateXml> characterTemplateXmls = new List<CharacterTemplateXml>();

        for (int i = 0; i < items.Count; i++)
        {
            itemXmls.Add(CreateItemXml(items[i]));
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            weaponXmls.Add(CreateWeaponXml(weapons[i]));
        }

        for (int i = 0; i < charTypes.Count; i++)
        {
            characterTypeXmls.Add(CreateCharacterTypeXml(charTypes[i]));
        }

        for (int i = 0; i < charTemplates.Count; i++)
        {
            characterTemplateXmls.Add(CreateItemXml(charTemplates[i]));
        }

        return new ObjectXmlContainer()
        {
            items = itemXmls,
            weapons = weaponXmls,
            charTyps = characterTypeXmls,
            charTemplates = characterTemplateXmls
        };
    }

    public static ItemXml CreateItemXml(Item item)
    {
        return new ItemXml()
        {
            id = item.id,
            itemType = (int)item.itemType,
            name = item.name,
            hp = item.hp,
            atk = item.atk,
            def = item.def,
            wis = item.wis,
            dex = item.dex,
            addHp = item.addHp,
            gold = item.gold,
            price = item.price,
            useCharType = item.useCharType,
            newCharType = item.newCharType,
            sell = item.sell,
        };
    }

    public static WeaponXml CreateWeaponXml(Weapon weapon)
    {
        return new WeaponXml()
        {
            id = weapon.id,
            name = weapon.name,
            directAtk = weapon.directAtk,
            indirectAtk = weapon.indirectAtk,
            directWis = weapon.directWis,
            indirectWis = weapon.indirectWis,
            price = weapon.price,
            sell = weapon.sell,
            atkTwice = weapon.atkTwice,
            atkPoint = weapon.atkPoint,
        };
    }

    public static CharacterTypeXml CreateCharacterTypeXml(CharacterType characterType)
    {
        return new CharacterTypeXml()
        {
            id = characterType.id,
            name = characterType.name,
            equipWeapons = characterType.equipWeapon,
            canFly = characterType.canFly,
            canHeal = characterType.canHeal,
        };
    }

    public static CharacterLevelTemplateXml CreateCharacterLevelTemplateXml(CharacterLevelTemplate characterLevelTemplate)
    {
        return new CharacterLevelTemplateXml()
        {
            id = characterLevelTemplate.id,
            level = characterLevelTemplate.level,
            exp = characterLevelTemplate.exp,
            hp = characterLevelTemplate.hp,
            atk = characterLevelTemplate.atk,
            def = characterLevelTemplate.def,
            wis = characterLevelTemplate.wis,
            dex = characterLevelTemplate.dex,
            mdef = characterLevelTemplate.mdef,
            equipWeapon = characterLevelTemplate.equipWeapon,
        };
    }

    public static List<CharacterLevelTemplateXml> CreateItemXmls(List<CharacterLevelTemplate> characterLevelTemplates)
    {
        List<CharacterLevelTemplateXml> characterLevelTemplateXmls = new List<CharacterLevelTemplateXml>();

        for (int i = 0; i < characterLevelTemplates.Count; i++)
        {
            characterLevelTemplateXmls.Add(CreateCharacterLevelTemplateXml(characterLevelTemplates[i]));
        }
        return characterLevelTemplateXmls;
    }

    public static CharacterTemplateXml CreateItemXml(CharacterTemplate characterTemplate)
    {
        return new CharacterTemplateXml()
        {
            id = characterTemplate.id,
            name = characterTemplate.name,
            race = characterTemplate.race,
            move = characterTemplate.move,
            enemy = characterTemplate.enemy,
            levelDatas = CreateItemXmls(characterTemplate.levelData),
        };
    }

    public static GameElement CreateGameElements(ObjectXmlContainer container)
    {
        List<Item> items = new List<Item>();
		List<Weapon> weapons = new List<Weapon>();
		List<CharacterType> races = new List<CharacterType>();
		List<CharacterTemplate> characters = new List<CharacterTemplate>();

		#region Item
		for (int i = 0; i < container.items.Count; i++)
		{
			string name = container.items[i].name;
			int hp = container.items[i].hp;
			int atk = container.items[i].atk;
			int def = container.items[i].def;
			int wis = container.items[i].wis;
			int dex = container.items[i].dex;
			int addHp = container.items[i].addHp;
			int gold = container.items[i].gold;
			int price = container.items[i].price;
			int itemType = container.items[i].itemType;
			int useCharType = container.items[i].useCharType;
			int newCharType = container.items[i].newCharType;
			bool sell = container.items[i].sell;
			int id = container.items[i].id;

			Item newItem = new Item(id, (ItemType)itemType, name, hp, atk, def, wis, dex, addHp, gold, price, useCharType, newCharType, sell);
			items.Add(newItem);
		}
		#endregion

		#region Weapon
		for (int i = 0; i < container.weapons.Count; i++)
		{
			string name = container.weapons[i].name;
			int directAtk = container.weapons[i].directAtk;
			int indirectAtk = container.weapons[i].indirectAtk;
			int directWis = container.weapons[i].directWis;
			int indirectWis = container.weapons[i].indirectWis;
			int price = container.weapons[i].price;
			bool sell = container.weapons[i].sell;
			bool atkTwice = container.weapons[i].atkTwice;
			int id = container.weapons[i].id;

			Weapon newWeapon = new Weapon(id, name, directAtk, indirectAtk, directWis, indirectWis, price, sell, atkTwice);
			weapons.Add(newWeapon);
		}
		#endregion

		#region Race
		for (int i = 0; i < container.charTyps.Count; i++)
		{
			string name = container.charTyps[i].name;
			List<int> equipWeapons = container.charTyps[i].equipWeapons;
			bool canFly = container.charTyps[i].canFly;
			bool canHeal = container.charTyps[i].canHeal;
			int id = container.charTyps[i].id;

			CharacterType newRace = new CharacterType(id, name, equipWeapons, canFly, canHeal);
			races.Add(newRace);
		}
		#endregion

		#region Character
		for (int i = 0; i < container.charTemplates.Count; i++)
		{


			string name = container.charTemplates[i].name;
			int race = container.charTemplates[i].race;
			uint move = container.charTemplates[i].move;
			int id = container.charTemplates[i].id;
			bool enemy = container.charTemplates[i].enemy;
			List<CharacterLevelTemplate> charLvs = new List<CharacterLevelTemplate>();

			foreach (var lv in container.charTemplates[i].levelDatas)
			{
				charLvs.Add(new CharacterLevelTemplate(lv.id, lv.level, lv.exp, lv.hp, lv.atk, lv.def, lv.wis, lv.dex, lv.mdef, lv.equipWeapon));
			}

			CharacterTemplate newCharacter = new CharacterTemplate(id, name, race, move, enemy, charLvs);
			characters.Add(newCharacter);

		}
        #endregion

        return new GameElement(items, weapons, races, characters);
    }

    public static void XmlSave<T>(T objectContainer, string filename)
    {
        var serializer = new XmlSerializer(typeof(T));
        using (var stream = new FileStream(filename, FileMode.Create))
        {
            serializer.Serialize(stream, objectContainer);
        }
    }

    public static T XmlLoad<T>(string filename)
    {
        var serializer = new XmlSerializer(typeof(T));
        using (var stream = new FileStream(filename, FileMode.Open))
        {
            return (T)serializer.Deserialize(stream);
        }
    }
}
