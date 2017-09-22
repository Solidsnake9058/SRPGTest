using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimationController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SendDamage()
    {
        BattleManager.instance.SendDamage();
    }

    public void EndBattle()
    {
        BattleManager.instance.EndBattle();
    }


}
