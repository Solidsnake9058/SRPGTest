using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class HexTile : IGameItem, IPointerClickHandler, IPointerEnterHandler
{
    protected static StageMapManager m_StageMapManager { get { return GameMidiator.m_Instance.m_StageMapManager; } }
    protected static PlayerDataManager m_PlayerDataManager { get { return GameMidiator.m_Instance.m_PlayerDataManager; } }
    protected static ScenarionManager m_ScenarionManager { get { return GameMidiator.m_Instance.m_ScenarionManager; } }
    protected static GameUIManager m_GameUIManager { get { return GameMidiator.m_Instance.m_GameUIManager; } }

    public HexCoord m_Hex { get; private set; }

    private SpriteMetarial m_Prefab;
    [SerializeField]
    private Renderer m_Visual;
    public Transform tileLine;
    public bool m_IsHighLight { get; private set; }

    public TileType m_TileType { get; private set; }
    public TileType2D m_TileType2D { get; private set; }

    public float m_MovementCost { get; private set; }
    public bool m_Impassible { get; private set; }
    private int m_SpriteIndex;
    private int m_SpritChestIndex;

    public List<HexTile> m_Neighbors { get; private set; }

    public float m_DefenseRate { get; private set; }

    //[Header("Chest Setting")]
    private bool m_IsHaveChest = false;
    private bool m_IsChestOpened = false;
    public bool m_IsShop { get; private set; }
    private int m_Gold;
    private int m_ItemId;
    private int m_WeaponId;

    private UnityAction<PointerEventData> m_ClickAction;

    public Vector3 HexTilePos()
    {
        return m_Hex.PositionSqr();
    }

    public TileData CreateTileXml()
    {
        return new TileData()
        {
            id = (int)m_TileType2D,
            spriteIndex = m_SpriteIndex,
            locX = m_Hex.m_Q,
            locY = m_Hex.m_R,
            gold = m_Gold,
            itemId = m_ItemId,
            weaponId = m_WeaponId,
            spriteChestIndex = m_SpritChestIndex,
            isShop = m_IsShop
        };
    }

    private void GenerateNeighbors()
    {
        m_Neighbors = new List<HexTile>();
        for (int i = 0; i < 6; i++)
        {
            HexTile tile = m_StageMapManager.GetMapTile(m_Hex.HexNeighbor(i));
            if (tile != null)
            {
                m_Neighbors.Add(tile);
            }
        }

        //[+1,0][-1,0][0,+1][0,-1][+1,-1][-1,+1]
        //for (int i = 0; i < 6; i++)
        //{
        //    Vector2 n = MapHexIndex(CubeNeighbor(new Vector2(m_Hex.m_Q, m_Hex.m_R), i));

        //    if (n.x < 0 || n.x > m_MapSizeX - 1 - (n.y % 2) || n.y < 0 || n.y > m_MapSizeY - 1)
        //    {
        //        continue;
        //    }
        //    if (SceneManager.GetActiveScene().name == "GameScene")
        //    {
        //        //m_Neighbors.Add(GameMidiator.m_Instance.m_StageMapManager.m_MapHex[(int)n.y][(int)n.x]);
        //    }
        //    else if (SceneManager.GetActiveScene().name == "MapCreatorScene")
        //    {
        //        m_Neighbors.Add(MapCreatorManager.instance.mapHex[(int)n.y][(int)n.x]);
        //    }

        //}
    }

    public void SetHightLight(bool isHighLight, bool isAtk)
    {
        m_IsHighLight = isHighLight;
        m_Visual.material.color = m_IsHighLight ? (isAtk ? GameManager.m_Instance.m_AttackTileColor : GameManager.m_Instance.m_MoveTileColor) : Color.white;
    }


    //public static Vector2 CubeDirection(int direction)
    //{
    //    return m_CubeDirections[direction];
    //}

    //public static Vector2 CubeNeighbor(Vector2 cube, int direction)
    //{
    //    return cube + CubeDirection(direction);
    //}

    //public static Vector2 Scale(Vector2 pos, int k)
    //{
    //    return pos * k;
    //}

    //public static List<Vector2> CubeRing(Vector2 center, int radius)
    //{
    //    List<Vector2> results = new List<Vector2>();
    //    if (radius <= 0)
    //    {
    //        results.Add(center);
    //        return results;
    //    }

    //    var cube = center + Scale(CubeDirection(4), radius);
    //    for (int i = 0; i < 6; i++)
    //    {
    //        for (int j = 0; j < radius; j++)
    //        {
    //            results.Add(cube);
    //            cube = CubeNeighbor(cube, i);
    //        }
    //    }
    //    return results;
    //}

    //public static List<Vector2> CubeSpiral(Vector2 center, int min, int max)
    //{

    //    List<Vector2> results = new List<Vector2>();
    //    if (min > max)
    //    {
    //        return new List<Vector2>();
    //    }
    //    for (int i = min; i <= max; i++)
    //    {
    //        results.AddRange(CubeRing(center, i));
    //    }
    //    return results;
    //}

    //public static List<HexTile> GetCubeRingTile(Vector2 center, int radius, int mapSizeX, int mapSizeY)
    //{
    //    List<HexTile> cubeRingTile = new List<HexTile>();
    //    List<Vector2> cubeRing = CubeRing(center, radius);

    //    for (int i = 0; i < cubeRing.Count; i++)
    //    {
    //        Vector2 n = MapHexIndex(cubeRing[i]);

    //        if (n.x < 0 || n.x > mapSizeX - 1 - (n.y % 2) || n.y < 0 || n.y > mapSizeY - 1)
    //        {
    //            continue;
    //        }
    //        if (SceneManager.GetActiveScene().name == "GameScene")
    //        {
    //            cubeRingTile.Add(GameMidiator.m_Instance.m_StageMapManager.m_MapHex[(int)n.y][(int)n.x]);
    //        }
    //    }
    //    return cubeRingTile;
    //}

    public static List<HexCoord> GetCubeRingTile(HexCoord center, int radius)
    {
        List<HexCoord> cubeRingTile = new List<HexCoord>();
        List<HexCoord> cubeRing = HexCoord.HexRing(center, radius);
        for (int i = 0; i < cubeRing.Count; i++)
        {
            HexTile tile = m_StageMapManager.GetMapTile(cubeRing[i]);
            if (tile == null)
            {
                cubeRingTile.Remove(tile.m_Hex);
            }
        }
        return cubeRingTile;
    }

    //public static List<HexTile> GetCubeSpiralTile(Vector2 center, int min, int max, int mapSizeX, int mapSizeY)
    //{
    //    List<HexTile> cubeRingTile = new List<HexTile>();
    //    List<Vector2> cubeRing = CubeSpiral(center, min, max);

    //    for (int i = 0; i < cubeRing.Count; i++)
    //    {
    //        Vector2 n = MapHexIndex(cubeRing[i]);

    //        if (n.x < 0 || n.x > mapSizeX - 1 - (n.y % 2) || n.y < 0 || n.y > mapSizeY - 1)
    //        {
    //            continue;
    //        }
    //        if (SceneManager.GetActiveScene().name == "GameScene")
    //        {
    //            cubeRingTile.Add(GameMidiator.m_Instance.m_StageMapManager.m_MapHex[(int)n.y][(int)n.x]);
    //        }
    //    }
    //    return cubeRingTile;
    //}


    //public static HexCoord Subtract(HexCoord a, HexCoord b)
    //{
    //    return new HexCoord(a.m_Q - b.m_Q, a.m_R - b.m_R);
    //}

    //public static int Length(HexCoord hex)
    //{
    //    return (int)((Math.Abs(hex.m_Q) + Math.Abs(hex.m_R) + Math.Abs(hex.m_Z)) / 2);
    //}

    //public static int Distance(HexCoord a, HexCoord b)
    //{
    //    return Length(Subtract(a, b));
    //}

    //public static Vector2 MapHexIndex(Vector2 pos)
    //{
    //    return new Vector2(pos.x + (((int)pos.y) >> 1), pos.y);
    //}

    public void TileInitialize(TileData tileData)
    {
        SetType2D((TileType2D)tileData.id, tileData.spriteIndex);
        m_SpritChestIndex = tileData.spriteChestIndex;
        m_Hex = new HexCoord(tileData.locX, tileData.locY);
        m_Gold = tileData.gold;
        m_ItemId = tileData.itemId;
        m_WeaponId = tileData.weaponId;
        m_IsShop = tileData.isShop;
        if (m_Gold > 0 || m_ItemId > 0 || m_WeaponId > 0)
        {
            m_IsHaveChest = true;
            m_IsChestOpened = false;
        }
        transform.localPosition = HexTilePos();
    }

    public override void GameSetting()
    {
        GenerateNeighbors();
        transform.name = "Tile [" + m_Hex.m_Q + "," + m_Hex.m_R + "," + m_Hex.m_Z + "]";
        SetCliclAction(GameManager.m_Instance != null);
    }

    public void SetChestState(bool isOpened)
    {
        if (isOpened)
        {
            SetOpenChest();
        }
    }

    public bool OpenChest()
    {
        if (m_IsHaveChest && !m_IsChestOpened)
        {
            SetOpenChest();
            m_PlayerDataManager.GetChest(m_Gold, m_ItemId, m_WeaponId);
            return true;
        }
        return false;
    }

    private void SetOpenChest()
    {
        GameObject container = transform.Find("Visuals").gameObject;
        SetType2D(m_TileType2D, m_SpritChestIndex + container.GetComponentInChildren<SpriteMetarial>().GetSpritesCount());
        m_IsChestOpened = true;
    }

    public void SetCliclAction(bool isGaming)
    {
        if (isGaming)
        {
            m_ClickAction = ClickEventGame;

        }
        else
        {
            m_ClickAction = ClickEventCreator;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        m_ClickAction?.Invoke(eventData);
    }

    public void ClickEventGame(PointerEventData eventData)
    {
        if (m_ScenarionManager.m_IsScenarionRunning)
        {
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left && !m_GameUIManager.m_IsUIMenuShowing)
        {
            Player player = GameManager.m_Instance.GetSelectedPlayer();
            if (player != null)
            {
                PlayerState playerState = player.m_PlayerState;
                switch (playerState)
                {
                    case PlayerState.Active:
                    case PlayerState.Wait:
                        GameManager.m_Instance.ShowPlayerTileMenu(m_Hex, m_IsShop);
                        break;
                    case PlayerState.Move:
                        GameManager.m_Instance.MoveCurrentPlayer(this);
                        break;
                    case PlayerState.Action:
                        GameManager.m_Instance.AttackWithCurrentPlayer(m_Hex);
                        break;
                }
            }
            else
            {
                GameManager.m_Instance.ShowPlayerTileMenu(m_Hex, m_IsShop);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            GameManager.m_Instance.Cancel();
        }
    }

    public void ClickEventCreator(PointerEventData eventData)
    {
        if (!MapCreatorManager.instance.isScenarioMode)
        {
            if (MapCreatorManager.instance.isGetPos)
            {
                MapCreatorManager.instance.conditionGetPosX.text = m_Hex.m_Q.ToString();
                MapCreatorManager.instance.conditionGetPosY.text = m_Hex.m_R.ToString();
                MapCreatorManager.instance.pointer.position = transform.position;
            }
            else
            {
                switch (MapCreatorManager.instance.settingSelection)
                {
                    case MapSettingType.Tile:
                        //SetType(MapCreatorManager.instance.pallerSelection);
                        SetType2D(MapCreatorManager.instance.pallerSelection2D, MapCreatorManager.instance.spriteIndex);
                        if (MapCreatorManager.instance.pallerSelection2D == TileType2D.Plain && MapCreatorManager.instance.isHaveChest.isOn)
                        {
                            m_IsHaveChest = true;
                            m_Gold = Convert.ToInt32(MapCreatorManager.instance.chestGoldInput.text);
                            m_ItemId = MapCreatorManager.instance.chestItem;
                            m_WeaponId = MapCreatorManager.instance.chestWeapon;
                        }
                        else if (MapCreatorManager.instance.pallerSelection2D == TileType2D.Villa && MapCreatorManager.instance.isShop.isOn)
                        {
                            m_IsShop = true;
                        }
                        else
                        {
                            m_IsHaveChest = false;
                            m_IsShop = false;
                            m_Gold = 0;
                            m_ItemId = -1;
                            m_WeaponId = -1;
                        }
                        break;
                    case MapSettingType.Player:
                        if (eventData.button == PointerEventData.InputButton.Left)
                        {
                            MapCreatorManager.instance.SetPlayer(m_Hex, transform.position);
                        }
                        else if (eventData.button == PointerEventData.InputButton.Right)
                        {
                            MapCreatorManager.instance.SetPlayer(m_Hex, transform.position, true);
                        }
                        break;
                    case MapSettingType.Enemy:
                        if (eventData.button == PointerEventData.InputButton.Left)
                        {
                            MapCreatorManager.instance.SetEnemyPlayer(m_Hex, transform.position);
                        }
                        else if (eventData.button == PointerEventData.InputButton.Right)
                        {
                            MapCreatorManager.instance.SetEnemyPlayer(m_Hex, transform.position, true);
                        }
                        break;
                }
            }
        }
        else
        {

            if (MapCreatorManager.instance.isGetPos)
            {
                MapCreatorManager.instance.getPosX.text = m_Hex.m_Q.ToString();
                MapCreatorManager.instance.getPosY.text = m_Hex.m_R.ToString();
                MapCreatorManager.instance.pointer.position = transform.position;
            }
        }
    }

    public void ClickEvent(PointerEventData eventData)
    {
        m_ClickAction?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SceneManager.GetActiveScene().name == "MapCreatorScene" && Input.GetMouseButton(0))
        {
            if (!MapCreatorManager.instance.isScenarioMode)
            {
                switch (MapCreatorManager.instance.settingSelection)
                {
                    case MapSettingType.Tile:
                        //SetType(MapCreatorManager.instance.pallerSelection);
                        SetType2D(MapCreatorManager.instance.pallerSelection2D, MapCreatorManager.instance.spriteIndex);
                        if (MapCreatorManager.instance.pallerSelection2D == TileType2D.Plain && MapCreatorManager.instance.isHaveChest.isOn)
                        {
                            m_Gold = Convert.ToInt32(MapCreatorManager.instance.chestGoldInput.text);
                            m_ItemId = MapCreatorManager.instance.chestItem;
                            m_WeaponId = MapCreatorManager.instance.chestWeapon;
                        }
                        break;
                    case MapSettingType.Player:
                        break;
                    case MapSettingType.Enemy:
                        break;
                }

            }
        }
    }

    public void SetType2D(TileType2D t, int index)
    {
        m_TileType2D = t;
        m_SpriteIndex = index;
        m_Impassible = false;
        switch (t)
        {
            case TileType2D.Impassible:
                m_MovementCost = 9999;
                m_Impassible = true;
                m_Prefab = TilePrefabHolder.m_Instance.m_TileImpassiblePrefab;
                break;
            case TileType2D.Road:
                m_MovementCost = 1;
                m_DefenseRate = 0;
                m_Prefab = TilePrefabHolder.m_Instance.m_TileRoadPrefab;
                break;
            case TileType2D.Plain:
                m_MovementCost = 1.5f;
                m_DefenseRate = 10;
                m_Prefab = TilePrefabHolder.m_Instance.m_TilePlainPrefab;
                break;
            case TileType2D.Wasteland:
                m_MovementCost = 3;
                m_DefenseRate = 30;
                m_Prefab = TilePrefabHolder.m_Instance.m_TileWastelandPrefab;
                break;
            case TileType2D.Villa:
                m_MovementCost = 1;
                m_DefenseRate = 50;
                m_Prefab = TilePrefabHolder.m_Instance.m_TileVillaPrefab;
                break;
            case TileType2D.Forest:
                m_MovementCost = 2;
                m_DefenseRate = 40;
                m_Prefab = TilePrefabHolder.m_Instance.m_TileTreePrefab;
                break;
        }
        GenerateVisuals();
    }

    public void GenerateVisuals()
    {
        GameObject container = transform.Find("Visuals").gameObject;
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Destroy(container.transform.GetChild(i).gameObject);
        }
        SpriteMetarial newVisual = Instantiate(m_Prefab, transform.position, m_Prefab.transform.rotation, container.transform);
        newVisual.SetSprite(m_SpriteIndex);
        m_Visual = newVisual.GetComponent<Renderer>();
    }

    public void SetShowUI()
    {
        tileLine.localPosition = Vector3.zero;
    }

    public void SetHideUI()
    {
        tileLine.localPosition = new Vector3(0, -100, 0);
    }
}
