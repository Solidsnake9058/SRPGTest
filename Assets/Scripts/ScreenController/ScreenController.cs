﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScreenController : MonoBehaviour
{
    public static ScreenController instance;

    public float mSpeed = 5;

    public float zoomMin = 5f;
    public float zoomMax = 13f;
    public float zoomChange = 2f;

    public Image imageRight;
    public Image imageLeft;
    public Image imageUp;
    public Image imageDown;
    public Transform mainCamera;
    private new Camera camera;

    public Vector3 limitPointA;
    public Vector3 limitPointB;
    public Vector3 limitPointC;
    public Vector3 limitPointD;

    public ArrowButton btnRight;
    public ArrowButton btnDown;
    public ArrowButton btnLeft;
    public ArrowButton btnUp;

    Transform playerUITransform;
    List<PlayerUI> playerUIs;

    private void Awake()
    {
        instance = this;
        imageRight.enabled = imageLeft.enabled = imageUp.enabled = imageDown.enabled = false;
        camera = GetComponentInChildren<Camera>();

        playerUITransform = transform.Find("PlayerUIs");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mPoint = Input.mousePosition;

        imageRight.rectTransform.position = new Vector2(imageRight.rectTransform.position.x, mPoint.y);
        imageLeft.rectTransform.position = new Vector2(imageLeft.rectTransform.position.x, mPoint.y);
        imageUp.rectTransform.position = new Vector2(mPoint.x, imageUp.rectTransform.position.y);
        imageDown.rectTransform.position = new Vector2(mPoint.x, imageDown.rectTransform.position.y);

    }

    public void ResetCamera()
    {
        btnRight.pivot = PivotType.Right;
        btnDown.pivot = PivotType.Down;
        btnLeft.pivot = PivotType.Left;
        btnUp.pivot = PivotType.Up;
        mainCamera.rotation = Quaternion.identity;
        for (int i = 0; i < playerUIs.Count; i++)
        {
            playerUIs[i].transform.Rotate(45, 45, 0);
        }
    }

    public void SetPlayerUIs()
    {
        playerUIs = (playerUITransform.GetComponentsInChildren<PlayerUI>()).ToList<PlayerUI>(); ;
    }

    public void TurnCameraRight()
    {
        PivotType temp = btnRight.pivot;
        btnRight.pivot = btnDown.pivot;
        btnDown.pivot = btnLeft.pivot;
        btnLeft.pivot = btnUp.pivot;
        btnUp.pivot = temp;
        mainCamera.Rotate(0, (int)System.Math.Round((mainCamera.rotation.y + 90f) / 10, 0) * 10, 0);
        for (int i = 0; i < playerUIs.Count; i++)
        {
            playerUIs[i].transform.Rotate(Vector3.up, 90);
        }

    }

    public void RemoveUI(string uiName)
    {
        playerUIs.Remove(playerUIs.Where(x => x.name == uiName).FirstOrDefault());
    }

    public void TurnCameraLeft()
    {
        PivotType temp = btnUp.pivot;
        btnUp.pivot = btnLeft.pivot;
        btnLeft.pivot = btnDown.pivot;
        btnDown.pivot = btnRight.pivot;
        btnRight.pivot = temp;
        mainCamera.Rotate(0, (int)System.Math.Round((mainCamera.rotation.y - 90f) / 10, 0) * 10, 0);
        for (int i = 0; i < playerUIs.Count; i++)
        {
            playerUIs[i].transform.Rotate(Vector3.up, -90);
        }

    }

    public void SetPlayerUIIsShow(bool isShowPlayerUI)
    {
        for (int i = 0; i < playerUIs.Count; i++)
        {
            if (isShowPlayerUI)
            {
                playerUIs[i].SetShowUI();
            }
            else
            {
                playerUIs[i].SetHideUI();
            }
        }
    }

    public void SetLimitPoint(Vector3 connerPointA, Vector3 connerPointB, Vector3 connerPointC, Vector3 connerPointD)
    {
        limitPointA = GetCrossPoint(connerPointA, connerPointB, 1, -1);
        limitPointB = GetCrossPoint(connerPointB, connerPointC, -1, 1);
        limitPointC = GetCrossPoint(connerPointC, connerPointD, 1, -1);
        limitPointD = GetCrossPoint(connerPointD, connerPointA, -1, 1);
    }

    private Vector3 GetCrossPoint(Vector3 point1, Vector3 point2, float m1, float m2)
    {
        float x = ((point1.x * m1 - point2.x * m2) - point1.z + point2.z) / (m1 - m2);
        float z = (x - point1.x) * m1 + point1.z;

        return new Vector3(x, 0, z);
    }

    public void MoveCamera(PivotType pivot)
    {
        Vector3 move = new Vector3();

        switch (pivot)
        {
            case PivotType.Right:
                move = new Vector3(mSpeed, 0, -mSpeed);
                break;
            case PivotType.Left:
                move = new Vector3(-mSpeed, 0, mSpeed);
                break;
            case PivotType.Up:
                move = new Vector3(mSpeed, 0, mSpeed);
                break;
            case PivotType.Down:
                move = new Vector3(-mSpeed, 0, -mSpeed);
                break;
        }

        Vector3 newPoint = mainCamera.localPosition + move * Time.deltaTime;
        if (!isContain(limitPointA, limitPointB, limitPointC, limitPointD, newPoint))
        {
            return;
        }
        mainCamera.localPosition = newPoint;
    }

    public void SetCameraPos(Vector3 pos)
    {
        mainCamera.localPosition = pos;
    }

    public bool isContain(Vector3 mp1, Vector3 mp2, Vector3 mp3, Vector3 mp4, Vector3 mp)
    {
        if (Multiply(mp1, mp2, mp) * Multiply(mp3, mp4, mp) >= 0 && Multiply(mp4, mp1, mp) * Multiply(mp2, mp3, mp) >= 0)
        {
            return true;
        }
        return false;
    }

    private double Multiply(Vector3 p1, Vector3 p2, Vector3 p0)
    {
        return (p2.x - p1.x) * (p0.z - p1.z) - (p0.x - p1.x) * (p2.z - p1.z);
    }

    public void ZoomIn()
    {
        float newZoom = camera.orthographicSize - zoomChange;
        if (newZoom < zoomMin)
        {
            camera.orthographicSize = zoomMin;
        }
        else
        {
            camera.orthographicSize = newZoom;
        }
    }

    public void ZoomOut()
    {
        float newZoom = camera.orthographicSize + zoomChange;
        if (newZoom > zoomMax)
        {
            camera.orthographicSize = zoomMax;
        }
        else
        {
            camera.orthographicSize = newZoom;
        }
    }
}
