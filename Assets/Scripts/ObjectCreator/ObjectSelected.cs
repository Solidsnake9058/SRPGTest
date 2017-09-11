using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelected : MonoBehaviour {

    public void GetObject()
    {
        ObjectCreatorManager.instance.GetObject(this.name);
    }
}
