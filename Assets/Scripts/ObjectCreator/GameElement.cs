using System.Collections;
using System.Collections.Generic;


public class GameElement
{
    public List<Item> items { get; private set; }
    

    public List<Weapon> weapons { get; private set; }
    public List<CharacterType> races { get; private set; }
    public List<CharacterTemplate> characters { get; private set; }
    public Dictionary<int, Item> m_Items { get; private set; }
    public Dictionary<int, Weapon> m_Weapons { get; private set; }
    public Dictionary<int, CharacterType> m_Races { get; private set; }
    public Dictionary<int, CharacterTemplate> m_Characters { get; private set; }

    public GameElement(List<Item> items, List<Weapon> weapons, List<CharacterType> races, List<CharacterTemplate> characters)
    {
        this.items = items;
		this.weapons = weapons;
		this.races = races;
		this.characters = characters;

        m_Items = new Dictionary<int, Item>();
        m_Weapons = new Dictionary<int, Weapon>();
        m_Races = new Dictionary<int, CharacterType>();
        m_Characters = new Dictionary<int, CharacterTemplate>();
        for (int i = 0; i < items.Count; i++)
        {
            m_Items.Add(items[i].id, items[i]);
        }
        for (int i = 0; i < weapons.Count; i++)
        {
            m_Weapons.Add(weapons[i].id, weapons[i]);
        }
        for (int i = 0; i < races.Count; i++)
        {
            m_Races.Add(races[i].id, races[i]);
        }
        for (int i = 0; i < characters.Count; i++)
        {
            m_Characters.Add(characters[i].id, characters[i]);
        }
    }
}
