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

            if (open.Where(x => x.lastTile == current.lastTile && x.costOfPath <= current.costOfPath).Count() > 0)
            {
                continue;
            }

            if (closed.Contains(current.lastTile) || (current.lastTile != originTile && current.listOfTiles.GetRange(0, current.listOfTiles.Count - 2).Contains(current.lastTile)))
            {
                continue;
            }
            if (current.lastTile == desinationTile)
            {
                return current;
            }

            closed.Add(current.lastTile);

            foreach (HexTile t in current.lastTile.m_Neighbors)
            {
                HexTilePath newTilePath = new HexTilePath(current);
                List<Player> playerTiles = GameManager.instance.userPlayers.Values.Union(GameManager.instance.enemyPlayers.Values).Where(x => x.gridPosition == t.m_GridPosition).ToList();
                if (t.m_Impassible || occupied.Contains(t.m_GridPosition) || current.listOfTiles.Contains(t) || (!ignorePlayers && playerTiles.Count > 0))
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
