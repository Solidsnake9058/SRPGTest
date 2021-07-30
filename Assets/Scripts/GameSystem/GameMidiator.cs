using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMidiator : MonoBehaviour
{
    public static GameMidiator m_Instance { get; private set; }

    public GameManager m_GameManager { get; private set; }

    [Header("Sub system")]
    [SerializeField]
    private ElementManager _ElementManager = default;
    [SerializeField]
    private StageManager _StageManager = default;
    [SerializeField]
    private StageMapManager _StageMapManager = default;
    [SerializeField]
    private PlayerDataManager _PlayerDataManager = default;
    [SerializeField]
    private PlayerManager _PlayerManager = default;
    [SerializeField]
    private ScenarionManager _ScenarionManager = default;
    [SerializeField]
    private GameUIManager _GameUIManager = default;

    public ElementManager m_ElementManager { get { return _ElementManager; } }
    public StageManager m_StageManager { get { return _StageManager; } }
    public StageMapManager m_StageMapManager { get { return _StageMapManager; } }
    public PlayerDataManager m_PlayerDataManager { get { return _PlayerDataManager; } }

    public PlayerManager m_PlayerManager { get { return _PlayerManager; } }
    public ScenarionManager m_ScenarionManager { get { return _ScenarionManager; } }
    public GameUIManager m_GameUIManager { get { return _GameUIManager; } }

    void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        GameSetting();
    }

    private void GameSetting()
    {
        m_ElementManager.GameSetting();
        m_ScenarionManager.GameSetting();
        m_StageMapManager.GameSetting();
        m_StageManager.GameSetting();
        m_PlayerManager.GameSetting();

        m_GameUIManager.GameSetting();
    }

    private void Start()
    {
        m_GameManager = GameManager.m_Instance;
    }
}
