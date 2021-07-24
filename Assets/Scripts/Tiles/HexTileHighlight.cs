using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HexTileHighlight
{
    /*
    public static List<HexTile> FindHighlight(HexTile originTile, float movementPoints, bool isAttack = false)
    {
        return FindHighlight(originTile, movementPoints, new Vector2[0], isAttack);
    }

    public static List<HexTile> FindHighlight(HexTile originTile, float movementPoints, Vector2[] occupied)
    {
        return FindHighlight(originTile, movementPoints, occupied, false);
    }

    public static List<HexTile> FindHighlight(HexTile originTile, float movementPoints, Vector2[] occupied, bool isAttack)
    {
        List<HexTile> closed = new List<HexTile>();
        List<HexTilePath> open = new List<HexTilePath>();

        HexTilePath originPath = new HexTilePath();
        originPath.AddTile(originTile, isAttack);

        open.Add(originPath);

        while (open.Count > 0)
        {
            HexTilePath current = open[0];
            open.Remove(open[0]);
            bool isSkip = false;


            if (open.Where(x => x.lastTile == current.lastTile && x.costOfPath <= current.costOfPath).Count() > 0)
            {
                continue;
            }

            if (closed.Contains(current.lastTile) || (current.lastTile != originTile && current.listOfTiles.GetRange(0, current.listOfTiles.Count - 2).Contains(current.lastTile)))
            {
                continue;
            }
            if (current.costOfPath > movementPoints + originTile.m_MovementCost)
            {
                continue;
            }
            closed.Add(current.lastTile);

            foreach (HexTile t in current.lastTile.m_Neighbors)
            {
                if (t.m_Impassible || occupied.Contains(t.m_GridPosition) || current.listOfTiles.Contains(t))
                {
                    continue;
                }
                HexTilePath newTilePath = new HexTilePath(current);
                newTilePath.AddTile(t, isAttack);
                open.Add(newTilePath);
            }
        }
        closed.Remove(originTile);
        return closed;
    }
    */

    public static List<HexTile> FindHighlight(HexTile originTile, float movementPoints, HexCoord[] occupied, bool isAttack)
    {
        List<HexTile> closed = new List<HexTile>();
        List<HexTilePath> open = new List<HexTilePath>();

        HexTilePath originPath = new HexTilePath();
        originPath.AddTile(originTile, isAttack);

        open.Add(originPath);

        while (open.Count > 0)
        {
            HexTilePath current = open[0];
            open.Remove(open[0]);
            bool isSkip = false;

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
            if (!isSkip && current.costOfPath > movementPoints + originTile.m_MovementCost)
            {
                isSkip = true;
            }
            if (isSkip)
            {
                continue;
            }
            closed.Add(current.lastTile);

            List<HexTile> hexTiles = current.lastTile.m_Neighbors;
            for (int i = 0; i < hexTiles.Count; i++)
            {
                HexTile tile = hexTiles[i];
                if (closed.Contains(tile) ||
                    tile.m_Impassible ||
                    occupied.Contains(tile.m_Hex) ||
                    current.listOfTiles.Contains(tile))
                {
                    continue;
                }
                HexTilePath newTilePath = new HexTilePath(current);
                newTilePath.AddTile(tile, isAttack);
                open.Add(newTilePath);
            }
        }
        closed.Remove(originTile);
        return closed;
    }
}
