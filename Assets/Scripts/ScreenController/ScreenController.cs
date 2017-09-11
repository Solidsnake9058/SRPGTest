using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenController : MonoBehaviour
{
    public static ScreenController instance;

    public float mSpeed = 5;

    public Image imageRight;
    public Image imageLeft;
    public Image imageUp;
    public Image imageDown;
    public Transform mainCamera;

    private void Awake()
    {
        instance = this;
        imageRight.enabled = imageLeft.enabled = imageUp.enabled = imageDown.enabled = false;
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

        mainCamera.localPosition += move * Time.deltaTime;
    }

    public void SetCameraPos(Vector3 pos)
    {
        mainCamera.localPosition = pos;
    }
}
