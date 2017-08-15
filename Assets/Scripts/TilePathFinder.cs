using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TilePathFinder : MonoBehaviour
{

    public TilePathFinder()
    {

    }

    public static TilePath FindPath(Tile originTile, Tile desinationTile)
    {
        //Debug.Log("Ori:(" + originTile.gridPostion.x + "," + originTile.gridPostion.y + "), Des:(" + desinationTile.gridPostion.x + "," + desinationTile.gridPostion.y + ")");

        List<Tile> closed = new List<Tile>();
        List<TilePath> open = new List<TilePath>();

        TilePath originPath = new TilePath();
        originPath.addTile(originTile);

        open.Add(originPath);

        while (open.Count > 0)
        {
            TilePath current = open[0];
            open.Remove(open[0]);

            if (closed.Contains(current.lastTile))
            {
                continue;
            }
            if (current.lastTile == desinationTile)
            {
                return current;
            }

            closed.Add(current.lastTile);

            foreach (Tile t in current.lastTile.neighbors)
            {
                TilePath newTilePath = new TilePath(current);
                if (t.impassible)
                {
                    continue;
                }
                newTilePath.addTile(t);
                open.Add(newTilePath);
            }
        }
        closed.Remove(originTile);
        return null;
    }

}
