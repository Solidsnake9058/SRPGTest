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

    public void AddTile(HexTile t,bool isAttack = false)
    {
        costOfPath += isAttack ? 1 : t.m_MovementCost;
        listOfTiles.Add(t);
        lastTile = t;
    }

    public bool EvaluationPath(HexTilePath hexTilePath)
    {
        if (lastTile.Equals(hexTilePath.lastTile) && costOfPath <= hexTilePath.costOfPath)
        {
            return true;
        }
        return false;
    }
}