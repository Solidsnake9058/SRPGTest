using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabHolder : MonoBehaviour
{
    public static PrefabHolder instance { get; private set; }

    public GameObject base_tile_prefab;
    public HexTile m_HexTileBasePrefab;

    public GameObject tile_Normal_prefab;
    public GameObject tile_Difficult_prefab;
    public GameObject tile_VeryDifficult_prefab;
    public GameObject tile_Impassible_prefab;

    private void Awake()
    {
        instance = this;
    }

}
