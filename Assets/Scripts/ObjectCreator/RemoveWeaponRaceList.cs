using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveWeaponRaceList : MonoBehaviour
{
    public void RemoveRaceWeapon()
    {
        ObjectCreatorManager.instance.RemoveWeaponRaceList(this.name);
    }
}
