using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HexTilePathFinder : MonoBehaviour
{

    public HexTilePathFinder()
    {

    }

    public static HexTilePath FindPath(HexTile originTile, HexTile desinationTile)
    {
        return FindPath(originTile, desinationTile, new Vector2[0], false);
    }

    public static HexTilePath FindPath(HexTile originTile, HexTile desinationTile, bool ignorePlayer)
    {
        return FindPath(originTile, desinationTile, new Vector2[0], ignorePlayer);
    }

    public static HexTilePath FindPath(HexTile originTile, HexTile desinationTile, Vector2[] occupied)
    {
        return FindPath(originTile, desinationTile, occupied, false);
    }

    public static HexTilePath FindPath(HexTile originTile, HexTile desinationTile, Vector2[] occupied, bool ignorePlayers)
    {
        //Debug.Log("Ori:(" + originTile.gridPostion.x + "," + originTile.gridPostion.y + "), Des:(" + desinationTile.gridPostion.x + "," + desinationTile.gridPostion.y + ")");

        List<HexTile> closed = new List<HexTile>();
        List<HexTilePath> open = new List<HexTilePath>();

        HexTilePath originPath = new HexTilePath();
        originPath.addTile(originTile);

        open.Add(originPath);

        while (open.Count > 0)
        {
            HexTilePath current = open[0];
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

            foreach (HexTile t in current.lastTile.neighbors)
            {
                HexTilePath newTilePath = new HexTilePath(current);
                List<Player> playerTiles = GameManager.instance.players.Where(x => x.gridPosition == t.gridPosition).ToList();
                if (t.impassible || occupied.Contains(t.gridPosition) || (!ignorePlayers && playerTiles.Count > 0))
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
