using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class HexTilePathFinder
{
    public static HexTilePath FindPath(HexTile originTile, HexTile desinationTile, bool ignorePlayer)
    {
        return FindPath(originTile, desinationTile, new HexCoord[0], ignorePlayer);
    }

    //public static HexTilePath FindPath(HexTile originTile, HexTile desinationTile, Vector2[] occupied)
    //{
    //    return FindPath(originTile, desinationTile, occupied, false);
    //}

    //public static HexTilePath FindPath(HexTile originTile, HexTile desinationTile, Vector2[] occupied, bool ignorePlayers)
    //{
    //    //Debug.Log("Ori:(" + originTile.gridPostion.x + "," + originTile.gridPostion.y + "), Des:(" + desinationTile.gridPostion.x + "," + desinationTile.gridPostion.y + ")");

    //    List<HexTile> closed = new List<HexTile>();
    //    List<HexTilePath> open = new List<HexTilePath>();

    //    HexTilePath originPath = new HexTilePath();
    //    originPath.AddTile(originTile);

    //    open.Add(originPath);

    //    while (open.Count > 0)
    //    {
    //        HexTilePath current = open[0];
    //        open.Remove(open[0]);

    //        if (open.Where(x => x.lastTile == current.lastTile && x.costOfPath <= current.costOfPath).Count() > 0)
    //        {
    //            continue;
    //        }

    //        if (closed.Contains(current.lastTile) || (current.lastTile != originTile && current.listOfTiles.GetRange(0, current.listOfTiles.Count - 2).Contains(current.lastTile)))
    //        {
    //            continue;
    //        }
    //        if (current.lastTile == desinationTile)
    //        {
    //            return current;
    //        }

    //        closed.Add(current.lastTile);

    //        foreach (HexTile t in current.lastTile.m_Neighbors)
    //        {
    //            HexTilePath newTilePath = new HexTilePath(current);
    //            List<Player> playerTiles = GameManager.m_Instance.userPlayers.Values.Union(GameManager.m_Instance.enemyPlayers.Values).Where(x => x.gridPosition == t.m_GridPosition).ToList();
    //            if (t.m_Impassible || occupied.Contains(t.m_GridPosition) || current.listOfTiles.Contains(t) || (!ignorePlayers && playerTiles.Count > 0))
    //            {
    //                continue;
    //            }
    //            newTilePath.AddTile(t);
    //            open.Add(newTilePath);
    //        }
    //    }
    //    closed.Remove(originTile);
    //    return null;
    //}

    public static HexTilePath FindPath(HexTile originTile, HexTile desinationTile, HexCoord[] occupied, bool ignorePlayers)
    {
        //Debug.Log("Ori:(" + originTile.gridPostion.x + "," + originTile.gridPostion.y + "), Des:(" + desinationTile.gridPostion.x + "," + desinationTile.gridPostion.y + ")");

        List<HexTile> closed = new List<HexTile>();
        List<HexTilePath> open = new List<HexTilePath>();

        HexTilePath originPath = new HexTilePath();
        originPath.AddTile(originTile);

        open.Add(originPath);

        while (open.Count > 0)
        {
            HexTilePath current = open[0];
            open.Remove(open[0]);
            bool isSkip = false;

            if (closed.Contains(current.lastTile))
            {
                isSkip = true;
            }

            for (int i = 0; !isSkip && i < open.Count; i++)
            {
                if (open[i].EvaluationPath(current))
                {
                    isSkip = true;
                }
            }

            for (int i = 0; !isSkip && i < current.listOfTiles.Count - 2; i++)
            {
                if (current.listOfTiles[i].Equals(current.lastTile))
                {
                    Debug.LogWarning("Skip path by looping");
                    isSkip = true;
                }
            }

            if (isSkip)
            {
                continue;
            }

            if (current.lastTile == desinationTile)
            {
                return current;
            }

            closed.Add(current.lastTile);

            List<HexTile> hexTiles = current.lastTile.m_Neighbors;
            for (int i = 0; i < hexTiles.Count; i++)
            {
                HexTile tile = hexTiles[i];
                if (closed.Contains(tile) ||
                    tile.m_Impassible ||
                    occupied.Contains(tile.m_Hex) ||
                    current.listOfTiles.Contains(tile) ||
                    (!ignorePlayers && GameMidiator.m_Instance.m_PlayerManager.CheckHexTileEmpty(tile.m_Hex)))
                {
                    continue;
                }
                HexTilePath newTilePath = new HexTilePath(current);
                newTilePath.AddTile(tile);
                open.Add(newTilePath);
            }
        }
        closed.Remove(originTile);
        return null;
    }
}
