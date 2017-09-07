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

    [XmlAttribute("locX")]
    public int locX;

    [XmlAttribute("locY")]
    public int locY;

}

[XmlRoot("MapCollection")]
public class MapXmlContainer
{
    [XmlAttribute("sizeX")]
    public int sizeX;
    [XmlAttribute("sizeY")]
    public int sizeY;

    [XmlArray("Tiles")]
    [XmlArrayItem("Tile")]
    public List<TileXml> tiles = new List<TileXml>();
}

public static class MapSaveLoad
{
    public static MapXmlContainer CreateMapContainer(List<List<Tile>> map)
    {
        List<TileXml> tiles = new List<TileXml>();

        for (int i = 0; i < map.Count; i++)
        {
            for (int j = 0; j < map[i].Count; j++)
            {
                tiles.Add(MapSaveLoad.CreateTileXml(map[i][j]));
            }
        }

        return new MapXmlContainer()
        {
            sizeX = map.Count,
            sizeY = map[0].Count,
            tiles = tiles
        };
    }

    public static MapXmlContainer CreateMapContainer(List<List<HexTile>> map)
	{
		List<TileXml> tiles = new List<TileXml>();

		for (int i = 0; i < map.Count; i++)
		{
			for (int j = 0; j < map[i].Count; j++)
			{
				tiles.Add(MapSaveLoad.CreateTileXml(map[i][j]));
			}
		}

		return new MapXmlContainer()
		{
			sizeX = map[0].Count,
			sizeY = map.Count,
			tiles = tiles
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

    public static TileXml CreateTileXml(HexTile tile)
	{
        return new TileXml()
        {
            id = (int)tile.type,
            locX = (int)tile.hex.q,
            locY = (int)tile.hex.r
        };
	}

    public static void Save(MapXmlContainer mapContainer, string filename)
    {
        var serializer = new XmlSerializer(typeof(MapXmlContainer));
        using (var stream = new FileStream(filename, FileMode.Create))
        {
            serializer.Serialize(stream, mapContainer);
        }
    }

    public static MapXmlContainer Load(string filename)
    {
        var serializer = new XmlSerializer(typeof(MapXmlContainer));
        using (var stream = new FileStream(filename, FileMode.Open))
        {
            return serializer.Deserialize(stream) as MapXmlContainer;
        }
    }
}
