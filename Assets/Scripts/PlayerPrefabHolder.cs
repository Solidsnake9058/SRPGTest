using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefabHolder : MonoBehaviour {

    public static PlayerPrefabHolder instance { get; private set; }

    public GameObject userPlayer_prefab;
    public GameObject enemyPlayer_prefab;

    private void Awake()
    {
        instance = this;
    }
}
