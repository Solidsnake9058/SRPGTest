using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class ScreenControlUI : IGameUISystem
{
    protected static GameUIManager m_GameUIManager { get { return GameMidiator.m_Instance.m_GameUIManager; } }

    //public static ScreenControlUI m_Instance;

    [SerializeField]
    private float m_MoveSpeed = 5;

    [SerializeField]
    private float m_ZoomMin = 5f;
    [SerializeField]
    private float m_ZoomMax = 13f;
    [SerializeField]
    private float m_ZoomChange = 2f;

    [SerializeField]
    private RectTransform m_ArrowWidthGroup = default;
    [SerializeField]
    private RectTransform m_ArrowHeightGroup = default;

    [SerializeField]
    private Button m_TurnRightButton = default;
    [SerializeField]
    private Button m_TurnLeftButton = default;
    [SerializeField]
    private Button m_ZoomInButton = default;
    [SerializeField]
    private Button m_ZoomOutButton = default;

    [SerializeField]
    private Transform m_MainCameraTrans = default;
    [SerializeField]
    private CinemachineVirtualCamera m_Cinemachine = default;
    private Camera m_Camera;

    private Vector3 m_LimitPointA;
    private Vector3 m_LimitPointB;
    private Vector3 m_LimitPointC;
    private Vector3 m_LimitPointD;

    [SerializeField]
    private ArrowButton[] m_ArrowButtons = default;
    private List<PlayerUI> m_PlayerUIs = new List<PlayerUI>();

    private Dictionary<PivotType, Vector3> m_DicMoveDir = new Dictionary<PivotType, Vector3>()
    {{ PivotType.Up, new Vector3(1, 0, 1) },
    { PivotType.Down, new Vector3(-1, 0, -1) },
    { PivotType.Left, new Vector3(-1, 0, 1) },
    { PivotType.Right, new Vector3(1, 0, -1) }};

    public bool m_IsCameraMoving { get; private set; }

    public override void GameSetting()
    {
        m_Camera = Camera.main;
        for (int i = 0; i < m_ArrowButtons.Length; i++)
        {
            m_ArrowButtons[i].GameSetting();
        }
        m_TurnRightButton.onClick.AddListener(TurnCameraRight);
        m_TurnLeftButton.onClick.AddListener(TurnCameraLeft);
        m_ZoomInButton.onClick.AddListener(ZoomIn);
        m_ZoomOutButton.onClick.AddListener(ZoomOut);
    }

    public override void SystemUpdate()
    {
        Vector2 mPoint = Input.mousePosition;
        m_ArrowWidthGroup.anchoredPosition = new Vector2(0, mPoint.y * m_GameUIManager.UITransRate.y - m_GameUIManager.CanvasRefRes.y / 2);
        m_ArrowHeightGroup.anchoredPosition = new Vector2(mPoint.x * m_GameUIManager.UITransRate.x - m_GameUIManager.CanvasRefRes.x / 2, 0);
        for (int i = 0; i < m_ArrowButtons.Length; i++)
        {
            m_ArrowButtons[i].SystemUpdate();
        }
    }

    public void ResetCamera()
    {
        for (int i = 0; i < m_ArrowButtons.Length; i++)
        {
            m_ArrowButtons[i].Reset();
        }
        m_MainCameraTrans.rotation = Quaternion.identity;
        for (int i = 0; i < m_PlayerUIs.Count; i++)
        {
            m_PlayerUIs[i].transform.Rotate(45, 45, 0);
        }
    }

    public void SetPlayerUI(PlayerUI playerUI)
    {
        m_PlayerUIs.Add(playerUI);
    }

    public void TurnCameraRight()
    {
        for (int i = 0; i < m_ArrowButtons.Length; i++)
        {
            m_ArrowButtons[i].LastPivot();
        }
        float angle = (m_MainCameraTrans.eulerAngles.y + 360 + 90) % 360;
        m_MainCameraTrans.eulerAngles = new Vector3(0, angle, 0);
        SetPlayerUIRotation();
    }

    public void TurnCameraLeft()
    {
        for (int i = 0; i < m_ArrowButtons.Length; i++)
        {
            m_ArrowButtons[i].NextPivot();
        }
        float angle = (m_MainCameraTrans.eulerAngles.y + 360 - 90) % 360;
        m_MainCameraTrans.eulerAngles = new Vector3(0, angle, 0);
        SetPlayerUIRotation();
    }

    public void RemoveUI(string uiName)
    {
        m_PlayerUIs.Remove(m_PlayerUIs.Where(x => x.name == uiName).FirstOrDefault());
    }

    public void RemoveUI(bool isEnemy, int index)
    {
        for (int i = 0; i < m_PlayerUIs.Count; i++)
        {
            if (m_PlayerUIs[i].CheckID(isEnemy, index))
            {
                Destroy(m_PlayerUIs[i].gameObject);
                m_PlayerUIs.RemoveAt(i);
                return;
            }
        }
    }

    public void SetPlayerUIRotation()
    {
        for (int i = 0; i < m_PlayerUIs.Count; i++)
        {
            m_PlayerUIs[i].transform.eulerAngles = new Vector3(0, Mathf.Abs(m_MainCameraTrans.eulerAngles.y) + 45, 0);
        }
    }

    public void SetPlayerUIShow(bool isShowPlayerUI)
    {
        isShowPlayerUI &= SaveManager.GetIsShowPlayerUI();
        for (int i = 0; i < m_PlayerUIs.Count; i++)
        {
            if (isShowPlayerUI)
            {
                m_PlayerUIs[i].SetShowUI();
            }
            else
            {
                m_PlayerUIs[i].SetHideUI();
            }
        }
    }

    public void SetLimitPoint(Vector3 connerPointA, Vector3 connerPointB, Vector3 connerPointC, Vector3 connerPointD)
    {
        m_LimitPointA = GetCrossPoint(connerPointA, connerPointB, 1, -1);
        m_LimitPointB = GetCrossPoint(connerPointB, connerPointC, -1, 1);
        m_LimitPointC = GetCrossPoint(connerPointC, connerPointD, 1, -1);
        m_LimitPointD = GetCrossPoint(connerPointD, connerPointA, -1, 1);
    }

    private Vector3 GetCrossPoint(Vector3 point1, Vector3 point2, float m1, float m2)
    {
        float x = ((point1.x * m1 - point2.x * m2) - point1.z + point2.z) / (m1 - m2);
        float z = (x - point1.x) * m1 + point1.z;

        return new Vector3(x, 0, z);
    }

    public void MoveCamera(PivotType pivot)
    {
        Vector3 move = m_DicMoveDir[pivot] * m_MoveSpeed;
        Vector3 newPoint = m_MainCameraTrans.localPosition + move * Time.deltaTime;
        if (!IsContainArea(m_LimitPointA, m_LimitPointB, m_LimitPointC, m_LimitPointD, newPoint))
        {
            return;
        }

        m_MainCameraTrans.localPosition = newPoint;
    }

    public void SetCameraPos(Vector3 pos)
    {
        m_MainCameraTrans.localPosition = pos;
    }

    public void SetCameraRot(Vector3 pos)
    {
        m_MainCameraTrans.rotation = Quaternion.Euler(pos);
    }

    public void MoveCameraPos(Vector3 pos, bool isDark, float time = 0, float stayTime = 0)
    {
        if (time > 0)
        {
            StartCoroutine(MoveCamera(pos, isDark, time, stayTime));
        }
        else
        {
            m_MainCameraTrans.localPosition = pos;
            m_IsCameraMoving = false;
        }
    }

    private IEnumerator MoveCamera(Vector3 pos, bool isDark, float time, float stayTime)
    {
        float timeCur = 0;
        float rate = 0;
        Vector3 posCur = m_MainCameraTrans.localPosition;
        m_IsCameraMoving = true;
        while (timeCur < time)
        {
            rate = timeCur / time;
            timeCur = Mathf.Min(timeCur + Time.deltaTime, time);
            m_MainCameraTrans.localPosition = Vector3.Lerp(posCur, pos, timeCur / time);
            if (isDark)
            {
                m_GameUIManager.m_BlackFrontUI.SetDarkValue(rate);
            }
            yield return null;
        }
        yield return new WaitForSeconds(stayTime);
        m_IsCameraMoving = false;
    }


    public bool IsContainArea(Vector3 mp1, Vector3 mp2, Vector3 mp3, Vector3 mp4, Vector3 mp)
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
        float newZoom = m_Cinemachine.m_Lens.OrthographicSize - m_ZoomChange;
        SetZoom(newZoom);
    }

    public void ZoomOut()
    {
        float newZoom = m_Cinemachine.m_Lens.OrthographicSize + m_ZoomChange;
        SetZoom(newZoom);
    }

    private void SetZoom(float newZoom)
    {
        newZoom = Mathf.Clamp(newZoom, m_ZoomMin, m_ZoomMax);
        m_Cinemachine.m_Lens.OrthographicSize = newZoom;
    }
}
