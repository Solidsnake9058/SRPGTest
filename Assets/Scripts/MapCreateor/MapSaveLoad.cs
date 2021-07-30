using UnityEngine;
using System.Collections;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

public class TileXml
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("spritIndex")]
    public int spriteIndex;

    [XmlAttribute("spritChestIndex")]
    public int spriteChestIndex;

    [XmlAttribute("isChestOpened")]
    public bool isChestOpened;

    [XmlAttribute("locX")]
    public int locX;

    [XmlAttribute("locY")]
    public int locY;

    [XmlAttribute("gold")]
    public int gold;

    [XmlAttribute("itemId")]
    public int itemId;

    [XmlAttribute("weaponId")]
    public int weaponId;

    [XmlAttribute("isShop")]
    public bool isShop;

    public TileXml()
    {

    }

    public TileXml(int locX,int locY)
    {
        id = (int)TileType2D.Plain;
        spriteIndex = 0;
        spriteChestIndex = 0;
        this.locX = locX;
        this.locY = locY;
        gold = 0;
        itemId = -1;
        weaponId = -1;
        isShop = false;
    }
}

[XmlRoot("MapCollection")]
public class MapContainer
{
    [XmlAttribute("stageTitle")]
    public string stageTitle;

    [XmlAttribute("sizeX")]
    public int sizeX;
    [XmlAttribute("sizeY")]
    public int sizeY;

    [XmlArray("Tiles")]
    [XmlArrayItem("Tile")]
    public List<TileXml> tiles = new List<TileXml>();

    [XmlArray("UserPlayerRecords")]
    [XmlArrayItem("UserPlayerRecord")]
    public List<PlayerRecord> userPlayerRecords = new List<PlayerRecord>();

    [XmlArray("EnemyPlayerRecords")]
    [XmlArrayItem("EnemyPlayerRecord")]
    public List<PlayerRecord> enemyPlayerRecords = new List<PlayerRecord>();

    [XmlArray("shopItemList")]
    [XmlArrayItem("shopItemList")]
    public List<int> shopItemList = new List<int>();

    [XmlArray("shopWeaponList")]
    [XmlArrayItem("shopWeaponList")]
    public List<int> shopWeaponList = new List<int>();

    [XmlArray("scenarioList")]
    [XmlArrayItem("scenarioList")]
    public List<Scenarion> scenarionList = new List<Scenarion>();

    [XmlArray("stageClearConditionList")]
    [XmlArrayItem("stageClearConditionList")]
    public List<StageClearCondition> stageClearConditionList = new List<StageClearCondition>();

    public Dictionary<int, Dictionary<int, TileXml>> m_TileDataMap = new Dictionary<int, Dictionary<int, TileXml>>();

    public void InitTileDataMap()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (!m_TileDataMap.ContainsKey(tiles[i].locX))
            {
                m_TileDataMap.Add(tiles[i].locX, new Dictionary<int, TileXml>());
            }
            m_TileDataMap[tiles[i].locX].Add(tiles[i].locY, tiles[i]);
        }
    }
}

public static class MapSaveLoad
{
    public static MapContainer CreateMapContainer(List<List<Tile>> map)
    {
        List<TileXml> tiles = new List<TileXml>();

        for (int i = 0; i < map.Count; i++)
        {
            for (int j = 0; j < map[i].Count; j++)
            {
                tiles.Add(CreateTileXml(map[i][j]));
            }
        }

        return new MapContainer()
        {
            sizeX = map.Count,
            sizeY = map[0].Count,
            tiles = tiles,
        };
    }

    public static MapContainer CreateMapContainer(List<List<HexTile>> map, List<PlayerRecord> userPlayerRecords, List<PlayerRecord> enemyPlayerRecords, List<int> shopItemList, List<int> shopWeaponList, List<Scenarion> scenarioList, List<StageClearCondition> stageClearConditionList)
    {
        List<TileXml> tiles = new List<TileXml>();

        for (int i = 0; i < map.Count; i++)
        {
            for (int j = 0; j < map[i].Count; j++)
            {
                tiles.Add(map[i][j].CreateTileXml());
            }
        }

        return new MapContainer()
        {
            sizeX = map[0].Count,
            sizeY = map.Count,
            tiles = tiles,
            userPlayerRecords = userPlayerRecords,
            enemyPlayerRecords = enemyPlayerRecords,
            shopItemList = shopItemList,
            shopWeaponList = shopWeaponList,
            scenarionList = scenarioList,
            stageClearConditionList = stageClearConditionList
        };
    }

    public static TileXml CreateTileXml(Tile tile)
    {
        return new TileXml()
        {
            id = (int)tile.type,
            locX = (int)tile.gridPosition.x,
            locY = (int)tile.gridPosition.y
        };
    }

    //public static TileXml CreateTileXml(HexTile tile)
    //{
    //    return new TileXml()
    //    {
    //        id = (int)tile.m_TileType2D,
    //        spritIndex = tile.m_SpriteIndex,
    //        locX = tile.m_Hex.m_Q,
    //        locY = tile.m_Hex.m_R,
    //        gold = tile.m_Gold,
    //        itemId = tile.m_ItemId,
    //        weaponId = tile.m_WeaponId,
    //        spritChestIndex = tile.m_SpritChestIndex,
    //        isShop = tile.m_IsShop
    //    };
    //}

    public static void Save(MapContainer mapContainer, string filename)
    {
        var serializer = new XmlSerializer(typeof(MapContainer));
        using (var stream = new FileStream(filename, FileMode.Create))
        {
            serializer.Serialize(stream, mapContainer);
        }
    }

    public static MapContainer Load(string filename)
    {
        var serializer = new XmlSerializer(typeof(MapContainer));
        using (var stream = new FileStream(filename, FileMode.Open))
        {
            return serializer.Deserialize(stream) as MapContainer;
        }
    }
}
