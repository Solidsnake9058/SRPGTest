using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HexTilePath
{
    public List<HexTile> listOfTiles = new List<HexTile>();

    public float costOfPath = 0f;

    public HexTile lastTile;

    public HexTilePath() { }

    public HexTilePath(HexTilePath tp)
    {
        listOfTiles = tp.listOfTiles.ToList();
        costOfPath = tp.costOfPath;
        lastTile = tp.lastTile;
    }

    public void addTile(HexTile t,bool isAttack = false)
    {
        costOfPath += isAttack ? 1 : t.movementCost;
        listOfTiles.Add(t);
        lastTile = t;
    }
}