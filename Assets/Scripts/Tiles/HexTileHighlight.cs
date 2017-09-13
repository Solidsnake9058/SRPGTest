using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HexTileHighlight
{
    public HexTileHighlight()
    {

    }
    public static List<HexTile> FindHighlight(HexTile originTile, int movementPoints, bool isAttack = false)
    {
        return FindHighlight(originTile, movementPoints, new Vector2[0], isAttack);
    }

    public static List<HexTile> FindHighlight(HexTile originTile, int movementPoints, Vector2[] occupied)
    {
        return FindHighlight(originTile, movementPoints, occupied, false);
    }

    public static List<HexTile> FindHighlight(HexTile originTile,int movementPoints, Vector2[] occupied, bool isAttack)
    {
        List<HexTile> closed = new List<HexTile>();
        List<HexTilePath> open = new List<HexTilePath>();

        HexTilePath originPath = new HexTilePath();
        originPath.addTile(originTile, isAttack);

        open.Add(originPath);

        while (open.Count > 0)
        {
            HexTilePath current = open[0];
            open.Remove(open[0]);

            if (closed.Contains(current.lastTile))
            {
                continue;
            }
            if (current.costOfPath > movementPoints + originTile.movementCost)
            {
                continue;
            }

            closed.Add(current.lastTile);

            foreach (HexTile t in current.lastTile.neighbors)
            {
                if (t.impassible || occupied.Contains(t.gridPosition))
                {
                    continue;
                }
                HexTilePath newTilePath = new HexTilePath(current);
                newTilePath.addTile(t, isAttack);
                open.Add(newTilePath);
            }
        }
        closed.Remove(originTile);
        return closed;
    }
}
