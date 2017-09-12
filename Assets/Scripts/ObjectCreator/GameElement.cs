using System.Collections;
using System.Collections.Generic;


public class GameElement
{
    public List<Item> items { get; private set; }
    public List<Weapon> weapons { get; private set; }
    public List<CharacterType> races { get; private set; }
    public List<CharacterTemplate> characters { get; private set; }

    public GameElement(List<Item> items, List<Weapon> weapons, List<CharacterType> races, List<CharacterTemplate> characters)
    {
        this.items = items;
		this.weapons = weapons;
		this.races = races;
		this.characters = characters;
    }
}
