using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCharLevelList : MonoBehaviour {

    public void RemoveRaceWeapon()
    {
        ObjectCreatorManager.instance.RemoveCharacterLevelList(this.name);
    }
}
