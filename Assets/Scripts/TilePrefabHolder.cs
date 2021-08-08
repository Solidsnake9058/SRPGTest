using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePrefabHolder : MonoBehaviour
{

    public static TilePrefabHolder m_Instance { get; private set; }

    public HexTile m_HexTileBasePrefab;
    public SpriteMetarial m_TileRoadPrefab;
    public SpriteMetarial m_TilePlainPrefab;
    public SpriteMetarial m_TileWastelandPrefab;
    public SpriteMetarial m_TileVillaPrefab;
    public SpriteMetarial m_TileTreePrefab;
    public SpriteMetarial m_TileImpassiblePrefab;

    private void Awake()
    {
        m_Instance = this;
    }
}
