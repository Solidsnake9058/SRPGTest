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
    public InputField itemMDef;
    public InputField itemAddHP;
    public InputField itemGold;
    public InputField itemPrice;
    public Dropdown itemUseRace;
    public Dropdown itemNewRace;
    public Toggle itemCanSell;
    public InputField itemNotice;

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
    public InputField weaponNotice;

    //Race group
    private int raceId = -1;
    private List<int> raceWeapons;
    [Header("Race group")]
    public InputField raceName;
    public Toggle raceCanFly;
    public Toggle raceCanHeal;
	public InputField raceHealRange;
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
    public InputField charGold;
    public Dropdown charWeapon;
    public Toggle charIsEnemy;
    public RectTransform charLevelList;
    public GameObject charLevelSelectPrefab;

    public Button charAddLevel;
    public Button charClearLevel;

    //private
    private CreateType createType = CreateType.none;
    private List<Item> items;
    private List<Weapon> weapons;
    private List<CharacterType> races;
    private List<CharacterTemplate> characters;
    private List<CharacterLevelTemplate> characterLevels;
    private GameElement gameElement;

    private Dictionary<int, string> dicItemType;
    private Dictionary<int, string> dicRace;
    private Dictionary<int, string> dicWeapon;

    private string filename = "/Resources/ObjectJson.txt";

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

    //Click create type
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

    public void ResetObject()
    {
        switch (createType)
        {
            case CreateType.none:
                break;
            case CreateType.item:
                ResetItem();
                break;
            case CreateType.weapon:
                ResetWeapon();
                break;
            case CreateType.race:
                ResetRace();
                break;
            case CreateType.character:
                ResetCharacter();
                break;
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
                if (AddWeapon())
                {
                    ResetWeapon();
                    LoadWeaponObjects();
                    SetWeapon();
                }
                break;
            case CreateType.race:
                if (AddRace())
                {
                    ResetRace();
                    LoadRaceObjects();
                }
                break;
            case CreateType.character:
                if (AddCharacter())
                {
                    ResetCharacter();
                    LoadCharacterObjects();
                }
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
                break;
            case CreateType.weapon:
                RemoveWeapon();
                SetWeapon();
                break;
            case CreateType.race:
                RemoveRace();
                SetRace();
                break;
            case CreateType.character:
                RemoveCharacter();
                break;
        }
        ReloadObjects();
    }

    public void UpdateObject()
    {
        switch (createType)
        {
            case CreateType.none:
                break;
            case CreateType.item:
                UpdateItem();
                break;
            case CreateType.weapon:
                UpdateWeapon();
                SetWeapon();
                break;
            case CreateType.race:
                UpdateRace();
                SetRace();
                break;
            case CreateType.character:
                UpdateCharacter();
                break;
        }
        ReloadObjects();
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
        gameElement = new GameElement(items, weapons, races, characters);
        raceWeapons = new List<int>();

        dicItemType = new Dictionary<int, string>();
        dicRace = new Dictionary<int, string>();
        dicWeapon = new Dictionary<int, string>();
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
                ResetWeapon();
                EnableGroup(weaponGroup);
                LoadWeaponObjects();
                break;
            case CreateType.race:
                ResetRace();
                EnableGroup(raceGroup);
                LoadRaceObjects();
                break;
            case CreateType.character:
                ResetCharacter();
                EnableGroup(charGroup);
                LoadCharacterObjects();
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
                LoadWeapon(id);
                break;
            case CreateType.race:
                LoadRace(id);
                break;
            case CreateType.character:
                LoadCharacter(id);
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
        itemMDef.text = "";
        itemAddHP.text = "";
        itemGold.text = "";
        itemPrice.text = "";
        itemItemType.value = 0;
        itemUseRace.value = 0;
        itemNewRace.value = 0;
        itemCanSell.isOn = false;
        itemNotice.text = "";
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
            int mdef = Convert.ToInt32(string.IsNullOrEmpty(itemMDef.text) ? "0" : itemMDef.text);
            int addHp = Convert.ToInt32(string.IsNullOrEmpty(itemAddHP.text) ? "0" : itemAddHP.text);
            int gold = Convert.ToInt32(string.IsNullOrEmpty(itemGold.text) ? "0" : itemGold.text);
            int price = Convert.ToInt32(string.IsNullOrEmpty(itemPrice.text) ? "0" : itemPrice.text);
            int itemType = dicItemType.Where(x=>x.Value == itemItemType.options[itemItemType.value].text).FirstOrDefault().Key;
            int useCharType = dicRace.Where(x => x.Value == itemUseRace.options[itemUseRace.value].text).FirstOrDefault().Key;
            int newCharType = dicRace.Where(x => x.Value == itemNewRace.options[itemNewRace.value].text).FirstOrDefault().Key;
            string notice = itemNotice.text;
            bool sell = itemCanSell.isOn;
            int id = newId == -1 ? (items.Count > 0 ? items.Select(x => x.id).Max() + 1 : 0) : newId;
            if (items.Where(x => x.name == name).Count() > 0)
            {
                Debug.LogError("Item exist");
                return false;
            }

            Item newItem = new Item(id, (ItemType)itemType, name, hp, atk, def, wis, dex, mdef, addHp, gold, price, useCharType, newCharType, sell, notice);
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
            itemMDef.text = temp.mdef.ToString();
            itemAddHP.text =temp.addHp.ToString();
            itemGold.text = temp.gold.ToString();
            itemPrice.text = temp.price.ToString();
            itemItemType.value = itemItemType.options.FindIndex(x => x.text == dicItemType[(int)temp.itemType]);
            itemUseRace.value = itemUseRace.options.FindIndex(x => x.text == dicRace[temp.useCharType]);
            itemNewRace.value = itemNewRace.options.FindIndex(x => x.text == dicRace[temp.newCharType]); 
            itemCanSell.isOn = temp.sell;
            itemNotice.text = temp.notice;

            itemItemType.RefreshShownValue();
            itemUseRace.RefreshShownValue();
            itemNewRace.RefreshShownValue();
        }
    }

    private void RemoveItem()
    {
        items.Remove(items.Where(x => x.id == itemId).FirstOrDefault());
        itemId = -1;
    }

    private void UpdateItem()
    {
        int tempId = itemId;
        Item tempItem = new Item();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == tempId)
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
        weaponNotice.text = "";
    }

    private bool AddWeapon(int newId = -1)
    {
        if (string.IsNullOrEmpty(weaponName.text))
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
            string notice = weaponNotice.text;
            bool sell = weaponCanSell.isOn;
            bool atkTwice = weaponCanTwice.isOn;

            int id = newId == -1 ? (weapons.Count > 0 ? weapons.Select(x => x.id).Max() + 1 : 0) : newId;
            if (weapons.Where(x => x.name == name).Count() > 0)
            {
                Debug.LogError("Weapons exist");
                return false;
            }

            Weapon newWeapon = new Weapon(id, name, directAtk, indirectAtk, directWis, indirectWis, price, sell, atkTwice, notice);
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
            weaponCanTwice.isOn = temp.atkTwice;
            weaponNotice.text = temp.notice;
        }
    }

    private void RemoveWeapon()
    {
        if (races.Where(x => x.equipWeapon.Contains(weaponId)).Count() > 0)
        {
            Debug.Log("Weapon is using");
            return;
        }

        weapons.Remove(weapons.Where(x => x.id == weaponId).FirstOrDefault());
        weaponId = -1;
    }

    private void UpdateWeapon()
    {
        int tempId = weaponId;
        Weapon tempWeapon = new Weapon();
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].id == tempId)
            {
                tempWeapon = weapons[i];
                weapons.RemoveAt(i);
                break;
            }
        }
        if (!AddWeapon(tempId))
        {
            weapons.Add(tempWeapon);
        }
    }

    private void LoadWeaponObjects()
    {
        ClearObjectList();
        weapons.Sort((x, y) => { return x.id.CompareTo(y.id); });

        for (int i = 0; i < weapons.Count; i++)
        {
            GameObject newObject = (GameObject)Instantiate(objectSelectPrefab, Vector3.zero, Quaternion.Euler(new Vector3()));
            newObject.GetComponent<Button>().name = weapons[i].id.ToString();
            newObject.GetComponent<Button>().GetComponentInChildren<Text>().text = weapons[i].name;
            newObject.transform.SetParent(objectList);
        }
    }
    #endregion

    #region Race Control

    private void ResetRace()
    {
        raceId = -1;
        raceWeapons = new List<int>();
        raceName.text = "";
        raceCanFly.isOn = false;
        raceCanHeal.isOn = false;
        raceHealRange.text = "";
        ReloadWeaponRace();
    }

    private bool AddRace(int newId = -1)
    {
        if (string.IsNullOrEmpty(raceName.text))
        {
            return false;
        }
        try
        {
            string raceNameString = raceName.text.Trim();
            bool canFly = raceCanFly.isOn;
            bool canHeal = raceCanHeal.isOn;
            int healRange = raceCanHeal.isOn ? (Convert.ToInt32(raceHealRange.text) >= 1 ? Convert.ToInt32(raceHealRange.text) : 1) : 0;

            int id = newId == -1 ? (races.Count > 0 ? races.Select(x => x.id).Max() + 1 : 0) : newId;
            if (races.Where(x => x.name == raceNameString).Count() > 0)
            {
                Debug.LogError("Weapons exist");
                return false;
            }

            CharacterType newRace = new CharacterType(id, raceNameString, raceWeapons, canFly, canHeal, healRange);
            races.Add(newRace);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
        return true;
    }

    private void LoadRace(string id)
    {
        CharacterType temp = races.Where(x => x.id.ToString() == id).First();
        if (temp != null)
        {
            raceId = temp.id;
            raceName.text = temp.name;
            raceWeapons = temp.equipWeapon;
            raceCanFly.isOn = temp.canFly;
            raceCanHeal.isOn = temp.canHeal;
            raceHealRange.text = temp.healRange.ToString();
            ReloadWeaponRace();
        }
    }

    private void RemoveRace()
    {
        if (characters.Where(x => x.race == raceId).Count() > 0 || items.Where(x => x.useCharType == raceId).Count() > 0 || items.Where(x => x.newCharType == raceId).Count() > 0)
        {
            Debug.LogWarning("Race is using");
            return;
        }

        races.Remove(races.Where(x => x.id == raceId).FirstOrDefault());
        raceId = -1;
    }

    private void UpdateRace()
    {
        int tempId = raceId;
        CharacterType tempRace = new CharacterType();
        for (int i = 0; i < races.Count; i++)
        {
            if (races[i].id == tempId)
            {
                tempRace = races[i];
                races.RemoveAt(i);
                break;
            }
        }
        if (!AddRace(tempId))
        {
            races.Add(tempRace);
        }
    }

    private void LoadRaceObjects()
    {
        ClearObjectList();
        races.Sort((x, y) => { return x.id.CompareTo(y.id); });

        for (int i = 0; i < races.Count; i++)
        {
            GameObject newObject = (GameObject)Instantiate(objectSelectPrefab, Vector3.zero, Quaternion.Euler(new Vector3()));
            newObject.GetComponent<Button>().name = races[i].id.ToString();
            newObject.GetComponent<Button>().GetComponentInChildren<Text>().text = races[i].name;
            newObject.transform.SetParent(objectList);
        }
    }


    public void AddWeaponRaceList()
    {
        int id = dicWeapon.Where(x => x.Value == raceWeapon.options[raceWeapon.value].text).FirstOrDefault().Key;
        if (raceWeapons.Count == 0 || (!raceWeapons.Contains(id)))
        {
            raceWeapons.Add(id);
        }

        ReloadWeaponRace();
    }

    public void RemoveWeaponRaceList(string id)
    {
        if (characters.Where(x => x.race == raceId && x.levelData.Where(y => y.equipWeapon.ToString() == id).Count() > 0).Count() > 0)
        {
            Debug.Log("Race weapon is using");
            return;
        }

        raceWeapons.Remove(Convert.ToInt32(id));

        ReloadWeaponRace();
    }

    private void ReloadWeaponRace()
    {
        ClearWeaponRaceList();
        Dictionary<int, string> temp = dicWeapon.Where(x => raceWeapons.Contains(x.Key)).ToDictionary(y => y.Key, y => y.Value);

        temp.OrderBy(x => x.Key);
        foreach (var t in temp)
        {
            GameObject newObject = (GameObject)Instantiate(raceWeaponSelectPrefab, Vector3.zero, Quaternion.Euler(new Vector3()));
            newObject.GetComponentInChildren<Button>().name = t.Key.ToString();
            newObject.transform.Find("WeaponName").GetComponent<Text>().text = t.Value;
            newObject.transform.SetParent(raceWeaponList);
        }
    }

    private void ClearWeaponRaceList()
    {
        for (int i = 0; i < raceWeaponList.transform.childCount; i++)
        {
            Destroy(raceWeaponList.transform.GetChild(i).gameObject);
        }
    }

    #endregion

    #region Character

    private void ResetCharacter()
    {
        charId = -1;
        charName.text = "";
        charRace.value = 0;
        charMove.text = "";
        charLevel.text = "";
        charExp.text = "";
        charHP.text = "";
        charAtk.text = "";
        charDef.text = "";
        charWis.text = "";
        charDex.text = "";
        charMdef.text = "";
        charGold.text = "";
        charWeapon.value = 0;
        charIsEnemy.isOn = false;
        characterLevels = new List<CharacterLevelTemplate>();
        ClearCharacterLevelList();
        ChangeCharacterEnemy();
    }

    private bool AddCharacter(int newId = -1)
    {
        if (string.IsNullOrEmpty(charName.text))
        {
            return false;
        }
        try
        {
            string name = charName.text.Trim();
            int race = dicRace.Where(x => x.Value == charRace.options[charRace.value].text).FirstOrDefault().Key;
            float move = (float)Convert.ToDouble(string.IsNullOrEmpty(charMove.text) ? "0" : charMove.text);
            int id = newId == -1 ? (characters.Count > 0 ? characters.Select(x => x.id).Max() + 1 : 0) : newId;
            bool enemy = charIsEnemy.isOn;
            if (characters.Where(x => x.name == name).Count() > 0)
            {
                Debug.LogError("Character exist");
                return false;
            }

            if (!charIsEnemy.isOn && !AddLevelList(newId))
            {
                return false;
            }

            CharacterTemplate newCharacter = new CharacterTemplate( id,  name,  race,  move, enemy,  characterLevels);
            characters.Add(newCharacter);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
        return true;
    }

    private void LoadCharacter(string id)
    {
        CharacterTemplate temp = characters.Where(x => x.id.ToString() == id).First();
        if (temp != null)
        {
            charId = temp.id;
            charName.text = temp.name;
            charRace.value = charRace.options.FindIndex(x => x.text == dicRace[(int)temp.race]);
            charMove.text = temp.move.ToString();
            charIsEnemy.isOn = temp.enemy;

            SetCharWeapon();
            if (charIsEnemy.isOn)
            {
                characterLevels = temp.levelData;
            }
            else
            {
                charLevel.text = temp.levelData[0].level.ToString();
                charExp.text = temp.levelData[0].exp.ToString();
                charHP.text = temp.levelData[0].hp.ToString();
                charAtk.text = temp.levelData[0].atk.ToString();
                charDef.text = temp.levelData[0].def.ToString();
                charWis.text = temp.levelData[0].wis.ToString();
                charDex.text = temp.levelData[0].dex.ToString();
                charMdef.text = temp.levelData[0].mdef.ToString();
                charGold.text = temp.levelData[0].gold.ToString();
                charWeapon.value = charWeapon.options.FindIndex(x => x.text == dicWeapon[(int)temp.levelData[0].equipWeapon]);

                charWeapon.RefreshShownValue();
            }
            ReloadCharacterLevel();
        }
    }

    private void RemoveCharacter()
    {
        characters.Remove(characters.Where(x => x.id == charId).FirstOrDefault());
        charId = -1;
    }

    private void UpdateCharacter()
    {
        int tempId = charId;
        CharacterTemplate tempChar = new CharacterTemplate();
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].id == tempId)
            {
                tempChar = characters[i];
                characters.RemoveAt(i);
                break;
            }
        }
        if (!AddCharacter(tempId))
        {
            characters.Add(tempChar);
        }
    }

    private void LoadCharacterObjects()
    {
        ClearObjectList();
        characters.Sort((x, y) => { return x.id.CompareTo(y.id); });

        for (int i = 0; i < characters.Count; i++)
        {
            GameObject newObject = (GameObject)Instantiate(objectSelectPrefab, Vector3.zero, Quaternion.Euler(new Vector3()));
            newObject.GetComponent<Button>().name = characters[i].id.ToString();
            newObject.GetComponent<Button>().GetComponentInChildren<Text>().text = characters[i].name;
            newObject.transform.SetParent(objectList);
        }
    }


    public void AddLevelList()
    {
        AddLevelList(-1);
    }

    private bool AddLevelList(int newId)
    {
        try
        {
            uint level = Convert.ToUInt32(string.IsNullOrEmpty(charLevel.text) ? "0" : charLevel.text);
            uint exp = Convert.ToUInt32(string.IsNullOrEmpty(charExp.text) ? "0" : charExp.text);
            uint hp = Convert.ToUInt32(string.IsNullOrEmpty(charHP.text) ? "0" : charHP.text);
            uint atk = Convert.ToUInt32(string.IsNullOrEmpty(charAtk.text) ? "0" : charAtk.text);
            uint def = Convert.ToUInt32(string.IsNullOrEmpty(charDef.text) ? "0" : charDef.text);
            uint wis = Convert.ToUInt32(string.IsNullOrEmpty(charWis.text) ? "0" : charWis.text);
            uint dex = Convert.ToUInt32(string.IsNullOrEmpty(charDex.text) ? "0" : charDex.text);
            uint mdef = Convert.ToUInt32(string.IsNullOrEmpty(charMdef.text) ? "0" : charMdef.text);
            uint gold = Convert.ToUInt32(string.IsNullOrEmpty(charGold.text) || !charIsEnemy.isOn ? "0" : charGold.text);
            int equipWeapon = dicWeapon.Where(x => x.Value == charWeapon.options[charWeapon.value].text).FirstOrDefault().Key;

            int id = newId == -1 ? (characterLevels.Count > 0 ? characterLevels.Select(x => x.id).Max() + 1 : 0) : newId;
            //if (characterLevels.Where(x => x.level == level).Count() > 0)
            //{
            //    Debug.LogError("Level exist");
            //    return false;
            //}

            CharacterLevelTemplate newLevel = new CharacterLevelTemplate(id, level, exp, hp, atk, def, wis, dex, mdef, gold, equipWeapon);

            characterLevels.Add(newLevel);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
        ReloadCharacterLevel();

        return true;
    }

    public void RemoveCharacterLevelList(string id)
    {
        characterLevels.Remove(characterLevels.Where(x => x.id.ToString() == id).FirstOrDefault());

        ReloadCharacterLevel();
    }

    public void ClearCharacterLevel()
    {
        characterLevels = new List<CharacterLevelTemplate>();
        ReloadCharacterLevel();
    }

    private void ReloadCharacterLevel()
    {
        //Lv1 HP30 Exp500 Atk10 Def10 Wis10 Dex10 Mded10 Wp:Short Sword
        ClearCharacterLevelList();
        foreach (var t in characterLevels)
        {
            GameObject newObject = (GameObject)Instantiate(charLevelSelectPrefab, Vector3.zero, Quaternion.Euler(new Vector3()));
            newObject.GetComponentInChildren<Button>().name = t.id.ToString();
            newObject.transform.Find("LevelInfo").GetComponent<Text>().text = string.Format("Lv{0} HP{1} Exp{2} Atk{3} Def{4} Wis{5} Dex{6} Mded{7} Gold{9} Weapon:{8}", t.level, t.hp, t.exp, t.atk, t.def, t.wis, t.dex, t.mdef, weapons.Where(x => x.id == t.equipWeapon).FirstOrDefault().name, t.gold);
            newObject.transform.SetParent(charLevelList);
        }
    }

    private void ClearCharacterLevelList()
    {
        for (int i = 0; i < charLevelList.transform.childCount; i++)
        {
            Destroy(charLevelList.transform.GetChild(i).gameObject);
        }
    }

    public void ChangeCharacterEnemy()
    {
        if (charIsEnemy.isOn)
        {
            charAddLevel.interactable = charClearLevel.interactable = true;
        }
        else
        {
            charAddLevel.interactable = charClearLevel.interactable = false;
            ClearCharacterLevel();
        }
    }

    #endregion

    #region IO
    public void SaveObjects()
    {
        ObjectSaveLoad.JsonSave(new GameElement(items, weapons, races, characters), filename);
    }

    public void LoadObjects()
    {

        if (!System.IO.File.Exists(filename))
        {
            Debug.Log(filename + ":File is not exist!");
            return;
        }
        try
        {
            InitialObjects();
            DisableAllGroup();
            ClearObjectList();
            createType = CreateType.none;
            gameElement = ObjectSaveLoad.JsonLoad<GameElement>(filename);

            items = gameElement.items;
            weapons = gameElement.weapons;
            races = gameElement.races;
            characters = gameElement.characters;

            SetItemType();
            SetWeapon();
            SetRace();

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    #endregion

    private void Awake()
    {
        filename = Application.dataPath + filename;
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        DisableAllGroup();
        InitialObjects();

        SetItemType();
        SetRace();
        SetWeapon();

        LoadObjects();

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
        dicRace = new Dictionary<int, string>();

        dicRace.Add(-1, "None");

        itemUseRace.options.Clear();
        itemNewRace.options.Clear();
        charRace.options.Clear();

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

        SetCharWeapon();
    }

    private void SetWeapon()
    {
        raceWeapon.options.Clear();
        dicWeapon = new Dictionary<int, string>();

        foreach (Weapon weapon in weapons)
        {
            dicWeapon.Add(weapon.id, weapon.name);
            raceWeapon.options.Add(new Dropdown.OptionData() { text = weapon.name });
        }
        raceWeapon.RefreshShownValue();
    }

    public void SetCharWeapon()
    {
        charWeapon.options.Clear();
        if (races.Count > 0)
        {
            CharacterType temp = races[dicRace.Where(x => x.Value == charRace.options[charRace.value].text).FirstOrDefault().Key];
            List<int> tempList = temp.equipWeapon;

            List<Weapon> tempWps = weapons.Where(x => tempList.Contains(x.id)).ToList();
            foreach (Weapon weapon in tempWps)
            {
                charWeapon.options.Add(new Dropdown.OptionData() { text = weapon.name });
            }
        }

        charWeapon.RefreshShownValue();
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
