using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMapManager : IGameItem
{
    [SerializeField]
    private Transform m_MapTransform;

    public List<List<HexTile>> m_MapHex { get; private set; }
    private List<HexTile> m_MapHexList = new List<HexTile>();


    public void CreateStageMap(MapContainer mapContainer)
    {
        Vector3 connerPointA = Vector3.zero;
        Vector3 connerPointB = Vector3.zero;
        Vector3 connerPointC = Vector3.zero;
        Vector3 connerPointD = Vector3.zero;
        for (int i = 0; i < m_MapHexList.Count; i++)
        {
            Destroy(m_MapHexList[i].gameObject);
        }

        m_MapHex = new List<List<HexTile>>();
        m_MapHexList = new List<HexTile>();
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
                tile.TileInitialize(tileData, 0, i, j);
                row.Add(tile);
                m_MapHexList.Add(tile);
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

        for (int i = 0; i < m_MapHexList.Count; i++)
        {
            m_MapHexList[i].GameSetting();
        }
        connerPointD = new Vector3(0, 0, -mapContainer.sizeY + 1);
        connerPointC = new Vector3(connerPointB.x, 0, connerPointD.z);

        ScreenController.m_Instance.SetLimitPoint(connerPointA, connerPointB, connerPointC, connerPointD);
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

    public HexTile GetMapTile(HexCoord hex)
    {
        if (hex.m_R >= 0 && hex.m_R < m_MapHex.Count && hex.m_Offset >= 0 && hex.m_Offset < m_MapHex[hex.m_R].Count)
        {
            return m_MapHex[hex.m_R][hex.m_Offset];
        }
        return null;
    }

    public HexTile GetMapTile(int x, int y)
    {
        HexCoord hex = new HexCoord(x, y);
        return GetMapTile(hex);
    }

    public HexTile GetMapTile(float x, float y)
    {
        HexCoord hex = new HexCoord((int)x, (int)y);
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

    public HexTilePath FindPath(HexCoord originHex, HexCoord desinationHex)
    {
        return HexTilePathFinder.FindPath(GetMapTile(originHex), GetMapTile(desinationHex), new HexCoord[0], false);
    }

    public Vector3[] FindPath(HexCoord originHex, HexCoord desinationHex, bool ignorePlayer)
    {
        return GetPathPosition(HexTilePathFinder.FindPath(GetMapTile(originHex), GetMapTile(desinationHex), new HexCoord[0], ignorePlayer).listOfTiles);
    }

    public Vector3[] FindPath(HexCoord originHex, HexCoord desinationHex, HexCoord[] occupied)
    {
        return GetPathPosition(HexTilePathFinder.FindPath(GetMapTile(originHex), GetMapTile(desinationHex), occupied, false).listOfTiles);
    }


    public List<HexTile> FindHighlight(HexTile originTile, float movementPoints, HexCoord[] occupied, bool isAttack)
    {
        return HexTileHighlight.FindHighlight(originTile, movementPoints, occupied, isAttack);
    }
    public List<HexCoord> FindHighlight(HexCoord originHex, float movementPoints, bool isAttack = false)
    {
        List<HexCoord> hexes = new List<HexCoord>();
        List<HexTile> tiles = FindHighlight(GetMapTile(originHex), movementPoints, new HexCoord[0], isAttack);
        for (int i = 0; i < tiles.Count; i++)
        {
            hexes.Add(tiles[i].m_Hex);
        }
        return hexes;
    }
    public List<HexCoord> FindHighlight(HexCoord originHex, float movementPoints, HexCoord[] occupied)
    {
        List<HexCoord> hexes = new List<HexCoord>();
        List<HexTile> tiles = FindHighlight(GetMapTile(originHex), movementPoints, occupied, false);
        for (int i = 0; i < tiles.Count; i++)
        {
            hexes.Add(tiles[i].m_Hex);
        }
        return hexes;
    }
    public void HighlightTileAt(HexCoord originHex, float distance, bool ignorePlayers = true)
    {
        List<HexCoord> highlightTiles = new List<HexCoord>();
        if (ignorePlayers)
        {
            highlightTiles = FindHighlight(originHex, distance, true);
        }
        else
        {
            highlightTiles = FindHighlight(originHex, distance, GameMidiator.m_Instance.m_PlayerManager.GetPlayerHexes(new HexCoord(-999, -999)).ToArray());
        }

        HighlightTileAt(highlightTiles);
    }

    public void HighlightTileAt(List<HexCoord> highlightTiles)
    {
        for (int i = 0; i < highlightTiles.Count; i++)
        {
            GetMapTile(highlightTiles[i]).SetHightLight(true, false);
        }
    }
}
