using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMapManager : IGameItem
{
    [SerializeField]
    private Transform m_MapTransform;

    public List<List<Tile>> m_Map { get; private set; }
    public List<List<HexTile>> m_MapHex { get; private set; }


    public void CreateStageMap(MapContainer mapContainer)
    {
        Vector3 connerPointA = Vector3.zero;
        Vector3 connerPointB = Vector3.zero;
        Vector3 connerPointC = Vector3.zero;
        Vector3 connerPointD = Vector3.zero;

        m_MapHex = new List<List<HexTile>>();
        ////Hexagons
        for (int j = 0; j < mapContainer.sizeY; j++)
        {
            int offset = j >> 1;
            List<HexTile> row = new List<HexTile>();
            for (int i = -offset; i < mapContainer.sizeX - offset; i++)
            {
                if ((j & 1) == 1 && i == mapContainer.sizeX - offset - 1)
                {
                    continue;
                }
                HexTile tile = Instantiate(PrefabHolder.instance.m_HexTileBasePrefab, m_MapTransform);
                TileXml tileData = mapContainer.m_TileDataMap[i][j];
                tile.TileInitialize(tileData, 0, i, j, mapContainer.sizeX, mapContainer.sizeY);
                row.Add(tile);
                if (j == 0)
                {
                    if (i == 0)
                    {
                        connerPointA = tile.HexTilePos();
                    }
                    else if (i == mapContainer.sizeX - offset - 1)
                    {
                        connerPointB = tile.HexTilePos();
                    }
                }
            }
            m_MapHex.Add(row);
        }
        connerPointD = new Vector3(0, 0, -mapContainer.sizeY + 1);
        connerPointC = new Vector3(connerPointB.x, 0, connerPointD.z);

        ScreenController.instance.SetLimitPoint(connerPointA, connerPointB, connerPointC, connerPointD);
    }

    public void RemoveHighlightTiles()
    {
        for (int i = 0; i < m_MapHex.Count; i++)
        {
            for (int j = 0; j < m_MapHex[i].Count; j++)
            {
                if (!m_MapHex[i][j].m_Impassible)
                {
                    m_MapHex[i][j].SetHightLight(false, false);
                }
            }
        }
    }

    public void SetTileLineIsShow(bool isShowTile)
    {
        for (int i = 0; i < m_MapHex.Count; i++)
        {
            for (int j = 0; j < m_MapHex[i].Count; j++)
            {
                if (isShowTile)
                {
                    m_MapHex[i][j].SetShowUI();
                }
                else
                {
                    m_MapHex[i][j].SetHideUI();
                }
            }
        }
    }

    public HexTile GetMapTile(HexTile.HexCoord hex)
    {
        if (hex.m_R < m_MapHex.Count && hex.m_Offset < m_MapHex[hex.m_R].Count)
        {
            return m_MapHex[hex.m_R][hex.m_Offset];
        }
        return null;
    }

    public HexTile GetMapTile(int x, int y)
    {
        HexTile.HexCoord hex = new HexTile.HexCoord(x, y);
        return GetMapTile(hex);
    }

    public HexTile GetMapTile(float x, float y)
    {
        HexTile.HexCoord hex = new HexTile.HexCoord((int)x, (int)y);
        return GetMapTile(hex);
    }

    public Vector3[] GetPathPosition(List<HexTile> path)
    {
        Vector3[] points = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            points[i] = path[i].HexTilePos();
        }
        return points;
    }

    public HexTilePath FindPath(HexTile.HexCoord originHex, HexTile.HexCoord desinationHex)
    {
        return HexTilePathFinder.FindPath(GetMapTile(originHex), GetMapTile(desinationHex), new HexTile.HexCoord[0], false);
    }

    public Vector3[] FindPath(HexTile.HexCoord originHex, HexTile.HexCoord desinationHex, bool ignorePlayer)
    {
        return GetPathPosition(HexTilePathFinder.FindPath(GetMapTile(originHex), GetMapTile(desinationHex), new HexTile.HexCoord[0], ignorePlayer).listOfTiles);
    }

    public Vector3[] FindPath(HexTile.HexCoord originHex, HexTile.HexCoord desinationHex, HexTile.HexCoord[] occupied)
    {
        return GetPathPosition(HexTilePathFinder.FindPath(GetMapTile(originHex), GetMapTile(desinationHex), occupied, false).listOfTiles);
    }
}
