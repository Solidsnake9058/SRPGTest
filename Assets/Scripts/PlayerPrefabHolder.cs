using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefabHolder : MonoBehaviour {

    public static PlayerPrefabHolder instance { get; private set; }

    public Player m_UserPlayerPrefab;
    public Player m_EnemyPlayerPrefab;

    public ActorPlayer m_ActorPlayerPrefab;

    public GameObject playerModelPrefab01;

    private void Awake()
    {
        instance = this;
    }
}
