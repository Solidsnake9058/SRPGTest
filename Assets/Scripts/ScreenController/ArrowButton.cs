using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArrowButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler {

    public PivotType pivot = PivotType.Right;
    private bool isPress = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPress = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPress = false;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (isPress)
        {
            ScreenController.instance.MoveCamera(pivot);
        }
	}
}
