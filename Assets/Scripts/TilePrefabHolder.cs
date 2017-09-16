using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePrefabHolder : MonoBehaviour
{

    public static TilePrefabHolder instance { get; private set; }

    public GameObject tile_Road_prefab;
    public GameObject tile_Plain_prefab;
    public GameObject tile_Wasteland_prefab;
    public GameObject tile_Villa_prefab;
    public GameObject tile_Forest_prefab;
    public GameObject tile_Impassible_prefab;

    private void Awake()
    {
        instance = this;
    }
}
