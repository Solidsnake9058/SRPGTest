using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ObjectCreatorManager : MonoBehaviour
{
    public static ObjectCreatorManager instance { get; private set; }

    //Layout
    [Header("Layout")]
    public CanvasGroup itemGroup;
    public CanvasGroup weaponGroup;
    public CanvasGroup raceGroup;
    public CanvasGroup charGroup;
    public RectTransform objectList;
    public GameObject objectSelectPrefab;

    //Item group
    private int itemId = -1;
    [Header("Item group")]
    public InputField itemName;
    public Dropdown itemItemType;
    public InputField itemHP;
    public InputField itemAtk;
    public InputField itemDef;
    public InputField itemWis;
    public InputField itemDex;
    public InputField itemAddHP;
    public InputField itemGold;
    public InputField itemPrice;
    public Dropdown itemUseRace;
    public Dropdown itemNewRace;
    public Toggle itemCanSell;

    //Weapon group
    private int weaponId = -1;
    [Header("Weapon group")]
    public InputField weaponName;
    public InputField weaponDirectAtk;
    public InputField weaponIndirectAtk;
    public InputField weaponDirectWis;
    public InputField weaponIndirectWis;
    public InputField weaponPrice;
    public Toggle weaponCanSell;
    public Toggle weaponCanTwice;

    //Race group
    private int raceId = -1;
    List<int> raceWeapons;
    [Header("Race group")]
    public InputField raceName;
    public Toggle raceCanFly;
    public Toggle raceCanHeal;
    public Dropdown raceWeapon;
    public RectTransform raceWeaponList;
    public GameObject raceWeaponSelectPrefab;

    //Character group
    private int charId = -1;
    [Header("Character group")]
    public InputField charName;
    public Dropdown   charRace;
    public InputField charMove;
    public InputField charLevel;
    public InputField charExp;
    public InputField charHP;
    public InputField charAtk;
    public InputField charDef;
    public InputField charWis;
    public InputField charDex;
    public InputField charMdef;
    public Dropdown charWeapon;
    public Toggle charIsEnemy;
    public RectTransform charLevelList;
    public GameObject charLevelSelectPrefab;

    //private
    private CreateType createType = CreateType.none;
    private List<Item> items;
    private List<Weapon> weapons;
    private List<CharacterType> races;
    private List<CharacterTemplate> characters;
    private List<CharacterLevelTemplate> characterLevels;
    private Dictionary<int, string> dicItemType;
    private Dictionary<int, string> dicRace;
    private Dictionary<int, string> dicWeapon;

    private string filename = "Objects.xml";

    #region UI Method
    public void DisableGroup(CanvasGroup group)
    {
        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void EnableGroup(CanvasGroup group)
    {
        group.alpha = 1;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public void ControlGroup(Button btn)
    {
        DisableAllGroup();
        switch (btn.name)
        {
            case "Item":
                createType = CreateType.item;
                break;
            case "Weapon":
                createType = CreateType.weapon;
                break;
            case "Race":
                createType = CreateType.race;
                break;
            case "Character":
                createType = CreateType.character;
                break;
        }
        ReloadObjects();
    }

    public void ClearObjectList()
    {
        for (int i = 0; i < objectList.transform.childCount; i++)
        {
            Destroy(objectList.transform.GetChild(i).gameObject);
        }
    }

    public void AddObject()
    {
        switch (createType)
        {
            case CreateType.none:
                break;
            case CreateType.item:
                if (AddItem())
                {
                    ResetItem();
                    LoadItemObjects();
                }
                break;
            case CreateType.weapon:
                break;
            case CreateType.race:
                break;
            case CreateType.character:
                break;
        }

    }

    public void Delete()
    {
        switch (createType)
        {
            case CreateType.none:
                break;
            case CreateType.item:
                RemoveItem();
                ReloadObjects();
                break;
            case CreateType.weapon:
                break;
            case CreateType.race:
                break;
            case CreateType.character:
                break;
        }
    }

    public void UpdateObject()
    {
        switch (createType)
        {
            case CreateType.none:
                break;
            case CreateType.item:
                UpdateItem();
                ReloadObjects();
                break;
            case CreateType.weapon:
                break;
            case CreateType.race:
                break;
            case CreateType.character:
                break;
        }
    }

    private void LoadItemObjects()
    {
        ClearObjectList();
        items.Sort((x, y) => { return x.id.CompareTo(y.id); });

        for (int i = 0; i < items.Count; i++)
        {
            GameObject newObject = (GameObject)Instantiate(objectSelectPrefab, Vector3.zero, Quaternion.Euler(new Vector3()));
            newObject.GetComponent<Button>().name = items[i].id.ToString();
            newObject.GetComponent<Button>().GetComponentInChildren<Text>().text = items[i].name;
            newObject.transform.SetParent(objectList);
        }
    }

    private void DisableAllGroup()
    {
        DisableGroup(itemGroup);
        DisableGroup(weaponGroup);
        DisableGroup(raceGroup);
        DisableGroup(charGroup);
    }

    private void InitialObjects()
    {
        items = new List<Item>();
        weapons = new List<Weapon>();
        races = new List<CharacterType>();
        characters = new List<CharacterTemplate>();
        characterLevels = new List<CharacterLevelTemplate>();
    }

    private void ReloadObjects()
    {
        ClearObjectList();
        switch (createType)
        {
            case CreateType.item:
                ResetItem();
                EnableGroup(itemGroup);
                LoadItemObjects();
                break;
            case CreateType.weapon:
                EnableGroup(weaponGroup);

                ClearObjectList();
                break;
            case CreateType.race:
                EnableGroup(raceGroup);

                ClearObjectList();
                break;
            case CreateType.character:
                EnableGroup(charGroup);

                ClearObjectList();
                break;
        }
    }

    public void GetObject(string id)
    {
        switch (createType)
        {
            case CreateType.item:
                LoadItem(id);
                break;
            case CreateType.weapon:
                break;
            case CreateType.race:
                break;
            case CreateType.character:
                break;
        }
    }

    #endregion

    #region Item Control
    private void ResetItem()
    {
        itemId = -1;
        itemName.text = "";
        itemHP.text = "";
        itemAtk.text = "";
        itemDef.text = "";
        itemWis.text = "";
        itemDex.text = "";
        itemAddHP.text = "";
        itemGold.text = "";
        itemPrice.text = "";
        itemItemType.value = 0;
        itemUseRace.value = 0;
        itemNewRace.value = 0;
        itemCanSell.isOn = false;
    }

    private bool AddItem(int newId = -1)
    {
        if (string.IsNullOrEmpty(itemName.text))
        {
            return false;
        }
        try
        {
            string name = itemName.text.Trim();
            int hp = Convert.ToInt32(string.IsNullOrEmpty(itemHP.text) ? "0" : itemHP.text);
            int atk = Convert.ToInt32(string.IsNullOrEmpty(itemAtk.text) ? "0" : itemAtk.text);
            int def = Convert.ToInt32(string.IsNullOrEmpty(itemDef.text) ? "0" : itemDef.text);
            int wis = Convert.ToInt32(string.IsNullOrEmpty(itemWis.text) ? "0" : itemWis.text);
            int dex = Convert.ToInt32(string.IsNullOrEmpty(itemDex.text) ? "0" : itemDex.text);
            int addHp = Convert.ToInt32(string.IsNullOrEmpty(itemAddHP.text) ? "0" : itemAddHP.text);
            int gold = Convert.ToInt32(string.IsNullOrEmpty(itemGold.text) ? "0" : itemGold.text);
            int price = Convert.ToInt32(string.IsNullOrEmpty(itemPrice.text) ? "0" : itemPrice.text);
            int itemType = dicItemType.Where(x=>x.Value == itemItemType.options[itemItemType.value].text).FirstOrDefault().Key;
            int useCharType = dicRace.Where(x => x.Value == itemUseRace.options[itemUseRace.value].text).FirstOrDefault().Key;
            int newCharType = dicRace.Where(x => x.Value == itemNewRace.options[itemNewRace.value].text).FirstOrDefault().Key;         
            bool sell = itemCanSell.isOn;
            int id = newId == -1 ? (items.Count > 0 ? items.Select(x => x.id).Max() + 1 : 0) : newId;
            if (items.Where(x => x.name == name).Count() > 0)
            {
                Debug.LogError("Item exist");
                return false;
            }

            Item newItem = new Item(id, (ItemType)itemType, name, hp, atk, def, wis, dex, addHp, gold, price, useCharType, newCharType, sell);
            items.Add(newItem);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
        return true;
    }

    private void LoadItem(string id)
    {
        Item temp = items.Where(x => x.id.ToString() == id).First();
        if (temp != null)
        {
            itemId = temp.id;
            itemName.text = temp.name;
            itemHP.text = temp.hp.ToString();
            itemAtk.text =temp.atk.ToString();
            itemDef.text =temp.def.ToString();
            itemWis.text =temp.wis.ToString();
            itemDex.text = temp.dex.ToString();
            itemAddHP.text =temp.addHp.ToString();
            itemGold.text = temp.gold.ToString();
            itemPrice.text = temp.price.ToString();
            itemItemType.value = itemUseRace.options.FindIndex(x => x.text == dicItemType[(int)temp.itemType]);
            itemUseRace.value = itemUseRace.options.FindIndex(x => x.text == dicRace[temp.useCharType]);
            itemNewRace.value = itemNewRace.options.FindIndex(x => x.text == dicRace[temp.newCharType]); 
            itemCanSell.isOn = temp.sell;
        }
    }

    private void RemoveItem()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == itemId)
            {
                items.RemoveAt(i);
                break;
            }
        }
        itemId = -1;
    }

    private void UpdateItem()
    {
        int tempId = itemId;
        Item tempItem = new Item();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == itemId)
            {
                tempItem = items[i];
                items.RemoveAt(i);
                break;
            }
        }
        if (!AddItem(tempId))
        {
            items.Add(tempItem);
        }
    }

    #endregion

    #region Weapon Control
    private void ResetWeapon()
    {
        weaponId = -1;
        weaponName.text = "";
        weaponDirectAtk.text = "";
        weaponIndirectAtk.text = "";
        weaponDirectWis.text = "";
        weaponIndirectWis.text = "";
        weaponPrice.text = "";
        weaponCanSell.isOn = false;
    }

    private bool AddWeapon()
    {
        if (string.IsNullOrEmpty(itemName.text))
        {
            return false;
        }
        try
        {
            string name = weaponName.text.Trim();
            int directAtk = Convert.ToInt32(string.IsNullOrEmpty(weaponDirectAtk.text) ? "0" : weaponDirectAtk.text);
            int indirectAtk = Convert.ToInt32(string.IsNullOrEmpty(weaponIndirectAtk.text) ? "0" : weaponIndirectAtk.text);
            int directWis = Convert.ToInt32(string.IsNullOrEmpty(weaponDirectWis.text) ? "0" : weaponDirectWis.text);
            int indirectWis = Convert.ToInt32(string.IsNullOrEmpty(weaponIndirectWis.text) ? "0" : weaponIndirectWis.text);
            int price = Convert.ToInt32(string.IsNullOrEmpty(weaponPrice.text) ? "0" : weaponPrice.text);
            bool sell = weaponCanSell.isOn;
            bool atkTwice = weaponCanTwice.isOn;

            int id = weapons.Count > 0 ? weapons.Select(x => x.id).Max() + 1 : 0;
            if (weapons.Where(x => x.name == name).Count() > 0)
            {
                Debug.LogError("Weapons exist");
                return false;
            }

            Weapon newWeapon = new Weapon(id, name, directAtk, indirectAtk, directWis, indirectWis, price, sell, atkTwice);
            weapons.Add(newWeapon);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
        return true;
    }

    private void LoadWeapon(string id)
    {
        Weapon temp = weapons.Where(x => x.id.ToString() == id).First();
        if (temp != null)
        {
            weaponId = temp.id;
            weaponName.text = temp.name;
            weaponDirectAtk.text = temp.directAtk.ToString();
            weaponIndirectAtk.text = temp.indirectAtk.ToString();
            weaponDirectWis.text = temp.directWis.ToString();
            weaponIndirectWis.text = temp.indirectWis.ToString();
            weaponPrice.text = temp.price.ToString();
            weaponCanSell.isOn = temp.sell;
        }
    }

    private void RemoveWeapon()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].id == weaponId)
            {
                weapons.RemoveAt(i);
                break;
            }
        }
        weaponId = -1;
    }

    #endregion


    #region IO
    public void SaveObjects()
    {
        ObjectSaveLoad.XmlSave(ObjectSaveLoad.CreateObjectContainer(items, weapons, races, characters), filename);
    }

    public void LoadObjects()
    {

        if (!System.IO.File.Exists("Objects.xml"))
        {
            Debug.Log("File is not exist!");
            return;
        }
        try
        {
            ObjectXmlContainer container = ObjectSaveLoad.XmlLoad<ObjectXmlContainer>(filename);
            InitialObjects();
            DisableAllGroup();
            ClearObjectList();
            createType = CreateType.none;

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


        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    #endregion

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        DisableAllGroup();
        InitialObjects();
        dicItemType = new Dictionary<int, string>();
        dicRace = new Dictionary<int, string>();
        dicWeapon = new Dictionary<int, string>();

        SetItemType();
        SetRace();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetItemType()
    {
        dicItemType.Add((int)ItemType.cure, "Cure");
        dicItemType.Add((int)ItemType.resurge, "Resurge");
        dicItemType.Add((int)ItemType.special, "Special");

        itemItemType.options.Clear();
        foreach (string value in dicItemType.Values)
        {
            itemItemType.options.Add(new Dropdown.OptionData() { text = value });
        }

        itemItemType.RefreshShownValue();
    }

    private void SetRace()
    {
        dicRace.Add(-1, "None");

        itemUseRace.options.Clear();
        itemNewRace.options.Clear();

        itemUseRace.options.Add(new Dropdown.OptionData() { text = "None" });
        itemNewRace.options.Add(new Dropdown.OptionData() { text = "None" });

        foreach (CharacterType race in races)
        {
            dicRace.Add(race.id, race.name);
            itemUseRace.options.Add(new Dropdown.OptionData() { text = race.name });
            itemNewRace.options.Add(new Dropdown.OptionData() { text = race.name });
            charRace.options.Add(new Dropdown.OptionData() { text = race.name });
        }
        itemUseRace.RefreshShownValue();
        itemNewRace.RefreshShownValue();
        charRace.RefreshShownValue();
    }
}

public enum CreateType
{
    none,
    item,
    weapon,
    race,
    character
}
